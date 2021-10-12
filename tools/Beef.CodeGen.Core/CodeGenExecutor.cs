// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Converters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
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
        /// Initializes a new instance of the <see cref="CodeGenExecutorArgs"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="assemblies">The <see cref="Assemblies"/>.</param>
        /// <param name="parameters">The <see cref="Parameters"/>.</param>
        public CodeGenExecutorArgs(ILogger logger, IEnumerable<Assembly>? assemblies = null, Dictionary<string, string?>? parameters = null)
        {
            Logger = Check.NotNull(logger, nameof(Logger));

            if (assemblies != null)
                Assemblies.AddRange(assemblies);

            Assemblies.Add(typeof(CodeGenExecutorArgs).Assembly);

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
        /// Gets or sets the script file/resource name (orchestrates template execution).
        /// </summary>
        public string? ScriptFile { get; set; }

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
        public Dictionary<string, string?> Parameters { get; private set; } = new Dictionary<string, string?>();

        /// <summary>
        /// Indicates whether the <i>code generator</i> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        public bool ExpectNoChange { get; internal set; }

        /// <summary>
        /// Indicates whether the <i>code generator</i> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        public bool IsSimulation { get; internal set; }
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
        Database
    }

    /// <summary>
    /// Represents the code generation executor.
    /// </summary>
    public class CodeGenExecutor
    {
        private readonly CodeGenExecutorArgs _args;

        /// <summary>
        /// Gets the <see cref="CodeGenStatistics"/>.
        /// </summary>
        public CodeGenStatistics Statistics { get; } = new CodeGenStatistics();

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
            try
            {
                // Create the code-generator with the specified script.
                var cg = new CodeGenerator(new CodeGeneratorArgs(_args.ScriptFile!) { Assemblies = _args.Assemblies.ToArray(), DirectoryBase = _args.OutputPath, Parameters = _args.Parameters, Logger = _args.Logger, IsSimulation = _args.IsSimulation });
                CodeGenStatistics stats;

                // Perform the code-generation; with legacy XML support.
                switch (_args.ConfigFile!.Extension.ToUpperInvariant())
                {
                    // XML not natively supported so must be converted to YAML.
                    case ".XML":
                        using (var xfs = _args.ConfigFile!.OpenRead())
                        {
                            var xml = await XDocument.LoadAsync(xfs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                            if (cg.Scripts.GetConfigType() == typeof(Config.Entity.CodeGenConfig))
                            {
                                var sr = new StringReader(new EntityXmlToYamlConverter().ConvertXmlToYaml(xml).Yaml);
                                stats = cg.Generate(sr, Utility.StreamContentType.Yaml, _args.ConfigFile.FullName);
                            }
                            else if (cg.Scripts.GetConfigType() == typeof(Config.Database.CodeGenConfig))
                            {
                                var sr = new StringReader(new DatabaseXmlToYamlConverter().ConvertXmlToYaml(xml).Yaml);
                                stats = cg.Generate(sr, Utility.StreamContentType.Yaml, _args.ConfigFile.FullName);
                            }
                            else
                                throw new CodeGenException($"Configuration Type '{cg.Scripts.GetConfigType().FullName}' is not expected; must be either '{typeof(Config.Entity.CodeGenConfig).FullName}' or '{typeof(Config.Database.CodeGenConfig).FullName}'.");
                        }

                        break;

                    default:
                        stats = cg.Generate(_args.ConfigFile!.FullName);
                        break;
                }

                // Update the global statistics.
                Statistics.Add(stats);
            }
            catch (CodeGenException gcex)
            {
                _args.Logger.LogError(gcex.Message);
                if (gcex.InnerException != null)
                    _args.Logger.LogError(gcex.InnerException.Message);

                _args.Logger.LogInformation(string.Empty);
                return false;
            }

            if (_args.ExpectNoChange && (Statistics.CreatedCount != 0 || Statistics.UpdatedCount != 0))
            {
                _args.Logger.LogError("Unexpected changes detected; one or more files were created and/or updated.");
                return false;
            }

            return true;
        }
    }
}