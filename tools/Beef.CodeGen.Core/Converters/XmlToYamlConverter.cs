// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Config.Entity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Beef.CodeGen.Converters
{
    /// <summary>
    /// Provides the YAML format arguments.
    /// </summary>
    internal class YamlFormatArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YamlFormatArgs"/>.
        /// </summary>
        /// <param name="writer">The <see cref="StringWriter"/>.</param>
        public YamlFormatArgs(StringWriter writer) => Writer = Check.NotNull(writer, nameof(writer));

        /// <summary>
        /// Gets the <see cref="StringWriter"/>.
        /// </summary>
        public StringWriter Writer { get; private set; }

        /// <summary>
        /// Gets or sets the number of indent characters.
        /// </summary>
        public int Indent { get; set; } = -2;
    }

    /// <summary>
    /// XML to YAML converter. Generates an opinionated terse YAML format.
    /// </summary>
    public static class XmlToYamlConverter
    {
        /// <summary>
        /// Converts an existing <b>Entity XML</b> document into the equivlent new YAML format.
        /// </summary>
        /// <param name="xml">The existing <see cref="XDocument"/>.</param>
        /// <returns>The new YAML formatted <see cref="string"/>.</returns>
        public static string ConvertEntityXmlToYaml(XDocument xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (xml.Root.Name.LocalName != "CodeGeneration")
                throw new ArgumentException("Root element must be named 'CodeGeneration'.", nameof(xml));

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var (entity, type, _) = GetConfigInfo(xml.Root.Name.LocalName);
                WriteElement(new YamlFormatArgs(sw), xml.Root, entity, type);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the configuration for a given XML element.
        /// </summary>
        private static (ConfigurationEntity entity, Type type, string name) GetConfigInfo(string? name) => name switch
        {
            "CodeGeneration" => (ConfigurationEntity.CodeGen, typeof(Config.Entity.CodeGenConfig), ""),
            "Entity" => (ConfigurationEntity.Entity, typeof(EntityConfig), "entities"),
            "Property" => (ConfigurationEntity.Property, typeof(PropertyConfig), "properties"),
            "Operation" => (ConfigurationEntity.Operation, typeof(OperationConfig), "operations"),
            "Parameter" => (ConfigurationEntity.Parameter, typeof(ParameterConfig), "parameters"),
            "Const" => (ConfigurationEntity.Const, typeof(ConstConfig), "consts"),
            _ => (ConfigurationEntity.None, typeof(object), ""),
        };

        /// <summary>
        /// Writes the XML element as YAML.
        /// </summary>
        private static void WriteElement(YamlFormatArgs args, XElement xml, ConfigurationEntity entity, Type type)
        {
            WriteComments(args, xml);

            if (args.Indent == 0)
                args.Writer.Write("- { ");
            else if (args.Indent > 0)
                args.Writer.Write($"{new string(' ', args.Indent)}  {{ ");

            args.Indent += 2;

            WriteAttributes(args, entity, type, xml);

            // Group by element name, then process as a collection.
            foreach (var grp in xml.Elements().GroupBy(x => x.Name).Select(g => new { g.Key, Children = xml.Elements(g.Key) }))
            {
                var info = GetConfigInfo(grp.Key.LocalName);
                if (info.entity == ConfigurationEntity.None)
                    return;

                if (args.Indent > 0)
                    args.Writer.Write(", ");

                args.Writer.Write($"{info.name}:");
                if (args.Indent > 0)
                    args.Writer.Write(" [");

                args.Writer.WriteLine();
                int i = 0;

                foreach (var child in grp.Children)
                {
                    if (i++ > 0)
                    {
                        args.Writer.Write(" }");
                        if (args.Indent > 0)
                            args.Writer.Write(",");

                        args.Writer.WriteLine();
                    }

                    if (args.Indent == 0 && !xml.Equals(xml.Document.Root.Elements().First()))
                        args.Writer.WriteLine();

                    WriteElement(args, child, info.entity, info.type);
                }

                args.Writer.WriteLine(" }");
                if (args.Indent > 0)
                    args.Writer.Write($"{new string(' ', args.Indent)}]");
            }

            args.Indent -= 2;
        }

        /// <summary>
        /// Writes the XML comments as YAML.
        /// </summary>
        private static void WriteComments(YamlFormatArgs args, XElement xml)
        {
            if (xml.PreviousNode != null && xml.PreviousNode.NodeType == System.Xml.XmlNodeType.Comment)
            {
                using var sr = new StringReader(((XComment)xml.PreviousNode).Value);
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;
                    else
                        args.Writer.WriteLine($"{new string(' ', args.Indent + 2)}# {line.Trim()}");
                }
            }
        }

        /// <summary>
        /// Writes the XML attribues as YAML.
        /// </summary>
        private static void WriteAttributes(YamlFormatArgs args, ConfigurationEntity ce, Type type, XElement xml)
        {
            var needsComma = false;

            foreach (var att in xml.Attributes())
            {
                var jname = XmlJsonRename.GetJsonName(ce, att.Name.LocalName);
                var pi = type.GetProperty(StringConversion.ToPascalCase(jname)!);
                if (pi == null || pi.GetCustomAttribute<JsonPropertyAttribute>() == null)
                    continue;

                if (needsComma)
                    args.Writer.Write(", ");

                var val = att.Value;
                if (val.IndexOfAny(new char[] { ':', '{', '}', '[', ']', ',', '&', '*', '#', '?', '|', '-', '<', '>', '=', '!', '%', '@', '\\', '\"' }) >= 0)
                    val = $"'{val.Replace("'", "''", StringComparison.InvariantCultureIgnoreCase)}'";

                args.Writer.Write($"{jname}: {val}");

                if (args.Indent > 0)
                    needsComma = true;
                else
                    args.Writer.WriteLine();
            }
        }
    }
}