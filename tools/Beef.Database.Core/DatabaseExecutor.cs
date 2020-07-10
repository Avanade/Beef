// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public class DatabaseExecutor : ExecutorBase
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
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
            public string? Name;
            public SqlObjectReader? Reader;
            public int? Order;
            public string? FileName;
        }

        /// <summary>
        /// Represents the Database being upgraded.
        /// </summary>
        private class Db : DatabaseBase
        {
            public Db(string cs) : base(cs) { }
        }

        private readonly Db _db;

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard <i>Beef</i> <b>dbo</b> schema objects.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <param name="codeGenArgs">The <see cref="DatabaseExecutorCommand.CodeGen"/> arguments.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static async Task<int> RunAsync(DatabaseExecutorCommand command, string connectionString, bool useBeefDbo, Assembly[] assemblies, CodeGenExecutorArgs? codeGenArgs = null)
        {
            using var em = ExecutionManager.Create(() => new DatabaseExecutor(command, connectionString, useBeefDbo ? assemblies.Prepend(typeof(DatabaseExecutor).Assembly).ToArray() : assemblies, codeGenArgs));
            await em.RunAsync().ConfigureAwait(false);
            return HandleRunResult(em);
        }

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard <i>Beef</i> <b>dbo</b> schema objects.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static async Task<int> RunAsync(DatabaseExecutorCommand command, string connectionString, bool useBeefDbo, params Assembly[] assemblies)
        {
            using var em = ExecutionManager.Create(() => new DatabaseExecutor(command, connectionString, useBeefDbo ? assemblies.Prepend(typeof(DatabaseExecutor).Assembly).ToArray() : assemblies, null!));
            await em.RunAsync().ConfigureAwait(false);
            return HandleRunResult(em);
        }

        /// <summary>
        /// Handles the execution manager run result.
        /// </summary>
        private static int HandleRunResult(ExecutionManager em)
        {
            if (em.HadExecutionException)
                throw new InvalidOperationException($"Database executor failed with an unhandled exception: {em.ExecutionException!.Message}", em.ExecutionException);

            return em.LastExecutor == null ? -1 : em.LastExecutor.ReturnCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutor"/> class.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        /// <param name="codeGenArgs">The <see cref="DatabaseExecutorCommand.CodeGen"/> arguments.</param>
        public DatabaseExecutor(DatabaseExecutorCommand command, string connectionString, Assembly[] assemblies, CodeGenExecutorArgs? codeGenArgs = null)
        {
            _command = command;
            _connectionString = Check.NotEmpty(connectionString, nameof(connectionString));
            _assemblies = assemblies;

            Check.IsFalse(_command.HasFlag(DatabaseExecutorCommand.CodeGen) && codeGenArgs == null, nameof(codeGenArgs), "The code generation arguments must be provided when the 'command' includes 'CodeGen'.");
            _codeGenArgs = codeGenArgs ?? new CodeGenExecutorArgs();
            if (_codeGenArgs != null && !_codeGenArgs.Parameters.ContainsKey("ConnectionString"))
                _codeGenArgs.Parameters.Add("ConnectionString", _connectionString);

            _db = new Db(_connectionString);

            _assemblies.ForEach(ass => _namespaces.Add(ass.GetName().Name!));
            ReturnCode = -1;
        }

        /// <summary>
        /// Execute the database upgrade.
        /// </summary>
        protected override async Task OnRunAsync(ExecutorRunArgs args)
        {
            var ls = new LoggerSink();

            if (_command.HasFlag(DatabaseExecutorCommand.Drop))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB DROP: Checking database existence and dropping where found...");
                await TimeExecutionAsync(() => { DropDatabase.For.SqlDatabase(_connectionString, ls); return Task.FromResult(true); }).ConfigureAwait(false);
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Create))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB CREATE: Checking database existence and creating where not found...");
                await TimeExecutionAsync(() => { EnsureDatabase.For.SqlDatabase(_connectionString, ls); return Task.FromResult(true); }).ConfigureAwait(false);
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Migrate))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB MIGRATE: Migrating the database...");
                Logger.Default.Info($"Probing for embedded resources: {(string.Join(", ", GetNamespacesWithSuffix($"{MigrationsNamespace}.*.sql")))}");

                DatabaseUpgradeResult? result = null;
                await TimeExecutionAsync(() =>
                {
                    result = DeployChanges.To
                        .SqlDatabase(_connectionString)
                        .WithScripts(GetMigrationScripts(_assemblies))
                        .WithoutTransaction()
                        .LogTo(ls)
                        .Build()
                        .PerformUpgrade();

                    return Task.FromResult(result.Successful);
                }).ConfigureAwait(false);

                if (!result!.Successful)
                {
                    Logger.Default.Exception(result.Error);
                    return;
                }
            }

            if (_command.HasFlag(DatabaseExecutorCommand.CodeGen))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB CODEGEN: Code-gen database objects...");
                CodeGenConsole.LogCodeGenExecutionArgs(_codeGenArgs);

                if (!await TimeExecutionAsync(async () =>
                {
                    var em = ExecutionManager.Create(() => new CodeGenExecutor(_codeGenArgs));
                    await em.RunAsync().ConfigureAwait(false);
                    return em.StopExecutor?.Exception == null;
                }).ConfigureAwait(false))
                {
                    return;
                }
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Schema))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB SCHEMA: Drops and creates the database objects...");

                if (!await TimeExecutionAsync(() => DropAndCreateAllObjectsAsync(new string[] { "dbo", "Ref" })).ConfigureAwait(false))
                    return;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Reset))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB RESET: Resets database by dropping data from all tables...");

                if (!await TimeExecutionAsync(() => DeleteAllAndResetAsync()).ConfigureAwait(false))
                    return;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.Data))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB DATA: Insert or merge the embedded YAML data...");

                if (!await TimeExecutionAsync(() => InsertOrMergeYamlDataAsync()).ConfigureAwait(false))
                    return;
            }

            if (_command.HasFlag(DatabaseExecutorCommand.ScriptNew))
            {
                Logger.Default.Info(string.Empty);
                Logger.Default.Info(new string('-', 80));
                Logger.Default.Info("DB SCRIPTNEW: Creating a new SQL script from embedded template...");

                if (!await TimeExecutionAsync(() => CreateScriptNewAsync()).ConfigureAwait(false))
                    return;
            }

            ReturnCode = 0;
        }

        /// <summary>
        /// Times the execution and reports result.
        /// </summary>
        private static async Task<bool> TimeExecutionAsync(Func<Task<bool>> action)
        {
            var sw = Stopwatch.StartNew();
            var result = await action().ConfigureAwait(false);
            sw.Stop();
            Logger.Default.Info($"Complete [{sw.ElapsedMilliseconds}ms].");
            return result;
        }

        /// <summary>
        /// Gets all the migration scripts from the assemblies and ensures order.
        /// </summary>
        private List<SqlScript> GetMigrationScripts(Assembly[] assemblies)
        {
            var scripts = new List<SqlScript>();
            var count = 0;

            foreach (var ass in assemblies)
            {
                foreach (var name in ass.GetManifestResourceNames().Where(x => ScriptsNamespaceFilter(x)))
                {
                    scripts.Add(SqlScript.FromStream(name, ass.GetManifestResourceStream(name), Encoding.Default, new SqlScriptOptions { RunGroupOrder = count, ScriptType = DbUp.Support.ScriptType.RunOnce }));
                }

                count++;
            }

            return scripts;
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
        private async Task<bool> DropAndCreateAllObjectsAsync(string[] schemaOrder)
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
                    var sor = SqlObjectReader.Read(ass.GetManifestResourceStream(name)!);
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
            foreach (var sr in list.OrderByDescending(x => x.Order).ThenByDescending(x => x.Reader!.Order).ThenByDescending(x => x.Name))
            {
                sb.AppendLine($"DROP {sr.Reader!.Type} IF EXISTS [{sr.Reader.Schema}].[{sr.Reader.Name}]");
            }

            if (!await ExecuteSqlStatementAsync(() => _db.SqlStatement(sb.ToString()).NonQueryAsync(), "the drop of all existing (known) database objects.").ConfigureAwait(false))
                return false;

            // Execute each script one-by-one.
            foreach (var sr in list.OrderBy(x => x.Order).ThenBy(x => x.Reader!.Order).ThenBy(x => x.Name))
            {
                if (!await ExecuteSqlStatementAsync(() => _db.SqlStatement(sr.Reader!.GetSql()).NonQueryAsync(), $"{(sr.FileName == null ? "resource" : "file")} {(sr.FileName ?? sr.Name)}").ConfigureAwait(false))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Rename file to resource name format.
        /// </summary>
        private string RenameFileToResourceName(FileInfo fi)
        {
            var dir = RenameFileToResourceNameReplace(fi.DirectoryName.Substring(_codeGenArgs.OutputPath!.FullName.Length + 1));
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
        private static async Task<bool> ExecuteSqlStatementAsync(Func<Task> func, string text)
        {
            try
            {
                Logger.Default.Info($"Executing {text}");
                await func().ConfigureAwait(false);
                return true;
            }
            catch (DbException dex)
            {
                Logger.Default.Error($"Execution failed with: {dex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete all data from all tables.
        /// </summary>
        private async Task<bool> DeleteAllAndResetAsync()
        {
            using var st = typeof(DatabaseExecutor).Assembly.GetManifestResourceStream("Beef.Database.Core.Resources.DeleteAllAndReset.sql")!;
            using var sr = new StreamReader(st);
            return await ExecuteSqlStatementAsync(() => _db.SqlStatement(sr.ReadToEnd()).NonQueryAsync(), "the deletion of all data from all tables (excluding 'dbo' schema).").ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts or merges the embedded YAML data.
        /// </summary>
        private async Task<bool> InsertOrMergeYamlDataAsync()
        {
            // Get all the database table schema information.
            Logger.Default.Info($"Querying database for all existing table and column configurations...");
            await SqlDataUpdater.RegisterDatabaseAsync(_db, "Ref").ConfigureAwait(false);

            // Parse all resources and get ready for the SQL code gen.
            Logger.Default.Info($"Probing for embedded resources: {(string.Join(", ", GetNamespacesWithSuffix($"{DataNamespace}.*.yaml")))}");
            foreach (var ass in _assemblies)
            {
                foreach (var name in ass.GetManifestResourceNames())
                {
                    if (!_namespaces.Any(x => name.StartsWith(x + $".{DataNamespace}.", StringComparison.InvariantCulture) && name.EndsWith(".yaml", StringComparison.InvariantCulture)))
                        continue;

                    Logger.Default.Info($"Parsing and executing: {name}");
                    var sdm = SqlDataUpdater.ReadYaml(ass.GetManifestResourceStream(name)!);
                    await sdm.GenerateSqlAsync((a) =>
                    {
                        Logger.Default.Info("");
                        Logger.Default.Info($"Executing: {a.OutputFileName} ->");
                        Logger.Default.Info(a.Content);
                        if (a.Content != null)
                        {
                            var rows = _db.SqlStatement(a.Content).ScalarAsync<int>().GetAwaiter().GetResult();
                            Logger.Default.Info($"Result: {rows} rows affected.");
                        }
                    }).ConfigureAwait(false);
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the new script from the template.
        /// </summary>
        private async Task<bool> CreateScriptNewAsync()
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

            using var sr = new StringReader("<CodeGeneration />");
            var cg = CodeGenerator.Create(System.Xml.Linq.XElement.Load(sr));
            cg.CopyParameters(_codeGenArgs.Parameters);
            cg.CodeGenerated += (s, e) =>
            {
                File.WriteAllText(fi.FullName, e.Content);
            };

            using (var st = typeof(DatabaseExecutor).Assembly.GetManifestResourceStream("Beef.Database.Core.Resources.ScriptNew_sql.xml"))
            {
                await cg.GenerateAsync(System.Xml.Linq.XElement.Load(st)).ConfigureAwait(false);
            }

            Logger.Default.Info($"Script file created: {fi.FullName}");
            return true;
        }
    }
}