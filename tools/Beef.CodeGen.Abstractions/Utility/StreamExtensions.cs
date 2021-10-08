// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Beef.CodeGen.Utility
{
    /// <summary>
    /// Provides <see cref="Stream"/> extension methods.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="json"/> <see cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="json">The JSON <see cref="Stream"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeJson<T>(this Stream json) where T : class => (T?)DeserializeJson(json, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="json"/> <see cref="Stream"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="Stream"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeJson(this Stream json, Type? type)
        {
            using var jsr = new StreamReader(json ?? throw new ArgumentNullException(nameof(json)));
            using var jr = new JsonTextReader(jsr);
            return JsonSerializer.Create().Deserialize(jr, type);
        }

        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="yaml"/> <see cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="yaml">The YAML <see cref="Stream"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeYaml<T>(this Stream yaml) where T : class => (T?)DeserializeYaml(yaml, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="yaml"/> <see cref="Stream"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="Stream"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeYaml(this Stream yaml, Type? type)
        {
            using var ysr = new StreamReader(yaml ?? throw new ArgumentNullException(nameof(yaml)));
            var yml = new DeserializerBuilder().Build().Deserialize(ysr);

#pragma warning disable IDE0063 // Use simple 'using' statement; cannot as need to be more explicit with managing the close and dispose.
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(new SerializerBuilder().JsonCompatible().Build().Serialize(yml!));
                    sw.Flush();

                    ms.Position = 0;
                    return DeserializeJson(ms, type);
                }
            }
#pragma warning restore IDE0063        
        }
    }
}