// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>CodeGen Console</b> that facilitates the code generation output by handling the standard console arguments invoking the underlying <see cref="CodeGeneratorEventArgs"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class CodeGenConsole
    {
        private readonly CommandArgument _configArg;
        private readonly CommandOption _scriptOpt;
        private readonly CommandOption _templateOpt;
        private readonly CommandOption _outputOpt;
        private readonly CommandOption _assembliesOpt;
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly CommandOption _paramsOpt;
        private readonly CommandOption _expectNoChange;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/>.
        /// </summary>
        /// <returns>The <see cref="CodeGenConsole"/>.</returns>
        public static CodeGenConsole Create()
        {
            return new CodeGenConsole();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        private CodeGenConsole()
        {
            App = new CommandLineApplication()
            {
                Name = "beef.codegen",
                Description = "Business Entity Execution Framework (Beef) Code Generator."
            };

            App.HelpOption(true);

            _configArg = App.Argument("config", "CodeGeneration configuration YAML/JSON/XML file.")
                .IsRequired()
                .Accepts(v => v.ExistingFile());

            _scriptOpt = App.Option("-s|--script", "Execution script file or embedded resource name.", CommandOptionType.SingleValue)
                .IsRequired();

            _templateOpt = App.Option("-t|--template", "Templates path (defaults to embedded resources).", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _outputOpt = App.Option("-o|--output", "Output path (defaults to current path).", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _assembliesOpt = App.Option("-a|--assembly", "Assembly name containing scripts (multiple can be specified).", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new AssemblyValidator(_assemblies)));

            _paramsOpt = App.Option("-p|--param", "Name=Value pair(s) passed into code generation.", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new ParamsValidator()));

            _expectNoChange = App.Option("--expectNoChanges", "Expect no changes in the output and error where changes are detected (e.g. within build pipeline).", CommandOptionType.NoValue);

            _logger = (Logger.Default ??= new ColoredConsoleLogger(nameof(CodeGenConsole)));

            App.OnExecuteAsync(async (_) => await RunRunAwayAsync().ConfigureAwait(false));
        }
    
        /// <summary>
        /// Gets the underlying <see cref="CommandLineApplication"/>.
        /// </summary>
        public CommandLineApplication App { get; }

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public Task<int> RunAsync(string? args = null)
        {
            if (string.IsNullOrEmpty(args))
                return RunAsync(Array.Empty<string>());

            // See for inspiration: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
            var regex = Regex.Matches(args, @"\G(""((""""|[^""])+)""|(\S+)) *");
            var array = regex.Cast<Match>()
                         .Select(m => Regex.Replace(
                             m.Groups[2].Success
                                 ? m.Groups[2].Value
                                 : m.Groups[4].Value, @"""""", @"""")).ToArray();

            return RunAsync(array);
        }

        /// <summary>
        /// Runs the code generation using the passed array of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            try
            {
                return await App.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                _logger.LogError(cpex.Message);
                if (cpex.InnerException != null)
                    _logger.LogError(cpex.InnerException.Message);

                return -1;
            }
        }

        /// <summary>
        /// Coordinates the run (overall execution).
        /// </summary>
        private async Task<int> RunRunAwayAsync() /* Inspired by https://www.youtube.com/watch?v=ikMiQZF-mAY */
        {
            var args = new CodeGenExecutorArgs(_logger, _assembliesOpt.HasValue() ? ((AssemblyValidator)_assembliesOpt.Validators.First()).Assemblies : _assemblies, CreateParamDict(_paramsOpt))
            {
                ConfigFile = new FileInfo(_configArg.Value),
                ScriptFile = new FileInfo(_scriptOpt.Value()),
                TemplatePath = _templateOpt.HasValue() ? new DirectoryInfo(_templateOpt.Value()) : null,
                OutputPath = new DirectoryInfo(_outputOpt.HasValue() ? _outputOpt.Value() : Environment.CurrentDirectory),
                ExpectNoChange = _expectNoChange.HasValue()
            };

            WriteHeader(args);

            var cge = new CodeGenExecutor(args);
            var sw = Stopwatch.StartNew();

            var result = await cge.RunAsync().ConfigureAwait(false);

            sw.Stop();
            WriteFooter(_logger, sw, cge);
            return result ? 0 : -1;
        }

        /// <summary>
        /// Creates a param (name=value) pair dictionary from the command option values.
        /// </summary>
        public static Dictionary<string, string> CreateParamDict(CommandOption cmdOpt)
        {
            if (cmdOpt == null)
                throw new ArgumentNullException(nameof(cmdOpt));

            var pd = new Dictionary<string, string>();
            foreach (var p in cmdOpt.Values.Where(x => !string.IsNullOrEmpty(x)))
            {
                string[] parts = CreateKeyValueParts(p!);
                pd.Add(parts[0], parts[1]);
            }

            return pd;
        }

        /// <summary>
        /// Creates Key=Value parts.
        /// </summary>
        internal static string[] CreateKeyValueParts(string text)
        {
            var pos = text.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
            if (pos < 0)
                return Array.Empty<string>();

            return new string[] { text.Substring(0, pos), text[(pos + 1)..] };
        }

        /// <summary>
        /// Logs (writes) the <see cref="CodeGenExecutorArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGenExecutorArgs"/>.</param>
        /// <param name="paramsOnly">Indicates whether to log on the parameters collection only.</param>
        public static void LogCodeGenExecutionArgs(CodeGenExecutorArgs args, bool paramsOnly = false)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (!paramsOnly)
            {
                args.Logger.LogInformation($"  Config = {args.ConfigFile?.Name}");
                if (args.ScriptFile == null)
                    args.Logger.LogInformation("  Script = (none)");
                else
                    args.Logger.LogInformation($"  Script = {(args.ScriptFile.Exists ? args.ScriptFile?.FullName : args.ScriptFile?.Name)}");

                args.Logger.LogInformation($"  Template = {args.TemplatePath?.FullName}");
                args.Logger.LogInformation($"  Output = {args.OutputPath?.FullName}");
                args.Logger.LogInformation($"  ExpectNoChange = {args.ExpectNoChange}");
            }

            args.Logger.LogInformation($"  Params{(args.Parameters.Count == 0 ? " = none" : ":")}");

            foreach (var p in args.Parameters)
            {
                Console.WriteLine($"    {p.Key} = {p.Value}");
            }
        }

        /// <summary>
        /// Writes the header information.
        /// </summary>
        private void WriteHeader(CodeGenExecutorArgs args)
        {
            WriteMasthead(_logger);
            LogCodeGenExecutionArgs(args);
            _logger.LogInformation(string.Empty);
        }

        /// <summary>
        /// Writes the mast head information.
        /// </summary>
        public static void WriteMasthead(ILogger? logger = null)
        {
            logger ??= new ColoredConsoleLogger(nameof(CodeGenConsole));

            // http://www.patorjk.com/software/taag/#p=display&f=Calvin%20S&t=Beef%20Code-Gen%20Tool%0A
            logger.LogInformation(@"
╔╗ ┌─┐┌─┐┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
╠╩╗├┤ ├┤ ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││  
╚═╝└─┘└─┘└    ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘
");
            logger.LogInformation($"Business Entity Execution Framework (Beef) Code Generator [v{typeof(CodeGenConsole).Assembly.GetName().Version?.ToString(3)}].");
            logger.LogInformation(string.Empty);
        }

        /// <summary>
        /// Write the footer information.
        /// </summary>
        public static void WriteFooter(ILogger logger, Stopwatch sw, CodeGenExecutor cge)
        {
            logger.LogInformation(string.Empty);
            logger.LogInformation($"Beef Code-Gen Tool complete [{sw?.ElapsedMilliseconds}ms, Unchanged = {cge.OverallNotChangedCount}, Updated = {cge.OverallUpdatedCount}, Created = {cge.OverallCreatedCount}, TotalLines = {cge.OverallLinesOfCodeCount}].");
            logger.LogInformation(string.Empty);
        }
    }
}