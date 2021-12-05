// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.Diagnostics;
using DbEx.Migration;
using DbEx.Migration.SqlServer;
using Microsoft.Extensions.Logging;
using OnRamp;
using System;
using System.Threading.Tasks;

namespace Beef.Database.Core
{
    /// <summary>
    /// Represents the database executor.
    /// </summary>
    public class DatabaseExecutor : SqlServerMigrator
    {
        private readonly DatabaseExecutorArgs _args;
        private readonly bool _codeGen = false;

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseExecutorArgs"/>.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static async Task<int> RunAsync(DatabaseExecutorArgs args) => (await new DatabaseExecutor(args ?? throw new ArgumentNullException(nameof(args))).MigrateAsync().ConfigureAwait(false)) ? 0 : 1;

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DatabaseExecutor(DatabaseExecutorArgs args) : base(args.ConnectionString!, ConvertMigrationCommand(args.Command, args.SupportedCommands), args.Logger ?? new ColoredConsoleLogger(nameof(DatabaseConsole)), args.Assemblies.ToArray())
        {
            _args = args;
            if (_args.UseBeefDbo && !_args.Assemblies.Contains(typeof(DatabaseConsole).Assembly))
                _args.Assemblies.Insert(0, typeof(DatabaseConsole).Assembly);

            if (_args.Command.HasFlag(DatabaseExecutorCommand.CodeGen) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.CodeGen))
                _codeGen = true;

            ParserArgs.RefDataColumnDefaults.TryAdd("IsActive", _ => true);
            ParserArgs.RefDataColumnDefaults.TryAdd("SortOrder", i => i);
        }

        /// <summary>
        /// Converts the <see cref="DatabaseExecutorCommand"/> into the <see cref="MigrationCommand"/> equivalent.
        /// </summary>
        private static MigrationCommand ConvertMigrationCommand(DatabaseExecutorCommand cmd, DatabaseExecutorCommand supported)
        {
            var mc = MigrationCommand.None;
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Drop, MigrationCommand.Drop);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Create, MigrationCommand.Create);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Migrate, MigrationCommand.Migrate);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Schema, MigrationCommand.Schema);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Reset, MigrationCommand.Reset);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Data, MigrationCommand.Data);
            ConvertMigrationCommand(ref mc, cmd, supported, DatabaseExecutorCommand.Script, MigrationCommand.Script);

            return mc;
        }

        /// <summary>
        /// Converts the selected command item.
        /// </summary>
        private static void ConvertMigrationCommand(ref MigrationCommand mc, DatabaseExecutorCommand cmd, DatabaseExecutorCommand supported, DatabaseExecutorCommand ifCmd, MigrationCommand thenCmd)
        {
            if (cmd.HasFlag(ifCmd) && supported.HasFlag(ifCmd))
                mc |= thenCmd;
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnBeforeCommandAsync(MigrationCommand command, bool isSelected)
        {
            if (!_codeGen || command != MigrationCommand.Schema)
                return true;

            CodeGenStatistics stats = null!;

            return await CommandExecuteAsync("DATABASE CODEGEN: Code-gen database objects...", async () =>
            {
                var cga = new OnRamp.CodeGeneratorArgs();
                cga.CopyFrom(_args);
                cga.Assemblies.Add(typeof(Beef.CodeGen.CodeGenConsole).Assembly);
                cga.ConfigFileName ??= CodeGenFileManager.GetConfigFilename(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory(), CommandType.Database, _args.GetCompany(), _args.GetAppName());
                cga.ValidateCompanyAndAppName();

                _args.Logger?.LogInformation(string.Empty);
                OnRamp.Console.CodeGenConsole.WriteStandardizedArgs(cga);

                try
                {
                    stats = await CodeGenConsole.ExecuteCodeGenerationAsync(cga).ConfigureAwait(false);
                    return true;
                }
                catch (CodeGenException cgex)
                {
                    _args.Logger?.LogError(cgex.Message);
                    _args.Logger?.LogError(string.Empty);
                    return false;
                }
                catch (CodeGenChangesFoundException cgcfex) 
                {
                    _args.Logger?.LogError(cgcfex.Message);
                    _args.Logger?.LogError(string.Empty);
                    return false;
                }
            }, () => $", Files: Unchanged = {stats.NotChangedCount}, Updated = {stats.UpdatedCount}, Created = {stats.CreatedCount}, TotalLines = {stats.LinesOfCodeCount}").ConfigureAwait(false);
        }
    }
}