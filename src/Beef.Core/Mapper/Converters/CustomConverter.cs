// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
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
            ToDest = new CustomConverterToDest(this);
            ToSrce = new CustomConverterToSrce(this);
        }

        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType => typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType => typeof(TDestProperty);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType => typeof(TSrceProperty).IsGenericType && typeof(TSrceProperty).GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(typeof(TSrceProperty)) : typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType => typeof(TDestProperty).IsGenericType && typeof(TDestProperty).GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(typeof(TDestProperty)) : typeof(TDestProperty);

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
        object? IPropertyMapperConverter.ConvertToDest(object? value) => ConvertToDest((TSrceProperty)value!);

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
        object? IPropertyMapperConverter.ConvertToSrce(object? value) => ConvertToSrce((TDestProperty)value!);

        /// <summary>
        /// Gets the <see cref="CustomConverterToDest"/>.
        /// </summary>
        public CustomConverterToDest ToDest { get; }

        /// <summary>
        /// Gets the <see cref="CustomConverterToSrce"/>.
        /// </summary>
        public CustomConverterToSrce ToSrce { get; }

        /// <summary>
        /// Represents a <see cref="CustomConverter{TSrceProperty, TDestProperty}"/> <see cref="IValueConverter{TSourceMember, TDestinationMember}"/> for source to destination conversion.
        /// </summary>
        public class CustomConverterToDest : IValueConverter<TSrceProperty, TDestProperty>
        {
            private readonly CustomConverter<TSrceProperty, TDestProperty> _parent;

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            internal CustomConverterToDest(CustomConverter<TSrceProperty, TDestProperty> parent) => _parent = parent;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="sourceMember"><inheritdoc/></param>
            /// <param name="context"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public TDestProperty Convert(TSrceProperty sourceMember, ResolutionContext context) => _parent.ConvertToDest(sourceMember);
        }

        /// <summary>
        /// Represents a <see cref="CustomConverter{TSrceProperty, TDestProperty}"/> <see cref="IValueConverter{TSourceMember, TDestinationMember}"/> for destination to source conversion.
        /// </summary>
        public class CustomConverterToSrce : IValueConverter<TDestProperty, TSrceProperty>
        {
            private readonly CustomConverter<TSrceProperty, TDestProperty> _parent;

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            internal CustomConverterToSrce(CustomConverter<TSrceProperty, TDestProperty> parent) => _parent = parent;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="sourceMember"><inheritdoc/></param>
            /// <param name="context"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public TSrceProperty Convert(TDestProperty sourceMember, ResolutionContext context) => _parent.ConvertToSrce(sourceMember);
        }
    }
}