// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents an <see cref="object"/> to JSON <see cref="string"/> converter.
    /// </summary>
    /// <typeparam name="TSrceProperty"></typeparam>
    public class ObjectToJsonConverter<TSrceProperty> : Singleton<ObjectToJsonConverter<TSrceProperty>>, IPropertyMapperConverter<TSrceProperty, string>
    {
        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType { get; } = typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType { get; } = typeof(string);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(TSrceProperty)) ?? typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = typeof(String);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public string ConvertToDest(TSrceProperty value)
        {
            if (value == null)
                return null;

            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public TSrceProperty ConvertToSrce(string value)
        {
            return String.IsNullOrEmpty(value) ? default(TSrceProperty) : JsonConvert.DeserializeObject<TSrceProperty>(value);
        }

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public object ConvertToDest(object value)
        {
            return ConvertToDest((TSrceProperty)value);
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