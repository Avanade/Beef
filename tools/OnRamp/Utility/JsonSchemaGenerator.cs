// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Newtonsoft.Json;
using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnRamp.Utility
{
    /// <summary>
    /// Represents a <c>JsonSchema</c> <see cref="Generate{T}(TextWriter, string?)">generator</see>.
    /// </summary>
    /// <remarks>See <see href="https://json-schema.org/"/> and  <see href="https://www.schemastore.org/json/"/>.</remarks>
    public static class JsonSchemaGenerator
    {
        /// <summary>
        /// Generates a <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="title">The title override.</param>
        public static void Generate<T>(string path, string? title = null) where T : ConfigBase, IRootConfig
        {
            using var tw = File.CreateText(path ?? throw new ArgumentNullException(nameof(path)));
            Generate<T>(tw, title);
        }

        /// <summary>
        /// Generates a <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="textWriter">The <see cref="TextWriter"/>.</param>
        /// <param name="title">The title override.</param>
        public static void Generate<T>(TextWriter textWriter, string? title = null) where T : ConfigBase, IRootConfig
        {
            var type = typeof(T);
            var types = new List<Type> { type };
            FindAllTypes(types, typeof(T));

            using var jtw = new JsonTextWriter(textWriter ?? throw new ArgumentNullException(nameof(textWriter))) { Formatting = Formatting.Indented };

            jtw.WriteStartObject();
            jtw.WritePropertyName("title");
            jtw.WriteValue(title ?? StringConversion.ToSentenceCase(type.Name));
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
            jtw.WriteValue($"#/definitions/{type.GetCustomAttribute<CodeGenClassAttribute>()?.Name}");
            jtw.WriteEndObject();
            jtw.WriteEndArray();

            jtw.WriteEndObject();
        }

        /// <summary>
        /// Recursively find all types (definitions).
        /// </summary>
        private static void FindAllTypes(List<Type> types, Type type)
        {
            foreach (var pi in type.GetProperties())
            {
                var pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
                if (pcsa != null)
                {
                    if (pi.PropertyType == typeof(List<string>))
                        continue;

                    var t = GetItemType(pi.PropertyType);
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
            var csa = type.GetCustomAttribute<CodeGenClassAttribute>();
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

                var psa = pi.GetCustomAttribute<CodeGenPropertyAttribute>();
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
                    var pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
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
                        var t = GetItemType(pi.PropertyType);

                        jtw.WriteStartObject();
                        jtw.WritePropertyName("$ref");
                        jtw.WriteValue($"#/definitions/{t.GetCustomAttribute<CodeGenClassAttribute>()!.Name}");
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
        /// Gets the item type from an Array or ICollection{T}.
        /// </summary>
        internal static Type GetItemType(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsValueType)
                throw new InvalidOperationException($"Type {type.Name} must be an Array or ICollection<T>.");

            if (type.IsArray)
                return type.GetElementType() ?? throw new InvalidOperationException($"Type {type.Name} is an Array but the Element Type is unable to be determined.");

            var t = type.GetInterfaces().FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (t != null)
                return ((t == typeof(ICollection<>)) ? type : t).GetGenericArguments()[0];

            throw new InvalidOperationException($"Type {type.Name} must be an Array or ICollection<T>.");
        }

        /// <summary>
        /// Cleans the string.
        /// </summary>
        private static string? CleanString(string? text) => text?.Replace('`', '\'');
    }
}