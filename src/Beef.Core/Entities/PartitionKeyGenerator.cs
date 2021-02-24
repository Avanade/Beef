// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Text;

namespace Beef.Entities
{
    /// <summary>
    /// Provides <see cref="IPartitionKey.PartitionKey"/> generator capabilities.
    /// </summary>
    public static class PartitionKeyGenerator
    {
        /// <summary>
        /// Represents the divider character where ETag value is made up of multiple parts.
        /// </summary>
        public const char DividerCharacter = '|';

        /// <summary>
        /// Generates a <see cref="IPartitionKey.PartitionKey"/> by serializing to JSON and performing an <see cref="System.Security.Cryptography.MD5"/> hash.
        /// </summary>
        /// <param name="args">The value or values that represent the <see cref="IPartitionKey.PartitionKey"/>.</param>
        /// <returns>The generated PartitionKey.</returns>
        public static string? Generate(params object[] args)
        {
            if (args == null || args.Length == 0 || (args.Length == 1 && args[0] == null))
                return null;

            if (args.Length == 1 && args[0] is string sarg)
                return sarg;

            var sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append((arg is IConvertible ic) ? ic.ToString(System.Globalization.CultureInfo.InvariantCulture) : JsonConvert.SerializeObject(arg));
                sb.Append(DividerCharacter);
            }

            var buf = Encoding.UTF8.GetBytes(sb.ToString());
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; not used for security, only used to calculate a hash/etag.
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return Convert.ToBase64String(hash);
        }
    }
}