﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.Data.Database;
using Beef.Database.Core.Sql;
using Beef.Diagnostics;
using Beef.Executors;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Database.Core
{
    /// <summary>
    /// Represents the database executor.
    /// </summary>
    public class DatabaseExecutor : ExecutorBase
    {
        /// <summary>
        /// Gets or sets the <b>Migrations</b> scripts namespace part name.
        /// </summary>
        public static string MigrationsNamespace { get; set; } = "Migrations";

        /// <summary>
        /// Gets or sets the <b>Schema</b> namespace part name.
        /// </summary>
        public static string SchemaNamespace { get; set; } = "Schema";

        /// <summary>
        /// Gets or sets the <b>Data</b> namespace part name.
        /// </summary>
        public static string DataNamespace { get; set; } = "Data";

        private readonly DatabaseExecutorCommand _command;
        private readonly string _connectionString;
        private readonly Assembly[] _assemblies;
        private readonly List<string> _namespaces = new List<string>();
        private readonly CodeGenExecutorArgs _codeGenArgs;

        /// <summary>
        /// Represents a DbUp to Beef Logger sink.
        /// </summary>
        private class LoggerSink : IUpgradeLog
        {
            public void WriteError(string format, params object[] args) => Logger.Default.Error(format, args);

            public void WriteInformation(string format, params object[] args) => Logger.Default.Info(format, args);

            public void WriteWarning(string format, params object[] args) => Logger.Default.Warning(format, args);
        }

        /// <summary>
        /// Represents the SQL schema script object.
        /// </summary>
        private class SqlSchemaScript
        {
            public string Name;
            public SqlObjectReader Reader;
            public int Order;
            public string FileName;
        }

        /// <summary>
        /// Represents the Database being upgraded.
        /// </summary>
        private class Db : Database<Db>
        {
            public Db(string cs) : base(cs) { }
        }

        private readonly Db _db;

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <param name="codeGenArgs">The <see cref="DatabaseExecutorCommand.CodeGen"/> arguments.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static int Run(DatabaseExecutorCommand command, string connectionString, Assembly[] assemblies, CodeGenExecutorArgs codeGenArgs = null)
        {
            using (var em = ExecutionManager.Create(() => new DatabaseExecutor(command, connectionString, assemblies, codeGenArgs)))
            {
                return HandleRunResult(em.Run());
            }
        }

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static int Run(DatabaseExecutorCommand command, string connectionString, params Assembly[] assemblies)
        {
            using (var em = ExecutionManager.Create(() => new DatabaseExecutor(command, connectionString, assemblies, null)))
            {
                return HandleRunResult(em.Run());
            }
        }

        /// <summary>
        /// Handles the execution manager run result.
        /// </summary>
        private static int HandleRunResult(ExecutionManager em)
        {
            if (em.HadExecutionException)
                throw new InvalidOperationException($"Database executor failed with an unhandled exception: {em.ExecutionException.Message}", em.ExecutionException);

            return em.LastExecutor == null ? -1 : em.LastExecutor.ReturnCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutor"/> class.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <param name="codeGenArgs">The <see cref="DatabaseExecutorCommand.CodeGen"/> arguments.</param>
        public DatabaseExecutor(DatabaseExecutorCommand command, string connectionString, Assembly[] assemblies, CodeGenExecutorArgs codeGenArgs = null)
        {
            _command = command;
            _connectionString = Check.NotEmpty(connectionString, nameof(connectionString));
            _assemblies = assemblies;

            Check.IsFalse(_command.HasFlag(DatabaseExecutorCommand.CodeGen) && codeGenArgs == null, nameof(codeGenArgs), "The code generation arguments must be provided when the 'command' includes 'CodeGen'.");
            _codeGenArgs = codeGenArgs;
            if (_codeGenArgs != null && !_codeGenArgs.Parameters.ContainsKey("ConnectionString"))
                _codeGenArgs.Parameters.Add("ConnectionString", _connectionString);

            _db = new Db(_connectionString);

            _assemblies.ForEach(ass => _namespaces.Add(ass.GetName().Name));
            ReturnCode = -1;
        }

        /// <summary>
        /// Execute the database upgrade.
        /// </summary>
        protected override Task OnRunAsync(ExecutorRunArgs args)
        {
            var ls = new LoggerSink();

            if (_command.HasFlag(DatabaseExecutorCommand.Drop))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB DROP: Checking database existence and dropping where found...");
                TimeExecution(() => { DropDatabase.For.SqlDatabase(_connectionString, ls); return true; });
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Create))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB CREATE: Checking database existence and creating where not found...");
                TimeExecution(() => { EnsureDatabase.For.SqlDatabase(_connectionString, ls); return true; });
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Migrate))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB MIGRATE: Migrating the database...");
                Logger.Default.Info($"Probing for embedded resources: {(String.Join(", ", GetNamespacesWithSuffix($"{MigrationsNamespace}.*.sql")))}");

                DatabaseUpgradeResult result = null;
                TimeExecution(() =>
                {
                    result = DeployChanges.To
                        .SqlDatabase(_connectionString)
                        .WithScriptsEmbeddedInAssemblies(_assemblies, x => ScriptsNamespaceFilter(x))
                        .WithTransactionPerScript()
                        .LogTo(ls)
                        .Build()
                        .PerformUpgrade();

                    return result.Successful;
                });

                if (!result.Successful)
                {
                    Logger.Default.Exception(result.Error);
                    return Task.CompletedTask;
                }
            }

            if (_command.HasFlag(DatabaseExecutorCommand.CodeGen))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB CODEGEN: Code-gen database objects...");
                CodeGenConsole.LogCodeGenExecutionArgs(_codeGenArgs);

                if (!TimeExecution(() =>
                {
                    var em = ExecutionManager.Create(() => new CodeGenExecutor(_codeGenArgs)).Run();
                    return em.StopExecutor?.Exception == null;
                }))
                    return Task.CompletedTask;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Schema))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB OBJECTS: Drops and creates the database objects...");

                if (!TimeExecution(() => DropAndCreateAllObjects(new string[] { "dbo", "Ref" })))
                    return Task.CompletedTask;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Reset))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB OBJECTS: Drops and creates the database objects...");

                if (!TimeExecution(() => DeleteAllAndResetIdent()))
                    return Task.CompletedTask;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Data))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB DATA: Insert or merge the embedded YAML data...");

                if (!TimeExecution(() => InsertOrMergeYamlData()))
                    return Task.CompletedTask;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.ScriptNew))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB SCRIPTNEW: Creating a new SQL script from embedded template...");

                if (!TimeExecution(() => CreateScriptNew()))
                    return Task.CompletedTask;
            }

            ReturnCode = 0;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Times the execution and reports result.
        /// </summary>
        private bool TimeExecution(Func<bool> action)
        {
            var sw = Stopwatch.StartNew();
            var result = action();
            sw.Stop();
            Logger.Default.Info($"Complete [{sw.ElapsedMilliseconds}ms].");
            return result;
        }

        /// <summary>
        /// Filters by the <see cref="MigrationsNamespace"/>.
        /// </summary>
        private bool ScriptsNamespaceFilter(string name)
        {
            return _namespaces.Any(x => name.StartsWith(x + $".{MigrationsNamespace}.", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Gets the namespaces with the namespace suffix applied.
        /// </summary>
        private string[] GetNamespacesWithSuffix(string suffix)
        {
            var list = new List<string>();
            _namespaces.ForEach(ns => list.Add(ns + "." + suffix));
            return list.ToArray();
        }

        /// <summary>
        /// Drops and/or Alter and/or Create Objects.
        /// </summary>
        private bool DropAndCreateAllObjects(string[] schemaOrder)
        {
            var list = new List<SqlSchemaScript>();

            // See if there are any files out there (recently generated).
            if (_codeGenArgs?.OutputPath != null)
            {
                Logger.Default.Info($"Probing for files: '{_codeGenArgs.OutputPath.FullName}*.sql'");
                foreach (var ns in _namespaces)
                {
                    var di = new DirectoryInfo(Path.Combine(_codeGenArgs.OutputPath.FullName, ns, SchemaNamespace));
                    if (di.Exists)
                    {
                        foreach (var fi in di.GetFiles("*.sql", SearchOption.AllDirectories))
                        {
                            var name = RenameFileToResourceName(fi);
                            var sor = SqlObjectReader.Read(fi.OpenRead());
                            if (!sor.IsValid)
                            {
                                Logger.Default.Error($"SQL object '{name}' is not considered valid: {sor.ErrorMessage}");
                                return false;
                            }

                            var sr = new SqlSchemaScript { Name = name, Reader = sor, FileName = fi.FullName.Substring(_codeGenArgs.OutputPath.FullName.Length + 1) };
                            sr.Order = Array.IndexOf(schemaOrder, sr.Reader.Schema);
                            if (sr.Order < 0)
                                sr.Order = schemaOrder.Length;

                            list.Add(sr);
                        }
                    }
                }
            }

            // Parse all resources and get ready for the SQL code gen.
            Logger.Default.Info($"Probing for embedded resources: {string.Join(", ", GetNamespacesWithSuffix($"{SchemaNamespace}.*.sql"))}");
            foreach (var ass in _assemblies)
            {
                foreach (var name in ass.GetManifestResourceNames())
                {
                    // Filter on suffix on: '.sql'.
                    if (!_namespaces.Any(x => name.StartsWith(x + $".{SchemaNamespace}.", StringComparison.InvariantCulture) && name.EndsWith(".sql", StringComparison.InvariantCulture)))
                        continue;

                    // Filter out any picked up from file system probe above.
                    if (list.Any(x => x.Name == name))
                        continue;

                    // Read from embedded resource and add.
                    var sor = SqlObjectReader.Read(ass.GetManifestResourceStream(name));
                    if (!sor.IsValid)
                    {
                        Logger.Default.Error($"SQL object '{name}' is not considered valid: {sor.ErrorMessage}");
                        return false;
                    }

                    var sr = new SqlSchemaScript { Name = name, Reader = sor };
                    sr.Order = Array.IndexOf(schemaOrder, sr.Reader.Schema);
                    if (sr.Order < 0)
                        sr.Order = schemaOrder.Length;

                    list.Add(sr);
                }
            }

            if (list.Count == 0)
            {
                Logger.Default.Info($"Nothing found.");
                return true;
            }

            // Drop all existing (in reverse order).
            var sb = new StringBuilder();
            foreach (var sr in list.OrderByDescending(x => x.Order).ThenByDescending(x => x.Reader.Order).ThenByDescending(x => x.Name))
            {
                sb.AppendLine($"DROP {sr.Reader.Type} IF EXISTS [{sr.Reader.Schema}].[{sr.Reader.Name}]");
            }

            if (!ExecuteSqlStatement(() => _db.SqlStatement(sb.ToString()).NonQuery(), "the drop of all existing (known) database objects."))
                return false;

            // Execute each script one-by-one.
            foreach (var sr in list.OrderBy(x => x.Order).ThenBy(x => x.Reader.Order).ThenBy(x => x.Name))
            {
                if (!ExecuteSqlStatement(() => _db.SqlStatement(sr.Reader.GetSql()).NonQuery(), $"{(sr.FileName == null ? "resource" : "file")} {(sr.FileName ?? sr.Name)}"))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Rename file to resource name format.
        /// </summary>
        private string RenameFileToResourceName(FileInfo fi)
        {
            var dir = RenameFileToResourceNameReplace(fi.DirectoryName.Substring(_codeGenArgs.OutputPath.FullName.Length + 1));
            var file = RenameFileToResourceNameReplace(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
            return dir + "." + file + RenameFileToResourceNameReplace(fi.Extension);
        }

        /// <summary>
        /// Replace the special characters to convert filename to resource name.
        /// </summary>
        private static string RenameFileToResourceNameReplace(string text)
        {
            return text.Replace(' ', '_').Replace('-', '_').Replace('\\', '.').Replace('/', '.');
        }

        /// <summary>
        /// Wraps the SQL statement(s) and reports success or failure.
        /// </summary>
        private static bool ExecuteSqlStatement(Action action, string text)
        {
            try
            {
                Logger.Default.Info($"Executing {text}");
                action();
                return true;
            }
            catch (DbException dex)
            {
                Logger.Default.Error($"Execution failed with: {dex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete all data from all tables and reset any identity (IDENT) columns to zero.
        /// </summary>
        private bool DeleteAllAndResetIdent()
        {
            using (var st = typeof(DatabaseExecutor).Assembly.GetManifestResourceStream("Beef.Database.Core.Resources.DeleteAllAndResetIdent.sql"))
            using (var sr = new StreamReader(st))
            {
                return ExecuteSqlStatement(() => _db.SqlStatement(sr.ReadToEnd()).NonQuery(), "the deletion of all data from all tables (excluding 'dbo' schema); then resetting any identity columns to zero.");
            }
        }

        /// <summary>
        /// Inserts or merges the embedded YAML data.
        /// </summary>
        private bool InsertOrMergeYamlData()
        {
            // Get all the database table schema information.
            Logger.Default.Info($"Querying database for all existing table and column configurations...");
            SqlDataUpdater.RegisterDatabase(_db, "Ref");

            // Parse all resources and get ready for the SQL code gen.
            Logger.Default.Info($"Probing for embedded resources: {(String.Join(", ", GetNamespacesWithSuffix($"{DataNamespace}.*.yaml")))}");
            foreach (var ass in _assemblies)
            {
                foreach (var name in ass.GetManifestResourceNames())
                {
                    if (!_namespaces.Any(x => name.StartsWith(x + $".{DataNamespace}.", StringComparison.InvariantCulture) && name.EndsWith(".yaml", StringComparison.InvariantCulture)))
                        continue;

                    Logger.Default.Info($"Parsing and executing: {name}");
                    var sdm = SqlDataUpdater.ReadYaml(ass.GetManifestResourceStream(name));
                    sdm.GenerateSql((a) =>
                    {
                        Logger.Default.Info($"Executing: {a.OutputFileName} ->");
                        Logger.Default.Info(a.Content);
                        _db.SqlStatement(a.Content).NonQuery();
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the new script from the template.
        /// </summary>
        private bool CreateScriptNew()
        {
            _codeGenArgs.Parameters.TryGetValue("ScriptNew", out var action);
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var fi = string.IsNullOrEmpty(action)
                ? new FileInfo(Path.Combine(di.FullName, MigrationsNamespace, $"{DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture)}-comment-text.sql"))
                : new FileInfo(Path.Combine(di.FullName, MigrationsNamespace,
#pragma warning disable CA1308 // Normalize strings to uppercase; by-design as lowercase is desired.
                    $"{DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture)}-{action.ToLowerInvariant()}-{(_codeGenArgs.Parameters.TryGetValue("Schema", out var schema) ? schema : "schema")}-{(_codeGenArgs.Parameters.TryGetValue("Table", out var table) ? table : "table")}.sql"));
#pragma warning restore CA1308 

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            using (var sr = (new StringReader("<CodeGeneration />")))
            {
                var cg = CodeGenerator.Create(System.Xml.Linq.XElement.Load(sr));
                cg.CopyParameters(_codeGenArgs.Parameters);
                cg.CodeGenerated += (s, e) =>
                {
                    File.WriteAllText(fi.FullName, e.Content);
                };

                using (var st = typeof(DatabaseExecutor).Assembly.GetManifestResourceStream("Beef.Database.Core.Resources.ScriptNew_sql.xml"))
                {
                    cg.Generate(System.Xml.Linq.XElement.Load(st));
                }

                Logger.Default.Info($"Script file created: {fi.FullName}");
                return true;
            }
        }
    }
}