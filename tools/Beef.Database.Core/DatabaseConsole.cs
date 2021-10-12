// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Beef.CodeGen.CodeGenConsole;

namespace Beef.Database.Core
{
    /// <summary>
    /// <b>Database Console</b> that facilitates the database tooling by handling the standard console arguments invoking the underlying <see cref="DatabaseExecutor"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class DatabaseConsole
    {
        private readonly CommandArgument<DatabaseExecutorCommand> _commandArg;
        private readonly CommandArgument _connectionStringArg;
        private readonly List<Assembly> _scriptAssemblies = new List<Assembly>();
        private readonly CommandOption _configOpt;
        private readonly CommandOption _scriptOpt;
        private readonly CommandOption _outputOpt;
        private readonly CommandOption _paramsOpt;
        private readonly CommandOption _schemaOrder;
        private readonly CommandOption<int> _supportedOpt;
        private readonly CommandOption _envVarNameOpt;
        private string? _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsole"/>.
        /// </summary>
        /// <returns>The <see cref="DatabaseConsole"/>.</returns>
        public static DatabaseConsole Create()
        {
            return new DatabaseConsole();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        private DatabaseConsole()
        {
            App = new CommandLineApplication()
            {
                Name = "beef.database",
                Description = "Business Entity Execution Framework (Beef) Database Tooling."
            };

            App.HelpOption(true);

            _commandArg = App.Argument<DatabaseExecutorCommand>("command", "Database command.").IsRequired();
            _connectionStringArg = App.Argument("connectionstring", "Database connection string.");
            App.Option("-a|--assembly", "Assembly name containing scripts (multiple can be specified, in order of use).", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new AssemblyValidator(_scriptAssemblies)));

            _configOpt = App.Option("-c|--config", "CodeGeneration configuration XML file.", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingFile());

            _scriptOpt = App.Option("-s|--script", "Execution script file or embedded resource name (defaults to Database.Xml) for code generation.", CommandOptionType.SingleValue);

            _outputOpt = App.Option("-o|--output", "Output path (defaults to current path) for code generation.", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _paramsOpt = App.Option("-p|--param", "Name=Value pair(s) passed into code generation.", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new ParamsValidator()));

            _schemaOrder = App.Option("-so|--schemaorder", "Schema priority order.", CommandOptionType.MultipleValue);
            _supportedOpt = App.Option<int>("-su|--supported", "Supported commands (integer)", CommandOptionType.SingleValue);
            _envVarNameOpt = App.Option<string>("-evn|--environmentVariableName", "Override the connection string using the specified environment variable name.", CommandOptionType.SingleValue);

            Logger.Default = _logger = new ColoredConsoleLogger(nameof(CodeGenConsole));

            App.OnValidate((ctx) => OnValidate());
            App.OnExecuteAsync((_) => RunRunAwayAsync());
        }

        /// <summary>
        /// Performs addition validations.
        /// </summary>
        private ValidationResult OnValidate()
        {
            _connectionString = _envVarNameOpt.HasValue() ? Environment.GetEnvironmentVariable(_envVarNameOpt.Value()!) : _connectionStringArg.Value;
            if (_connectionString == null)
                return new ValidationResult($"The connectionString command and/or --environmentVariableName option must be specified; with the latter at least resulting in a non-null value.");

            if (_commandArg.ParsedValue.HasFlag(DatabaseExecutorCommand.CodeGen) && _configOpt.Value() == null)
                return new ValidationResult($"The --config option is required when CodeGen is selected.");

            return ValidationResult.Success!;
        }

        /// <summary>
        /// Gets the underlying <see cref="CommandLineApplication"/>.
        /// </summary>
        public CommandLineApplication App { get; }

        /// <summary>
        /// Runs the database tooling using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">The database tooling arguments.</param>
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
        /// Runs the database tooling using the passed array of <paramref name="args"/>.
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
                _logger.LogInformation(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Coordinates the run (overall execution).
        /// </summary>
        private async Task<int> RunRunAwayAsync() /* Inspired by https://www.youtube.com/watch?v=ikMiQZF-mAY */
        {
            var args = new CodeGenExecutorArgs(_logger, _scriptAssemblies, CreateParamDict(_paramsOpt))
            {
                ConfigFile = _configOpt.HasValue() ? new FileInfo(_configOpt.Value()) : null,
                ScriptFile = _scriptOpt.HasValue() ? _scriptOpt.Value() : null,
                OutputPath = new DirectoryInfo(_outputOpt.HasValue() ? _outputOpt.Value() : Environment.CurrentDirectory)
            };

            WriteHeader(args);

            var dea = new DatabaseExecutorArgs(_commandArg.ParsedValue, _connectionString!, _scriptAssemblies.ToArray()) { CodeGenArgs = args, SupportedCommands = _supportedOpt.HasValue() ? (DatabaseExecutorCommand)_supportedOpt.ParsedValue : DatabaseExecutorCommand.All };
            if (_schemaOrder.HasValue())
                dea.SchemaOrder.AddRange(_schemaOrder.Values);

            var de = new DatabaseExecutor(dea);
            var sw = Stopwatch.StartNew();

            var result = await de.RunAsync().ConfigureAwait(false);

            sw.Stop();
            WriteFooter(sw);
            return result ? 0 : -1;
        }

        /// <summary>
        /// Writes the header information.
        /// </summary>
        private void WriteHeader(CodeGenExecutorArgs args)
        {
            WriteMasthead(_logger);
            _logger.LogInformation($"  Command = {_commandArg.ParsedValue}");
            _logger.LogInformation($"  ConnectionString = {_connectionStringArg.Value}");
            LogCodeGenExecutionArgs(args, !(_commandArg.ParsedValue.HasFlag(DatabaseExecutorCommand.CodeGen)));
        }

        /// <summary>
        /// Writes the mast head information.
        /// </summary>
        public static void WriteMasthead(ILogger? logger = null)
        {
            logger ??= new ColoredConsoleLogger(nameof(CodeGenConsole));

            // http://www.patorjk.com/software/taag/#p=display&f=Calvin%20S&t=Beef%20Database%20Tool%0A
            logger.LogInformation(@"
╔╗ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬  
╠╩╗├┤ ├┤ ├┤    ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││  
╚═╝└─┘└─┘└    ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘
");
            logger.LogInformation("Business Entity Execution Framework (Beef) Database Tooling.");
            logger.LogInformation(string.Empty);
        }

        /// <summary>
        /// Write the footer information.
        /// </summary>
        private void WriteFooter(Stopwatch sw)
        {
            _logger.LogInformation(string.Empty);
            _logger.LogInformation($"Beef Database Tool complete [{sw.ElapsedMilliseconds}ms].");
            _logger.LogInformation(string.Empty);
        }
    }
}