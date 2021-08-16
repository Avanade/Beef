// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Nullable{Int64}"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataNullableInt64IdConverter<TSrceProperty> : CustomConverter<TSrceProperty, long?> where TSrceProperty : ReferenceDataBaseInt64
    {
        private static readonly Lazy<ReferenceDataNullableInt64IdConverter<TSrceProperty>> _default = new(() => new ReferenceDataNullableInt64IdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataNullableInt64IdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataNullableInt64IdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataNullableInt64IdConverter() : base(
            s => s?.Id,
            d => d.HasValue ? (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d.Value)! : default!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Int64)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type 'Int64' to use this Converter.");
        }
    }
}