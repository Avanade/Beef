// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents an <see cref="JToken"/> to JSON <see cref="string"/> converter.
    /// </summary>
    public class JTokenToJsonConverter : Singleton<JTokenToJsonConverter>, IPropertyMapperConverter<JToken, string>
    {
        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType { get; } = typeof(JToken);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType { get; } = typeof(string);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(JToken)) ?? typeof(JToken);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = typeof(string);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public string ConvertToDest(JToken value)
        {
            if (value == null)
                return null;

            return value.ToString(Formatting.None);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public JToken ConvertToSrce(string value)
        {
            return String.IsNullOrEmpty(value) ? default : JToken.Parse(value);
        }

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public object ConvertToDest(object value)
        {
            return ConvertToDest((JToken)value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public object ConvertToSrce(object value)
        {
            return ConvertToSrce((string)value);
        }
    }
}