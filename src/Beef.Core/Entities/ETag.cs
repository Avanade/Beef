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
        /// Generates an ETag for a value by serializing to JSON and performing an <see cref="System.Security.Cryptography.MD5"/> hash.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="extra">Optional extra text(s) to append to the JSON to include in the underlying hash computation.</param>
        /// <returns>The generated ETag.</returns>
        public static string? Generate<T>(T? value, params string[] extra) where T : class
        {
            if (value == null)
                return null;

            var json = JsonConvert.SerializeObject(value);
            if (extra.Length > 0)
            {
                var sb = new StringBuilder(json);
                foreach (var ex in extra)
                {
                    sb.Append("----");
                    sb.Append(ex);
                }
            }

            var buf = Encoding.UTF8.GetBytes(json);
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; not used for security, only used to calculate a hash/etag.
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return Convert.ToBase64String(hash);
        }
    }
}