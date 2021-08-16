// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Nullable{Int32}"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataNullableInt32IdConverter<TSrceProperty> : CustomConverter<TSrceProperty, int?> where TSrceProperty : ReferenceDataBaseInt32
    {
        private static readonly Lazy<ReferenceDataNullableInt32IdConverter<TSrceProperty>> _default = new(() => new ReferenceDataNullableInt32IdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataNullableInt32IdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataNullableInt32IdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataNullableInt32IdConverter() : base(
            s => s?.Id,
            d => d.HasValue ? (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d.Value)! : default!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Int32)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type 'Int32' to use this Converter.");
        }
    }
}