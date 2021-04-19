// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>CodeGen Console</b> wrapper to simplify/standardise execution of the <see cref="CodeGenConsole"/>. 
    /// </summary>
    public class CodeGenConsoleWrapper
    {
        private string _entityScript = "EntityWebApiCoreAgent.xml";
        private string _refDataScript = "RefDataCoreCrud.xml";
        private string _dataModelScript = "DataModelOnly.xml";
        private string _databaseScript = "Database.xml";
        private string _exeDir;

        /// <summary>
        /// Gets or sets the <see cref="CommandType.Entity"/> command line template.
        /// </summary>
        public static string EntityCommandLineTemplate { get; set; }
            = "-s {{Script}} -o \"{{OutDir}}\" -p Company={{Company}} -p AppName={{AppName}} -p ApiName={{ApiName}}";

        /// <summary>
        /// Gets or sets the <see cref="CommandType.Database"/> command line template.
        /// </summary>
        public static string DatabaseCommandLineTemplate { get; set; } 
            = "-s {{Script}} -o \"{{OutDir}}\" -p Company={{Company}} -p AppName={{AppName}} -p AppDir={{AppName}}";

        /// <summary>
        /// Gets or sets the <see cref="CommandType.RefData"/> command line template.
        /// </summary>
        public static string RefDataCommandLineTemplate { get; set; }
            = "-s {{Script}} -o \"{{OutDir}}\" -p Company={{Company}} -p AppName={{AppName}} -p ApiName={{ApiName}}";

        /// <summary>
        /// Gets or sets the <see cref="CommandType.DataModel"/> command line template.
        /// </summary>
        public static string DataModelCommandLineTemplate { get; set; }
            = "-s {{Script}} -o \"{{OutDir}}\" -p Company={{Company}} -p AppName={{AppName}}";

        /// <summary>
        /// Gets or sets the <see cref="Assembly"/> portion command line template.
        /// </summary>
        public static string CommandLineAssemblyTemplate { get; set; } = " -a \"{{Assembly}}\"";

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsoleWrapper"/> class defaulting to <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="outDir">The output path/directory.</param>
        /// <returns>The <see cref="CodeGenConsoleWrapper"/> instance.</returns>
        public static CodeGenConsoleWrapper Create(string company, string appName, string apiName = "Api", string? outDir = null)
        {
            return new CodeGenConsoleWrapper(new Assembly[] { Assembly.GetCallingAssembly() }, company, appName, apiName, outDir);
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
        public static CodeGenConsoleWrapper Create(Assembly[] assemblies, string company, string appName, string apiName = "Api", string? outDir = null)
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
        private CodeGenConsoleWrapper(Assembly[] assemblies, string company, string appName, string apiName = "Api", string? outDir = null)
        {
            Company = Check.NotEmpty(company, nameof(company));
            AppName = Check.NotEmpty(appName, nameof(appName));
            ApiName = Check.NotEmpty(apiName, nameof(apiName));

            _exeDir = CodeGenFileManager.GetExeDirectory();
            OutDir = string.IsNullOrEmpty(outDir) ? new DirectoryInfo(_exeDir).Parent.FullName : outDir;
            Assemblies = new List<Assembly>(assemblies ?? Array.Empty<Assembly>());
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
        public List<Assembly> Assemblies { get; private set; }

        /// <summary>
        /// Indicates whether <see cref="CommandType.Entity"/> is supported (defaults to <c>true</c>).
        /// </summary>
        public bool IsEntitySupported { get; set; } = true;

        /// <summary>
        /// Indicates whether <see cref="CommandType.Database"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsDatabaseSupported { get; set; } = false;

        /// <summary>
        /// Indicates whether <see cref="CommandType.RefData"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsRefDataSupported { get; set; } = false;

        /// <summary>
        /// Indicates whether <see cref="CommandType.DataModel"/> is supported (defaults to <c>false</c>).
        /// </summary>
        public bool IsDataModelSupported { get; set; } = false;

        /// <summary>
        /// Sets the <see cref="IsEntitySupported"/>, <see cref="IsDatabaseSupported"/> and <see cref="IsRefDataSupported"/> options.
        /// </summary>
        /// <param name="entity">Indicates whether the entity code generation should take place.</param>
        /// <param name="database">Indicates whether the database generation should take place.</param>
        /// <param name="refData">Indicates whether the reference data generation should take place.</param>
        /// <param name="dataModel">Indicates whether the data model generation should take place.</param>
        /// <returns>The <see cref="CodeGenConsoleWrapper"/> to support method chaining/fluent style.</returns>
        public CodeGenConsoleWrapper Supports(bool entity = true, bool database = false, bool refData = false, bool dataModel = false)
        {
            IsEntitySupported = entity;
            IsDatabaseSupported = database;
            IsRefDataSupported = refData;
            IsDataModelSupported = dataModel;
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.Database"/> (defaults to <c>EntityWebApiCoreAgent.xml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsoleWrapper EntityScript(string script)
        {
            _entityScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.DataModel"/> (defaults to <c>DataModelOnly.xml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsoleWrapper DataModelScript(string script)
        {
            _dataModelScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.RefData"/> (defaults to <c>RefDataCoreCrud.xml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsoleWrapper RefDataScript(string script)
        {
            _refDataScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name for the <see cref="CommandType.Database"/> (defaults to <c>Database.Xml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to supported fluent-style method-chaining.</returns>
        public CodeGenConsoleWrapper DatabaseScript(string script)
        {
            _databaseScript = Check.NotEmpty(script, nameof(script));
            return this;
        }

        /// <summary>
        /// Executes the underlying <see cref="CodeGenConsole"/> using the code generation arguments.
        /// </summary>
        /// <param name="args">The code generation arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsucessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            using var app = new CommandLineApplication()
            {
                Name = "beef.codegen",
                Description = "Business Entity Execution Framework (Beef) Code Generator."
            };

            var cmd = app.Argument<CommandType>("command", "Execution command type: Entity, Database, RefData or All.", false).IsRequired();
            var cs = app.Option("-cs|--connectionString", "Override the connection string for Database.", CommandOptionType.SingleValue);
            var cf = app.Option("-cf|--configFile", "Override the filename for the configuration.", CommandOptionType.SingleValue).Accepts(v => v.ExistingFile());
            var sf = app.Option("-s|--scriptFile", "Override the filename for the script orchestration.", CommandOptionType.SingleValue).Accepts(v => v.ExistingFile());
            var enc = app.Option("-enc|--expectNoChanges", "Expect no changes in the artefact output and error where changes are detected (e.g. within build pipeline).", CommandOptionType.NoValue);
            var x2y = app.Option("-x2y|--xmlToYaml", "Convert the XML configuration into YAML equivalent (will not codegen).", CommandOptionType.NoValue);

            app.OnExecuteAsync(async (_) =>
            {
                var ct = cmd.Value == null ? CommandType.All : Enum.Parse<CommandType>(cmd.Value, true);
                string? cfn = null;
                if (cf.HasValue())
                {
                    if (ct == CommandType.All)
                        throw new CommandParsingException(app, "Command 'All' is not compatible with --configFile; the command must be more specific when using a specified configuration file.");

                    cfn = cf.Value()!;
                }

                string? sfn = null;
                if (sf.HasValue())
                {
                    if (ct == CommandType.All)
                        throw new CommandParsingException(app, "Command 'All' is not compatible with --scriptFile; the command must be more specific when using a specified script file.");

                    sfn = sf.Value()!;
                }

                if (x2y.HasValue())
                {
                    if (ct == CommandType.All)
                        throw new CommandParsingException(app, "Command 'All' is not compatible with --xmlToYaml; the command must be more specific when converting XML configuration to YAML.");

                    CodeGenConsole.WriteMasthead();
                    return await CodeGenFileManager.ConvertXmlToYamlAsync(ct, cfn ?? CodeGenFileManager.GetConfigFilename(_exeDir, ct, Company, AppName)).ConfigureAwait(false);
                }

                var encArg = enc.HasValue() ? " --expectNoChanges" : string.Empty;

                var rc = 0;
                if (IsDatabaseSupported && ct.HasFlag(CommandType.Database))
                    rc = await CodeGenConsole.Create().RunAsync(AppendAssemblies(ReplaceMoustache($"\"{cfn ?? CodeGenFileManager.GetConfigFilename(_exeDir, CommandType.Database, Company, AppName)}\"" + " " + DatabaseCommandLineTemplate, sfn ?? _databaseScript) + (cs.HasValue() ? $" -p \"ConnectionString={cs.Value()}\"" : "") + encArg)).ConfigureAwait(false);

                if (rc == 0 && IsRefDataSupported && ct.HasFlag(CommandType.RefData))
                    rc = await CodeGenConsole.Create().RunAsync(AppendAssemblies(ReplaceMoustache($"\"{cfn ?? CodeGenFileManager.GetConfigFilename(_exeDir, CommandType.RefData, Company, AppName)}\"" + " " + RefDataCommandLineTemplate, sfn ?? _refDataScript) + encArg)).ConfigureAwait(false);

                if (rc == 0 && IsEntitySupported && ct.HasFlag(CommandType.Entity))
                    rc = await CodeGenConsole.Create().RunAsync(AppendAssemblies(ReplaceMoustache($"\"{cfn ?? CodeGenFileManager.GetConfigFilename(_exeDir, CommandType.Entity, Company, AppName)}\"" + " " + EntityCommandLineTemplate, sfn ?? _entityScript) + encArg)).ConfigureAwait(false);

                if (rc == 0 && IsDataModelSupported && ct.HasFlag(CommandType.DataModel))
                    rc = await CodeGenConsole.Create().RunAsync(AppendAssemblies(ReplaceMoustache($"\"{cfn ?? CodeGenFileManager.GetConfigFilename(_exeDir, CommandType.DataModel, Company, AppName)}\"" + " " + DataModelCommandLineTemplate, sfn ?? _dataModelScript) + encArg)).ConfigureAwait(false);

                return rc;
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
        private string ReplaceMoustache(string text, string script)
        {
            text = text.Replace("{{Script}}", script, StringComparison.OrdinalIgnoreCase);
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

            Assemblies.ForEach(a => text += CommandLineAssemblyTemplate.Replace("{{Assembly}}", a.FullName, StringComparison.OrdinalIgnoreCase));
            return text;
        }
    }
}