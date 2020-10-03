// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
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
        /// Gets or sets the level in the hierarchy.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the number of indent characters.
        /// </summary>
        public int Indent { get; set; } = -2;
    }

    /// <summary>
    /// XML to YAML converter. Generates an opinionated terse YAML format.
    /// </summary>
    internal abstract class XmlToYamlConverter
    {
        /// <summary>
        /// Gets the <see cref="CodeGen.ConfigType"/>.
        /// </summary>
        protected abstract ConfigType ConfigType { get; }

        /// <summary>
        /// Converts an existing XML document into the equivlent new YAML format.
        /// </summary>
        /// <param name="xml">The existing <see cref="XDocument"/>.</param>
        /// <returns>The new YAML formatted <see cref="string"/>.</returns>
        internal string ConvertXmlToYaml(XDocument xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (xml.Root.Name.LocalName != "CodeGeneration")
                throw new ArgumentException("Root element must be named 'CodeGeneration'.", nameof(xml));

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var (entity, type, _) = GetEntityConfigInfo(xml.Root.Name.LocalName);
                WriteElement(new YamlFormatArgs(sw), xml.Root, ConfigType, entity, type);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the configuration for a given XML element.
        /// </summary>
        protected abstract (ConfigurationEntity entity, Type type, string name) GetEntityConfigInfo(string? name);

        /// <summary>
        /// Writes the XML element as YAML.
        /// </summary>
        private void WriteElement(YamlFormatArgs args, XElement xml, ConfigType ct, ConfigurationEntity entity, Type type)
        {
            WriteComments(args, xml);

            if (args.Indent == 0)
                args.Writer.Write("- { ");
            else if (args.Indent > 0)
                args.Writer.Write($"{new string(' ', args.Indent)}  {{ ");

            args.Indent += 2;

            WriteAttributes(args, ct, entity, type, xml);

            // Group by element name, then process as a collection.
            foreach (var grp in xml.Elements().GroupBy(x => x.Name).Select(g => new { g.Key, Children = xml.Elements(g.Key) }))
            {
                var info = GetEntityConfigInfo(grp.Key.LocalName);
                if (info.entity == ConfigurationEntity.None)
                    return;

                if (args.Indent > 0)
                    args.Writer.Write(", ");

                args.Writer.Write($"{info.name}:");
                if (args.Indent > 0)
                {
                    args.Writer.Write(" [");
                    if (args.Level >= 2)
                        args.Indent += 2;
                }

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

                    if (args.Indent == 0 && !child.Equals(xml.Document.Root.Elements().First()))
                        args.Writer.WriteLine();

                    args.Level++;
                    WriteElement(args, child, ct, info.entity, info.type);
                    args.Level--;
                }

                args.Writer.WriteLine(" }");
                if (args.Indent > 0)
                {
                    args.Writer.Write($"{new string(' ', args.Indent)}]");
                    if (args.Level >= 2)
                        args.Indent -= 2;
                }
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
        private static void WriteAttributes(YamlFormatArgs args, ConfigType ct, ConfigurationEntity ce, Type type, XElement xml)
        {
            var needsComma = false;

            foreach (var att in xml.Attributes())
            {
                var jname = XmlYamlTranslate.GetYamlName(ct, ce, att.Name.LocalName);
                var pi = type.GetProperty(StringConversion.ToPascalCase(jname)!);
                if (pi == null || pi.GetCustomAttribute<JsonPropertyAttribute>() == null)
                    continue;

                if (needsComma)
                    args.Writer.Write(", ");

                var val = XmlYamlTranslate.GetYamlValue(ct, ce, att.Name.LocalName, att.Value);
                if (val == null)
                    continue;

                if (!(val.StartsWith("[", StringComparison.OrdinalIgnoreCase) && val.EndsWith("]", StringComparison.OrdinalIgnoreCase)))
                {
                    if (val.IndexOfAny(new char[] { ':', '{', '}', '[', ']', ',', '&', '*', '#', '?', '|', '-', '<', '>', '=', '!', '%', '@', '\\', '\"' }) >= 0)
                        val = $"'{val.Replace("'", "''", StringComparison.InvariantCultureIgnoreCase)}'";

                    if (string.Compare(val, "NULL", StringComparison.InvariantCultureIgnoreCase) == 0)
                        val = $"'{val}'";
                }

                args.Writer.Write($"{jname}: {val}");

                if (args.Indent > 0)
                    needsComma = true;
                else
                    args.Writer.WriteLine();
            }
        }
    }

    /// <summary>
    /// Entity XML to YAML converter. Generates an opinionated terse YAML format.
    /// </summary>
    internal class EntityXmlToYamlConverter : XmlToYamlConverter
    {
        /// <summary>
        /// Gets the <see cref="CodeGen.ConfigType"/>.
        /// </summary>
        protected override ConfigType ConfigType => ConfigType.Entity;

        /// <summary>
        /// Gets the configuration for a given XML element.
        /// </summary>
        protected override (ConfigurationEntity entity, Type type, string name) GetEntityConfigInfo(string? name) => name switch
        {
            "CodeGeneration" => (ConfigurationEntity.CodeGen, typeof(Config.Entity.CodeGenConfig), ""),
            "Entity" => (ConfigurationEntity.Entity, typeof(Config.Entity.EntityConfig), "entities"),
            "Property" => (ConfigurationEntity.Property, typeof(Config.Entity.PropertyConfig), "properties"),
            "Operation" => (ConfigurationEntity.Operation, typeof(Config.Entity.OperationConfig), "operations"),
            "Parameter" => (ConfigurationEntity.Parameter, typeof(Config.Entity.ParameterConfig), "parameters"),
            "Const" => (ConfigurationEntity.Const, typeof(Config.Entity.ConstConfig), "consts"),
            _ => (ConfigurationEntity.None, typeof(object), ""),
        };
    }

    /// <summary>
    /// Database XML to YAML converter. Generates an opinionated terse YAML format.
    /// </summary>
    internal class DatabaseXmlToYamlConverter : XmlToYamlConverter
    {
        /// <summary>
        /// Gets the <see cref="CodeGen.ConfigType"/>.
        /// </summary>
        protected override ConfigType ConfigType => ConfigType.Database;

        /// <summary>
        /// Gets the configuration for a given XML element.
        /// </summary>
        protected override (ConfigurationEntity entity, Type type, string name) GetEntityConfigInfo(string? name) => name switch
        {
            "CodeGeneration" => (ConfigurationEntity.CodeGen, typeof(Config.Database.CodeGenConfig), ""),
            "Table" => (ConfigurationEntity.Table, typeof(Config.Database.TableConfig), "tables"),
            "StoredProcedure" => (ConfigurationEntity.StoredProcedure, typeof(Config.Database.StoredProcedureConfig), "storedProcedures"),
            "Parameter" => (ConfigurationEntity.Parameter, typeof(Config.Database.ParameterConfig), "parameters"),
            "OrderBy" => (ConfigurationEntity.OrderBy, typeof(Config.Database.OrderByConfig), "orderby"),
            "Where" => (ConfigurationEntity.Where, typeof(Config.Database.WhereConfig), "where"),
            "Execute" => (ConfigurationEntity.Execute, typeof(Config.Database.ExecuteConfig), "execute"),
            _ => (ConfigurationEntity.None, typeof(object), ""),
        };
    }
}