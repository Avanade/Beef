// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Database.Core
{
    /// <summary>
    /// <b>Beef</b>-specific database console that encapsulates the database tooling functionality as specified by the <see cref="DatabaseExecutorCommand"/> flags.
    /// </summary>
    public sealed class DatabaseConsole
    {
        private const string EntryAssemblyOnlyOptionName = "EO";
        private const string XmlToYamlOptionName = "X2Y";
        private readonly Dictionary<string, CommandOption> _options = new Dictionary<string, CommandOption>();
        private CommandArgument<DatabaseExecutorCommand>? _cmdArg;
        private CommandArgument? _scriptNewArg;

        /// <summary>
        /// Gets the default database script name.
        /// </summary>
        public const string DefaultDatabaseScript = "Database.yaml";

        /// <summary>
        /// Gets the default masthead text.
        /// </summary>
        /// <remarks>Defaults to 'Beef Database Tool' formatted using <see href="http://www.patorjk.com/software/taag/#p=display&amp;f=Calvin%20S&amp;t=Beef%20Database%20Tool%0A"/>.</remarks>
        public const string DefaultMastheadText = @"
╔╗ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬  
╠╩╗├┤ ├┤ ├┤    ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││  
╚═╝└─┘└─┘└    ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘
";

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsole"/> class using the specified parameters.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard <i>Beef</i> <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabaseConsole"/> instance.</returns>
        public static DatabaseConsole Create(string connectionString, string company, string appName, bool useBeefDbo = true)
            => Create(new DatabaseConsoleArgs { ConnectionString = Check.NotEmpty(connectionString, nameof(connectionString)), UseBeefDbo = useBeefDbo }
                .AddParameter(CodeGen.CodeGenConsole.CompanyParamName, Check.NotEmpty(company, nameof(company)))
                .AddParameter(CodeGen.CodeGenConsole.AppNameParamName, Check.NotEmpty(appName, nameof(appName)))
                .AddAssembly(Assembly.GetEntryAssembly()!));

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsole"/> class using the <see cref="DatabaseConsoleArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseConsoleArgs"/>.</param>
        /// <returns>The <see cref="DatabaseConsole"/> instance.</returns>
        public static DatabaseConsole Create(DatabaseConsoleArgs? args = null) => new DatabaseConsole(args ?? new DatabaseConsoleArgs());

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConsole"/> class.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseConsoleArgs"/>.</param>
        private DatabaseConsole(DatabaseConsoleArgs args)
        {
            Args = args;

            if (Args.OutputDirectory == null)
                Args.OutputDirectory = new DirectoryInfo(CodeGenFileManager.GetExeDirectory()).Parent;

            if (string.IsNullOrEmpty(Args.ScriptFileName))
                Args.ScriptFileName = DefaultDatabaseScript;

            var assembly = typeof(DatabaseConsole).Assembly!;
            Name = assembly.GetName()?.Name ?? throw new InvalidOperationException("Unable to infer name.");
            Text = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? throw new InvalidOperationException("Unable to infer text.");
            Version = assembly.GetName()?.Version?.ToString(3);
            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? Text;
        }

        /// <summary>
        /// Gets the <see cref="DatabaseConsoleArgs"/>.
        /// </summary>
        public DatabaseConsoleArgs Args { get; }

        /// <summary>
        /// Gets the application/command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the application/command short text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the application/command description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the application/command version.
        /// </summary>
        public string? Version { get; }

        /// <summary>
        /// Gets or sets the masthead text.
        /// </summary>
        /// <remarks>Defaults to <see cref="DefaultMastheadText"/>.</remarks>
        public string? MastheadText { get; set; } = DefaultMastheadText;

        /// <summary>
        /// Adds the <paramref name="assemblies"/> containing the embedded resources.
        /// </summary>
        /// <param name="assemblies">The assemblies containing the embedded resources.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsole Assemblies(params Assembly[] assemblies)
        {
            Args.AddAssembly(assemblies);
            return this;
        }

        /// <summary>
        /// Sets (overrides) the output <see cref="DirectoryInfo"/> where the generated artefacts are to be written.
        /// </summary>
        /// <param name="path">The output <see cref="DirectoryInfo"/>.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsole OutputDirectory(string path)
        {
            Args.OutputDirectory = new DirectoryInfo(Check.NotEmpty(path, nameof(path)));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name (defaults to <see cref="DefaultDatabaseScript"/>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsole DatabaseScript(string script)
        {
            Args.ScriptFileName = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the supported <see cref="DatabaseExecutorCommand"/> (defaults to <see cref="DatabaseExecutorCommand.All"/>.
        /// </summary>
        /// <param name="command">The supported <see cref="DatabaseExecutorCommand"/>.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsole Supports(DatabaseExecutorCommand command)
        {
            Args.SupportedCommands = command;
            return this;
        }

        /// <summary>
        /// Adds the <paramref name="schemas"/> to the schema order.
        /// </summary>
        /// <param name="schemas">The schema names to add.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsole SchemaOrder(params string[] schemas)
        {
            Args.SchemaOrder.AddRange(schemas);
            return this;
        }

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string? args = null) => await RunAsync(CodeGenConsoleBase.SplitArgumentsIntoArray(args)).ConfigureAwait(false);

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> array.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            // Final set up of console arguments.
            Args.Logger ??= new ConsoleLogger(PhysicalConsole.Singleton);
            Diagnostics.Logger.Default ??= Args.Logger;

            // Set up the app.
            using var app = new CommandLineApplication(PhysicalConsole.Singleton) { Name = Name, Description = Description };
            app.HelpOption();

            _cmdArg = app.Argument<DatabaseExecutorCommand>("command", "Database command.").IsRequired();
            _options.Add(nameof(DatabaseConsoleArgs.ConnectionString), app.Option("-cs|--connection-string", "Database connection string.", CommandOptionType.SingleValue));
            _options.Add(nameof(DatabaseConsoleArgs.ConnectionStringEnvironmentVariableName), app.Option("-cv|--connection-varname", "Database connection string environment variable name.", CommandOptionType.SingleValue));
            _options.Add(nameof(DatabaseConsoleArgs.SchemaOrder), app.Option("-so|--schema-order", "Database schema name (multiple can be specified in priority order).", CommandOptionType.MultipleValue));
            _options.Add(nameof(DatabaseConsoleArgs.Assemblies), app.Option("-a|--assembly", "Assembly containing embedded resources (multiple can be specified in probing order).", CommandOptionType.MultipleValue));
            _options.Add(EntryAssemblyOnlyOptionName, app.Option("-eo|--entry-assembly-only", "Use the entry assembly only.", CommandOptionType.NoValue));
            _options.Add(nameof(DatabaseConsoleArgs.Parameters), app.Option("-p|--param", "Parameter expressed as a 'Name=Value' pair (multiple can be specified).", CommandOptionType.MultipleValue));
            _options.Add(nameof(DatabaseConsoleArgs.SupportedCommands), app.Option<int>("-su|--supported", "Supported commands (integer)", CommandOptionType.SingleValue));
            _options.Add(nameof(DatabaseConsoleArgs.ScriptFileName), app.Option("-s|--script", "Script orchestration file name. [CodeGen]", CommandOptionType.SingleValue));
            _options.Add(nameof(DatabaseConsoleArgs.ConfigFileName), app.Option("-c|--config", "Configuration data file name. [CodeGen]", CommandOptionType.SingleValue));
            _options.Add(nameof(DatabaseConsoleArgs.OutputDirectory), app.Option("-o|--output", "Output directory path. [CodeGen]", CommandOptionType.MultipleValue).Accepts(v => v.ExistingDirectory("Output directory path does not exist.")));
            _options.Add(nameof(DatabaseConsoleArgs.ExpectNoChanges), app.Option("-enc|--expect-no-changes", "Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline). [CodeGen]", CommandOptionType.NoValue));
            _options.Add(nameof(DatabaseConsoleArgs.IsSimulation), app.Option("-sim|--simulation", "Indicates whether the code-generation is a simulation (i.e. does not update the artefacts). [CodeGen]", CommandOptionType.NoValue));
            _options.Add(XmlToYamlOptionName, app.Option("-x2y|--xml-to-yaml", "Convert the XML configuration into YAML equivalent (will not codegen). [CodeGen]", CommandOptionType.NoValue));
            _scriptNewArg = app.Argument("script-new-args", "Additional arguments. [ScriptNew]", multipleValues: true);

            app.OnValidate(ctx =>
            {
                Args.Command = _cmdArg.ParsedValue;

                UpdateStringOptionMulti(nameof(DatabaseConsoleArgs.SchemaOrder), v =>
                {
                    Args.SchemaOrder.Clear();
                    Args.SchemaOrder.AddRange(v);
                });

                var vr = new AssemblyValidator(Args).GetValidationResult(GetCommandOption(nameof(DatabaseConsoleArgs.Assemblies)), ctx);
                if (vr != ValidationResult.Success)
                    return vr;

                UpdateBooleanOption(EntryAssemblyOnlyOptionName, () =>
                {
                    Args.Assemblies.Clear();
                    Args.AddAssembly(Assembly.GetEntryAssembly()!);
                }, () =>
                {
                    if (Args.UseBeefDbo && !Args.Assemblies.Contains(typeof(DatabaseConsole).Assembly))
                        Args.Assemblies.Add(typeof(DatabaseConsole).Assembly);
                });

                vr = new ParametersValidator(Args).GetValidationResult(GetCommandOption(nameof(DatabaseConsoleArgs.Parameters)), ctx);
                if (vr != ValidationResult.Success)
                    return vr;

                UpdateTypedOption<int>(nameof(DatabaseConsoleArgs.SupportedCommands), v => Args.SupportedCommands = (DatabaseExecutorCommand)v);
                UpdateStringOption(nameof(DatabaseConsoleArgs.ScriptFileName), v => Args.ScriptFileName = v);
                UpdateStringOption(nameof(DatabaseConsoleArgs.ConfigFileName), v => Args.ConfigFileName = v);
                UpdateStringOption(nameof(DatabaseConsoleArgs.OutputDirectory), v => Args.OutputDirectory = new DirectoryInfo(v));
                UpdateBooleanOption(nameof(DatabaseConsoleArgs.ExpectNoChanges), () => Args.ExpectNoChanges = true);
                UpdateBooleanOption(nameof(DatabaseConsoleArgs.IsSimulation), () => Args.IsSimulation = true);

                Args.AddScriptNewArguments(_scriptNewArg.Values.Where(x => !string.IsNullOrEmpty(x)).OfType<string>().Distinct().ToArray());
                if (Args.ScriptNewArguments.Count > 0 && _cmdArg.ParsedValue != DatabaseExecutorCommand.ScriptNew)
                    return new ValidationResult("Additional arguments can only be specified when the command is '{nameof(DatabaseExecutorCommand.ScriptNew)}'.", new string[] { "args" });

                if (GetCommandOption(XmlToYamlOptionName).HasValue() && _cmdArg.ParsedValue != DatabaseExecutorCommand.CodeGen)
                    return new ValidationResult($"Command '{_cmdArg.ParsedValue}' is not compatible with --xml-to-yaml; the command must be '{nameof(DatabaseExecutorCommand.CodeGen)}'.");

                UpdateStringOption(nameof(DatabaseConsoleArgs.ConnectionStringEnvironmentVariableName), v => Args.ConnectionStringEnvironmentVariableName = v);
                Args.OverrideConnectionString(GetCommandOption(nameof(DatabaseConsoleArgs.ConnectionString)).Value());

                return ValidationResult.Success!;
            });

            // Set up the code generation execution.
            app.OnExecuteAsync(_ => RunRunaway());

            // Execute the command-line app.
            try
            {
                return await app.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                Args.Logger?.LogError(cpex.Message);
                Args.Logger?.LogError(string.Empty);
                return 1;
            }
        }

        /// <summary>
        /// Gets the selected <see cref="CommandOption"/> for the specfied key.
        /// </summary>
        private CommandOption GetCommandOption(string key) => _options[key];

        /// <summary>
        /// Updates the command option from a string option.
        /// </summary>
        private void UpdateStringOption(string key, Action<string> action)
        {
            var co = GetCommandOption(key);
            if (co.HasValue())
            {
                var v = co.Value();
                if (!string.IsNullOrEmpty(v))
                    action.Invoke(v);
            }
        }

        /// <summary>
        /// Updates the command option from a multiple string option.
        /// </summary>
        private void UpdateStringOptionMulti(string key, Action<string[]> action)
        {
            var co = GetCommandOption(key);
            if (co.HasValue())
                action.Invoke(co.Values.Where(x => !string.IsNullOrEmpty(x)).OfType<string>().Distinct().ToArray());
        }

        /// <summary>
        /// Updates the command option from a boolean option.
        /// </summary>
        private void UpdateBooleanOption(string key, Action action, Action? elseAction = null)
        {
            var co = GetCommandOption(key);
            if (co.HasValue())
                action.Invoke();
            else
                elseAction?.Invoke();
        }

        /// <summary>
        /// Updates the command opton from a typed option.
        /// </summary>
        private void UpdateTypedOption<T>(string key, Action<T> action)
        {
            var co = (CommandOption<T>)GetCommandOption(key);
            if (co.HasValue())
                action.Invoke(co.ParsedValue);
        }

        /// <summary>
        /// Performs the actual database processing.
        /// </summary>
        private async Task<int> RunRunaway() /* Method name inspired by: Slade - Run Runaway - https://www.youtube.com/watch?v=gMxcGaAwy-Q */
        {
            try
            {
                // Write the masthead and headser.
                if (MastheadText != null)
                    Args.Logger?.LogInformation(MastheadText);

                // Write the header.
                Args.Logger?.LogInformation($"{Text}{(Version == null ? "" : $" [v{Version}]")}");
                Args.Logger?.LogInformation(string.Empty);

                if (GetCommandOption(XmlToYamlOptionName).HasValue())
                {
                    var success = await CodeGenFileManager.ConvertXmlToYamlAsync(CommandType.Database, CodeGenFileManager.GetConfigFilename(CodeGenFileManager.GetExeDirectory(), CommandType.Database, Args.GetCompany(), Args.GetAppName())).ConfigureAwait(false);
                    return success ? 0 : 4;
                }

                // Write the options.
                Args.Logger?.LogInformation($"Command = {_cmdArg!.ParsedValue}");
                Args.Logger?.LogInformation($"SchemaOrder = {string.Join(", ", Args.SchemaOrder.ToArray())}");

                Args.Logger?.LogInformation($"Parameters{(Args.Parameters.Count == 0 ? " = none" : ":")}");
                foreach (var p in Args.Parameters)
                {
                    Args.Logger?.LogInformation($"  {p.Key} = {p.Value}");
                }

                Args.Logger?.LogInformation($"Assemblies{(Args.Assemblies.Count == 0 ? " = none" : ":")}");
                foreach (var a in Args.Assemblies)
                {
                    Args.Logger?.LogInformation($"  {a.FullName}");
                }

                Args.Logger?.LogInformation(string.Empty);

                // Run the database executor.
                var de = new DatabaseExecutor(new DatabaseExecutorArgs(Args));
                var sw = Stopwatch.StartNew();

                var ok = await de.RunAsync().ConfigureAwait(false);
                if (!ok)
                    return 5;

                // Write the footer.
                sw.Stop();
                Args.Logger?.LogInformation(string.Empty);
                Args.Logger?.LogInformation($"Complete. [{sw.ElapsedMilliseconds}ms]");
                Args.Logger?.LogInformation(string.Empty);

                return 0;
            }
            catch (CodeGenException gcex)
            {
                if (gcex.Message != null)
                {
                    Args.Logger?.LogError(gcex.Message);
                    if (gcex.InnerException != null)
                        Args.Logger?.LogError(gcex.InnerException.Message);

                    Args.Logger?.LogError(string.Empty);
                }

                return 2;
            }
            catch (CodeGenChangesFoundException cgcfex)
            {
                Args.Logger?.LogError(cgcfex.Message);
                Args.Logger?.LogError(string.Empty);
                return 3;
            }
        }
    }
}