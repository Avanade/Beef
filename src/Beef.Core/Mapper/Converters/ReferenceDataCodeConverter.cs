// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="string"/>-based <see cref="ReferenceDataBase.Code"/> conversion.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataCodeConverter<TSrceProperty> : CustomConverter<TSrceProperty, string?> where TSrceProperty : ReferenceDataBase
    {
        private static readonly Lazy<ReferenceDataCodeConverter<TSrceProperty>> _default = new(() => new ReferenceDataCodeConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataCodeConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataCodeConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataCodeConverter() : base(
            s => s?.Code,
            d => (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetByCode(d)!)
        { }
    }
}