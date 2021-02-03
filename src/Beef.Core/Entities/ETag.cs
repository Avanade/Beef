// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Text;

namespace Beef.Entities
{
    /// <summary>
    /// Provides <see cref="IETag.ETag"/> capabilities.
    /// </summary>
    public static class ETag
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

            // Where value is IConvertible use ToString otherwise JSON serialize.
            var txt = (value is IConvertible ic) ? ic.ToString(System.Globalization.CultureInfo.InvariantCulture) : JsonConvert.SerializeObject(value);
            if (parts.Length > 0)
            {
                var sb = new StringBuilder(txt);
                foreach (var ex in parts)
                {
                    sb.Append(DividerCharacter);
                    sb.Append(ex);
                }
            }

            var buf = Encoding.UTF8.GetBytes(txt);
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; not used for security, only used to calculate a hash/etag.
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return Convert.ToBase64String(hash);
        }
    }
}