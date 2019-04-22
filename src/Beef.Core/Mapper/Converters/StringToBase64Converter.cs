// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="string"/> to <see cref="byte"/> <see cref="Array"/> (uses <see cref="Convert.FromBase64String(string)"/> and <see cref="Convert.ToBase64String(byte[])"/>).
    /// </summary>
    public class StringToBase64Converter : Singleton<StringToBase64Converter>, IPropertyMapperConverter<string, byte[]>
    {
        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType { get; } = typeof(string);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType { get; } = typeof(byte[]);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = typeof(string);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = typeof(byte[]);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public byte[] ConvertToDest(string value)
        {
            if (value == null)
                return new byte[0];

            return Convert.FromBase64String(value);
        }

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public object ConvertToDest(object value)
        {
            return ConvertToDest((string)value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public string ConvertToSrce(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public object ConvertToSrce(object value)
        {
            return ConvertToSrce((byte[])value);
        }
    }
}
