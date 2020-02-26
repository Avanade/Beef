// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.Diagnostics;
using Beef.Executors;
using McMaster.Extensions.CommandLineUtils;
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
        private readonly CommandOption _templateOpt;
        private readonly CommandOption _outputOpt;
        private readonly CommandOption _paramsOpt;

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
            App = new CommandLineApplication(false)
            {
                Name = "beef.database",
                Description = "Business Entity Execution Framework (Beef) Database Tooling."
            };

            App.HelpOption(true);

            _commandArg = App.Argument<DatabaseExecutorCommand>("command", "Database command.").IsRequired();
            _connectionStringArg = App.Argument("connectionstring", "Database connection string.").IsRequired();
            App.Option("-a|--assembly", "Assembly name containing scripts (multiple can be specified).", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new AssemblyValidator(_scriptAssemblies)));

            _configOpt = App.Option("-c|--config", "CodeGeneration configuration XML file.", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingFile());

            _scriptOpt = App.Option("-s|--script", "Execution script file or embedded resource name (defaults to Database.Xml) for code generation.", CommandOptionType.SingleValue)
                .Accepts(v => v.Use(new FileResourceValidator()));

            _templateOpt = App.Option("-t|--template", "Templates path (defaults to embedded resources) for code generation.", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _outputOpt = App.Option("-o|--output", "Output path (defaults to current path) for code generation.", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _paramsOpt = App.Option("-p|--param", "Name=Value pair(s) passed into code generation.", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new ParamsValidator()));

            App.OnValidate((ctx) => OnValidate());
            App.OnExecuteAsync((_) => RunRunAwayAsync());
        }

        /// <summary>
        /// Performs addition validations.
        /// </summary>
        private ValidationResult OnValidate()
        {
            if (_commandArg.ParsedValue.HasFlag(DatabaseExecutorCommand.CodeGen))
            {
                if (_configOpt.Value() == null)
                    return new ValidationResult($"The --config option is required when CodeGen is selected.");
            }

            return ValidationResult.Success;
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
            SetupExecutionContext();

            try
            {
                return await App.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                Console.WriteLine(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Creates <see cref="ExecutionManager"/> and coordinates the run (overall execution).
        /// </summary>
        private async Task<int> RunRunAwayAsync() /* Inspired by https://www.youtube.com/watch?v=ikMiQZF-mAY */
        {
            var args = new CodeGenExecutorArgs(_scriptAssemblies, CreateParamDict(_paramsOpt))
            {
                ConfigFile = new FileInfo(_configOpt.Value()),
                ScriptFile = new FileInfo(_scriptOpt.Value()),
                TemplatePath = _templateOpt.HasValue() ? new DirectoryInfo(_templateOpt.Value()) : null,
                OutputPath = new DirectoryInfo(_outputOpt.HasValue() ? _outputOpt.Value() : Environment.CurrentDirectory)
            };

            WriteHeader(args);

            using (var em = ExecutionManager.Create(() => new DatabaseExecutor(_commandArg.ParsedValue, _connectionStringArg.Value!, _scriptAssemblies.ToArray(), args)))
            {
                var sw = Stopwatch.StartNew();

                await em.RunAsync().ConfigureAwait(false);

                sw.Stop();
                WriteFooter(sw);
            }

            return 0;
        }

        /// <summary>
        /// Set up the <see cref="ExecutionContext"/> including the log to console binding.
        /// </summary>
        private static void SetupExecutionContext()
        {
            if (!ExecutionContext.HasCurrent)
                ExecutionContext.SetCurrent(new ExecutionContext());

            if (!ExecutionContext.Current.HasLogger)
            {
                ExecutionContext.Current.RegisterLogger((largs) =>
                {
                    switch (largs.Type)
                    {
                        case LogMessageType.Critical:
                        case LogMessageType.Error:
                            ConsoleWriteLine(largs.ToString(), ConsoleColor.Red);
                            break;

                        case LogMessageType.Warning:
                            ConsoleWriteLine(largs.ToString(), ConsoleColor.Yellow);
                            break;

                        case LogMessageType.Info:
                            ConsoleWriteLine(largs.ToString());
                            break;

                        case LogMessageType.Debug:
                        case LogMessageType.Trace:
                            ConsoleWriteLine(largs.ToString(), ConsoleColor.Cyan);
                            break;
                    }
                });
            }
        }

        /// <summary>
        /// Writes the header information.
        /// </summary>
        private void WriteHeader(CodeGenExecutorArgs args)
        {
            Console.WriteLine(App.Description);
            Console.WriteLine();
            Console.WriteLine($"  Command = {_commandArg.ParsedValue}");
            Console.WriteLine($"  ConnectionString = {_connectionStringArg.Value}");
            CodeGenConsole.LogCodeGenExecutionArgs(args, !(_commandArg.ParsedValue.HasFlag(DatabaseExecutorCommand.CodeGen)));
        }

        /// <summary>
        /// Write the footer information.
        /// </summary>
        private static void WriteFooter(Stopwatch sw)
        {
            Console.WriteLine();
            Console.WriteLine($"Database complete [{sw.ElapsedMilliseconds}ms].");
            Console.WriteLine();
        }

        /// <summary>
        /// Writes the specified text to the console.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="foregroundColor">The foreground <see cref="ConsoleColor"/>.</param>
        private static void ConsoleWriteLine(string? text = null, ConsoleColor? foregroundColor = null)
        {
            if (string.IsNullOrEmpty(text))
                Console.WriteLine();
            else
            {
                var currColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor ?? currColor;
                Console.WriteLine(text);
                Console.ForegroundColor = currColor;
            }
        }
    }
}