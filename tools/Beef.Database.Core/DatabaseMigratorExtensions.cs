// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using DbEx.Migration;
using Microsoft.Extensions.Logging;
using OnRamp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database
{
    /// <summary>
    /// Provides extension methods for the <see cref="DatabaseMigrationBase"/>.
    /// </summary>
    public static class DatabaseMigrationExtensions
    {
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

            // Walk the assembly hierarchy.
            var alist = new List<Assembly>();
            var maAss = typeof(MigrationArgs).Assembly;
            var type = migrator.GetType();
            do
            {
                if (!alist.Contains(type.Assembly))
                    alist.Add(type.Assembly);

                // Ensure the _Beef.Database.Core_ assembly is included, and within the correct order.
                if (type.Assembly.GetReferencedAssemblies().Any(x => x.Name == maAss.GetName().Name))
                {
                    if (!alist.Contains(maAss))
                        alist.Add(maAss);
                }

                type = type.BaseType!;
            } while (type != typeof(object));

            cga.AddAssembly(alist.ToArray());
            cga.AddAssembly(margs.Assemblies.ToArray());

            cga.AddParameters(margs.Parameters);
            cga.ValidateCompanyAndAppName();
            cga.ScriptFileName = margs.ScriptFileName;
            cga.ConfigFileName = margs.ConfigFileName ?? CodeGenFileManager.GetConfigFilename(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory(), CommandType.Database, cga.GetCompany(true), cga.GetAppName(true));
            cga.AddDatabaseMigrator(migrator);

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