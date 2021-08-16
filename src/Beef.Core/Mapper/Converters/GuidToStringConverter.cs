// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="Guid"/> to <see cref="string"/> converter.
    /// </summary>
    public class GuidToStringConverter : CustomConverter<Guid, string?>
    {
        private static readonly Lazy<GuidToStringConverter> _default = new(() => new GuidToStringConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static GuidToStringConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidToStringConverter"/> class.
        /// </summary>
        public GuidToStringConverter() : base(
            s => s.ToString(),
            d => string.IsNullOrEmpty(d) ? Guid.Empty : Guid.Parse(d))
        { }
    }

    /// <summary>
    /// Represents a <see cref="Nullable{Guid}"/> to <see cref="string"/> converter.
    /// </summary>
    public class NullableGuidToStringConverter : CustomConverter<Guid?, string?>
    {
        private static readonly Lazy<NullableGuidToStringConverter> _default = new(() => new NullableGuidToStringConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static NullableGuidToStringConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidToStringConverter"/> class.
        /// </summary>
        public NullableGuidToStringConverter() : base(
            s => s?.ToString(),
            d => string.IsNullOrEmpty(d) ? (Guid?)null : Guid.Parse(d))
        { }
    }
}