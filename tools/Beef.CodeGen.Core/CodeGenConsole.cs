// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using OnRamp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>Beef</b>-specific code-generation console that inherits from <see cref="OnRamp.Console.CodeGenConsole"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class CodeGenConsole : OnRamp.Console.CodeGenConsole
    {
        private string _entityScript = "EntityWebApiCoreAgent.yaml";
        private string _refDataScript = "RefDataCoreCrud.yaml";
        private string _dataModelScript = "DataModelOnly.yaml";
        private string _databaseScript = "Database.yaml";

        private CommandArgument<CommandType>? _cmdArg;

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
        /// Gets the 'DatabaseMigrator' <see cref="CodeGeneratorArgsBase.Parameters"/> name.
        /// </summary>
        public const string DatabaseMigratorParamName = "DatabaseMigrator";

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
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsole.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(string company, string appName, string apiName = "Api", string? outputDirectory = null) => Create(new Assembly[] { Assembly.GetCallingAssembly() }, company, appName, apiName, outputDirectory);

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="assemblies">The list of additional assemblies to probe for resources.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsole.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(Assembly[] assemblies, string company, string appName, string apiName = "Api", string? outputDirectory = null)
        {
            var args = new CodeGeneratorArgs { OutputDirectory = string.IsNullOrEmpty(outputDirectory) ? new DirectoryInfo(GetBaseExeDirectory()).Parent : new DirectoryInfo(outputDirectory) };
            args.AddAssembly(typeof(CodeGenConsole).Assembly);
            args.AddAssembly(assemblies);
            args.AddParameter(CompanyParamName, company ?? throw new ArgumentNullException(nameof(company)));
            args.AddParameter(AppNameParamName, appName ?? throw new ArgumentNullException(nameof(appName)));
            args.AddParameter(ApiNameParamName, apiName ?? throw new ArgumentNullException(nameof(apiName)));
            return new CodeGenConsole(args) { BypassOnWrites = true };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        internal CodeGenConsole(CodeGeneratorArgs args) : base(args, OnRamp.Console.SupportedOptions.All)
        {
            MastheadText = DefaultMastheadText;
            Args.CreateConnectionStringEnvironmentVariableName ??= csargs => $"{args.GetCompany()?.Replace(".", "_", StringComparison.InvariantCulture)}_{args.GetAppName()?.Replace(".", "_", StringComparison.InvariantCulture)}_ConnectionString";
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
            _entityScript = script ?? throw new ArgumentNullException(nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.DataModel"/> (defaults to <c>DataModelOnly.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole DataModelScript(string script)
        {
            _dataModelScript = script ?? throw new ArgumentNullException(nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.RefData"/> (defaults to <c>RefDataCoreCrud.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole RefDataScript(string script)
        {
            _refDataScript = script ?? throw new ArgumentNullException(nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.Database"/> (defaults to <c>Database.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsole DatabaseScript(string script)
        {
            _databaseScript = script ?? throw new ArgumentNullException(nameof(script));
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
            Args.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBeforeExecute(CommandLineApplication app)
        {
            _cmdArg = app.Argument<CommandType>("command", "Execution command type.", false).IsRequired();
        }

        /// <inheritdoc/>
        protected override ValidationResult? OnValidation(ValidationContext context)
        {
            var cmd = _cmdArg!.ParsedValue;
            if (cmd == CommandType.All)
            {
                if (!string.IsNullOrEmpty(Args.ScriptFileName))
                    return new ValidationResult("Command 'All' is not compatible with --script; the command must be more specific when using a specified configuration file.");

                if (!string.IsNullOrEmpty(Args.ConfigFileName))
                    return new ValidationResult("Command 'All' is not compatible with --config; the command must be more specific when using a specified configuration file.");
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
        protected override async Task<CodeGenStatistics> OnCodeGenerationAsync()
        {
            OnWriteMasthead();
            OnWriteHeader();
            
            var cmd = _cmdArg!.ParsedValue;
            var exedir = GetBaseExeDirectory();

            var company = Args.GetCompany(false);
            var appName = Args.GetAppName(false);
            if (company == null || appName == null)
                throw new CodeGenException($"Parameters '{CompanyParamName}' and {AppNameParamName}  must be specified.");

            var count = 0;
            var stats = new CodeGenStatistics();
            if (IsDatabaseSupported && cmd.HasFlag(CommandType.Database))
                stats.Add(await ExecuteCodeGenerationAsync(_databaseScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.Database, company, appName), count++).ConfigureAwait(false));

            if (IsRefDataSupported && cmd.HasFlag(CommandType.RefData))
                stats.Add(await ExecuteCodeGenerationAsync(_refDataScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.RefData, company, appName), count++).ConfigureAwait(false));

            if (IsEntitySupported && cmd.HasFlag(CommandType.Entity))
                stats.Add(await ExecuteCodeGenerationAsync(_entityScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.Entity, company, appName), count++).ConfigureAwait(false));

            if (IsDataModelSupported && cmd.HasFlag(CommandType.DataModel))
                stats.Add(await ExecuteCodeGenerationAsync(_dataModelScript, CodeGenFileManager.GetConfigFilename(exedir, CommandType.DataModel, company, appName), count++).ConfigureAwait(false));

            if (cmd.HasFlag(CommandType.Clean))
                ExecuteCleanAsync();

            if (count > 1)
            {
                Args.Logger?.LogInformation("{Content}", new string('-', 80));
                Args.Logger?.LogInformation("{Content}", "");
                Args.Logger?.LogInformation("{Content}", $"{AppName} OVERALL. {stats.ToSummaryString()}");
                Args.Logger?.LogInformation("{Content}", "");
            }

            return stats;
        }

        /// <summary>
        /// Execute the selection code-generation.
        /// </summary>
        private async Task<CodeGenStatistics> ExecuteCodeGenerationAsync(string scriptName, string configName, int count)
        {
            // Update the files.
            var args = new CodeGeneratorArgs();
            args.CopyFrom(Args);
            args.ScriptFileName ??= scriptName;
            args.ConfigFileName ??= configName;

            if (count > 0)
            {
                args.Logger?.LogInformation("{Content}", new string('-', 80));
                args.Logger?.LogInformation("{Content}", "");
            }

            OnWriteArgs(args);

            // Execute the code-generation.
            var stats = await ExecuteCodeGenerationAsync(args).ConfigureAwait(false);

            // Write results.
            OnWriteFooter(stats);
            return stats;
        }

        /// <summary>
        /// Executes the code generation.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        /// <returns>The <see cref="CodeGenStatistics"/>.</returns>
        public static async Task<CodeGenStatistics> ExecuteCodeGenerationAsync(CodeGeneratorArgsBase args)
        {
            var cg = await OnRamp.CodeGenerator.CreateAsync<CodeGenerator>(args).ConfigureAwait(false);
            var fi = new FileInfo(args.ConfigFileName ?? throw new CodeGenException("Configuration file not specified."));
            return await cg.GenerateAsync(fi.FullName).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the clean.
        /// </summary>
        private void ExecuteCleanAsync()
        {
            if (Args.OutputDirectory == null)
                return;

            Args.Logger?.LogInformation("{Content}", $"Cleaning: {Args.OutputDirectory.FullName}");
            Args.Logger?.LogInformation("{Content}", string.Empty);
            Args.Logger?.LogInformation("{Content}", "The following 'Generated' directories were cleaned/deleted:");
            int fileCount = 0;
            bool dirDeleted = false;
            var sw = Stopwatch.StartNew();

            var list = Args.OutputDirectory.EnumerateDirectories("Generated", SearchOption.AllDirectories)
                .Where(x => !x.FullName.Contains(Path.Combine("obj", "debug"), StringComparison.OrdinalIgnoreCase) && !x.FullName.Contains(Path.Combine("obj", "release"), StringComparison.OrdinalIgnoreCase) 
                   && !x.FullName.Contains(Path.Combine("bin", "debug"), StringComparison.OrdinalIgnoreCase) && !x.FullName.Contains(Path.Combine("bin", "release"), StringComparison.OrdinalIgnoreCase));

            if (list != null)
            {
                int count;
                foreach (var di in list.Where(x => x.Exists))
                {
                    dirDeleted = true;
                    fileCount += count = di.GetFiles().Length;
                    Args.Logger?.LogWarning("  {Directory} [{FileCount} files]", di.FullName, count);
                    di.Delete(true);
                }
            }

            sw.Stop();
            if (!dirDeleted)
                Args.Logger?.LogInformation("{Content}", "  ** No directories found.");

            Args.Logger?.LogInformation("{Content}", string.Empty);
            Args.Logger?.LogInformation("{Content}", $"{AppName} Complete. [{sw.Elapsed.TotalMilliseconds}ms, Files: {fileCount}]");
            Args.Logger?.LogInformation("{Content}", string.Empty);
        }
    }
}