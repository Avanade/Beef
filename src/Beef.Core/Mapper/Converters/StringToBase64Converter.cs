// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="string"/> to <see cref="byte"/> <see cref="Array"/> (uses <see cref="Convert.FromBase64String(string)"/> and <see cref="Convert.ToBase64String(byte[])"/>).
    /// </summary>
    public class StringToBase64Converter : CustomConverter<string?, byte[]>
    {
        private static readonly Lazy<StringToBase64Converter> _default = new(() => new StringToBase64Converter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static StringToBase64Converter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JObjectToJsonConverter"/> class.
        /// </summary>
        public StringToBase64Converter() : base(
            s => s == null ? Array.Empty<byte>() : Convert.FromBase64String(s),
            d => Convert.ToBase64String(d))
        { }
    }
}