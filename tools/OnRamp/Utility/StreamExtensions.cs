// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Newtonsoft.Json;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides <see cref="TextReader"/> extension methods.
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="json"/> <see cref="TextReader"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="json">The JSON <see cref="TextReader"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeJson<T>(this TextReader json) where T : class => (T?)DeserializeJson(json, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="json"/> <see cref="TextReader"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="TextReader"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeJson(this TextReader json, Type type) => JsonSerializer.Create().Deserialize(json ?? throw new ArgumentNullException(nameof(json)), type);

        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="yaml"/> <see cref="TextReader"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="yaml">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeYaml<T>(this TextReader yaml) where T : class => (T?)DeserializeYaml(yaml, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="yaml"/> <see cref="TextReader"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="TextReader"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeYaml(this TextReader yaml, Type type)
        {
            var yml = new DeserializerBuilder().Build().Deserialize(yaml);

#pragma warning disable IDE0063 // Use simple 'using' statement; cannot as need to be more explicit with managing the close and dispose.
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(new SerializerBuilder().JsonCompatible().Build().Serialize(yml!));
                    sw.Flush();

                    ms.Position = 0;
                    using var sr = new StreamReader(ms);
                    return DeserializeJson(sr, type);
                }
            }
#pragma warning restore IDE0063        
        }
    }
}