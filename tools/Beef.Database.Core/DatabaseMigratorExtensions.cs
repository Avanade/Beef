// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using DbEx;
using DbEx.DbSchema;
using DbEx.Migration;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Likely to support in the future.")]
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

        /// <summary>
        /// Performs the database entity YAML code-generation execution.
        /// </summary>
        /// <param name="migrator">The <see cref="DatabaseMigrationBase"/>.</param>
        /// <param name="schema">The schema name (where supported).</param>
        /// <param name="tables">The table name array.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><c>true</c> indicates success; otherwise, <c>false</c>. Additionally, on success the code-generation file will be returned.</returns>
        public static async Task<(bool Success, string? Statistics)> ExecuteYamlCodeGenAsync(this DatabaseMigrationBase migrator, string? schema, string[] tables, CancellationToken cancellationToken = default)
        {
            // Infer database schema.
            migrator.Logger.LogInformation("{Content}", "  Querying database to infer table(s)/column(s) schema...");
            var dbTables = await migrator.Database.SelectSchemaAsync(migrator.DatabaseSchemaConfig, migrator.Args.DataParserArgs, cancellationToken).ConfigureAwait(false);

            var data = new { Tables = new List<DbTableSchema>() };
            foreach (var table in tables)
            {
                var dbTable = dbTables.FirstOrDefault(x => x.Schema == schema && x.Name == table);
                if (dbTable is null)
                {
                    migrator.Logger.LogError("{Content}", (schema is null) ? $"Code-generation for entity 'YAML' table '{table}' not found in database." : $"Code-generation for entity 'YAML' schema '{schema}' and '{table}' not found in database.");
                    return (false, null);
                }

                data.Tables.Add(dbTable);
            }

            // Find the resource.
            using var sr = StreamLocator.GetTemplateStreamReader("Entity_yaml", migrator.ArtefactResourceAssemblies.ToArray(), StreamLocator.HandlebarsExtensions).StreamReader 
                ?? throw new InvalidOperationException("Embedded Template resource 'Entity_yaml' is required and was not found within the selected assemblies.");

            // Set the filename.
            if (migrator.Args.OutputDirectory == null)
                throw new InvalidOperationException("Args.OutputDirectory has not been correctly determined.");

            var fn = Path.Combine(migrator.Args.OutputDirectory.FullName, "temp.entity.beef-5.yaml");
            var fi = new FileInfo(fn);

            // Execute the code-generation and save.
            var cg = new HandlebarsCodeGenerator(sr);
            var yaml = cg.Generate(data);
            await File.WriteAllTextAsync(fi.FullName, new HandlebarsCodeGenerator(sr).Generate(data), cancellationToken).ConfigureAwait(false);

            migrator.Logger.LogWarning("{Content}", $"Script file created: {fi.FullName}");
            return (true, string.Empty);
        }
    }
}