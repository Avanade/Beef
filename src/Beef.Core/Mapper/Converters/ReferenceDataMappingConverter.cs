// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="ReferenceDataBase"/> mapping <see cref="ReferenceDataBase.SetMapping{T}(string, T)"/> conversion.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="Default"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
    public abstract class ReferenceDataMappingConverter<TDefault, TSrceProperty, TDestProperty> : IPropertyMapperConverter<TSrceProperty, TDestProperty> 
        where TDefault : ReferenceDataMappingConverter<TDefault, TSrceProperty, TDestProperty>, new()
        where TSrceProperty : ReferenceDataBase
        where TDestProperty : IComparable
    {
        private static readonly Lazy<TDefault> _default = new Lazy<TDefault>(() => new TDefault(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static TDefault Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataMappingConverter{TDefault, TSrceProperty, TDestProperty}"/> class.
        /// </summary>
        /// <param name="name">The <see cref="ReferenceDataBase"/> mapping name.</param>
        protected ReferenceDataMappingConverter(string name)
        {
            Name = !string.IsNullOrEmpty(name) ? name :  throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> mapping name.
        /// </summary>
        public string Name { get; private set; }

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
            if (value == null)
                return default(TDestProperty);

            ReferenceDataConverterUtils.CheckIsValid(((IPropertyMapperConverter)this).SrceType, value);
            if (!value.TryGetMapping(Name, out IComparable mval))
                throw new InvalidOperationException($"The ReferenceData does not containing a '{Name}' Mapping property/value.");

            return (TDestProperty)mval;
        }

        /// <summary>
        /// Converts the destination <paramref name="value"/> to the source equivalent.
        /// </summary>
        /// <param name="value">The destination value.</param>
        /// <returns>The source value.</returns>
        public TSrceProperty ConvertToSrce(TDestProperty value)
        {
            if (value == null)
                return null;

            return (TSrceProperty)ReferenceDataConverterUtils.CheckConverted(((IPropertyMapperConverter)this).SrceType, ReferenceDataManager.Current[typeof(TSrceProperty)].GetByMappingValue(Name, value), value);
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
