// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly List<string> _schemaOrder = new List<string>();
        private string _exeDir;
        private string _outDir;
        private string _script = "Database.xml";
        private DatabaseExecutorCommand _supports = DatabaseExecutorCommand.All;

        /// <summary>
        /// Gets the command line template.
        /// </summary>
        public static string CommandLineTemplate { get; set; }
            = "{{Command}} \"{{ConnectionString}}\" {{Assembly}} -c \"{{ConfigFile}}\" -s {{Script}} -o \"{{OutDir}}\" -su {{Supported}} -p Company={{Company}} -p AppName={{AppName}} -p AppDir={{AppName}}";

        /// <summary>
        /// Gets the command line assembly portion template.
        /// </summary>
        public static string CommandLineAssemblyTemplate { get; set; } = "-a \"{{Assembly}}\" ";

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsoleWrapper"/> class.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabaseConsoleWrapper"/> instance.</returns>
        public static DatabaseConsoleWrapper Create(string connectionString, string company, string appName, bool useBeefDbo = true)
        {
            return new DatabaseConsoleWrapper(connectionString, company, appName, useBeefDbo).Assemblies(Assembly.GetEntryAssembly()!);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConsoleWrapper"/> class.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        private DatabaseConsoleWrapper(string connectionString, string company, string appName, bool useBeefDbo = true)
        {
            ConnectionString = Check.NotEmpty(connectionString, nameof(connectionString));
            Company = Check.NotEmpty(company, nameof(company));
            AppName = Check.NotEmpty(appName, nameof(appName));

            if (useBeefDbo)
            {
                _assemblies.Add(typeof(DatabaseConsoleWrapper).Assembly);
                _schemaOrder.Add("dbo");
            }

            OverrideConnectionString();

            _exeDir = CodeGenFileManager.GetExeDirectory();
            _outDir = new DirectoryInfo(_exeDir).Parent.FullName;
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
        /// Gets the company name.
        /// </summary>
        public string Company { get; private set; }

        /// <summary>
        /// Gets the application/domain name.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Adds the <paramref name="assemblies"/> containing the embedded resources.
        /// </summary>
        /// <param name="assemblies">The assemblies containing the embedded resources.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsoleWrapper Assemblies(params Assembly[] assemblies)
        {
            _assemblies.InsertRange(0, assemblies);
            return this;
        }

        /// <summary>
        /// Sets (overrides) the output <paramref name="path"/>/directory (defaults to <c>./..</c>).
        /// </summary>
        /// <param name="path">The output path/directory.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsoleWrapper OutputDirectory(string path)
        {
            _outDir = Check.NotEmpty(path, nameof(path));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name (defaults to <c>Database.Xml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsoleWrapper DatabaseScript(string script)
        {
            _script = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the supported <see cref="DatabaseExecutorCommand"/> (defaults to <see cref="DatabaseExecutorCommand.All"/>.
        /// </summary>
        /// <param name="command">The supported <see cref="DatabaseExecutorCommand"/>.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsoleWrapper Supports(DatabaseExecutorCommand command)
        {
            _supports = command;
            return this;
        }

        /// <summary>
        /// Adds the <paramref name="schemas"/> to the schema order.
        /// </summary>
        /// <param name="schemas">The schema names to add.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public DatabaseConsoleWrapper SchemaOrder(params string[] schemas)
        {
            _schemaOrder.AddRange(schemas);
            return this;
        }

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
            var sn = app.Argument("parameters", "Additional parameters used by the ScriptNew command.", multipleValues: true);
            var cs = app.Option("-cs|--connectionstring", "Override the database connection string.", CommandOptionType.SingleValue);
            var eo = app.Option("-eo|--entry-assembly-only", "Override assemblies to use the entry assembly only.", CommandOptionType.NoValue);
            var x2y = app.Option("-x2y|--xmlToYaml", "Convert the XML configuration into YAML equivalent (will not codegen).", CommandOptionType.NoValue);
            var evn = app.Option("-evn|--environmentVariableName", "Override the default environment variable name for the connection string.", CommandOptionType.SingleValue);
            var ps = app.Option("-p|--param", "Name=Value pair.", CommandOptionType.MultipleValue).Accepts(v => v.Use(new ParamsValidator()));
            var so = app.Option("-s|--script", "Override the script resource name.", CommandOptionType.SingleValue);

            app.OnExecuteAsync(async (_) =>
            {
                if (x2y.HasValue())
                {
                    if (cmd.ParsedValue != DatabaseExecutorCommand.CodeGen)
                        throw new CommandParsingException(app, $"Command '{cmd.ParsedValue}' is not compatible with --xmlToYaml; the command must be '{DatabaseExecutorCommand.CodeGen}'.");

                    DatabaseConsole.WriteMasthead();
                    return await CodeGenFileManager.ConvertXmlToYamlAsync(CommandType.Database, CodeGenFileManager.GetConfigFilename(_exeDir, CommandType.Database, Company, AppName)).ConfigureAwait(false);
                }

                var script = so.HasValue() ? so.Value() : _script;
                var sb = new StringBuilder();
                if (eo.HasValue())
                    sb.Append(ReplaceMoustache(CommandLineAssemblyTemplate, null!, null!, Assembly.GetEntryAssembly()?.FullName!, script!));
                else
                    _assemblies.ForEach(a => sb.Append(ReplaceMoustache(CommandLineAssemblyTemplate, null!, null!, a.FullName!, script!)));

                var rargs = ReplaceMoustache(CommandLineTemplate, cmd.Value!, cs.Value() ?? ConnectionString, sb.ToString(), script!);

                for (int i = 0; i < sn.Values.Count; i++)
                {
                    rargs += $" -p Param{i}={sn.Values[i]}";
                }

                foreach (var p in ps.Values)
                {
                    rargs += $" -p {p}";
                }

                _schemaOrder.ForEach(so => rargs += $" -so {so}");

                return await DatabaseConsole.Create().RunAsync(rargs).ConfigureAwait(false);
            });

            try
            {
                return await app.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                Console.Error.WriteLine(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Replace the moustache placeholders.
        /// </summary>
        private string ReplaceMoustache(string text, string command, string connectionString, string assembly, string script)
        {
            text = text.Replace("{{Command}}", command, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{ConfigFile}}", CodeGen.CodeGenFileManager.GetConfigFilename(_exeDir, CodeGen.CommandType.Database, Company, AppName), StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{ConnectionString}}", connectionString, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Assembly}}", assembly, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Company}}", Company, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{AppName}}", AppName, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Script}}", script, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{Supported}}", ((int)_supports).ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
            return text.Replace("{{OutDir}}", _outDir, StringComparison.OrdinalIgnoreCase);
        }
    }
}