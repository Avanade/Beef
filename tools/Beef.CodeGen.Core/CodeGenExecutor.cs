// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Executors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// The <see cref="CodeGenExecutor"/> arguments.
    /// </summary>
    public class CodeGenExecutorArgs
    {
        /// <summary>
        /// Gets or sets the configuration <see cref="FileInfo"/> (data source).
        /// </summary>
        public FileInfo ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the script <see cref="FileInfo"/> (orchestrates template execution).
        /// </summary>
        public FileInfo ScriptFile { get; set; }

        /// <summary>
        /// Gets or sets the template path <see cref="DirectoryInfo"/> (<c>null</c> indicates to use embedded resources).
        /// </summary>
        public DirectoryInfo TemplatePath { get; set; }

        /// <summary>
        /// Gets or sets the output path <see cref="DirectoryInfo"/> (defaults to the executing assembly path).
        /// </summary>
        public DirectoryInfo OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the assemblies that should be probed to find the embedded resources.
        /// </summary>
        public List<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Gets or sets dictionary of parameter name/value pairs.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }
    }

    /// <summary>
    /// Represents the code generation executor.
    /// </summary>
    public class CodeGenExecutor : ExecutorBase
    {
        private CodeGenExecutorArgs _args;
        private int CreatedCount;
        private int UpdatedCount;
        private int NotChangedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenExecutor"/> class.
        /// </summary>
        /// <param name="args">The <see cref="CodeGenExecutorArgs"/>.</param>
        public CodeGenExecutor(CodeGenExecutorArgs args)
        {
            _args = Check.NotNull(args, nameof(args));
        }

        /// <summary>
        /// Executes the selected code generation.
        /// </summary>
        protected override Task OnRunAsync(ExecutorRunArgs args)
        {
            try
            {
                // Load the script file instructions.
                XElement xmlScript = (_args.ScriptFile.Exists) ? XElement.Load(_args.ScriptFile.FullName) : ResourceManager.GetScriptContentXml(_args.ScriptFile.Name, _args.Assemblies.ToArray());
                if (xmlScript.Name != "Script")
                    throw new CodeGenException("The Script XML file must have a root element named 'Script'.");

                if (xmlScript.Elements("Generate").Count() == 0)
                    throw new CodeGenException("The Script XML file must have at least a single 'Generate' element.");

                // Create the code generator instance.
                string outputDir = null;

                var gen = CodeGenerator.Create(XElement.Load(_args.ConfigFile.FullName), GetLoaders(xmlScript));
                gen.CodeGenerated += (sender, e) => { CodeGenerated(outputDir, e); };

                // Execute each of the script instructions.
                foreach (var scriptEle in xmlScript.Elements("Generate"))
                {
                    // As this can be long running, check and see if a stop has been initiated.
                    if (this.State != ExecutionState.Running)
                        return Task.CompletedTask;

                    string template = null;
                    string outDir = null;
                    var otherParameters = new Dictionary<string, string>();
                    foreach (var att in scriptEle.Attributes())
                    {
                        switch (att.Name.LocalName)
                        {
                            case "Template": template = att.Value; break;
                            case "OutDir": outDir = att.Value; break;
                            default: otherParameters.Add(att.Name.LocalName, att.Value); break;
                        }
                    }

                    // Manage the parameters.
                    gen.ClearParameters();
                    gen.CopyParameters(_args.Parameters);
                    gen.CopyParameters(otherParameters);

                    // Log progress.
                    CreatedCount = 0;
                    UpdatedCount = 0;
                    NotChangedCount = 0;
                    Logger.Default.Info("  Template: {0}", template);

                    XElement xmlTemplate;
                    if (_args.TemplatePath != null)
                    {
                        var fi = new FileInfo(Path.Combine(_args.TemplatePath.FullName, template));
                        if (!fi.Exists)
                            throw new CodeGenException(string.Format("The Template XML file '{0}' does not exist.", fi.FullName));

                        xmlTemplate = XElement.Load(fi.FullName);
                    }
                    else
                    {
                        xmlTemplate = ResourceManager.GetTemplateContentXml(template, _args.Assemblies.ToArray()) ??
                            throw new CodeGenException(string.Format("The Template XML resource '{0}' does not exist.", template));
                    }

                    // Execute the code generation itself.
                    outputDir = Path.Combine(_args.OutputPath.FullName, SubstituteOutputDir(outDir));
                    gen.Generate(xmlTemplate);

                    // Provide statistics.
                    Logger.Default.Info("   [Files: Unchanged = {0}, Updated = {1}, Created = {2}]", NotChangedCount, UpdatedCount, CreatedCount);
                }
            }
            catch (CodeGenException gcex)
            {
                Logger.Default.Error(gcex.Message);
                Logger.Default.Info(string.Empty);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle the output directory substitution.
        /// </summary>
        private string SubstituteOutputDir(string outputDir)
        {
            var dir = outputDir;
            foreach (var p in _args.Parameters)
            {
                dir = dir.Replace("{{" + p.Key + "}}", p.Value);
            }

            if (dir.Contains("{{") || dir.Contains("}}"))
                throw new CodeGenException(string.Format("Unhandled substitution characters have been found in the Script OutDir {0}.", dir));

            return dir;
        }

        /// <summary>
        /// Gets the configured set of loaders.
        /// </summary>
        private List<ICodeGenConfigLoader> GetLoaders(XElement xmlScript)
        {
            var loaders = new List<ICodeGenConfigLoader>();

            var lt = xmlScript.Attribute("LoaderType")?.Value;
            if (string.IsNullOrEmpty(lt))
                return loaders;

            var type = Type.GetType(lt, false, true);
            if (type == null)
                throw new CodeGenException($"The Script XML 'LoaderType' does not exist: {lt}.");

            if (Activator.CreateInstance(type) is ICodeGenConfigGetLoaders cgl)
            {
                loaders.AddRange(cgl.GetLoaders());
                return loaders;
            }

            throw new CodeGenException($"The Script XML 'LoaderType' does not implement 'ICodeGenConfigGetLoaders': {lt}.");
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

            var fi = new FileInfo(Path.Combine(dir, e.OutputFileName));
            if (fi.Exists)
            {
                if (e.IsOutputNewOnly)
                    return; 

                var prevContent = File.ReadAllText(fi.FullName);
                if (string.Compare(e.Content, prevContent) == 0)
                {
                    NotChangedCount++;
                    return;
                }

                UpdatedCount++;
                File.WriteAllText(fi.FullName, e.Content);
                Logger.Default.Warning("    Updated -> {0}", fi.FullName.Substring(outputDir.Length));
            }
            else
            {
                CreatedCount++;
                File.WriteAllText(fi.FullName, e.Content);
                Logger.Default.Warning("    Created -> {0}", fi.FullName.Substring(outputDir.Length));
            }
        }
    }
}
