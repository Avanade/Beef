// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Converters;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using OnRamp;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>Beef</b>-specific code-generation console that inherits from <see cref="OnRamp.Console.CodeGenConsoleBase"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class CodeGenConsole : OnRamp.Console.CodeGenConsoleBase
    {
        private string _entityScript = "EntityWebApiCoreAgent.yaml";
        private string _refDataScript = "RefDataCoreCrud.yaml";
        private string _dataModelScript = "DataModelOnly.yaml";
        private string _databaseScript = "Database.yaml";

        private CommandArgument<CommandType>? _cmdArg;
        private CommandOption? _x2yOpt;

        /// <summary>
        /// Gets the 'Company' <see cref="CodeGeneratorArgsBase.Parameters"/> name.
        /// </summary>
        public const string CompanyParamName = "Company";

        /// <summary>
        /// Gets the 'AppName' <see cref="CodeGeneratorArgsBase.Parameters"/> name.
        /// </summary>
        public const string AppNameParamName = "AppName";

        /// <summary>
        /// Gets the 'ApiName' <see cref="CodeGeneratorArgsBase.Parameters"/> name.
        /// </summary>
        public const string ApiNameParamName = "ApiName";

        /// <summary>
        /// Gets the default masthead text.
        /// </summary>
        /// <remarks>Defaults to 'Beef Code-Gen Tool' formatted using <see href="http://www.patorjk.com/software/taag/#p=display&amp;f=Calvin%20S&amp;t=Beef%20Code-Gen%20Tool%0A"/>.</remarks>
        public const string DefaultMastheadText = @"
╔╗ ┌─┐┌─┐┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
╠╩╗├┤ ├┤ ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││  
╚═╝└─┘└─┘└    ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘
";

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class defaulting to <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(string company, string appName, string apiName = "Api", string? outputDirectory = null) => Create(new Assembly[] { Assembly.GetCallingAssembly() }, company, appName, apiName, outputDirectory);

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="assemblies">The list of additional assemblies to probe for resources.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(Assembly[] assemblies, string company, string appName, string apiName = "Api", string? outputDirectory = null)
        {
            var args = new CodeGeneratorArgs { OutputDirectory = string.IsNullOrEmpty(outputDirectory) ? new DirectoryInfo(GetBaseExeDirectory()).Parent : new DirectoryInfo(outputDirectory) };
            args.AddAssembly(typeof(CodeGenConsole).Assembly);
            args.AddAssembly(assemblies);
            args.AddParameter(CompanyParamName, Check.NotEmpty(company, nameof(company)));
            args.AddParameter(AppNameParamName, Check.NotEmpty(appName, nameof(appName)));
            args.AddParameter(ApiNameParamName, Check.NotEmpty(apiName, nameof(apiName)));
            return new CodeGenConsole(args) { BypassOnWrites = true };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        internal CodeGenConsole(CodeGeneratorArgs args) : base(typeof(CodeGenConsole).Assembly, args, Assembly.GetEntryAssembly()!.GetName().Name, options: OnRamp.Console.SupportedOptions.All)
        {
            MastheadText = DefaultMastheadText;
            Args.CreateConnectionStringEnvironmentVariableName ??= csargs => $"{csargs.GetCompany()?.Replace(".", "_", StringComparison.InvariantCulture)}_{csargs.GetAppName()?.Replace(".", "_", StringComparison.InvariantCulture)}_ConnectionString";
        }

        /// <summary>
        /// Indicates whether <see cref="CommandType.Entity"/> is supported (defaults to <c>true</c>).
        /// </summary>
        public bool IsEntitySupported { get; set; } = true;

        /// <summary>
        /// Indicates whether <see cref="CommandType.Database"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsDatabaseSupported { get; set; } = false;

        /// <summary>
        /// Indicates whether <see cref="CommandType.RefData"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsRefDataSupported { get; set; } = false;

        /// <summary>
        /// Indicates whether <see cref="CommandType.DataModel"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsDataModelSupported { get; set; } = false;

        /// <summary>
        /// Sets the <see cref="IsEntitySupported"/>, <see cref="IsDatabaseSupported"/> and <see cref="IsRefDataSupported"/> options.
        /// </summary>
        /// <param name="entity">Indicates whether the entity code generation should take place.</param>
        /// <param name="database">Indicates whether the database generation should take place.</param>
        /// <param name="refData">Indicates whether the reference data generation should take place.</param>
        /// <param name="dataModel">Indicates whether the data model generation should take place.</param>
        /// <returns>The <see cref="CodeGenConsole"/> to support method chaining/fluent style.</returns>
        public CodeGenConsole Supports(bool entity = true, bool database = false, bool refData = false, bool dataModel = false)
        {
            IsEntitySupported = entity;
            IsDatabaseSupported = database;
            IsRefDataSupported = refData;
            IsDataModelSupported = dataModel;
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.Database"/> (defaults to <c>EntityWebApiCoreAgent.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole EntityScript(string script)
        {
            _entityScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.DataModel"/> (defaults to <c>DataModelOnly.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole DataModelScript(string script)
        {
            _dataModelScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.RefData"/> (defaults to <c>RefDataCoreCrud.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole RefDataScript(string script)
        {
            _refDataScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.Database"/> (defaults to <c>Database.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole DatabaseScript(string script)
        {
            _databaseScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the default database connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        /// <remarks>Acts as the default; the command line option '<c>-cs|--connectionString</c>' and environment variable take precedence.</remarks>
        public CodeGenConsole DatabaseConnectionString(string connectionString)
        {
            Args.ConnectionString = Check.NotEmpty(connectionString, nameof(connectionString));
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBeforeExecute(CommandLineApplication app)
        {
            _cmdArg = app.Argument<CommandType>("command", "Execution command type.", false).IsRequired();
            _x2yOpt = app.Option("-x2y|--xml-to-yaml", "Convert the XML configuration into YAML equivalent (will not codegen).", CommandOptionType.NoValue);
        }

        /// <inheritdoc/>
        protected override ValidationResult? OnValidation(ValidationContext context)
        {
            Diagnostics.Logger.Default ??= Args.Logger;

            var cmd = _cmdArg!.ParsedValue;
            if (cmd == CommandType.All)
            {
                if (!string.IsNullOrEmpty(Args.ScriptFileName))
                    return new ValidationResult("Command 'All' is not compatible with --script; the command must be more specific when using a specified configuration file.");

                if (!string.IsNullOrEmpty(Args.ConfigFileName))
                    return new ValidationResult("Command 'All' is not compatible with --config; the command must be more specific when using a specified configuration file.");

                if (_x2yOpt!.HasValue())
                    return new ValidationResult("Command 'All' is not compatible with --xml-to-yaml; the command must be more specific when converting XML configuration to YAML.");
            }
            else
            {
                var vr = CheckCommandIsSupported(cmd, CommandType.Entity, IsEntitySupported);
                vr ??= CheckCommandIsSupported(cmd, CommandType.RefData, IsRefDataSupported);
                vr ??= CheckCommandIsSupported(cmd, CommandType.DataModel, IsDataModelSupported);
                vr ??= CheckCommandIsSupported(cmd, CommandType.Database, IsDatabaseSupported);
                if (vr != null)
                    return vr;
            }

            Args.ValidateCompanyAndAppName();

            return ValidationResult.Success;
        }

        /// <summary>
        /// Check command is supported.
        /// </summary>
        private static ValidationResult? CheckCommandIsSupported(CommandType act, CommandType exp, bool isSupported) => act == exp && !isSupported ? new ValidationResult($"Command '{act}' is not supported.") : null;

        /// <inheritdoc/>
        protected override CodeGenStatistics? OnCodeGeneration()
        {
            OnWriteMasthead();
            OnWriteHeader();
            
            var cmd = _cmdArg!.ParsedValue;
            var exedir = GetBaseExeDirectory();

            var company = Args.GetCompany(false);
            var appName = Args.GetAppName(false);
            if (company == null || appName == null)
                throw new CodeGenException($"Parameters '{CompanyParamName}' and {AppNameParamName}  must be specified.");

            // Where XML to YAML requested do so, then exit.
            if (_x2yOpt!.HasValue())
                return CodeGenFileManager.ConvertXmlToYamlAsync(cmd, Args.ConfigFileName ?? CodeGenFileManager.GetConfigFilename(exedir, cmd, company, appName)).GetAwaiter().GetResult() ? new CodeGenStatistics() : null;

            var count = 0;
            var stats = new CodeGenStatistics();
            if (IsDatabaseSupported && cmd.HasFlag(CommandType.Database))
                stats.Add(ExecuteCodeGeneration(_databaseScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.Database, company, appName), ref count));

            if (IsRefDataSupported && cmd.HasFlag(CommandType.RefData))
                stats.Add(ExecuteCodeGeneration(_refDataScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.RefData, company, appName), ref count));

            if (IsEntitySupported && cmd.HasFlag(CommandType.Entity))
                stats.Add(ExecuteCodeGeneration(_entityScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.Entity, company, appName), ref count));

            if (IsDataModelSupported && cmd.HasFlag(CommandType.DataModel))
                stats.Add(ExecuteCodeGeneration(_dataModelScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.DataModel, company, appName), ref count));

            if (count > 1)
            {
                Args.Logger?.LogInformation(new string('-', 80));
                Args.Logger?.LogInformation("");
                Args.Logger?.LogInformation($"OVERALL. {stats.ToSummaryString()}");
                Args.Logger?.LogInformation("");
            }

            return stats;
        }

        /// <summary>
        /// Execute the selection code-generation.
        /// </summary>
        private CodeGenStatistics ExecuteCodeGeneration(string scriptName, string configName, ref int count)
        {
            // Update the files.
            var args = new CodeGeneratorArgs();
            args.CopyFrom(Args);
            args.ScriptFileName ??= scriptName;
            args.ConfigFileName ??= configName;

            if (count++ > 0)
            {
                args.Logger?.LogInformation(new string('-', 80));
                args.Logger?.LogInformation("");
            }

            OnWriteArgs(args);

            // Execute the code-generation.
            var stats = ExecuteCodeGeneration(args);

            // Write results.
            OnWriteFooter(stats);
            return stats;
        }

        /// <summary>
        /// Executes the code generation.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        /// <returns>The <see cref="CodeGenStatistics"/>.</returns>
        public static CodeGenStatistics ExecuteCodeGeneration(CodeGeneratorArgsBase args)
        {
            CodeGenStatistics stats;
            var cg = new CodeGenerator(args);
            var fi = new FileInfo(args.ConfigFileName);
            switch (fi.Extension.ToUpperInvariant())
            {
                // XML not natively supported so must be converted to YAML.
                case ".XML":
                    using (var xfs = fi.OpenText())
                    {
                        var xml = XDocument.Load(xfs, LoadOptions.None);
                        if (cg.Scripts.GetConfigType() == typeof(Config.Entity.CodeGenConfig))
                        {
                            var sr = new StringReader(new EntityXmlToYamlConverter().ConvertXmlToYaml(xml).Yaml);
                            stats = cg.Generate(sr, OnRamp.Utility.StreamContentType.Yaml, fi.FullName);
                        }
                        else if (cg.Scripts.GetConfigType() == typeof(Config.Database.CodeGenConfig))
                        {
                            var sr = new StringReader(new DatabaseXmlToYamlConverter().ConvertXmlToYaml(xml).Yaml);
                            stats = cg.Generate(sr, OnRamp.Utility.StreamContentType.Yaml, fi.FullName);
                        }
                        else
                            throw new CodeGenException($"Configuration Type '{cg.Scripts.GetConfigType().FullName}' is not expected; must be either '{typeof(Config.Entity.CodeGenConfig).FullName}' or '{typeof(Config.Database.CodeGenConfig).FullName}'.");
                    }

                    break;

                default:
                    stats = cg.Generate(fi.FullName);
                    break;
            }

            return stats;
        }
    }
}