// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a property mapper value converter.
    /// </summary>
    public interface IPropertyMapperConverter
    {
        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type SrceType { get; }

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type DestType { get; }

        /// <summary>
        /// Gets the underlying source property <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type SrceUnderlyingType { get; }

        /// <summary>
        /// Gets the underlying destination property <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type DestUnderlyingType { get; }

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        object? ConvertToDest(object? value);

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        object? ConvertToSrce(object? value);
    }

    /// <summary>
    /// Represents a typed mapper property value converter.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
    public interface IPropertyMapperConverter<TSrceProperty, TDestProperty> : IPropertyMapperConverter
    {
        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        TDestProperty ConvertToDest(TSrceProperty value);

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        TSrceProperty ConvertToSrce(TDestProperty value);
    }
}