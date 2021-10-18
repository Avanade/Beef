﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Generators;
using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides the console capabilities.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">The console arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public static async Task<int> Main(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            Logger.Default = new ColoredConsoleLogger("CodeGenConsole");

            // Check for special case / internal use arguments.
            if (args.Length == 1)
            {
                MarkdownDocumentationGenerator.OnFileCreation = fn => Logger.Default.LogWarning($" > {fn}");
                switch (args[0].ToUpperInvariant())
                {
                    case "--GENERATEENTITYXMLSCHEMA": return SpecialActivitiesCenter("Generate Entity XML Schema", "./Schema/codegen.entity.xsd", fn => XmlSchemaGenerator.Create<Config.Entity.CodeGenConfig>(ConfigType.Entity).Save(fn, System.Xml.Linq.SaveOptions.None));
                    case "--GENERATEENTITYJSONSCHEMA": return SpecialActivitiesCenter("Generate Entity JSON Schema", "./Schema/entity.beef.json", fn => JsonSchemaGenerator.Generate<Config.Entity.CodeGenConfig>(fn, "JSON Schema for Beef Entity code-generation (https://github.com/Avanade/Beef)."));
                    case "--GENERATEDATABASEXMLSCHEMA": return SpecialActivitiesCenter("Generate Database XML Schema", "./Schema/codegen.table.xsd", fn => XmlSchemaGenerator.Create<Config.Database.CodeGenConfig>(ConfigType.Database).Save(fn, System.Xml.Linq.SaveOptions.None));
                    case "--GENERATEDATABASEJSONSCHEMA": return SpecialActivitiesCenter("Generate Database JSON Schema", "./Schema/database.beef.json", fn => JsonSchemaGenerator.Generate<Config.Database.CodeGenConfig>(fn, "JSON Schema for Beef Database code-generation (https://github.com/Avanade/Beef)."));
                    case "--GENERATEENTITYMARKDOWN": return SpecialActivitiesCenter("Generate Entity YAML documentation markdown file(s)", "../../docs/", dn => GenerateMarkdown<Config.Entity.CodeGenConfig>(dn, ConfigType.Entity, true));
                    case "--GENERATEENTITYXMLMARKDOWN": return SpecialActivitiesCenter("Generate Entity XML documentation markdown file(s)", "../../docs/", dn => GenerateMarkdown<Config.Entity.CodeGenConfig>(dn, ConfigType.Entity, false));
                    case "--GENERATEDATABASEMARKDOWN": return SpecialActivitiesCenter("Generate Database YAML documentation markdown file(s)", "../../docs/", dn => GenerateMarkdown<Config.Database.CodeGenConfig>(dn, ConfigType.Database, true));
                    case "--GENERATEDATABASEXMLMARKDOWN": return SpecialActivitiesCenter("Generate Database XML documentation markdown file(s)", "../../docs/", dn => GenerateMarkdown<Config.Database.CodeGenConfig>(dn, ConfigType.Database, false));
                }
            }

            var a = new OnRamp.CodeGeneratorArgs().AddAssembly(typeof(CodeGenConsole).Assembly).AddAssembly(Assembly.GetCallingAssembly());
            var c = OnRamp.Console.CodeGenConsole.Create<CodeGenConsole>(name: Assembly.GetCallingAssembly().GetName().Name, options: OnRamp.Console.SupportedOptions.All);
            c.MastheadText = CodeGenConsole.DefaultMastheadText;
            return await c.RunAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Manage/orchestrate the special activities execution.
        /// </summary>
        private static int SpecialActivitiesCenter(string title, string filename, Action<string> action)
        {
            // Method name inspired by: https://en.wikipedia.org/wiki/Special_Activities_Center
            Logger.Default.LogInformation("Business Entity Execution Framework (Beef) Code Generator - ** Special Activities Center **");
            Logger.Default.LogInformation($" Action: {title}");
            Logger.Default.LogInformation($" Filename: {filename}");

            var sw = Stopwatch.StartNew();
            action(filename);
            sw.Stop();
            Logger.Default.LogInformation(string.Empty);
            Logger.Default.LogInformation($"CodeGen complete [{sw.ElapsedMilliseconds}ms].");
            Logger.Default.LogInformation(string.Empty);
            return 0;
        }

        /// <summary>
        /// Invoke the <see cref="MarkdownDocumentationGenerator"/>.
        /// </summary>
        private static void GenerateMarkdown<T>(string directory, ConfigType configType, bool isYaml) where T : ConfigBase, IRootConfig
            => MarkdownDocumentationGenerator.Generate<T>(createFileName: (_, cgca) => $"{configType}-{cgca.Name}-{(isYaml ? "Config" : "Config-Xml")}.md",
                directory: directory, includeExample: isYaml, addBreaksBetweenSections: true, propertyData: pd =>
                {
                    if (isYaml)
                        return;

                    if (!Enum.TryParse<ConfigurationEntity>(pd.Class!.Name, out var ce))
                        ce = ConfigurationEntity.CodeGen;

                    pd.Name = XmlYamlTranslate.GetXmlName(configType, ce, pd.Name!);
                    var xpsa = XmlYamlTranslate.GetXmlPropertySchemaAttribute(configType, ce, pd.Name).Attribute;
                    if (xpsa != null)
                        pd.Psa = xpsa;
                });
    }
}