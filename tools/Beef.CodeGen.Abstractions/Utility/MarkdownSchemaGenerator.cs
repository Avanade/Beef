// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Beef.CodeGen.Utility
{
    /// <summary>
    /// Represents a schema <c>Markdown</c> file <see cref="Generate">generator</see>.
    /// </summary>
    public static class MarkdownSchemaGenerator
    {
        /// <summary>
        /// Generates <c>Markdown</c> for each <see cref="Type"/> (recursively) from the specified root <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="createFileName">The function to create the file name; defaults to '<c>Xxx.md</c>' where 'Xxx' is the <see cref="ClassSchemaAttribute.Name"/>.</param>
        /// <param name="directory">The directory in which to create the markdown files; defaults to <see cref="Environment.CurrentDirectory"/>.</param>
        /// <param name="includeExample">Indicates whether to include the <see cref="ClassSchemaAttribute.Markdown"/> in the file.</param>
        /// <param name="addBreaksBetweenSections">Indicates whether to include additional breaks (<c>&lt;br/&gt;</c>) between sections.</param>
        /// <param name="propertyData">The action to optionally enable manipulation of the <see cref="MarkdownSchemaGeneratorPropertyData"/> before being written to the file.</param>
        public static void Generate<T>(Func<ClassSchemaAttribute, string>? createFileName = null, string? directory = null, bool includeExample = true, bool addBreaksBetweenSections = false, Action<MarkdownSchemaGeneratorPropertyData>? propertyData = null) where T : ConfigBase, IRootConfig =>
            WriteObject(typeof(T), createFileName ?? ((csa) => $"{csa.Name}.md"), directory ?? Environment.CurrentDirectory, includeExample, addBreaksBetweenSections, propertyData);

        /// <summary>
        /// Writes the markdown for the object.
        /// </summary>
        private static void WriteObject(Type type, Func<ClassSchemaAttribute, string> createFileName, string directory, bool includeExample, bool addBreaksBetweenSections, Action<MarkdownSchemaGeneratorPropertyData>? propertyData)
        {
            // Where the type does not have ClassSchemaAttribute then continue.
            var csa = type.GetCustomAttribute<ClassSchemaAttribute>();
            if (csa == null)
                return;

            // Get file name and create.
            var fn = createFileName(csa) ?? throw new InvalidOperationException("The createFileName function must not return a null.");
            using var tw = File.CreateText(Path.Combine(directory, fn));

            // Get all the properties prior to write.
            var pdlist = new List<MarkdownSchemaGeneratorPropertyData>();
            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jpa == null)
                    continue;

                var pd = new MarkdownSchemaGeneratorPropertyData
                {
                    Name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name),
                    Property = pi,
                    Psa = pi.GetCustomAttribute<PropertySchemaAttribute>()
                };

                if (pd.Psa == null)
                {
                    pd.Pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                    if (pd.Pcsa == null)
                        throw new InvalidOperationException($"Type '{type.Name}' Property '{pi.Name}' does not have a required PropertySchemaAttribute or PropertyCollectionSchemaAttribute.");

                    pd.Category = pd.Pcsa.Category;
                }
                else
                    pd.Category = pd.Psa.Category;

                propertyData?.Invoke(pd);

                pdlist.Add(pd);
            }

            var title = csa.Title;
            if (title != null && title.EndsWith('.'))
                title = title[..^1];

            tw.WriteLine($"# {title}");
            tw.WriteLine();
            tw.WriteLine(csa.Description);
            if (!string.IsNullOrEmpty(csa.Markdown))
            {
                tw.WriteLine();
                tw.WriteLine(csa.Markdown);
            }

            tw.WriteLine();
            if (addBreaksBetweenSections)
            {
                tw.WriteLine("<br/>");
                tw.WriteLine();
            }

            if (includeExample && !string.IsNullOrEmpty(csa.ExampleMarkdown))
            {
                tw.WriteLine("## Example");
                tw.WriteLine();
                tw.WriteLine(csa.ExampleMarkdown);
                tw.WriteLine();
                if (addBreaksBetweenSections)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }

            var cats = type.GetCustomAttributes<CategorySchemaAttribute>();

            if (cats.Count() > 1)
            {
                tw.WriteLine("## Property categories");
                tw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.");
                tw.WriteLine();
                tw.WriteLine("Category | Description");
                tw.WriteLine("-|-");

                foreach (var cat in cats)
                {
                    tw.WriteLine($"[`{cat.Category}`](#{cat.Category}) | {cat.Title}");
                }

                tw.WriteLine();
                tw.WriteLine("The properties with a bold name are those that are more typically used (considered more important).");
                tw.WriteLine();
                if (addBreaksBetweenSections)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }
            else
            {
                tw.WriteLine("## Properties");
                tw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).");
                tw.WriteLine();
            }

            foreach (var cat in cats)
            {
                if (cats.Count() > 1)
                {
                    tw.WriteLine($"## {cat.Category}");
                    tw.Write(cat.Title);
                    if (cat.Description != null)
                        tw.Write($" {cat.Description}");

                    tw.WriteLine();
                    tw.WriteLine();
                }

                tw.WriteLine("Property | Description");
                tw.WriteLine("-|-");

                foreach (var p in pdlist.Where(x => x.Category == cat.Category))
                {
                    if (p.Psa != null)
                        WriteTableItem(tw, p.Name, p.Psa.Title, p.Psa.Description, null, p.Psa.IsMandatory, p.Psa.IsImportant, p.Psa.Options);
                    else
                    {
                        var pt = JsonSchemaGenerator.GetItemType(p.Property!.PropertyType);
                        var ptcsa = pt.GetCustomAttribute<ClassSchemaAttribute>()!;
                        if (ptcsa != null)
                        {
                            var ptfn = createFileName(ptcsa) ?? throw new InvalidOperationException("The createFileName function must not return a null.");
                            WriteTableItem(tw, p.Name, $"The corresponding [`{ptcsa.Name}`]({ptfn}) collection.", p.Pcsa!.Description, p.Pcsa.Markdown, p.Pcsa.IsMandatory, p.Pcsa.IsImportant);
                        }
                        else if (p.Pcsa != null)
                            WriteTableItem(tw, p.Name, p.Pcsa.Title, p.Pcsa.Description, p.Pcsa.Markdown, p.Pcsa.IsMandatory, p.Pcsa.IsImportant);
                    }
                }

                tw.WriteLine();

                if (addBreaksBetweenSections && cats.Last() != cat)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }

            tw.Close();

            // Create markdown for each child collection type.
            foreach (var p in pdlist.Where(x => x.Pcsa != null))
            {
                WriteObject(JsonSchemaGenerator.GetItemType(p.Property!.PropertyType), createFileName, directory, includeExample, addBreaksBetweenSections, propertyData);
            }
        }

        /// <summary>
        /// Write the table line.
        /// </summary>
        private static void WriteTableItem(TextWriter tw, string? name, string? title, string? description = null, string? markdown = null, bool isMandatory = false, bool isImportant = false, string[]? options = null)
        {
            if (isMandatory || isImportant)
                tw.Write("**");

            tw.Write($"`{name}`");

            if (isMandatory || isImportant)
                tw.Write("**");

            tw.Write($" | {title}");

            if (options != null)
            {
                tw.Write(" Valid options are: ");
                var isFirst = true;
                foreach (var o in options)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        tw.Write(", ");

                    tw.Write($"`{o}`");
                }

                tw.Write(".");
            }

            if (isMandatory)
                tw.Write(" [Mandatory]");
            
            if (description != null)
                tw.Write($"<br/> {description}");

            if (markdown != null)
                tw.Write($"<br/> {markdown}");

            tw.WriteLine();
        }
    }

    /// <summary>
    /// Provides the property data used for the markdown schema generation.
    /// </summary>
    public class MarkdownSchemaGeneratorPropertyData
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo? Property { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PropertySchemaAttribute"/>.
        /// </summary>
        public PropertySchemaAttribute? Psa { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PropertyCollectionSchemaAttribute"/>.
        /// </summary>
        public PropertyCollectionSchemaAttribute? Pcsa { get; set; }
    }
}