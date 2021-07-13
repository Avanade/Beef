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
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(sw) { Formatting = Formatting.Indented };

            WriteObject(typeof(T), jtw, () =>
            {
                jtw.WritePropertyName("title");
                jtw.WriteValue(title);
                jtw.WritePropertyName("$schema");
                jtw.WriteValue("http://json-schema.org/draft-04/schema#");
            });

            return sb.ToString();
        }

        /// <summary>
        /// Writes the schema for the object.
        /// </summary>
        private static void WriteObject(Type type, JsonTextWriter jtw, Action? additional = null)
        {
            jtw.WriteStartObject();
            additional?.Invoke();

            jtw.WritePropertyName("type");
            jtw.WriteValue("object");
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
                    jtw.WriteStartArray();

                    if (pi.PropertyType == typeof(List<string>))
                    {
                        jtw.WriteStartObject();
                        jtw.WritePropertyName("type");
                        jtw.WriteValue("string");
                        jtw.WritePropertyName("uniqueItems");
                        jtw.WriteValue(true);
                        jtw.WriteEndObject();
                    }
                    else
                        WriteObject(ComplexTypeReflector.GetItemType(pi.PropertyType), jtw);

                    jtw.WriteEndArray();
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