// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.CodeGen.Generators;
using Beef.Data.Database;
using Beef.Database.Core.Sql;
using Beef.Diagnostics;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
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
    /// The <see cref="CodeGenExecutor"/> arguments.
    /// </summary>
    public class DatabaseExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutorArgs"/> class.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        public DatabaseExecutorArgs(DatabaseExecutorCommand command, string connectionString, params Assembly[] assemblies)
        {
            Command = command;
            ConnectionString = Check.NotNull(connectionString, nameof(connectionString));
            Assemblies = new List<Assembly>(assemblies);
        }

        /// <summary>
        /// Gets the <see cref="DatabaseExecutorCommand"/>.
        /// </summary>
        public DatabaseExecutorCommand Command { get; private set; }

        /// <summary>
        /// Gets or sets the support commands (<see cref="DatabaseExecutorCommand"/>).
        /// </summary>
        public DatabaseExecutorCommand SupportedCommands { get; set; } = DatabaseExecutorCommand.All;

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the <see cref="Assembly"/> list whose embedded resources will be probed.
        /// </summary>
        public List<Assembly> Assemblies { get; private set; }

        /// <summary>
        /// Indicates whether ot use the standard <i>Beef</i> <b>dbo</b> schema objects.
        /// </summary>
        public bool UseBeefDbo { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="DatabaseExecutorCommand.CodeGen"/> arguments.
        /// </summary>
        public CodeGenExecutorArgs? CodeGenArgs { get; set; }

        /// <summary>
        /// Gets the list of schemas in priority order (used to sequence the drop (reverse order) and create (specified order) of the database objects). Where <see cref="UseBeefDbo"/>
        /// is specified then <c>dbo</c> will automatically be inserted into first position (index zero) if not otherwise specified.
        /// </summary>
        public List<string?> SchemaOrder { get; private set; } = new List<string?>();

        /// <summary>
        /// Adds the <paramref name="schemas"/> to the <see cref="SchemaOrder"/>.
        /// </summary>
        /// <param name="schemas">The schemas.</param>
        /// <returns>The <see cref="DatabaseExecutorArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseExecutorArgs AddSchemaOrder(params string[] schemas)
        {
            SchemaOrder.AddRange(schemas);
            return this;
        }
    }

    /// <summary>
    /// Represents the database executor.
    /// </summary>
    public class DatabaseExecutor
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

        private readonly DatabaseExecutorArgs _args;
        private readonly List<string> _namespaces = new List<string>();
        private readonly ILogger _logger;

        /// <summary>
        /// Represents a DbUp to ILogger sink.
        /// </summary>
        private class LoggerSink : IUpgradeLog
        {
            private readonly ILogger _logger;

            public LoggerSink(ILogger logger) => _logger = Check.NotNull(logger, nameof(logger));

            public void WriteError(string format, params object[] args) => _logger.LogError(format, args);

            public void WriteInformation(string format, params object[] args) => _logger.LogInformation(format, args);

            public void WriteWarning(string format, params object[] args) => _logger.LogWarning(format, args);
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

        private Db? _db = null;

        /// <summary>
        /// Runs the <see cref="DatabaseExecutor"/> directly.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseExecutorArgs"/>.</param>
        /// <returns>The return code; zero equals success.</returns>
        public static async Task<int> RunAsync(DatabaseExecutorArgs args)
        {
            var de = new DatabaseExecutor(args);
            return await de.RunAsync().ConfigureAwait(false) ? 0 : -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutor"/> class.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseExecutorArgs"/>.</param>
        public DatabaseExecutor(DatabaseExecutorArgs args)
        {
            _args = Check.NotNull(args, nameof(args));
            Logger.Default = _logger = new ColoredConsoleLogger(nameof(DatabaseConsole));

            Check.IsFalse(_args.Command.HasFlag(DatabaseExecutorCommand.CodeGen) && _args.CodeGenArgs == null, nameof(args), "The code generation arguments must be provided when the 'command' includes 'CodeGen'.");
            if (_args.CodeGenArgs != null && !_args.CodeGenArgs.Parameters.ContainsKey("ConnectionString"))
                _args.CodeGenArgs.Parameters.Add("ConnectionString", _args.ConnectionString);

            if (_args.UseBeefDbo)
                _args.Assemblies.Insert(0, typeof(DatabaseConsoleWrapper).Assembly);

            _args.Assemblies.ForEach(ass => _namespaces.Add(ass.GetName().Name!));
        }

        /// <summary>
        /// Execute the database upgrade.
        /// </summary>
        public async Task<bool> RunAsync()
        {
            try
            {
                _db = new Db(_args.ConnectionString);
                LoggerSink? ls = null;

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Drop))
                {
                    ls = new LoggerSink(_logger);
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB DROP: Checking database existence and dropping where found...");
                    if (!await TimeExecutionAsync(() => { DropDatabase.For.SqlDatabase(_args.ConnectionString, ls); return Task.FromResult(true); }).ConfigureAwait(false))
                        return false;
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Create) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.Create))
                {
                    if (ls == null)
                        ls = new LoggerSink(_logger);

                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB CREATE: Checking database existence and creating where not found...");
                    if (!await TimeExecutionAsync(() => { EnsureDatabase.For.SqlDatabase(_args.ConnectionString, ls); return Task.FromResult(true); }).ConfigureAwait(false))
                        return false;
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Migrate) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.Migrate))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB MIGRATE: Migrating the database...");
                    _logger.LogInformation($"Probing for embedded resources: {(string.Join(", ", GetNamespacesWithSuffix($"{MigrationsNamespace}.*.sql")))}");

                    DatabaseUpgradeResult? result = null;
                    if (!await TimeExecutionAsync(() =>
                    {
                        result = DeployChanges.To
                            .SqlDatabase(_args.ConnectionString)
                            .WithScripts(GetMigrationScripts(_args.Assemblies))
                            .WithoutTransaction()
                            .LogTo(ls)
                            .Build()
                            .PerformUpgrade();

                        return Task.FromResult(result.Successful);
                    }).ConfigureAwait(false))
                        return false;

                    if (!result!.Successful)
                    {
                        _logger.LogError(result.Error, result.Error.Message);
                        return false;
                    }
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.CodeGen) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.CodeGen))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB CODEGEN: Code-gen database objects...");
                    CodeGenConsole.LogCodeGenExecutionArgs(_args.CodeGenArgs!);

                    var cge = new CodeGenExecutor(_args.CodeGenArgs!);
                    if (!await TimeExecutionAsync(async () =>
                    {
                        return await cge.RunAsync().ConfigureAwait(false);
                    }, true, () => $", Unchanged = { cge.Statistics.NotChangedCount}, Updated = { cge.Statistics.UpdatedCount}, Created = { cge.Statistics.CreatedCount}, TotalLines = { cge.Statistics.LinesOfCodeCount}").ConfigureAwait(false))
                    {
                        return false;
                    }
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Schema) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.Schema))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB SCHEMA: Drops and creates the database objects...");

                    if (_args.UseBeefDbo && !_args.SchemaOrder.Contains("dbo"))
                        _args.SchemaOrder.Insert(0, "dbo");

                    if (!await TimeExecutionAsync(() => DropAndCreateAllObjectsAsync(), true).ConfigureAwait(false))
                        return false;
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Reset))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB RESET: Resets database by dropping data from all tables...");

                    if (!await TimeExecutionAsync(() => DeleteAllAndResetAsync()).ConfigureAwait(false))
                        return false;
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.Data) && _args.SupportedCommands.HasFlag(DatabaseExecutorCommand.Data))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB DATA: Insert or merge the embedded YAML data...");

                    if (!await TimeExecutionAsync(() => InsertOrMergeYamlDataAsync()).ConfigureAwait(false))
                        return false;
                }

                if (_args.Command.HasFlag(DatabaseExecutorCommand.ScriptNew))
                {
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation(new string('-', 80));
                    _logger.LogInformation(string.Empty);
                    _logger.LogInformation("DB SCRIPTNEW: Creating a new SQL script from embedded template...");

                    if (!await TimeExecutionAsync(() => CreateScriptNewAsync()).ConfigureAwait(false))
                        return false;
                }

                _logger.LogInformation(string.Empty);
                _logger.LogInformation(new string('-', 80));

                return true;
            }
            finally
            {
                if (_db != null)
                    _db.Dispose();
            }
        }

        /// <summary>
        /// Times the execution and reports result.
        /// </summary>
        private async Task<bool> TimeExecutionAsync(Func<Task<bool>> action, bool addEmptyLineBeforeComplete = false, Func<string>? summaryText = null)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var result = await action().ConfigureAwait(false);
                sw.Stop();
                if (addEmptyLineBeforeComplete)
                    _logger.LogInformation(string.Empty);

                _logger.LogInformation($"Complete [{sw.ElapsedMilliseconds}ms{summaryText?.Invoke() ?? ""}].");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets all the migration scripts from the assemblies and ensures order.
        /// </summary>
        private List<SqlScript> GetMigrationScripts(IEnumerable<Assembly> assemblies)
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
        private async Task<bool> DropAndCreateAllObjectsAsync()
        {
            var list = new List<SqlSchemaScript>();

            // See if there are any files out there (recently generated).
            if (_args.CodeGenArgs?.OutputPath != null)
            {
                _logger.LogInformation($"Probing for files: '{_args.CodeGenArgs.OutputPath.FullName}*.sql'");
                foreach (var ns in _namespaces)
                {
                    var di = new DirectoryInfo(Path.Combine(_args.CodeGenArgs.OutputPath.FullName, ns, SchemaNamespace));
                    if (di.Exists)
                    {
                        foreach (var fi in di.GetFiles("*.sql", SearchOption.AllDirectories))
                        {
                            var name = RenameFileToResourceName(fi);
                            var sor = SqlObjectReader.Read(fi.OpenRead());
                            if (!sor.IsValid)
                            {
                                _logger.LogError($"SQL object '{name}' is not considered valid: {sor.ErrorMessage}");
                                return false;
                            }

                            var sr = new SqlSchemaScript { Name = name, Reader = sor, FileName = fi.FullName[(_args.CodeGenArgs.OutputPath.FullName.Length + 1)..] };
                            sr.Order = _args.SchemaOrder.IndexOf(sr.Reader.Schema!);
                            if (sr.Order < 0)
                                sr.Order = _args.SchemaOrder.Count;

                            list.Add(sr);
                        }
                    }
                }
            }

            // Parse all resources and get ready for the SQL code gen.
            _logger.LogInformation($"Probing for embedded resources: {string.Join(", ", GetNamespacesWithSuffix($"{SchemaNamespace}.*.sql"))}");
            foreach (var ass in _args.Assemblies)
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
                        _logger.LogError($"SQL object '{name}' is not considered valid: {sor.ErrorMessage}");
                        return false;
                    }

                    var sr = new SqlSchemaScript { Name = name, Reader = sor };
                    sr.Order = _args.SchemaOrder.IndexOf(sr.Reader.Schema!);
                    if (sr.Order < 0)
                        sr.Order = _args.SchemaOrder.Count;

                    list.Add(sr);
                }
            }

            if (list.Count == 0)
            {
                _logger.LogInformation($"Nothing found.");
                return true;
            }

            // Drop all existing (in reverse order).
            var sb = new StringBuilder();
            foreach (var sr in list.OrderByDescending(x => x.Order).ThenByDescending(x => x.Reader!.Order).ThenByDescending(x => x.Name))
            {
                if (sr.Reader!.Order > 0)
                    sb.AppendLine($"  DROP {sr.Reader.Type} IF EXISTS [{sr.Reader.Schema}].[{sr.Reader.Name}]");
            }

            if (!await ExecuteSqlStatementAsync(() => _db!.SqlStatement(sb.ToString()).NonQueryAsync(), "the drop of all existing (known) database objects.", sb.ToString()).ConfigureAwait(false))
                return false;

            // Execute each script one-by-one.
            _logger.LogInformation("Executing the create of all (known) database objects.");
            foreach (var sr in list.OrderBy(x => x.Order).ThenBy(x => x.Reader!.Order).ThenBy(x => x.Name))
            {
                if (!await ExecuteSqlStatementAsync(() => _db!.SqlStatement(sr.Reader!.GetSql()).NonQueryAsync(), $"{(sr.FileName == null ? "resource" : "file")} {sr.FileName ?? sr.Name}", indent: 2).ConfigureAwait(false))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Rename file to resource name format.
        /// </summary>
        private string RenameFileToResourceName(FileInfo fi)
        {
            var dir = RenameFileToResourceNameReplace(fi.DirectoryName[(_args.CodeGenArgs!.OutputPath!.FullName.Length + 1)..]);
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
        private async Task<bool> ExecuteSqlStatementAsync(Func<Task> func, string text, string? additionalInfo = null, int indent = 0)
        {
            try
            {
                _logger.LogInformation($"{new string(' ', indent)}Executing {text}");
                if (additionalInfo != null)
                    _logger.LogInformation($"{new string(' ', indent)}{additionalInfo}");

                await func().ConfigureAwait(false);
                return true;
            }
            catch (DbException dex)
            {
                _logger.LogError(dex, $"Execution failed with: {dex.Message}");
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
            return await ExecuteSqlStatementAsync(() => _db!.SqlStatement(sr.ReadToEnd()).NonQueryAsync(), "the deletion of all data from all tables (excluding 'dbo' and 'cdc' schema).").ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts or merges the embedded YAML data.
        /// </summary>
        private async Task<bool> InsertOrMergeYamlDataAsync()
        {
            // Get all the database table schema information.
            _logger.LogInformation($"Querying database for all existing table and column configurations...");
            await SqlDataUpdater.RegisterDatabaseAsync(_db!.GetConnection()!).ConfigureAwait(false);

            // Parse all resources and get ready for the SQL code gen.
            _logger.LogInformation($"Probing for embedded resources: {(string.Join(", ", GetNamespacesWithSuffix($"{DataNamespace}.*.yaml")))}");
            foreach (var ass in _args.Assemblies)
            {
                foreach (var name in ass.GetManifestResourceNames())
                {
                    if (!_namespaces.Any(x => name.StartsWith(x + $".{DataNamespace}.", StringComparison.InvariantCulture) && name.EndsWith(".yaml", StringComparison.InvariantCulture)))
                        continue;

                    _logger.LogInformation("");
                    _logger.LogInformation($"** Parsing and executing: {name}");
                    var sdm = SqlDataUpdater.ReadYaml(ass.GetManifestResourceStream(name)!);
                    await sdm.GenerateSqlAsync((a) =>
                    {
                        _logger.LogInformation("");
                        _logger.LogInformation($"---- Executing {a.OutputFileName}:");
                        _logger.LogInformation(a.Content);
                        if (a.Content != null)
                        {
                            var rows = _db!.SqlStatement(a.Content).ScalarAsync<int>().GetAwaiter().GetResult();
                            _logger.LogInformation($"Result: {rows} rows affected.");
                        }
                    }).ConfigureAwait(false);
                }
            }

            _logger.LogInformation("");

            return true;
        }

        /// <summary>
        /// Creates the new script from the template.
        /// </summary>
        private async Task<bool> CreateScriptNewAsync()
        {
            var data = new { _args.CodeGenArgs!.Parameters };
            var p0 = _args.CodeGenArgs.Parameters?.SingleOrDefault(x => string.Compare(x.Key, "Param0", StringComparison.InvariantCultureIgnoreCase) == 0).Value;
            var rn = typeof(DatabaseExecutor).Assembly.GetManifestResourceNames().SingleOrDefault(x => string.Compare(x, $"Beef.Database.Core.Resources.{p0 ?? "Default"}_sql.hbs", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (rn == null)
                throw new InvalidOperationException($"The ScriptNew '{p0}' argument is invalid.");
            
            var fn = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            if (_args.CodeGenArgs!.Parameters == null || _args.CodeGenArgs!.Parameters.Count == 0)
                fn += "-comment-text";
            else
                _args.CodeGenArgs!.Parameters.Where(x => x.Key.StartsWith("Param", StringComparison.InvariantCultureIgnoreCase)).ForEach(x => fn += $"-{x.Value}");

            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var fi = new FileInfo(Path.Combine(di.FullName, MigrationsNamespace, $"{fn.ToLowerInvariant()}.sql"));
            if (!fi.Directory.Exists)
                fi.Directory.Create();

            using (var st = typeof(DatabaseExecutor).Assembly.GetManifestResourceStream(rn))
            {
                using var tr = new StreamReader(st!);
                
                HandlebarsHelpers.RegisterHelpers();
                var hb = Handlebars.Compile(tr.ReadToEnd());
                await File.WriteAllTextAsync(fi.FullName, hb.Invoke(data)).ConfigureAwait(false);
            }

            _logger.LogWarning($"Script file created: {fi.FullName}");
            return true;
        }
    }
}