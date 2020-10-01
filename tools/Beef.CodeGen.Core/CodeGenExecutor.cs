// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Converters;
using Beef.CodeGen.Generators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace Beef.CodeGen
{
    /// <summary>
    /// The <see cref="CodeGenExecutor"/> arguments.
    /// </summary>
    public class CodeGenExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenExecutorArgs"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="assemblies">The <see cref="Assemblies"/>.</param>
        /// <param name="parameters">The <see cref="Parameters"/>.</param>
        public CodeGenExecutorArgs(ILogger logger, IEnumerable<Assembly>? assemblies = null, Dictionary<string, string>? parameters = null)
        {
            Logger = Check.NotNull(logger, nameof(Logger));

            if (assemblies != null)
                Assemblies.AddRange(assemblies);

            if (parameters != null)
                Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the configuration <see cref="FileInfo"/> (data source).
        /// </summary>
        public FileInfo? ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the script <see cref="FileInfo"/> (orchestrates template execution).
        /// </summary>
        public FileInfo? ScriptFile { get; set; }

        /// <summary>
        /// Gets or sets the template path <see cref="DirectoryInfo"/> (<c>null</c> indicates to use embedded resources).
        /// </summary>
        public DirectoryInfo? TemplatePath { get; set; }

        /// <summary>
        /// Gets or sets the output path <see cref="DirectoryInfo"/> (defaults to the executing assembly path).
        /// </summary>
        public DirectoryInfo? OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the assemblies that should be probed to find the embedded resources.
        /// </summary>
        public List<Assembly> Assemblies { get; private set; } = new List<Assembly>();

        /// <summary>
        /// Gets or sets dictionary of parameter name/value pairs.
        /// </summary>
        public Dictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Indicates whether the <i>code generator</i> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        public bool ExpectNoChange { get; internal set; }
    }

    /// <summary>
    /// The script arguments.
    /// </summary>
    public class CodeGenScriptArgs
    {
        /// <summary>
        /// Gets or sets the code-gen <see cref="Type"/>.
        /// </summary>
        public string? GenType { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        public string? OutDir { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// Gets the other parameters (not specifically defined).
        /// </summary>
        public Dictionary<string, string> OtherParameters { get; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Provides the configuration type selection.
    /// </summary>
    public enum ConfigType
    {
        /// <summary>
        /// Represents the <b>Entity</b> configuration.
        /// </summary>
        Entity,

        /// <summary>
        /// Represents the <b>Database</b> configuration.
        /// </summary>
        Database,
    }

    /// <summary>
    /// Represents the code generation executor.
    /// </summary>
    public class CodeGenExecutor
    {
        private readonly CodeGenExecutorArgs _args;
        private int CreatedCount;
        private int UpdatedCount;
        private int NotChangedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenExecutor"/> class.
        /// </summary>
        /// <param name="args">The <see cref="CodeGenExecutorArgs"/>.</param>
        public CodeGenExecutor(CodeGenExecutorArgs args) => _args = Check.NotNull(args, nameof(args));

        /// <summary>
        /// Executes the selected code generation.
        /// </summary>
        public async Task<bool> RunAsync()
        {
            var overallCreatedCount = 0;
            var overallUpdatedCount = 0;

            try
            {
                // Load the script configuration.
                var (scripts, configType) = await LoadScriptConfigAsync().ConfigureAwait(false);

                var cfg = await LoadConfigFileAsync(configType).ConfigureAwait(false);
                var rcfg = (IRootConfig)cfg;
                rcfg.ReplaceRuntimeParameters(_args.Parameters);
                cfg.Prepare(cfg, cfg);

                // Execute each of the script instructions.
                foreach (var script in scripts)
                {
                    NotChangedCount = 0;
                    UpdatedCount = 0;
                    CreatedCount = 0;
                    
                    // Log progress.
                    _args.Logger.LogInformation("  Template: {0} {1}", script.Template!, script.HelpText == null ? string.Empty : $"({script.HelpText})");

                    // Get the template contents.
                    var template = await GetTemplateContentsAsync(script).ConfigureAwait(false);

                    // Execute the new+improved handlebars-based code-gen.
                    rcfg!.ResetRuntimeParameters();
                    rcfg!.ReplaceRuntimeParameters(_args.Parameters);
                    rcfg!.ReplaceRuntimeParameters(script.OtherParameters);

                    var gt = Type.GetType(script.GenType!) ?? throw new CodeGenException($"GenType '{script.GenType}' was unable to be loaded.");
                    var cg = (CodeGeneratorBase)(Activator.CreateInstance(gt) ?? throw new CodeGenException($"GenType '{script.GenType}' was unable to be instantiated."));
                    cg.OutputFileName = script.FileName;
                    cg.OutputDirName = script.OutDir;
                    cg.Generate(template, cfg!, e => CodeGenerated(_args.OutputPath!.FullName, e));

                    // Provide statistics.
                    _args.Logger.LogInformation("   [Files: Unchanged = {0}, Updated = {1}, Created = {2}]", NotChangedCount, UpdatedCount, CreatedCount);

                    // Keep track of overall counts.
                    overallCreatedCount += CreatedCount;
                    overallUpdatedCount += UpdatedCount;
                }
            }
            catch (CodeGenException gcex)
            {
                _args.Logger.LogError(gcex.Message);
                if (gcex.InnerException != null)
                    _args.Logger.LogError(gcex.InnerException.Message);

                _args.Logger.LogInformation(string.Empty);
                return false;
            }

            if (_args.ExpectNoChange && (overallCreatedCount != 0 || overallUpdatedCount != 0))
            {
                _args.Logger.LogError("Unexpected changes detected; one or more files were created and/or updated.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the code-gen scripts configuration.
        /// </summary>
        private async Task<(List<CodeGenScriptArgs>, ConfigType)> LoadScriptConfigAsync()
        {
            var list = new List<CodeGenScriptArgs>();

            // Load the script XML content.
            XElement xmlScript;
            if (_args.ScriptFile!.Exists)
            {
                using var fs = File.OpenText(_args.ScriptFile.FullName);
                xmlScript = await XElement.LoadAsync(fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                var c = await ResourceManager.GetScriptContentAsync(_args.ScriptFile.Name, _args.Assemblies.ToArray()).ConfigureAwait(false);
                if (c == null)
                    throw new CodeGenException("The Script XML does not exist.");

                xmlScript = XElement.Parse(c);
            }

            if (xmlScript?.Name != "Script")
                throw new CodeGenException("The Script XML file must have a root element named 'Script'.");

            if (!xmlScript.Elements("Generate").Any())
                throw new CodeGenException("The Script XML file must have at least a single 'Generate' element.");

            foreach (var scriptEle in xmlScript.Elements("Generate"))
            {
                var args = new CodeGenScriptArgs();
                foreach (var att in scriptEle.Attributes())
                {
                    switch (att.Name.LocalName)
                    {
                        case "GenType": args.GenType = att.Value; break;
                        case "FileName": args.FileName = att.Value; break;
                        case "Template": args.Template = att.Value; break;
                        case "OutDir": args.OutDir = att.Value; break;
                        case "HelpText": args.HelpText = att.Value; break;
                        default: args.OtherParameters.Add(att.Name.LocalName, att.Value); break;
                    }
                }

                list.Add(args);
            }

            var ct = xmlScript.Attribute("ConfigType")?.Value ?? throw new CodeGenException("The Script XML file must have an attribute named 'ConfigType' within the root 'Script' element.");
            if (Enum.TryParse<ConfigType>(ct, true, out var configType))
                return (list, configType);

            throw new CodeGenException($"The Script XML file attribute named 'ConfigType' has an invalid value '{ct}'.");
        }

        /// <summary>
        /// Loads the configuration file.
        /// </summary>
        private async Task<ConfigBase> LoadConfigFileAsync(ConfigType configType)
        {
            try
            {
                switch (_args.ConfigFile!.Extension.ToUpperInvariant())
                {
                    case ".XML":
                        using (var xfs = _args.ConfigFile!.OpenRead())
                        {
                            var xml = await XDocument.LoadAsync(xfs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                            var yaml = configType == ConfigType.Entity ? new EntityXmlToYamlConverter().ConvertEntityXmlToYaml(xml) : new DatabaseXmlToYamlConverter().ConvertEntityXmlToYaml(xml);

                            using var sr = new StringReader(yaml);
                            var yml = new DeserializerBuilder().Build().Deserialize(sr);
                            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yml!);

                            using var jsr = new StringReader(json);
                            using var jr = new JsonTextReader(jsr);
                            var js = JsonSerializer.Create();
                            return CheckNotNull(js.Deserialize(jr, configType == ConfigType.Entity ? typeof(Config.Entity.CodeGenConfig) : typeof(Config.Database.CodeGenConfig)));
                        }

                    case ".YAML": throw new NotImplementedException();
                    case ".JSON": throw new NotImplementedException();
                    default: throw new CodeGenException($"The Config file '{_args.ConfigFile!.FullName}' is not a supported file type.");
                }
            }
            catch (CodeGenException) { throw; }
            catch (Exception ex) { throw new CodeGenException($"The contents of the Config File {_args.ConfigFile!.Name} are not valid.", ex); }
        }

        /// <summary>
        /// Checks not null and converts type.
        /// </summary>
        private ConfigBase CheckNotNull(object? val) => (ConfigBase)(val ?? throw new CodeGenException($"The contents of the Config File {_args.ConfigFile!.Name} are not valid."));

        /// <summary>
        /// Gets the template contents.
        /// </summary>
        private async Task<string> GetTemplateContentsAsync(CodeGenScriptArgs script)
        {
            if (_args.TemplatePath == null)
                return await ResourceManager.GetTemplateContentAsync(script.Template!, _args.Assemblies.ToArray()).ConfigureAwait(false) ??
                    throw new CodeGenException($"The Template embedded resource '{script.Template}' does not exist.");

            var fi = new FileInfo(Path.Combine(_args.TemplatePath.FullName, script.Template!));
            if (!fi.Exists)
                throw new CodeGenException($"The Template file '{fi.FullName}' does not exist.");

            return await File.ReadAllTextAsync(fi.FullName, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle the generation of an output file.
        /// </summary>
        private void CodeGenerated(string outputDir, CodeGeneratorEventArgs e)
        {
            string dir = outputDir;
            if (!string.IsNullOrEmpty(e.OutputDirName))
                dir = Path.Combine(dir, e.OutputDirName);

            if (!string.IsNullOrEmpty(e.OutputGenDirName))
                dir = Path.Combine(dir, e.OutputGenDirName);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fi = new FileInfo(Path.Combine(dir, e.OutputFileName!));
            if (fi.Exists)
            {
                if (e.IsOutputNewOnly)
                    return; 

                var prevContent = File.ReadAllText(fi.FullName);
                if (string.Compare(e.Content, prevContent, StringComparison.InvariantCulture) == 0)
                {
                    NotChangedCount++;
                    return;
                }

                UpdatedCount++;
                File.WriteAllText(fi.FullName, e.Content);
                _args.Logger.LogWarning("    Updated -> {0}", fi.FullName.Substring(outputDir.Length));
            }
            else
            {
                CreatedCount++;
                File.WriteAllText(fi.FullName, e.Content);
                _args.Logger.LogWarning("    Created -> {0}", fi.FullName.Substring(outputDir.Length));
            }
        }
    }
}