// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="Int64"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataInt64IdConverter<TSrceProperty> : CustomConverter<TSrceProperty, long> where TSrceProperty : ReferenceDataBaseInt64
    {
        private static readonly Lazy<ReferenceDataInt64IdConverter<TSrceProperty>> _default = new(() => new ReferenceDataInt64IdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataInt64IdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataInt64IdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataInt64IdConverter() : base(
            s => s?.Id ?? 0,
            d => (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d)!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Int64)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type 'Int64' to use this Converter.");
        }
    }
}