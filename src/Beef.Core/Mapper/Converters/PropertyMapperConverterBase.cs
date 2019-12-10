// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using System;
using System.Collections.Generic;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a mapper property value converter backed by a <see cref="TwoKeySetCache{TKey1, TKey2}"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="PropertyMapperConverterBase{T, TSrceProperty, TDestProperty}"/>; being itself for singleton purposes.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
    public abstract class PropertyMapperConverterBase<T, TSrceProperty, TDestProperty>
        : TwoKeySetCache<TSrceProperty, TDestProperty>, IPropertyMapperConverter<TSrceProperty, TDestProperty>
        where T : PropertyMapperConverterBase<T, TSrceProperty, TDestProperty>, new()
    {
#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, results in a consistent static defined default instance without the need to specify generic type to consume.
        /// <summary>
        /// Gets or sets the default instance.
        /// </summary>
        public static T Default { get; set; } = new T();
#pragma warning restore CA1000

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeySetCache{TColl, TItem}"/> class.
        /// </summary>
        /// <param name="loadCache">The function that is responsible for loading the collection (expects an enumerable <see cref="Tuple{TKey1, TKey2}"/>).</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to the class <see cref="Type"/> name.</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        protected PropertyMapperConverterBase(Func<object, IEnumerable<Tuple<TSrceProperty, TDestProperty>>> loadCache, string policyKey = null, object data = null)
            : base(loadCache, policyKey, data)
        { }

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
        Type IPropertyMapperConverter.SrceUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(TSrceProperty)) ?? typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType { get; } = Nullable.GetUnderlyingType(typeof(TDestProperty)) ?? typeof(TDestProperty);

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        public TDestProperty ConvertToDest(TSrceProperty value)
        {
            return GetByKey1(value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public TSrceProperty ConvertToSrce(TDestProperty value)
        {
            return GetByKey2(value);
        }

        /// <summary>
        /// Converts the source <paramref name="value"/> to the destination equivalent.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>The destination value.</returns>
        object IPropertyMapperConverter.ConvertToDest(object value)
        {
            return ConvertToDest((TSrceProperty)value);
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        object IPropertyMapperConverter.ConvertToSrce(object value)
        {
            return ConvertToSrce((TDestProperty)value);
        }
    }
}
