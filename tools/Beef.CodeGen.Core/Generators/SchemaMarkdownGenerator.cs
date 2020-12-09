// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents a schema <c>Markdown</c> generator.
    /// </summary>
    public static class SchemaMarkdownGenerator
    {
        private class PropertyData
        {
            public string? Name { get; set; }

            public PropertyInfo? Property { get; set; }

            public string? Category { get; set; }

            public PropertySchemaAttribute? Psa { get; set; }

            public PropertyCollectionSchemaAttribute? Pcsa { get; set; }
        }

        /// <summary>
        /// Creates a <see cref="string"/> as the <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="path">The path/directory to output the markdown file(s) to.</param>
        /// <param name="configType">The <see cref="ConfigType"/>.</param>
        /// <param name="isYamlVersion">Indicates whether to generate the YAML version.</param>
        public static void Create<T>(string path, ConfigType configType, bool isYamlVersion)
        {
            WriteObject(path, configType, typeof(T), isYamlVersion);
        }

        /// <summary>
        /// Writes the markdown for the object.
        /// </summary>
        private static void WriteObject(string path, ConfigType ct, Type type, bool isYaml)
        {
            var csa = type.GetCustomAttribute<ClassSchemaAttribute>();
            if (csa == null)
                return;

            if (!Enum.TryParse<ConfigurationEntity>(csa.Name, out var ce))
                ce = ConfigurationEntity.CodeGen;

            var fn = Path.Combine(path, $"{ct}-{csa.Name}-{(isYaml ? "Config" : "Config-Xml")}.md");
            Beef.Diagnostics.Logger.Default.LogWarning($" > Creating: {fn}");
            using var sw = File.CreateText(fn);

            var pdlist = new List<PropertyData>();

            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jpa == null)
                    continue;

                var pd = new PropertyData
                { 
                    Name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name),
                    Property = pi,
                    Psa = pi.GetCustomAttribute<PropertySchemaAttribute>()
                };

                if (!isYaml)
                {
                    pd.Name = XmlYamlTranslate.GetXmlName(ct, ce, pd.Name!);
                    var xpsa = XmlYamlTranslate.GetXmlPropertySchemaAttribute(ct, ce, pd.Name).Attribute;
                    if (xpsa != null)
                        pd.Psa = xpsa;
                }

                if (pd.Psa == null)
                {
                    pd.Pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                    if (pd.Pcsa == null)
                        throw new InvalidOperationException($"Type '{type.Name}' Property '{pi.Name}' does not have a required PropertySchemaAttribute or PropertyCollectionSchemaAttribute.");

                    pd.Category = pd.Pcsa.Category;
                }
                else
                    pd.Category = pd.Psa.Category;

                pdlist.Add(pd);
            }

            sw.WriteLine($"# {csa.Title} - {(isYaml ? "YAML/JSON" : "XML")}");
            sw.WriteLine();
            sw.WriteLine(csa.Description);
            if (!string.IsNullOrEmpty(csa.Markdown))
            {
                sw.WriteLine();
                sw.WriteLine(csa.Markdown);
            }

            sw.WriteLine();
            sw.WriteLine("<br/>");
            sw.WriteLine();

            if (isYaml && !string.IsNullOrEmpty(csa.ExampleMarkdown))
            {
                sw.WriteLine("## Example");
                sw.WriteLine();
                sw.WriteLine(csa.ExampleMarkdown);
                sw.WriteLine();
                sw.WriteLine("<br/>");
                sw.WriteLine();
            }

            var cats = type.GetCustomAttributes<CategorySchemaAttribute>();

            if (cats.Count() > 1)
            {
                sw.WriteLine("## Property categories");
                sw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).");
                sw.WriteLine();
                sw.WriteLine("Category | Description");
                sw.WriteLine("-|-");

                foreach (var cat in cats)
                {
                    sw.WriteLine($"[`{cat.Category}`](#{cat.Category}) | {cat.Title}");
                }

                sw.WriteLine();
                sw.WriteLine("<br/>");
                sw.WriteLine();
            }
            else
            {
                sw.WriteLine("## Properties");
                sw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).");
                sw.WriteLine();
            }

            foreach (var cat in cats)
            {
                if (cats.Count() > 1)
                {
                    sw.WriteLine($"## {cat.Category}");
                    sw.Write(cat.Title);
                    if (cat.Description != null)
                        sw.Write($" {cat.Description}");

                    sw.WriteLine();
                    sw.WriteLine();
                }

                sw.WriteLine("Property | Description");
                sw.WriteLine("-|-");

                foreach (var p in pdlist.Where(x => x.Category == cat.Category))
                {
                    if (p.Psa != null)
                        WriteTableItem(sw, p.Name, p.Psa.Title, p.Psa.Description, null, p.Psa.IsImportant, p.Psa.Options);
                    else
                    {
                        var pt = ComplexTypeReflector.GetItemType(p.Property!.PropertyType);
                        var ptcsa = pt.GetCustomAttribute<ClassSchemaAttribute>()!;
                        if (ptcsa != null)
                            WriteTableItem(sw, p.Name, $"The corresponding [`{ptcsa.Name}`]({ct}-{ptcsa.Name}-{(isYaml ? "Config" : "Config-Xml")}.md) collection.", p.Pcsa!.Description, p.Pcsa.Markdown, p.Pcsa.IsImportant);
                        else if (p.Pcsa != null)
                            WriteTableItem(sw, p.Name, p.Pcsa.Title, p.Pcsa.Description, p.Pcsa.Markdown, p.Pcsa.IsImportant);
                    }
                }

                sw.WriteLine();
                sw.WriteLine("<br/>");
                sw.WriteLine();
            }

            sw.WriteLine("<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>");

            // Done, close file, then move onto children.
            sw.Close();

            foreach (var p in pdlist.Where(x => x.Pcsa != null))
            {
                WriteObject(path, ct, ComplexTypeReflector.GetItemType(p.Property!.PropertyType), isYaml);
            }
        }

        /// <summary>
        /// Write the table line.
        /// </summary>
        private static void WriteTableItem(StreamWriter sw, string? name, string? title, string? description = null, string? markdown = null, bool isImportant = false, string[]? options = null)
        {
            if (isImportant)
                sw.Write("**");

            sw.Write($"`{name}`");

            if (isImportant)
                sw.Write("**");

            sw.Write($" | {title}");

            if (options != null)
            {
                sw.Write(" Valid options are: ");
                var isFirst = true;
                foreach (var o in options)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sw.Write(", ");

                    sw.Write($"`{o}`");
                }

                sw.Write(".");
            }

            if (description != null)
                sw.Write($" {description}");

            if (markdown != null)
                sw.Write($" {markdown}");

            sw.WriteLine();
        }
    }
}