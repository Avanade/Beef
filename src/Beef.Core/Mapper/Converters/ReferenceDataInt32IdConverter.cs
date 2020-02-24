// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Int32"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataInt32IdConverter<TSrceProperty> : Singleton<ReferenceDataInt32IdConverter<TSrceProperty>>, IPropertyMapperConverter<TSrceProperty, int> where TSrceProperty : ReferenceDataBaseInt
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ReferenceDataInt32IdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataInt32IdConverter()
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Int32)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc.ToString()}'; must be Type 'Int32' to use this Converter.");
        }

        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType { get; } = typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType { get; } = typeof(int);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(TSrceProperty)) ?? typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(int)) ?? typeof(int);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public int ConvertToDest(TSrceProperty value)
        {
            return value == null ? 0 : value.Id;
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public TSrceProperty ConvertToSrce(int value)
        {
            return (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(value)!;
        }

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
        object? IPropertyMapperConverter.ConvertToSrce(object? value)
        {
            return ConvertToSrce((int)value!);
        }
    }
}