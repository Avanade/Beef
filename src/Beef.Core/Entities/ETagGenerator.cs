// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Text;

namespace Beef.Entities
{
    /// <summary>
    /// Provides <see cref="IETag.ETag"/> generator capabilities.
    /// </summary>
    public static class ETagGenerator
    {
        /// <summary>
        /// Represents the divider character where ETag value is made up of multiple parts.
        /// </summary>
        public const char DividerCharacter = '|';

        /// <summary>
        /// Generates an ETag for a value by serializing to JSON and performing an <see cref="System.Security.Cryptography.MD5"/> hash.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="parts">Optional extra part(s) to append to the JSON to include in the underlying hash computation.</param>
        /// <returns>The generated ETag.</returns>
        public static string? Generate<T>(T? value, params string[] parts) where T : class
        {
            if (value == null)
                return null;

            // Where value is IComparable use ToString otherwise JSON serialize.
            var (Value, _) = ConvertToString(value);
            var txt = Value;

            if (parts.Length > 0)
            {
                var sb = new StringBuilder(txt);
                foreach (var ex in parts)
                {
                    sb.Append(DividerCharacter);
                    sb.Append(ex);
                }

                txt = sb.ToString();
            }

            return GenerateHash(txt);
        }

        /// <summary>
        /// Generates a hash of the string using <see cref="System.Security.Cryptography.MD5"/>.
        /// </summary>
        /// <param name="value">The text value to hash.</param>
        /// <returns>The hashed value.</returns>
        /// <remarks>The hash is <b>not</b> intended for <i>Cryptographic</i> usage; therefore using the MD5 algorithm is acceptable.</remarks>
        internal static string? GenerateHash(string? value)
        {
            var buf = Encoding.UTF8.GetBytes(value ?? throw new ArgumentNullException(nameof(value)));
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; not used for security, only used to calculate a hash/etag.
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Converts the value to a corresponding <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string equivalent and whether the contents are a JSON serialized representation.</returns>
        public static (string? Value, bool IsJsonSerialized) ConvertToString(object value)
        {
            if (value == null)
                return (null, false);

            if (value is string str)
                return (str, false);

            if (value is DateTime dte)
                return (dte.ToString("o"), false);

            return (value is IConvertible ic)
                ? (ic.ToString(System.Globalization.CultureInfo.InvariantCulture), false)
                : ((value is IComparable) ? (value.ToString(), false) : (JsonConvert.SerializeObject(value), true));
        }
    }
}