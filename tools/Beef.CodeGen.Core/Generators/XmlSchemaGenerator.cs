// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents an <c>XmlSchema</c> builder.
    /// </summary>
    /// <remarks>See http://www.w3.org/2001/XMLSchema </remarks>
    public static class XmlSchemaGenerator
    {
        /// <summary>
        /// Creates an <see cref="XDocument"/> as the <c>XmlSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="configType">The <see cref="ConfigType"/>.</param>
        /// <returns>An <see cref="XDocument"/>.</returns>
        public static XDocument Create<T>(ConfigType configType)
        {
            var ns = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "schema",
                    new XAttribute(XNamespace.Xmlns + "xs", ns),
                    new XAttribute("attributeFormDefault", "unqualified"),
                    new XAttribute("elementFormDefault", "unqualified")));

            WriteObject(configType, typeof(T), ns, xdoc.Root, true);

            return xdoc;
        }

        /// <summary>
        /// Writes the schema for the object.
        /// </summary>
        private static void WriteObject(ConfigType ct, Type type, XNamespace ns, XElement xe, bool isRoot)
        {
            var csa = type.GetCustomAttribute<CodeGenClassAttribute>();
            if (csa == null)
                throw new InvalidOperationException($"Type '{type.Name}' does not have a required ClassSchemaAttribute.");

            if (!Enum.TryParse<ConfigurationEntity>(csa.Name, out var ce))
                ce = ConfigurationEntity.CodeGen;

            var xml = new XElement(ns + "element");
            xml.Add(new XAttribute("name", csa.Name));
            if (!isRoot)
            {
                xml.Add(new XAttribute("minOccurs", "0"));
                xml.Add(new XAttribute("maxOccurs", "unbounded"));
            }

            xml.Add(new XElement(ns + "annotation", new XElement(ns + "documentation", csa.Title)));
            var xct = new XElement(ns + "complexType");

            // Add sub-collections within xs:choice element.
            var hasSeq = false;
            var xs = new XElement(ns + "choice");
            xs.Add(new XAttribute("maxOccurs", "unbounded"));
            foreach (var pi in type.GetProperties())
            {
                var pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
                if (pcsa == null)
                    continue;

                // Properties with List<string> are not compatible with XML and should be picked up as comma separated strings.
                if (pi.PropertyType == typeof(List<string>))
                    continue;

                WriteObject(ct, ComplexTypeReflector.GetItemType(pi.PropertyType), ns, xs, false);
                hasSeq = true;
            }

            if (hasSeq)
                xct.Add(xs);

            // Add properties as xs:attribute's.
            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jpa == null)
                    continue;

                var name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name)!;
                var xmlName = XmlYamlTranslate.GetXmlName(ct, ce, name);
                var xmlOverride = XmlYamlTranslate.GetXmlPropertySchemaAttribute(ct, ce, xmlName);

                var psa = xmlOverride.Attribute ?? pi.GetCustomAttribute<CodeGenPropertyAttribute>();
                if (psa == null)
                {
                    // Properties with List<string> are not compatible with XML and should be picked up as comma separated strings.
                    if (pi.PropertyType == typeof(List<string>))
                    {
                        var pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
                        if (pcsa == null)
                            continue;

                        var xpx = new XElement(ns + "attribute",
                            new XAttribute("name", xmlName),
                            new XAttribute("use", pcsa.IsMandatory ? "required" : "optional"));

                        xpx.Add(new XElement(ns + "annotation", new XElement(ns + "documentation", GetDocumentation(name, pcsa))));
                        xct.Add(xpx);
                    }

                    continue;
                }

                var xp = new XElement(ns + "attribute",
                    new XAttribute("name", xmlName),
                    new XAttribute("use", psa.IsMandatory ? "required" : "optional"));

                xp.Add(new XElement(ns + "annotation", new XElement(ns + "documentation", GetDocumentation(name, psa))));

                if (psa.Options == null)
                    xp.Add(new XAttribute("type", GetXmlType(pi, xmlOverride.Type)));
                else
                {
                    var xr = new XElement(ns + "restriction", new XAttribute("base", GetXmlType(pi, xmlOverride.Type)));
                    foreach (var opt in psa.Options)
                        xr.Add(new XElement(ns + "enumeration", new XAttribute("value", opt)));

                    xp.Add(new XElement(ns + "simpleType", xr));
                }

                xct.Add(xp);
            }

            // Add this type into the overall document.
            xml.Add(xct);
            xe.Add(xml);
        }

        /// <summary>
        /// Gets the corresponding XML type for the property.
        /// </summary>
        private static string GetXmlType(PropertyInfo pi, Type? overrideType)
        {
            var type = overrideType ?? pi.PropertyType;
            return type switch
            {
                Type t when t == typeof(string) => "xs:string",
                Type t when t == typeof(bool?) => "xs:boolean",
                Type t when t == typeof(int?) => "xs:int",
                _ => throw new InvalidOperationException($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' has a Type '{type.Name}' that is not supported."),
            };
        }

        /// <summary>
        /// Gets the documentation text.
        /// </summary>
        private static string GetDocumentation(string name, CodeGenPropertyAttribute psa) =>
            psa.Description == null ? (psa.Title ?? StringConversion.ToSentenceCase(name)!) : $"{psa.Title ?? StringConversion.ToSentenceCase(name)!} {psa.Description}";

        /// <summary>
        /// Gets the documentation text.
        /// </summary>
        private static string GetDocumentation(string name, CodeGenPropertyCollectionAttribute pcsa) =>
            pcsa.Description == null ? (pcsa.Title ?? StringConversion.ToSentenceCase(name)!) : $"{pcsa.Title ?? StringConversion.ToSentenceCase(name)!} {pcsa.Description}";
    }
}