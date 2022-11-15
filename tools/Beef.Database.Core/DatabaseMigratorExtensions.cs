// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using DbEx.Migration;
using Microsoft.Extensions.Logging;
using OnRamp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="DatabaseMigrationBase"/>.
    /// </summary>
    public static class DatabaseMigrationExtensions
    {
        /// <summary>
        /// Performs standard <see cref="DatabaseMigrationBase"/> initialization.
        /// </summary>
        /// <param name="migrator">The <see cref="DatabaseMigrationBase"/>.</param>
        public static void Initialization(this DatabaseMigrationBase migrator)
        {
            migrator.Args.DataParserArgs.RefDataColumnDefaults.TryAdd("IsActive", _ => true);
            migrator.Args.DataParserArgs.RefDataColumnDefaults.TryAdd("SortOrder", i => i);
        }

        /// <summary>
        /// Performs the database code-generation execution.
        /// </summary>
        /// <param name="migrator">The <see cref="DatabaseMigrationBase"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><c>true</c> indicates success; otherwise, <c>false</c>. Additionally, on success the code-generation statistics summary should be returned to append to the log.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Likely to suport in the future.")]
        public static async Task<(bool Success, string? Statistics)> ExecuteCodeGenAsync(this DatabaseMigrationBase migrator, CancellationToken cancellationToken = default)
        {
            var margs = (MigrationArgs)migrator.Args;

            var cga = new CodeGeneratorArgs();
            cga.CopyFrom(margs);
            cga.OutputDirectory = margs.OutputDirectory == null ? null : new DirectoryInfo(margs.OutputDirectory.FullName);
            cga.Logger = margs.Logger;
            cga.ExpectNoChanges = margs.ExpectNoChanges;
            cga.IsSimulation = margs.IsSimulation;
            cga.Assemblies.Add(typeof(DbEx.Console.MigrationConsoleBase).Assembly);
            cga.Assemblies.Add(typeof(Beef.CodeGen.CodeGenConsole).Assembly);
            cga.AddAssembly(margs.Assemblies.ToArray());
            cga.AddParameters(margs.Parameters);
            cga.ValidateCompanyAndAppName();
            cga.ScriptFileName = margs.ScriptFileName;
            cga.ConfigFileName = margs.ConfigFileName ?? CodeGenFileManager.GetConfigFilename(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory(), CommandType.Database, cga.GetCompany(true), cga.GetAppName(true));

            migrator.Logger.LogInformation("{Content}", string.Empty);
            OnRamp.Console.CodeGenConsole.WriteStandardizedArgs(cga);

            try
            {
                var stats = await CodeGenConsole.ExecuteCodeGenerationAsync(cga).ConfigureAwait(false);
                return (true, $", Files: Unchanged = {stats.NotChangedCount}, Updated = {stats.UpdatedCount}, Created = {stats.CreatedCount}, TotalLines = {stats.LinesOfCodeCount}");
            }
            catch (CodeGenException cgex)
            {
                migrator.Logger.LogError("{Content}", cgex.Message);
                migrator.Logger.LogInformation("{Content}", string.Empty);
                return (false, null);
            }
            catch (CodeGenChangesFoundException cgcfex)
            {
                migrator.Logger.LogError("{Content}", cgcfex.Message);
                migrator.Logger.LogInformation("{Content}", string.Empty);
                return (false, null);
            }
        }
    }
}