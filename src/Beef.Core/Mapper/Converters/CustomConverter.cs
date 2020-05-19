// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a custom converter where the conversion logic is explicitly defined.
    /// </summary>
    public class CustomConverter<TSrceProperty, TDestProperty> : IPropertyMapperConverter<TSrceProperty, TDestProperty>
    {
        private readonly Func<TSrceProperty, TDestProperty> _convertToDest;
        private readonly Func<TDestProperty, TSrceProperty> _convertToSrce;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConverter{TSrceProperty, TDestProperty}"/> class.
        /// </summary>
        /// <param name="convertToDest">The function to convert a <typeparamref name="TSrceProperty"/> to a <typeparamref name="TDestProperty"/>.</param>
        /// <param name="convertToSrce">The function to convert a <typeparamref name="TDestProperty"/> to a <typeparamref name="TSrceProperty"/>.</param>
        public CustomConverter(Func<TSrceProperty, TDestProperty> convertToDest, Func<TDestProperty, TSrceProperty> convertToSrce)
        {
            _convertToDest = Check.NotNull(convertToDest, nameof(convertToDest));
            _convertToSrce = Check.NotNull(convertToSrce, nameof(convertToSrce));
        }

        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType { get; } = typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType { get; } = typeof(TDestProperty);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = typeof(TDestProperty);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public TDestProperty ConvertToDest(TSrceProperty value) => _convertToDest(value);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        object? IPropertyMapperConverter.ConvertToDest(object? value)
        {
            return ConvertToDest((TSrceProperty)value!);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public TSrceProperty ConvertToSrce(TDestProperty value) => _convertToSrce(value);

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        object? IPropertyMapperConverter.ConvertToSrce(object? value)
        {
            return ConvertToSrce((TDestProperty)value!);
        }
    }
}