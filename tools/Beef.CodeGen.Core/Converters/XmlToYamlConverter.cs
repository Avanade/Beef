// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Newtonsoft.Json;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Indicates whether there were attributes written on the current line.
        /// </summary>
        public bool HasAttributes { get; set; }

        /// <summary>
        /// Gets the list of unknown attribute names.
        /// </summary>
        public List<string> UnknownAttributes { get; } = new List<string>();

        /// <summary>
        /// Writes the close squiggly bracket accounting for a needed space if an attribute has proceeded it.
        /// </summary>
        public void WriteClosingSquigglyBracket()
        {
            if (HasAttributes)
            {
                HasAttributes = false;
                Writer.Write(" ");
            }

            Writer.Write("}");
        }
    }

    /// <summary>
    /// XML to YAML converter. Generates an opinionated terse YAML format.
    /// </summary>
    public abstract class XmlToYamlConverter
    {
        /// <summary>
        /// Gets the <see cref="CodeGen.ConfigType"/>.
        /// </summary>
        protected abstract ConfigType ConfigType { get; }

        /// <summary>
        /// Converts an existing XML document into the equivlent new YAML format.
        /// </summary>
        /// <param name="xml">The existing <see cref="XDocument"/>.</param>
        /// <returns>The corresponding YAML and the list of unknown attributes.</returns>
        public (string Yaml, List<string> UnknownAttributes) ConvertXmlToYaml(XDocument xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (xml.Root.Name.LocalName != "CodeGeneration")
                throw new ArgumentException("Root element must be named 'CodeGeneration'.", nameof(xml));

            var sb = new StringBuilder();
            List<string> unknownAttributes;
            using (var sw = new StringWriter(sb))
            {
                var (entity, type, _) = GetEntityConfigInfo(xml.Root.Name.LocalName);
                var yfa = new YamlFormatArgs(sw);
                WriteElement(yfa, xml.Root, ConfigType, entity, type);
                unknownAttributes = yfa.UnknownAttributes;
            }

            return (sb.ToString(), unknownAttributes);
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

            args.Indent += 2;
            if (args.Indent == 2)
                args.Writer.Write("- { ");
            else if (args.Indent > 0)
                args.Writer.Write($"{new string(' ', args.Indent)}{{ ");

            WriteAttributes(args, ct, entity, type, xml);

            // Group by element name, then process as a collection.
            int i = 0;
            var coll = xml.Elements().GroupBy(x => x.Name).Select(g => new { g.Key, Children = xml.Elements(g.Key) });
            foreach (var grp in coll)
            {
                var info = GetEntityConfigInfo(grp.Key.LocalName);
                if (info.entity == ConfigurationEntity.None)
                    return;

                if (args.Indent > 0)
                {
                    args.Writer.WriteLine(",");
                    args.Indent += 2;
                    args.Writer.Write($"{new string(' ', args.Indent)}{info.name}:");
                }
                else
                    args.Writer.Write($"{info.name}:");

                if (args.Indent > 0)
                    args.Writer.Write(" [");

                args.Writer.WriteLine();
                int j = 0;

                foreach (var child in grp.Children)
                {
                    if (j++ > 0)
                    {
                        args.WriteClosingSquigglyBracket();
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

                args.WriteClosingSquigglyBracket();
                args.Writer.WriteLine();

                if (args.Indent > 0)
                {
                    args.Writer.Write($"{new string(' ', args.Indent)}]");
                    args.Indent -= 2;
                }

                i++;
                if (i == coll.Count())
                {
                    args.Writer.WriteLine();
                    if (args.Indent > 0)
                        args.Writer.Write($"{new string(' ', args.Indent)}");
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
                var pi = type.GetProperty(StringConverter.ToPascalCase(jname)!);
                var val = XmlYamlTranslate.GetYamlValue(ct, ce, att.Name.LocalName, att.Value);
                if (val == null)
                    continue;

                if (pi == null || pi.GetCustomAttribute<JsonPropertyAttribute>() == null)
                {
                    jname = att.Name.LocalName;
                    args.UnknownAttributes.Add($"{xml.Name.LocalName}.{jname} = {val}");
                }

                if (needsComma)
                    args.Writer.Write(", ");

                args.HasAttributes = true;
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
    public class EntityXmlToYamlConverter : XmlToYamlConverter
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
    public class DatabaseXmlToYamlConverter : XmlToYamlConverter
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
            "Query" => (ConfigurationEntity.Query, typeof(Config.Database.QueryConfig), "queries"),
            "QueryJoin" => (ConfigurationEntity.QueryJoin, typeof(Config.Database.QueryJoinConfig), "joins"),
            "QueryJoinOn" => (ConfigurationEntity.QueryJoinOn, typeof(Config.Database.QueryJoinOnConfig), "on"),
            "QueryOrder" => (ConfigurationEntity.QueryOrder, typeof(Config.Database.QueryOrderConfig), "order"),
            "QueryWhere" => (ConfigurationEntity.QueryWhere, typeof(Config.Database.QueryWhereConfig), "where"),
            "StoredProcedure" => (ConfigurationEntity.StoredProcedure, typeof(Config.Database.StoredProcedureConfig), "storedProcedures"),
            "Parameter" => (ConfigurationEntity.Parameter, typeof(Config.Database.ParameterConfig), "parameters"),
            "OrderBy" => (ConfigurationEntity.OrderBy, typeof(Config.Database.OrderByConfig), "orderby"),
            "Where" => (ConfigurationEntity.Where, typeof(Config.Database.WhereConfig), "where"),
            "Execute" => (ConfigurationEntity.Execute, typeof(Config.Database.ExecuteConfig), "execute"),
            _ => (ConfigurationEntity.None, typeof(object), ""),
        };
    }
}