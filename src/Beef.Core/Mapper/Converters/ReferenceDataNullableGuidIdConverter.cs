// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Nullable{Guid}"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataNullableGuidIdConverter<TSrceProperty> : CustomConverter<TSrceProperty, Guid?> where TSrceProperty : ReferenceDataBaseGuid
    {
        private static readonly Lazy<ReferenceDataNullableGuidIdConverter<TSrceProperty>> _default = new(() => new ReferenceDataNullableGuidIdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataNullableGuidIdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataNullableGuidIdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataNullableGuidIdConverter() : base(
            s => s?.Id,
            d => d.HasValue ? (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d.Value)! : default!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Guid)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type 'Guid' to use this Converter.");
        }
    }
}