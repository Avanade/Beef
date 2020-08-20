// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.Reflection;
using Newtonsoft.Json;
using System;
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
        /// <returns>An <see cref="XDocument"/>.</returns>
        public static XDocument Create<T>()
        {
            var ns = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "schema",
                    new XAttribute(XNamespace.Xmlns + "xs", ns),
                    new XAttribute("targetNamespace", "http://schemas.beef.com/codegen/2015/01/entity"),
                    new XAttribute("attributeFormDefault", "unqualified"),
                    new XAttribute("elementFormDefault", "qualified")));

            WriteObject(typeof(T), ns, xdoc.Root, true);

            return xdoc;
        }

        /// <summary>
        /// Writes the schema for the object.
        /// </summary>
        private static void WriteObject(Type type, XNamespace ns, XElement xe, bool isRoot)
        {
            var csa = type.GetCustomAttribute<ClassSchemaAttribute>();
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

            // Add sub-collections within xs:sequence element.
            var hasSeq = false;
            var xs = new XElement(ns + "sequence");
            foreach (var pi in type.GetProperties())
            {
                var pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                if (pcsa == null)
                    continue;

                WriteObject(ComplexTypeReflector.GetItemType(pi.PropertyType), ns, xs, false);
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

                var psa = pi.GetCustomAttribute<PropertySchemaAttribute>();
                if (psa == null)
                    continue;

                var name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name)!;
                var xp = new XElement(ns + "attribute",
                    new XAttribute("name", XmlJsonRename.GetXmlName(ce, name)),
                    new XAttribute("use", psa.IsMandatory ? "required" : "optional"));

                xp.Add(new XElement(ns + "annotation", new XElement(ns + "documentation", GetDocumentation(name, psa))));

                if (psa.Options == null)
                    xp.Add(new XAttribute("type", GetXmlType(pi)));
                else
                {
                    var xr = new XElement(ns + "restriction", new XAttribute("base", GetXmlType(pi)));
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
        private static string GetXmlType(PropertyInfo pi)
        {
            return pi.PropertyType switch
            {
                Type t when t == typeof(string) => "xs:string",
                Type t when t == typeof(bool?) => "xs:boolean",
                Type t when t == typeof(int?) => "xs:int",
                _ => throw new InvalidOperationException($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' has a Type '{pi.PropertyType.Name}' that is not supported."),
            };
        }

        /// <summary>
        /// Gets the documentation text.
        /// </summary>
        private static string GetDocumentation(string name, PropertySchemaAttribute psa) =>
            psa.Description == null ? (psa.Title ?? StringConversion.ToSentenceCase(name)!) : $"{psa.Title ?? StringConversion.ToSentenceCase(name)!} {psa.Description}";
    }
}