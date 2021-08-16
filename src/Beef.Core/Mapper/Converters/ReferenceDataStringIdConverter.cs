// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> mapper property value converter that enables <see cref="string"/>-based <see cref="ReferenceDataBase.Id"/> mapping.
    /// </summary>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public sealed class ReferenceDataStringIdConverter<TSrceProperty> : CustomConverter<TSrceProperty, string?> where TSrceProperty : ReferenceDataBaseString
    {
        private static readonly Lazy<ReferenceDataStringIdConverter<TSrceProperty>> _default = new(() => new ReferenceDataStringIdConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ReferenceDataStringIdConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataStringIdConverter{TSrceProperty}"/> class.
        /// </summary>
        public ReferenceDataStringIdConverter() : base(
            s => s?.Id,
            d => (TSrceProperty)ReferenceDataManager.Current[typeof(TSrceProperty)].GetById(d)!)
        {
            var tc = ReferenceDataBase.GetIdTypeCode(typeof(TSrceProperty));
            if (tc != ReferenceDataIdTypeCode.Int32)
                throw new InvalidOperationException($"ReferenceData '{GetType().Name}.Id' has Type of '{tc}'; must be Type '{typeof(string).Name}' to use this Converter.");
        }
    }
}