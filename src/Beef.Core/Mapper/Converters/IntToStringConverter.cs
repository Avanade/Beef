// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="int"/> to <see cref="string"/> converter.
    /// </summary>
    public class IntToStringConverter : CustomConverter<int, string?>
    {
        private static readonly Lazy<IntToStringConverter> _default = new(() => new IntToStringConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static IntToStringConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntToStringConverter"/> class.
        /// </summary>
        public IntToStringConverter() : base(
            s => s.ToString("0000000000", System.Globalization.CultureInfo.InvariantCulture),
            d => string.IsNullOrEmpty(d) ? 0 : int.Parse(d, System.Globalization.CultureInfo.InvariantCulture))
        { }
    }

    /// <summary>
    /// Represents a <see cref="Nullable"/> <see cref="int"/> to <see cref="string"/> converter.
    /// </summary>
    public class NullableIntToStringConverter : CustomConverter<int?, string?>
    {
        private static readonly Lazy<NullableIntToStringConverter> _default = new(() => new NullableIntToStringConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static NullableIntToStringConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntToStringConverter"/> class.
        /// </summary>
        public NullableIntToStringConverter() : base(
            s => s?.ToString("0000000000", System.Globalization.CultureInfo.InvariantCulture),
            d => string.IsNullOrEmpty(d) ? (int?)null : int.Parse(d, System.Globalization.CultureInfo.InvariantCulture))
        { }
    }
}