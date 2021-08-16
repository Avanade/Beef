// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Guid"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataGuidIdConverter<TSrceProperty> : CustomConverter<TSrceProperty, Guid> where TSrceProperty : ReferenceDataBaseGuid
    {
        private static readonly Lazy<ReferenceDataGuidIdConverter<TSrceProperty>> _default = new(() => new ReferenceDataGuidIdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataGuidIdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataGuidIdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataGuidIdConverter() : base(
            s => (s == null) ? Guid.Empty : s.Id,
            d => (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d)!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Guid)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type 'Guid' to use this Converter.");
        }
    }
}