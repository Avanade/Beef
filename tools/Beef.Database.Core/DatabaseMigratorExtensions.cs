// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using DbEx;
using DbEx.DbSchema;
using DbEx.Migration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OnRamp;
using OnRamp.Utility;
using System;
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
            cga.AddAssembly(migrator.AdjustArtefactResourceAssemblies());
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
        /// Adjusts the <see cref="DatabaseMigrationBase.ArtefactResourceAssemblies"/> to ensure that the <see cref="MigrationArgs"/>-<see cref="Assembly"/> is included.
        /// </summary>
        /// <param name="migrator">The <see cref="DatabaseMigrationBase"/>.</param>
        /// <returns>The <see cref="DatabaseMigrationBase.ArtefactResourceAssemblies"/> as an array.</returns>
        public static Assembly[] AdjustArtefactResourceAssemblies(this DatabaseMigrationBase migrator)
        {
            var la = (List<Assembly>)migrator.ArtefactResourceAssemblies;
            var ma = typeof(MigrationArgs).Assembly;
            if (!la.Contains(ma))
            {
                var ta = migrator.GetType().Assembly;
                var i = la.IndexOf(ta);
                if (i >= 0)
                    la.Insert(i + 1, ma);
            }

            return la.ToArray();
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
            const string templateFileName = "Entity_yaml";
            const string tempFileName = "temp.entity.beef-5.yaml";

            // Infer database schema.
            migrator.Logger.LogInformation("{Content}", "  ** Code-generation of temporary entity YAML requested **");
            migrator.Logger.LogInformation("{Content}", "  Querying database to infer table(s)/column(s) schema...");
            var dbTables = await migrator.Database.SelectSchemaAsync(migrator.DatabaseSchemaConfig, migrator.Args.DataParserArgs, cancellationToken).ConfigureAwait(false);

            var data = new DbData();
            foreach (var table in tables)
            {
                var name = CheckTableNameOptions(table, out var includeCrud, out var includeGetByArgs);
                var dbTable = dbTables.FirstOrDefault(x => x.Schema == schema && x.Name == name);
                if (dbTable is null)
                {
                    migrator.Logger.LogError("{Content}", $"Specified table {migrator.DatabaseSchemaConfig.ToFullyQualifiedTableName(schema!, name)} not found in database.");
                    return (false, null);
                }

                if (!data.Tables.Any(x => x.QualifiedName == dbTable.QualifiedName))
                    data.Tables.Add(new DbTableSchemaEx(dbTable) { IncludeCrud = includeCrud, IncludeGetByArgs = includeGetByArgs });
            }

            // Begin code-gen tasks.
            migrator.Logger.LogInformation("{Content}", $"  Generating YAML for tables: {string.Join(", ", data.Tables.Select(x => x.QualifiedName))}");
            using var sr = StreamLocator.GetTemplateStreamReader(templateFileName, migrator.AdjustArtefactResourceAssemblies(), StreamLocator.HandlebarsExtensions).StreamReader 
                ?? throw new InvalidOperationException($"Embedded Template resource '{templateFileName}' is required and was not found within the selected assemblies.");

            // Set the path/filename.
            var fn = Path.Combine(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory(), tempFileName);
            var fi = new FileInfo(fn);

            // Execute the code-generation proper and write contents (new or overwrite).
            await File.WriteAllTextAsync(fi.FullName, new HandlebarsCodeGenerator(sr).Generate(data), cancellationToken).ConfigureAwait(false);

            // Done, boom!
            migrator.Logger.LogInformation("{Content}", string.Empty);
            migrator.Logger.LogWarning("{Content}", $"Temporary entity file created: {fi.FullName}");
            migrator.Logger.LogInformation("{Content}", string.Empty);
            migrator.Logger.LogInformation("{Content}", "Copy+paste generated contents into the respective 'entity.beef-5.yaml' and/or 'refdata.beef-5.yaml' files and amend accordingly.");
            migrator.Logger.LogInformation("{Content}", $"Once complete, it is recommended that the '{tempFileName}' file is deleted as it otherwise not used.");

            return (true, string.Empty);
        }

        /// <summary>
        /// Check the table name options by checking the first character.
        /// </summary>
        private static string CheckTableNameOptions(string name, out bool includeCrud, out bool includeGetByArgs)
        {
            includeCrud = true;
            includeGetByArgs = false;

            while (true)
            {
                if (name.Length == 0)
                    return name;

                switch (name[0])
                {
                    case '!':
                        includeCrud = false;
                        name = name[1..];
                        break;

                    case '*':
                        includeGetByArgs = true;
                        name = name[1..];
                        break;

                    default:
                        return name;
                }
            }
        }

        /// <summary>
        /// Provides the database data contents for code-generation.
        /// </summary>
        private class DbData
        {
            public List<DbTableSchemaEx> Tables { get; } = new();

            public List<DbTableSchemaEx> RefDataTables => Tables.Where(x => x.IsRefData).ToList();

            public List<DbTableSchemaEx> StandardTables => Tables.Where(x => !x.IsRefData).ToList();
        }

        /// <summary>
        /// Extend the <see cref="DbTableSchema"/> to support code-gen requirements.
        /// </summary>
        private class DbTableSchemaEx : DbTableSchema
        {
            public DbTableSchemaEx(DbTableSchema table) : base(table) { }

            public bool IncludeCrud { get; set; }

            public bool IncludeGetByArgs { get; set; }
        }
    }
}