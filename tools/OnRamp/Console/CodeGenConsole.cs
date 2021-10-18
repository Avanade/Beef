// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnRamp.Console
{
    /// <summary>
    /// Console that facilitates the code generation by managing the standard console command-line arguments/options.
    /// </summary>
    /// <remarks>The standard console command-line arguments/options can be controlled via the constructor using the <see cref="SupportedOptions"/> flags. Additional capabilities can be added by inherting and overridding the
    /// <see cref="OnBeforeExecute(CommandLineApplication)"/>, <see cref="OnValidation(ValidationContext)"/> and <see cref="OnCodeGeneration"/>. Changes to the console output can be achieved by overridding
    /// <see cref="OnWriteMasthead"/>, <see cref="OnWriteHeader"/>, <see cref="OnWriteArgs(CodeGeneratorArgsBase)"/> and <see cref="OnWriteFooter(CodeGenStatistics)"/>.</remarks>
    public class CodeGenConsole
    {
        private readonly SupportedOptions _supportedOptions;
        private readonly Dictionary<SupportedOptions, CommandOption?> _options = new();

        /// <summary>
        /// Creates a new <see cref="CodeGenConsole"/> using <typeparamref name="T"/> to determine <see cref="Assembly"/> defaulting <see cref="Name"/> (with <see cref="AssemblyName.Name"/>), <see cref="Text"/> (with <see cref="AssemblyProductAttribute.Product"/>),
        /// <see cref="Description"/> (with <see cref="AssemblyDescriptionAttribute.Description"/>), and <see cref="Version"/> (with <see cref="AssemblyName.Version"/>).
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden or updated (<see cref="CodeGeneratorArgsBase.Assemblies"/> and <see cref="CodeGeneratorArgsBase.Parameters"/>) by the command-line argument values.</param>
        /// <param name="name">The application/command name; defaults to <see cref="AssemblyName.Name"/>.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; defaults to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        /// <returns>A new <see cref="CodeGenConsole"/>.</returns>
        public static CodeGenConsole Create<T>(CodeGeneratorArgs? args = null, string ? name = null, string? text = null, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All)
            => new(typeof(T).Assembly, args, name, text, description, version, options);

        /// <summary>
        /// Split an <paramref name="args"/> <see cref="string"/> into an <see cref="Array"/> of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The <see cref="Array"/> of arguments.</returns>
        public static string[] SplitArgumentsIntoArray(string? args)
        {
            if (string.IsNullOrEmpty(args))
                return Array.Empty<string>();

            // See for inspiration: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
            var regex = Regex.Matches(args, @"\G(""((""""|[^""])+)""|(\S+)) *");
            return regex.Cast<Match>()
                        .Select(m => Regex.Replace(
                            m.Groups[2].Success
                                ? m.Groups[2].Value
                                : m.Groups[4].Value, @"""""", @"""")).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="name">The application/command name.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; will default to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden or updated (<see cref="CodeGeneratorArgsBase.Assemblies"/> and <see cref="CodeGeneratorArgsBase.Parameters"/>) by the command-line argument values.</param>
        public CodeGenConsole(string name, string text, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All, CodeGeneratorArgs? args = null)
        {
            Args = args ?? new CodeGeneratorArgs();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Description = description ?? Text;
            Version = version;
            _supportedOptions = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class defaulting <see cref="Name"/> (with <see cref="AssemblyName.Name"/>), <see cref="Text"/> (with <see cref="AssemblyProductAttribute.Product"/>),
        /// <see cref="Description"/> (with <see cref="AssemblyDescriptionAttribute.Description"/>), and <see cref="Version"/> (with <see cref="AssemblyName.Version"/>) from the <paramref name="assembly"/> where not expressly provided.
        /// </summary>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden or updated (<see cref="CodeGeneratorArgsBase.Assemblies"/> and <see cref="CodeGeneratorArgsBase.Parameters"/>) by the command-line argument values.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to infer properties where not expressly provided.</param>
        /// <param name="name">The application/command name; defaults to <see cref="AssemblyName.Name"/>.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; defaults to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        public CodeGenConsole(Assembly assembly, CodeGeneratorArgs? args = null, string ? name = null, string? text = null, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Args = args ?? new CodeGeneratorArgs();
            var an = assembly.GetName();
            Name = name ?? an?.Name ?? throw new ArgumentException("Unable to infer name.", nameof(name));
            Text = text ?? assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? throw new ArgumentException("Unable to infer text.", nameof(text));
            Version = version ?? (assembly ?? throw new ArgumentNullException(nameof(assembly))).GetName()?.Version?.ToString(3);
            Description = description ?? assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? Text;
            _supportedOptions = options;
        }

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        public CodeGeneratorArgs Args { get; }

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
        /// Gets the <see cref="Args"/> <see cref="CodeGeneratorArgsBase.Logger"/>.
        /// </summary>
        protected ILogger? Logger => Args.Logger;

        /// <summary>
        /// Indicates whether to bypass standard execution of <see cref="OnWriteMasthead"/>, <see cref="OnWriteHeader"/>, <see cref="OnWriteArgs(CodeGeneratorArgsBase)"/> and <see cref="OnWriteFooter(CodeGenStatistics)"/>.
        /// </summary>
        protected bool BypassOnWrites { get; set; }

        /// <summary>
        /// Gets or sets the masthead text used by <see cref="OnWriteMasthead"/>.
        /// </summary>
        /// <remarks>Defaults to 'Code-Gen Tool' formatted using <see href="https://www.patorjk.com/software/taag/#p=display&amp;f=Calvin%20S&amp;t=OnRamp%20CodeGen%20Tool"/>.</remarks>
        public string? MastheadText { get; set; } = @"
╔═╗┌┐┌╦═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┬┐┌─┐╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
║ ║│││╠╦╝├─┤│││├─┘  ║  │ │ ││├┤ ║ ╦├┤ │││   ║ │ ││ ││  
╚═╝┘└┘╩╚═┴ ┴┴ ┴┴    ╚═╝└─┘─┴┘└─┘╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘
";

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string? args = null) =>  await RunAsync(SplitArgumentsIntoArray(args)).ConfigureAwait(false);

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> array.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            Args.Logger ??= new ConsoleLogger(PhysicalConsole.Singleton);

            // Set up the app.
            using var app = new CommandLineApplication(PhysicalConsole.Singleton) { Name = Name, Description = Description };
            app.HelpOption();

            _options.Add(SupportedOptions.ScriptFileName, _supportedOptions.HasFlag(SupportedOptions.ScriptFileName) ? app.Option("-s|--script", "Script orchestration file/resource name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.ConfigFileName, _supportedOptions.HasFlag(SupportedOptions.ConfigFileName) ? app.Option("-c|--config", "Configuration data file name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.OutputDirectory, _supportedOptions.HasFlag(SupportedOptions.OutputDirectory) ? app.Option("-o|--output", "Output directory path.", CommandOptionType.MultipleValue).Accepts(v => v.ExistingDirectory("Output directory path does not exist.")) : null);
            _options.Add(SupportedOptions.Assemblies, _supportedOptions.HasFlag(SupportedOptions.Assemblies) ? app.Option("-a|--assembly", "Assembly containing embedded resources (multiple can be specified in probing order).", CommandOptionType.MultipleValue) : null);
            _options.Add(SupportedOptions.Parameters, _supportedOptions.HasFlag(SupportedOptions.Parameters) ? app.Option("-p|--param", "Parameter expressed as a 'Name=Value' pair (multiple can be specified).", CommandOptionType.MultipleValue) : null);
            _options.Add(SupportedOptions.DatabaseConnectionString, _supportedOptions.HasFlag(SupportedOptions.DatabaseConnectionString) ? app.Option("-cs|--connection-string", "Database connection string.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName, _supportedOptions.HasFlag(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName) ? app.Option("-cv|--connection-varname", "Database connection string environment variable name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.ExpectNoChanges, _supportedOptions.HasFlag(SupportedOptions.ExpectNoChanges) ? app.Option("-enc|--expect-no-changes", "Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline).", CommandOptionType.NoValue) : null);
            _options.Add(SupportedOptions.IsSimulation, _supportedOptions.HasFlag(SupportedOptions.IsSimulation) ? app.Option("-sim|--simulation", "Indicates whether the code-generation is a simulation (i.e. does not create/update any artefacts).", CommandOptionType.NoValue) : null);

            // Set up the code generation validation.
            app.OnValidate(ctx =>
            {
                // Update the options from command line.
                UpdateStringOption(SupportedOptions.ScriptFileName, v => Args.ScriptFileName = v);
                UpdateStringOption(SupportedOptions.ConfigFileName, v => Args.ConfigFileName = v);
                UpdateBooleanOption(SupportedOptions.ExpectNoChanges, () => Args.ExpectNoChanges = true);
                UpdateBooleanOption(SupportedOptions.IsSimulation, () => Args.IsSimulation = true);
                UpdateStringOption(SupportedOptions.OutputDirectory, v => Args.OutputDirectory = new DirectoryInfo(v));

                var vr = ValidateMultipleValue(SupportedOptions.Assemblies, ctx, (ctx, co) => new AssemblyValidator(Args).GetValidationResult(co, ctx));
                if (vr != ValidationResult.Success)
                    return vr;

                vr = ValidateMultipleValue(SupportedOptions.Parameters, ctx, (ctx, co) => new ParametersValidator(Args).GetValidationResult(co, ctx));
                if (vr != ValidationResult.Success)
                    return vr;

                // Handle the connection string, in order of precedence: command-line argument, environment variable, what was passed as initial argument.
                var cs = GetCommandOption(SupportedOptions.DatabaseConnectionString);
                if (cs != null)
                {
                    var evn = GetCommandOption(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName)?.Value();
                    if (!string.IsNullOrEmpty(evn))
                        Args.ConnectionStringEnvironmentVariableName = evn;

                    Args.UpdateConnectionString(cs.Value());
                }

                // Invoke any additional.
                return OnValidation(ctx)!;
            });

            // Set up the code generation execution.
            app.OnExecute(() => RunRunaway());
            OnBeforeExecute(app);

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
        /// Gets the selected <see cref="CommandOption"/> for the specfied <paramref name="option"/> selection.
        /// </summary>
        /// <param name="option">The <see cref="SupportedOptions"/> option.</param>
        /// <returns>The corresponding <see cref="CommandOption"/> where found; otherwise, <c>null</c>.</returns>
        protected CommandOption? GetCommandOption(SupportedOptions option) => _options.GetValueOrDefault(option);

        /// <summary>
        /// Updates the command option from a string option.
        /// </summary>
        private void UpdateStringOption(SupportedOptions option, Action<string?> action)
        {
            var co = GetCommandOption(option);
            if (co != null && co.HasValue())
            {
                var val = co.Value();
                if (!string.IsNullOrEmpty(val))
                    action.Invoke(val);
            }
        }

        /// <summary>
        /// Updates the command option from a boolean option.
        /// </summary>
        private void UpdateBooleanOption(SupportedOptions option, Action action)
        {
            var co = GetCommandOption(option);
            if (co != null && co.HasValue())
                action.Invoke();
        }

        /// <summary>
        /// Validate multiple options.
        /// </summary>
        private ValidationResult ValidateMultipleValue(SupportedOptions option, ValidationContext ctx, Func<ValidationContext, CommandOption, ValidationResult> func)
        {
            var co = GetCommandOption(option);
            if (co == null)
                return ValidationResult.Success!;
            else
                return func(ctx, co);
        }

        /// <summary>
        /// Invoked before the underlying console execution occurs.
        /// </summary>
        /// <param name="app">The underlying <see cref="CommandLineApplication"/>.</param>
        /// <remarks>This enables additional configuration to the <paramref name="app"/> prior to execution. For example, adding additional command line arguments.</remarks>
        protected virtual void OnBeforeExecute(CommandLineApplication app) { }

        /// <summary>
        /// Invoked after command parsing is complete and before the underlying code-generation.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext"/>.</param>
        /// <returns>The <see cref="ValidationResult"/>.</returns>
        protected virtual ValidationResult? OnValidation(ValidationContext context) => ValidationResult.Success;

        /// <summary>
        /// Performs the actual code-generation.
        /// </summary>
        private int RunRunaway() /* Method name inspired by: Slade - Run Runaway - https://www.youtube.com/watch?v=gMxcGaAwy-Q */
        {
            try
            {
                // Write header, etc.
                if (!BypassOnWrites)
                {
                    OnWriteMasthead();
                    OnWriteHeader();
                    OnWriteArgs(Args);
                }

                // Run the code generator.
                var stats = OnCodeGeneration() ?? throw new InvalidOperationException("A CodeGenStatistics instance must be returned from OnRunCodeGenerator.");
                if (stats == null)
                    return 4;

                // Write footer and exit successfully.
                if (!BypassOnWrites)
                    OnWriteFooter(stats);

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

        /// <summary>
        /// Invoked to write the <see cref="MastheadText"/> to the <see cref="Logger"/>.
        /// </summary>
        protected virtual void OnWriteMasthead()
        {
            if (MastheadText != null)
                Logger?.LogInformation(MastheadText);
        }

        /// <summary>
        /// Invoked to instantiate and run a <see cref="CodeGenerator"/> using the <see cref="Args"/> returning the corresponding <see cref="CodeGenStatistics"/>.
        /// </summary>
        /// <remarks>The code invoked internally is: <c>return new CodeGenerator(Args).Generate();</c></remarks>
        /// <returns>The <see cref="CodeGenStatistics"/> where successful; otherwise, <c>null</c>. This will result in a '4' being returned by <see cref="RunAsync(string[])"/>.</returns>
        protected virtual CodeGenStatistics? OnCodeGeneration() => new CodeGenerator(Args).Generate();

        /// <summary>
        /// Invoked to write the header information to the <see cref="Logger"/>.
        /// </summary>
        /// <remarks>Writes the <see cref="Text"/> and <see cref="Version"/>.</remarks>
        protected virtual void OnWriteHeader()
        {
            Logger?.LogInformation($"{Text}{(Version == null ? "" : $" [v{Version}]")}");
            Logger?.LogInformation(string.Empty);
        }

        /// <summary>
        /// Invoked to write the <see cref="Args"/> to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/> to write.</param>
        protected virtual void OnWriteArgs(CodeGeneratorArgsBase args) => WriteStandardizedArgs(args);

        /// <summary>
        /// Write the <see cref="Args"/> to the <see cref="Logger"/> in a standardized (reusable) manner.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/> to write.</param>
        public static void WriteStandardizedArgs(CodeGeneratorArgsBase args)
        {
            if (args == null || args.Logger == null)
                return;

            args.Logger?.LogInformation($"Config = {args.ConfigFileName}");
            args.Logger?.LogInformation($"Script = {args.ScriptFileName}");
            args.Logger?.LogInformation($"OutDir = {args.OutputDirectory?.FullName}");
            args.Logger?.LogInformation($"ExpectNoChanges = {args.ExpectNoChanges}");
            args.Logger?.LogInformation($"IsSimulation = {args.IsSimulation}");

            args.Logger?.LogInformation($"Parameters{(args.Parameters.Count == 0 ? " = none" : ":")}");
            foreach (var p in args.Parameters)
            {
                args.Logger?.LogInformation($"  {p.Key} = {p.Value}");
            }

            args.Logger?.LogInformation($"Assemblies{(args.Assemblies.Count == 0 ? " = none" : ":")}");
            foreach (var a in args.Assemblies)
            {
                args.Logger?.LogInformation($"  {a.FullName}");
            }

            args.Logger?.LogInformation(string.Empty);
            args.Logger?.LogInformation("Scripts:");
        }

        /// <summary>
        /// Invoked to write the footer (<see cref="CodeGenStatistics.ToSummaryString"/>) information to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="stats"></param>
        protected virtual void OnWriteFooter(CodeGenStatistics stats)
        {
            Logger?.LogInformation(string.Empty);
            Logger?.LogInformation($"Complete. {stats.ToSummaryString()}");
            Logger?.LogInformation(string.Empty);
        }
    }
}