// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Builders;
using Beef.CodeGen.Generators;
using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
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
        /// <returns>A statuc code.</returns>
        public static async Task<int> Main(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            Logger.Default = new ColoredConsoleLogger(nameof(CodeGenConsole));

            // Check for special case / internal use arguments.
            if (args.Length == 1)
            {
                switch (args[0].ToUpperInvariant())
                {
                    case "--GENERATEENTITYXMLSCHEMA": return SpecialActivitiesCenter("Generate Entity XML Schema", "./Schema/codegen.entity.xsd", fn => XmlSchemaGenerator.Create<Config.Entity.CodeGenConfig>().Save(fn, System.Xml.Linq.SaveOptions.None));
                    case "--GENERATEENTITYJSONSCHEMA": return SpecialActivitiesCenter("Generate Entity JSON Schema", "./Schema/entity.beef.json", fn => File.WriteAllText(fn, JsonSchemaGenerator.Create<Config.Entity.CodeGenConfig>("JSON Schema for Beef Entity code-generation (https://github.com/Avanade/Beef).")));
                    case "--GENERATEDATABASEXMLSCHEMA": return SpecialActivitiesCenter("Generate Database XML Schema", "./Schema/codegen.table.xsd", fn => XmlSchemaGenerator.Create<Config.Database.CodeGenConfig>().Save(fn, System.Xml.Linq.SaveOptions.None));
                    case "--GENERATEDATABASEJSONSCHEMA": return SpecialActivitiesCenter("Generate Database JSON Schema", "./Schema/database.beef.json", fn => File.WriteAllText(fn, JsonSchemaGenerator.Create<Config.Database.CodeGenConfig>("JSON Schema for Beef Database code-generation (https://github.com/Avanade/Beef).")));
                }
            }

            return await CodeGenConsole.Create().RunAsync(args).ConfigureAwait(false);
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
    }
}