// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Executors;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>CodeGen Console</b> that facilitates the code generation output by handling the standard console arguments invoking the underlying <see cref="CodeGenerator"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class CodeGenConsole
    {
        #region InternalClasses

        /// <summary>
        /// Validates either existence of file or embedded resource.
        /// </summary>
        public class FileResourceValidator : IOptionValidator
        {
            /// <summary>
            /// Performs the validation.
            /// </summary>
            /// <param name="option">The <see cref="CommandOption"/>.</param>
            /// <param name="context">The <see cref="ValidationContext"/>.</param>
            /// <returns>The <see cref="ValidationResult"/>.</returns>
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                if (option.Value() != null && !File.Exists(option.Value()) && ResourceManager.GetScriptContent(option.Value()) == null)
                    return new ValidationResult($"The file or embedded resource '{option.Value()}' does not exist.");

                return ValidationResult.Success;
            }
        }

        /// <summary>
        /// Validate the Params to ensure format is correct and values are not duplicated.
        /// </summary>
        public class ParamsValidator : IOptionValidator
        {
            /// <summary>
            /// Performs the validation.
            /// </summary>
            /// <param name="option">The <see cref="CommandOption"/>.</param>
            /// <param name="context">The <see cref="ValidationContext"/>.</param>
            /// <returns>The <see cref="ValidationResult"/>.</returns>
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                var pd = new Dictionary<string, string>();

                foreach (var p in option.Values)
                {
                    string[] parts = CreateKeyValueParts(p);
                    if (parts.Length != 2)
                        return new ValidationResult($"The parameter '{p}' is not valid; must be formatted as Name=value.");

                    if (pd.ContainsKey(parts[0]))
                        return new ValidationResult($"The parameter '{p}' is not valid; name has been specified more than once.");

                    pd.Add(parts[0], parts[1]);
                }

                return ValidationResult.Success;
            }
        }

        /// <summary>
        /// Validates the assembly name(s).
        /// </summary>
        public class AssemblyValidator : IOptionValidator
        {
            /// <summary>
            /// Initilizes a new instance of the <see cref="AssemblyValidator"/> class.
            /// </summary>
            /// <param name="assemblies">The assemblies list to update.</param>
            public AssemblyValidator(List<Assembly> assemblies) => Assemblies = assemblies;

            /// <summary>
            /// Gets the list of assemblies.
            /// </summary>
            public List<Assembly> Assemblies { get; private set; }

            /// <summary>
            /// Performs the validation.
            /// </summary>
            /// <param name="option">The <see cref="CommandOption"/>.</param>
            /// <param name="context">The <see cref="ValidationContext"/>.</param>
            /// <returns>The <see cref="ValidationResult"/>.</returns>
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                foreach (var name in option.Values)
                {
                    try
                    {
                        Assemblies.Add(Assembly.Load(name));
                    }
                    catch (Exception ex)
                    {
                        return new ValidationResult($"The specified assembly '{name}' is invalid: {ex.Message}");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #endregion

        private readonly CommandArgument _configArg;
        private readonly CommandOption _scriptOpt;
        private readonly CommandOption _templateOpt;
        private readonly CommandOption _outputOpt;
        private readonly CommandOption _assembliesOpt;
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly CommandOption _paramsOpt;

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
            App = new CommandLineApplication(false)
            {
                Name = "beef.codegen",
                Description = "Business Entity Execution Framework (Beef) Code Generator."
            };

            App.HelpOption(true);

            _configArg = App.Argument("config", "CodeGeneration configuration XML file.")
                .IsRequired()
                .Accepts(v => v.ExistingFile());

            _scriptOpt = App.Option("-s|--script", "Execution script file or embedded resource name.", CommandOptionType.SingleValue)
                .IsRequired()
                .Accepts(v => v.Use(new FileResourceValidator()));

            _templateOpt = App.Option("-t|--template", "Templates path (defaults to embedded resources).", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _outputOpt = App.Option("-o|--output", "Output path (defaults to current path).", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            _assembliesOpt = App.Option("-a|--assembly", "Assembly name containing scripts (multiple can be specified).", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new AssemblyValidator(_assemblies)));

            _paramsOpt = App.Option("-p|--param", "Name=Value pair(s) passed into code generation.", CommandOptionType.MultipleValue)
                .Accepts(v => v.Use(new ParamsValidator()));

            App.OnExecute(() =>
            {
                // Check the Execution script file or embedded resource names.
                if (!File.Exists(_scriptOpt.Value()) && ResourceManager.GetScriptContent(_scriptOpt.Value()) == null)
                    throw new InvalidOperationException($"The file or embedded resource '{_scriptOpt.Value()}' does not exist.");

                return RunRunAwayAsync();
            });
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
        public int Run(string args = null)
        {
            if (string.IsNullOrEmpty(args))
                return Run(new string[0]);

            // See for inspiration: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
            var regex = Regex.Matches(args, @"\G(""((""""|[^""])+)""|(\S+)) *");
            var array = regex.Cast<Match>()
                         .Select(m => Regex.Replace(
                             m.Groups[2].Success
                                 ? m.Groups[2].Value
                                 : m.Groups[4].Value, @"""""", @"""")).ToArray();

            return Run(array);
        }

        /// <summary>
        /// Runs the code generation using the passed array of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public int Run(string[] args)
        {
            SetupExecutionContext();

            try
            {
                return App.Execute(args);
            }
            catch (CommandParsingException cpex)
            {
                Logger.Default.Error(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Creates <see cref="ExecutionManager"/> and coordinates the run (overall execution).
        /// </summary>
        private async Task<int> RunRunAwayAsync() /* Inspired by https://www.youtube.com/watch?v=ikMiQZF-mAY */
        {
            var args = new CodeGenExecutorArgs
            {
                ConfigFile = new FileInfo(_configArg.Value),
                ScriptFile = new FileInfo(_scriptOpt.Value()),
                TemplatePath = _templateOpt.HasValue() ? new DirectoryInfo(_templateOpt.Value()) : null,
                OutputPath = new DirectoryInfo(_outputOpt.HasValue() ? _outputOpt.Value() : Environment.CurrentDirectory),
                Assemblies = _assemblies,
                Parameters = CreateParamDict(_paramsOpt)
            };

            WriteHeader(args);

            var em = ExecutionManager.Create(() => new CodeGenExecutor(args));
            var sw = Stopwatch.StartNew();
            
            await em.RunAsync();

            sw.Stop();
            WriteFooter(sw);
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
        /// Writes the specified text to the console.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="foregroundColor">The foreground <see cref="ConsoleColor"/>.</param>
        private static void ConsoleWriteLine(string text = null, ConsoleColor? foregroundColor = null)
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

        /// <summary>
        /// Creates a param (name=value) pair dictionary from the command option values.
        /// </summary>
        public static Dictionary<string, string> CreateParamDict(CommandOption cmdOpt)
        {
            var pd = new Dictionary<string, string>();
            foreach (var p in cmdOpt.Values)
            {
                string[] parts = CreateKeyValueParts(p);
                pd.Add(parts[0], parts[1]);
            }

            return pd;
        }

        /// <summary>
        /// Creates Key=Value parts.
        /// </summary>
        private static string[] CreateKeyValueParts(string text)
        {
            var pos = text.IndexOf("=");
            if (pos < 0)
                return new string[0];

            return new string[] { text.Substring(0, pos), text.Substring(pos + 1) };
        }

        /// <summary>
        /// Writes the header information.
        /// </summary>
        private void WriteHeader(CodeGenExecutorArgs args) //DirectoryInfo outputDir, Dictionary<string, string> paramDict)
        {
            Logger.Default.Info(App.Description);
            Logger.Default.Info(null);
            LogCodeGenExecutionArgs(args);
            Logger.Default.Info(null);
        }

        /// <summary>
        /// Logs (writes) the <see cref="CodeGenExecutorArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGenExecutorArgs"/>.</param>
        /// <param name="paramsOnly">Indicates whether to log on the parameters collection only.</param>
        public static void LogCodeGenExecutionArgs(CodeGenExecutorArgs args, bool paramsOnly = false)
        {
            if (!paramsOnly)
            {
                Logger.Default.Info($"  Config = {args.ConfigFile?.Name}");
                Logger.Default.Info($"  Script = {(args.ScriptFile.Exists ? args.ScriptFile?.FullName : args.ScriptFile?.Name)}");
                Logger.Default.Info($"  Template = {args.TemplatePath?.FullName}");
                Logger.Default.Info($"  Output = {args.OutputPath?.FullName}");
            }

            Logger.Default.Info($"  Params{(args.Parameters.Count == 0 ? " = none" : ":")}");

            foreach (var p in args.Parameters)
            {
                Console.WriteLine($"    {p.Key} = {p.Value}");
            }
        }

        /// <summary>
        /// Write the footer information.
        /// </summary>
        private static void WriteFooter(Stopwatch sw)
        {
            Logger.Default.Info(null);
            Logger.Default.Info($"CodeGen complete [{sw.ElapsedMilliseconds}ms].");
            Logger.Default.Info(null);
        }
    }
}