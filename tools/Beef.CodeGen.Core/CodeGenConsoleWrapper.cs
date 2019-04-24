// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>CodeGen Console</b> wrapper to simplify/standardise execution of the <see cref="CodeGenConsole"/>. 
    /// </summary>
    public class CodeGenConsoleWrapper
    {
        private enum CommandType
        {
            Entity = 1,
            Database = 2,
            RefData = 4,
            All = Entity | Database | RefData
        }

        /// <summary>
        /// Gets the <see cref="CommandType.Entity"/> command line template.
        /// </summary>
        public static string EntityCommandLineTemplate { get; set; }
            = "{{Company}}.{{AppName}}.xml -s EntityWebApiCoreAgent.xml -o {{OutDir}} -p Company={{Company}} -p AppName={{AppName}} -p ApiName={{ApiName}}";

        /// <summary>
        /// Gets the <see cref="CommandType.Database"/> command line template.
        /// </summary>
        public static string DatabaseCommandLineTemplate { get; set; } 
            = "{{Company}}.{{AppName}}.Database.xml -s Database.xml -o {{OutDir}} -p Company={{Company}} -p AppName={{AppName}} -p AppDir={{AppName}}";

        /// <summary>
        /// Gets the <see cref="CommandType.RefData"/> command line template.
        /// </summary>
        public static string RefDataCommandLineTemplate { get; set; }
            = "{{Company}}.RefData.xml -s RefDataCoreCrud.xml -o {{OutDir}} -p Company={{Company}} -p AppName={{AppName}} -p ApiName={{ApiName}}";

        /// <summary>
        /// Gets the <see cref="Assembly"/> portion command line template.
        /// </summary>
        public static string CommandLineAssemblyTemplate { get; set; } = " -a \"{{Assembly}}\"";

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsoleWrapper"/> class defaulting to <see cref="Assembly.GetEntryAssembly"/>.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <returns>The <see cref="CodeGenConsoleWrapper"/> instance.</returns>
        public static CodeGenConsoleWrapper Create(string company, string appName, string apiName = "Api", string outDir = ".\\..")
        {
            return new CodeGenConsoleWrapper(new Assembly[] { Assembly.GetEntryAssembly() }, company, appName, apiName, outDir);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsoleWrapper"/> class.
        /// </summary>
        /// <param name="assemblies">The list of additional assemblies to probe for resources.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <returns>The <see cref="CodeGenConsoleWrapper"/> instance.</returns>
        public static CodeGenConsoleWrapper Create(Assembly[] assemblies, string company, string appName, string apiName = "Api", string outDir = ".\\..")
        {
            return new CodeGenConsoleWrapper(assemblies, company, appName, apiName, outDir);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsoleWrapper"/> class.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <param name="assemblies">Optional list of assemblies to probe for resources.</param>
        private CodeGenConsoleWrapper(Assembly[] assemblies, string company, string appName, string apiName = "Api", string outDir = ".\\..")
        {
            Company = Check.NotEmpty(company, nameof(company));
            AppName = Check.NotEmpty(appName, nameof(appName));
            ApiName = Check.NotEmpty(apiName, nameof(apiName));
            OutDir = Check.NotEmpty(outDir, nameof(outDir));
            Assemblies = assemblies;
        }

        /// <summary>
        /// Gets the company name.
        /// </summary>
        public string Company { get; private set; }

        /// <summary>
        /// Gets the application/domain name.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Gets the Web API name.
        /// </summary>
        public string ApiName { get; private set; }

        /// <summary>
        /// Gets the output path/directory.
        /// </summary>
        public string OutDir { get; private set; }

        /// <summary>
        /// Gets the list of additional assemblies to probe for resources.
        /// </summary>
        public Assembly[] Assemblies { get; private set; }

        /// <summary>
        /// Indicates whether <see cref="CommandType.Entity"/> is supported (defaults to <c>true</c>).
        /// </summary>
        public bool IsEntitySupported { get; set; } = true;

        /// <summary>
        /// Indicates whether <see cref="CommandType.Database"/> is supported (defaults to <c>true</c>).
        /// </summary>
        public bool IsDatabaseSupported { get; set; } = true;

        /// <summary>
        /// Indicates whether <see cref="CommandType.RefData"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsRefDataSupported { get; set; } = false;

        /// <summary>
        /// Sets the <see cref="IsEntitySupported"/>, <see cref="IsDatabaseSupported"/> and <see cref="IsRefDataSupported"/> options.
        /// </summary>
        /// <param name="entity">Indicates where the entity code generation should take place.</param>
        /// <param name="database">Indicates where the database generation should take place.</param>
        /// <param name="refData">Indicates where the reference data generation should take place.</param>
        /// <returns>The <see cref="CodeGenConsoleWrapper"/> to support method chaining/fluent style.</returns>
        public CodeGenConsoleWrapper Supports(bool entity = true, bool database = false, bool refData = false)
        {
            IsEntitySupported = entity;
            IsDatabaseSupported = database;
            IsRefDataSupported = refData;
            return this;
        }

        /// <summary>
        /// Executes the underlying <see cref="CodeGenConsole"/> using the code generation arguments.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "beef.codegen",
                Description = "Business Entity Execution Framework (Beef) Code Generator."
            };

            var cmd = app.Argument<CommandType>("command", "Execution command type: Entity, Database, RefData or All.", false).IsRequired();
            var cs = app.Option("-c|--connectionString", "Override the connection string for Database.", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var ct = Enum.Parse<CommandType>(cmd.Value, true);
                var rc = 0;
                if (IsDatabaseSupported && ct.HasFlag(CommandType.Database))
                    rc = CodeGenConsole.Create().Run(AppendAssemblies(ReplaceMoustache(DatabaseCommandLineTemplate) + (cs.HasValue() ? $" -p \"ConnectionString={cs.Value()}\"" : "")));

                if (IsRefDataSupported && ct.HasFlag(CommandType.RefData))
                    rc = CodeGenConsole.Create().Run(AppendAssemblies(ReplaceMoustache(RefDataCommandLineTemplate)));

                if (IsEntitySupported && ct.HasFlag(CommandType.Entity))
                    rc = CodeGenConsole.Create().Run(AppendAssemblies(ReplaceMoustache(EntityCommandLineTemplate)));
            });

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException cpex)
            {
                Console.WriteLine(cpex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Replace the moustache placeholders.
        /// </summary>
        private string ReplaceMoustache(string text)
        {
            text = text.Replace("{{Company}}", Company, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{AppName}}", AppName, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("{{ApiName}}", ApiName, StringComparison.OrdinalIgnoreCase);
            return text.Replace("{{OutDir}}", OutDir, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Appends configured assemblies.
        /// </summary>
        private string AppendAssemblies(string text)
        {
            if (Assemblies == null)
                return text;

            Assemblies.ForEach(a => text = text + CommandLineAssemblyTemplate.Replace("{{Assembly}}", a.FullName, StringComparison.OrdinalIgnoreCase));
            return text;
        }
    }
}
