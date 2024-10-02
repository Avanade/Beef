﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using DbEx;
using McMaster.Extensions.CommandLineUtils;
using OnRamp;
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
                Args.OutputDirectory = new DirectoryInfo(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory()).Parent;

            if (Args.MigrationCommand.HasFlag(MigrationCommand.CodeGen))
            {
                var p0 = Args.GetParameter<string>("Param0");
                if (p0 is not null)
                {
                    if (!p0.Equals("yaml", System.StringComparison.InvariantCultureIgnoreCase))
                        return new ValidationResult($"A '{nameof(MigrationCommand.CodeGen)}' command optionally supports a corresponding 'YAML' argument value only; '{p0}' is not supported.");

                    if (Args.MigrationCommand != MigrationCommand.CodeGen)
                        return new ValidationResult($"Code-generation for entity 'yaml' can only be used with the explicit usage of the '{nameof(MigrationCommand.CodeGen)}' command; '{Args.MigrationCommand}' is not supported.");
                }
            }

            return ValidationResult.Success;
        }
    }
}