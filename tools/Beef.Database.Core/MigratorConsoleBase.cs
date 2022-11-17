// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using OnRamp.Console;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Beef.Database
{
    /// <summary>
    /// Enables the base <i>Beef</i> console including extenfed command-line arguments/options.
    /// </summary>
    /// <typeparam name="TSelf">The <see cref="Type"/> itself.</typeparam>
    public abstract class MigrationConsoleBase<TSelf> : DbEx.Console.MigrationConsoleBase<TSelf> where TSelf : MigrationConsoleBase<TSelf>
    {
        /// <summary>
        /// Gets the default database script name.
        /// </summary>
        public const string DefaultDatabaseScript = "Database.yaml";

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationConsoleBase{TSelf}"/> class.
        /// </summary>
        /// <param name="args">The default <see cref="MigrationArgs"/> that will be overridden/updated by the command-line argument values.</param>
        protected MigrationConsoleBase(MigrationArgs? args = null) : base(args ?? new MigrationArgs()) 
        {
            if (string.IsNullOrEmpty(Args.ScriptFileName))
                Args.ScriptFileName = DefaultDatabaseScript;

            // http://www.patorjk.com/software/taag/#p=display&f=Calvin%20S&t=Beef%20Database%20Tool%0A
            MastheadText = @"
╔╗ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬  
╠╩╗├┤ ├┤ ├┤    ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││  
╚═╝└─┘└─┘└    ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘
";

            Args.CreateConnectionStringEnvironmentVariableName ??= csargs => $"{Args.GetCompany()?.Replace(".", "_", StringComparison.InvariantCulture)}_{Args.GetAppName()?.Replace(".", "_", StringComparison.InvariantCulture)}_ConnectionString";
        }

        /// <summary>
        /// Gets the <see cref="MigrationArgs"/>.
        /// </summary>
        public new MigrationArgs Args => (MigrationArgs)base.Args;

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name.
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public TSelf DatabaseScript(string script)
        {
            Args.ScriptFileName = script ?? throw new ArgumentNullException(nameof(script));
            return (TSelf)this;
        }

        /// <inheritdoc/>
        protected override void OnBeforeExecute(CommandLineApplication app)
        {
            ConsoleOptions.Add(nameof(MigrationArgs.ScriptFileName), app.Option("-s|--script", "Script orchestration file name. [CodeGen]", CommandOptionType.SingleValue));
            ConsoleOptions.Add(nameof(MigrationArgs.ConfigFileName), app.Option("-c|--config", "Configuration data file name. [CodeGen]", CommandOptionType.SingleValue));
            ConsoleOptions.Add(nameof(MigrationArgs.Parameters), app.Option("-p|--param", "Parameter expressed as a 'Name=Value' pair (multiple can be specified). [CodeGen]", CommandOptionType.MultipleValue));
            ConsoleOptions.Add(nameof(MigrationArgs.ExpectNoChanges), app.Option("-enc|--expect-no-changes", "Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline). [CodeGen]", CommandOptionType.NoValue));
            ConsoleOptions.Add(nameof(MigrationArgs.IsSimulation), app.Option("-sim|--simulation", "Indicates whether the code-generation is a simulation (i.e. does not update the artefacts). [CodeGen]", CommandOptionType.NoValue));
        }

        /// <inheritdoc/>
        protected override ValidationResult? OnValidation(ValidationContext context)
        {
            UpdateStringOption(nameof(MigrationArgs.ScriptFileName), v => Args.ScriptFileName = v);
            UpdateStringOption(nameof(MigrationArgs.ConfigFileName), v => Args.ConfigFileName = v);
            UpdateBooleanOption(nameof(MigrationArgs.ExpectNoChanges), () => Args.ExpectNoChanges = true);
            UpdateBooleanOption(nameof(MigrationArgs.IsSimulation), () => Args.IsSimulation = true);

            var vr = new ParametersValidator(Args).GetValidationResult(GetCommandOption(nameof(MigrationArgs.Parameters)), context);
            if (vr != ValidationResult.Success)
                return vr;

            if (Args.OutputDirectory == null)
                Args.OutputDirectory = Args.MigrationCommand == DbEx.MigrationCommand.Script
                    ? new DirectoryInfo(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory())
                    : new DirectoryInfo(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory()).Parent;

            return ValidationResult.Success;
        }

        /// <inheritdoc/>
        protected override void OnWriteArgs(DbEx.Migration.DatabaseMigrationBase migrator) => WriteStandardizedArgs(migrator, logger =>
        {
            logger?.LogInformation("{Content}", $"Parameters{(Args.Parameters.Count == 0 ? " = none" : ":")}");
            foreach (var p in Args.Parameters)
            {
                logger?.LogInformation("{Content}", $"  {p.Key} = {p.Value}");
            }
        });
    }
}