// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Database.Core
{
    /// <summary>
    /// <b>Database Console</b> wrapper to simplify/standardise execution of the <see cref="DatabaseConsole"/>. 
    /// </summary>
    /// <remarks>An environment variable named '<see cref="Company"/><see cref="AppName"/>ConnectionString' overrides any passed (default) <see cref="ConnectionString"/>.</remarks>
    public class DatabaseConsoleWrapper
    {
        /// <summary>
        /// Gets the command line template.
        /// </summary>
        public static string CommandLineTemplate { get; set; }
            = "{{Command}} \"{{ConnectionString}}\" {{Assembly}} -c {{Company}}.{{AppName}}.Database.xml -s Database.xml -o {{OutDir}} -p Company={{Company}} -p AppName={{AppName}} -p AppDir={{AppName}}";

        /// <summary>
        /// Gets the command line assembly portion template.
        /// </summary>
        public static string CommandLineAssemblyTemplate { get; set; } = "-a \"{{Assembly}}\" ";

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsoleWrapper"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies containing the embedded resources.</param>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabaseConsoleWrapper"/> instance.</returns>
        public static DatabaseConsoleWrapper Create(Assembly[] assemblies, string connectionString, string company, string appName, string outDir = ".\\..", bool useBeefDbo = true)
        {
            return new DatabaseConsoleWrapper(assemblies, connectionString, company, appName, outDir, useBeefDbo);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsoleWrapper"/> class defaulting to <see cref="Assembly.GetEntryAssembly"/>.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabaseConsoleWrapper"/> instance.</returns>
        public static DatabaseConsoleWrapper Create(string connectionString, string company, string appName, string outDir = ".\\..", bool useBeefDbo = true)
        {
            return new DatabaseConsoleWrapper(new Assembly[] { Assembly.GetEntryAssembly()! }, connectionString, company, appName, outDir, useBeefDbo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConsoleWrapper"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies containing the embedded resources.</param>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        private DatabaseConsoleWrapper(Assembly[] assemblies, string connectionString, string company, string appName, string outDir, bool useBeefDbo = true)
        {
            ConnectionString = Check.NotEmpty(connectionString, nameof(connectionString));
            Company = Check.NotEmpty(company, nameof(company));
            AppName = Check.NotEmpty(appName, nameof(appName));
            OutDir = Check.NotEmpty(outDir, nameof(outDir));
            if (assemblies == null)
                assemblies = Array.Empty<Assembly>();

            if (useBeefDbo)
            {
                var a = new List<Assembly> { typeof(DatabaseConsoleWrapper).Assembly };
                a.AddRange(assemblies);
                Assemblies.AddRange(a);
            }
            else
                Assemblies.AddRange(assemblies);

            OverrideConnectionString();
        }

        /// <summary>
        /// Override connection string if specified as an environment variable.
        /// </summary>
        private void OverrideConnectionString()
        {
            var cs = Environment.GetEnvironmentVariable($"{Company.Replace(".", "_", StringComparison.InvariantCulture)}_{AppName.Replace(".", "_", StringComparison.InvariantCulture)}_ConnectionString");
            if (cs != null)
                ConnectionString = cs;
        }

        /// <summary>
        /// Gets the default connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the assemblies containing the embedded resources.
        /// </summary>
        public List<Assembly> Assemblies { get; private set; } = new List<Assembly>();

        /// <summary>
        /// Gets the company name.
        /// </summary>
        public string Company { get; private set; }

        /// <summary>
        /// Gets the application/domain name.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Gets the output path/directory.
        /// </summary>
        public string OutDir { get; private set; }

        /// <summary>
        /// Executes the underlying <see cref="DatabaseConsole"/> using the database tooling arguments.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            using var app = new CommandLineApplication()
            {
                Name = "beef.database.core",
                Description = "Business Entity Execution Framework (Beef) Database Tooling."
            };

            var cmd = app.Argument<DatabaseExecutorCommand>("command", "Database command.").IsRequired();
            var cs = app.Option("-cs|--connectionstring", "Override the database connection string.", CommandOptionType.SingleValue);
            var eo = app.Option("-eo|--entry-assembly-only", "Override assemblies to use the entry assembly only.", CommandOptionType.NoValue);
            var ct = app.Option("-create|--scriptnew-create-table", "ScriptNew: use create '[schema.]table' template.", CommandOptionType.SingleValue);
            var at = app.Option("-alter|--scriptnew-alter-table", "ScriptNew: use alter '[schema.]table' template.", CommandOptionType.SingleValue);

            app.OnExecuteAsync(async (_) =>
            {
                var sb = new StringBuilder();
                if (eo.HasValue())
                    sb.Append(ReplaceMoustache(CommandLineAssemblyTemplate, null!, null!, Assembly.GetEntryAssembly()?.FullName!));
                else
                    Assemblies.ForEach(a => sb.Append(ReplaceMoustache(CommandLineAssemblyTemplate, null!, null!, a.FullName!)));

                var rargs = ReplaceMoustache(CommandLineTemplate, cmd.Value!, cs.Value() ?? ConnectionString, sb.ToString());
                if (ct.HasValue())
                    rargs += GetTableSchemaParams("Create", ct.Value()!);
                else if (at.HasValue())
                    rargs += GetTableSchemaParams("Alter", at.Value()!);

                await DatabaseConsole.Create().RunAsync(rargs).ConfigureAwait(false);
            });

            try
            {
                return await app.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                Console.WriteLine(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Get the table and schema paramaters from the argument.
        /// </summary>
        private string GetTableSchemaParams(string action, string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return $" -p ScriptNew={action} -p Schema={AppName} -p Table=Xxxxx";

            var parts = arg.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return $" -p ScriptNew={action} -p Schema={AppName} -p Table={parts[0]}";
            else if (parts.Length == 2)
                return $" -p ScriptNew={action} -p Schema={parts[0]} -p Table={parts[1]}";
            else
                return $" -p ScriptNew={action} -p Schema={AppName} -p Table=Xxxxx";
        }

        /// <summary>
        /// Replace the moustache placeholders.
        /// </summary>
        private string ReplaceMoustache(string text, string command, string connectionString, string assembly)
        {
            text = text.Replace("{{Command}}", command, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{ConnectionString}}", connectionString, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Assembly}}", assembly, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Company}}", Company, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{AppName}}", AppName, StringComparison.OrdinalIgnoreCase);
            return text.Replace("{{OutDir}}", OutDir, StringComparison.OrdinalIgnoreCase);
        }
    }
}