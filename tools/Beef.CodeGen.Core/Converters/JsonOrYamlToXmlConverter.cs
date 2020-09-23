// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace Beef.CodeGen.Converters
{
    /// <summary>
    /// Converts either YAML or JSON into the equivalent XML format. The XML is intended for internal use as the likes of formatting and comments are discarded.
    /// </summary>
    /// <remarks>This is considered temporary until the need for the XML version is no longer required.</remarks>
    public static class JsonOrYamlToXmlConverter
    {
        /// <summary>
        /// Converts the YAML <see cref="string"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="string"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertYaml(string yaml)
        {
            using var sr = new StringReader(yaml);
            return ConvertYaml(sr);
        }

        /// <summary>
        /// Converts the YAML <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertYaml(Stream s)
        {
            using var sr = new StreamReader(s);
            return ConvertYaml(sr);
        }

        /// <summary>
        /// Converts the YAML <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertYaml(TextReader tr)
        {
            var yaml = new DeserializerBuilder().Build().Deserialize(tr);
            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yaml!);
            return ConvertJson(json);
        }

        /// <summary>
        /// Converts the JSON <see cref="string"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="string"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertJson(string json)
        {
            using var sr = new StringReader(json);
            return ConvertJson(sr);
        }

        /// <summary>
        /// Converts the JSON <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertJson(Stream s)
        {
            using var sr = new StreamReader(s);
            return ConvertJson(sr);
        }

        /// <summary>
        /// Converts the JSON <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="XDocument"/>.</returns>
        public static XDocument ConvertJson(TextReader tr)
        {
            using var jr = new JsonTextReader(tr);
            var jo = JObject.Load(jr);

            var xdoc = new XDocument(new XElement("CodeGeneration"));
            var (entity, name) = GetConfigInfo(null);
            ConvertJson(entity, xdoc.Root, jo);
            return xdoc;
        }

        /// <summary>
        /// Converts the JSON into the XML equivalent.
        /// </summary>
        private static void ConvertJson(ConfigurationEntity entity, XElement xml, JObject jo)
        {
            foreach (var jp in jo.Children().OfType<JProperty>())
            {
                if (jp.Value.Type == JTokenType.Array)
                {
                    var ci = GetConfigInfo(jp.Name);
                    if (ci.entity == ConfigurationEntity.None)
                        continue;

                    var ja = jp.Value as JArray;
                    foreach (var ji in ja!)
                    {
                        if (ji.Type == JTokenType.Object)
                        {
                            var xe = new XElement(ci.name);
                            ConvertJson(ci.entity, xe, (JObject)ji);
                            xml.Add(xe);
                        }
                    }
                }
                else
                    xml.Add(new XAttribute(XmlYamlTranslate.GetXmlName(entity, jp.Name), jp.Value));
            }
        }


        /// <summary>
        /// Gets the configuration for a given JSON array property name.
        /// </summary>
        private static (ConfigurationEntity entity, string name) GetConfigInfo(string? name) => name switch
        {
            null => (ConfigurationEntity.CodeGen, "CodeGeneration"),
            "entities" => (ConfigurationEntity.Entity, "Entity"),
            "properties" => (ConfigurationEntity.Property, "Property"),
            "operations" => (ConfigurationEntity.Operation, "Operation"),
            "parameters" => (ConfigurationEntity.Parameter, "Parameter"),
            "consts" => (ConfigurationEntity.Const, "Const"),
            _ => (ConfigurationEntity.None, ""),
        };
    }
}