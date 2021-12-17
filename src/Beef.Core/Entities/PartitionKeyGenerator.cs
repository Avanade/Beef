// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Text;

namespace Beef.Entities
{
    /// <summary>
    /// Provides <see cref="IPartitionKey.PartitionKey"/> generator capabilities.
    /// </summary>
    public static class PartitionKeyGenerator
    {
        /// <summary>
        /// Generates a <see cref="IPartitionKey.PartitionKey"/> by serializing to JSON and performing an <see cref="System.Security.Cryptography.SHA256"/> hash.
        /// </summary>
        /// <param name="args">The value or values that represent the <see cref="IPartitionKey.PartitionKey"/>.</param>
        /// <returns>The generated PartitionKey.</returns>
        public static string? Generate(params object[] args)
        {
            if (args == null || args.Length == 0 || (args.Length == 1 && args[0] == null))
                return null;

            var (Value, IsJsonSerialized) = ETagGenerator.ConvertToString(args[0]);
            if (!IsJsonSerialized && args.Length == 1)
                return Value;

            var sb = new StringBuilder(Value);
            for (int i = 1; i < args.Length; i++)
            {
                sb.Append(ETagGenerator.DividerCharacter);
                sb.Append(ETagGenerator.ConvertToString(args[i]).Value);
            }

            return ETagGenerator.GenerateHash(sb.ToString());
        }
    }
}