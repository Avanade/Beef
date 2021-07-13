// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Beef.CodeGen.Builders
{
    /// <summary>
    /// Represents a <c>JsonSchema</c> builder.
    /// </summary>
    /// <remarks>See https://json-schema.org/ and https://www.schemastore.org/json/ </remarks>
    public static class JsonSchemaGenerator
    {
        /// <summary>
        /// Creates a <see cref="string"/> as the <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <returns>The JSON <see cref="string"/>.</returns>
        public static string Create<T>(string title)
        {
            var type = typeof(T);
            var types = new List<Type> { type };
            FindAllTypes(types, typeof(T));

            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(sw) { Formatting = Formatting.Indented };

            jtw.WriteStartObject();
            jtw.WritePropertyName("title");
            jtw.WriteValue(title);
            jtw.WritePropertyName("$schema");
            jtw.WriteValue("https://json-schema.org/draft-04/schema#");
            jtw.WritePropertyName("definitions");

            jtw.WriteStartObject();
            types.ForEach(t => WriteDefinition(t, jtw));
            jtw.WriteEndObject();

            jtw.WritePropertyName("allOf");
            jtw.WriteStartArray();
            jtw.WriteStartObject();
            jtw.WritePropertyName("$ref");
            jtw.WriteValue($"#/definitions/{type.GetCustomAttribute<ClassSchemaAttribute>()!.Name}");
            jtw.WriteEndObject();
            jtw.WriteEndArray();

            jtw.WriteEndObject();
            return sb.ToString();
        }

        /// <summary>
        /// Recursively find all types (definitions).
        /// </summary>
        private static void FindAllTypes(List<Type> types, Type type)
        {
            foreach (var pi in type.GetProperties())
            {
                var pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                if (pcsa != null)
                {
                    if (pi.PropertyType == typeof(List<string>))
                        continue;

                    var t = ComplexTypeReflector.GetItemType(pi.PropertyType);
                    if (types.Contains(t))
                        continue;

                    types.Add(t);
                    FindAllTypes(types, t);
                }
            }
        }

        /// <summary>
        /// Writes the object definition.
        /// </summary>
        private static void WriteDefinition(Type type, JsonTextWriter jtw)
        {
            var csa = type.GetCustomAttribute<ClassSchemaAttribute>();
            if (csa == null)
                throw new InvalidOperationException($"Type {type.Name} does not have required ClassSchemaAttribute defined.");

            jtw.WritePropertyName(csa.Name);


            jtw.WriteStartObject();
            jtw.WritePropertyName("type");
            jtw.WriteValue("object");
            jtw.WritePropertyName("title");
            jtw.WriteValue(CleanString(csa.Title) ?? StringConversion.ToSentenceCase(csa.Name)!);
            if (csa.Description != null)
            {
                jtw.WritePropertyName("description");
                jtw.WriteValue(CleanString(csa.Description));
            }

            jtw.WritePropertyName("properties");
            jtw.WriteStartObject();

            var rqd = new List<string>();

            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jpa == null)
                    continue;

                var name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name)!;
                jtw.WritePropertyName(name);
                jtw.WriteStartObject();

                var psa = pi.GetCustomAttribute<PropertySchemaAttribute>();
                if (psa != null)
                {
                    jtw.WritePropertyName("type");
                    jtw.WriteValue(GetJsonType(pi));
                    jtw.WritePropertyName("title");
                    jtw.WriteValue(CleanString(psa.Title) ?? StringConversion.ToSentenceCase(name)!);
                    if (psa.Description != null)
                    {
                        jtw.WritePropertyName("description");
                        jtw.WriteValue(CleanString(psa.Description));
                    }

                    if (psa.IsMandatory)
                        rqd.Add(name);

                    if (psa.Options != null)
                    {
                        jtw.WritePropertyName("enum");
                        jtw.WriteStartArray();

                        foreach (var opt in psa.Options)
                            jtw.WriteValue(opt);

                        jtw.WriteEndArray();
                    }
                }
                else
                {
                    var pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                    if (pcsa == null)
                        throw new InvalidOperationException($"Type '{type.Name}' Property '{pi.Name}' does not have a required PropertySchemaAttribute or PropertyCollectionSchemaAttribute.");

                    jtw.WritePropertyName("type");
                    jtw.WriteValue("array");
                    jtw.WritePropertyName("title");
                    jtw.WriteValue(CleanString(pcsa.Title) ?? StringConversion.ToSentenceCase(name)!);
                    if (pcsa.Description != null)
                    {
                        jtw.WritePropertyName("description");
                        jtw.WriteValue(CleanString(pcsa.Description));
                    }

                    jtw.WritePropertyName("items");

                    if (pi.PropertyType == typeof(List<string>))
                    {
                        jtw.WriteStartObject();
                        jtw.WritePropertyName("type");
                        jtw.WriteValue("string");
                        jtw.WriteEndObject();
                    }
                    else
                    {
                        var t = ComplexTypeReflector.GetItemType(pi.PropertyType);

                        jtw.WriteStartObject();
                        jtw.WritePropertyName("$ref");
                        jtw.WriteValue($"#/definitions/{t.GetCustomAttribute<ClassSchemaAttribute>()!.Name}");
                        jtw.WriteEndObject();
                    }
                }

                jtw.WriteEndObject();
            }

            jtw.WriteEndObject();

            if (rqd.Count > 0)
            {
                jtw.WritePropertyName("required");
                jtw.WriteStartArray();

                foreach (var name in rqd)
                    jtw.WriteValue(name);

                jtw.WriteEndArray();
            }

            jtw.WriteEndObject();
        }

        /// <summary>
        /// Gets the corresponding JSON type for the property.
        /// </summary>
        private static string GetJsonType(PropertyInfo pi)
        {
            return pi.PropertyType switch
            {
                Type t when t == typeof(string) => "string",
                Type t when t == typeof(bool?) => "boolean",
                Type t when t == typeof(int?) => "integer",
                _ => throw new InvalidOperationException($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' has a Type '{pi.PropertyType.Name}' that is not supported."),
            };
        }

        /// <summary>
        /// Cleans the string.
        /// </summary>
        private static string? CleanString(string? text) => text?.Replace('`', '\'');
    }
}