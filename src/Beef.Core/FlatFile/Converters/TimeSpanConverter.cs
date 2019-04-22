// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <see cref="TimeSpan"/> converter.
    /// </summary>
    public class TimeSpanConverter : ITextValueConverter<TimeSpan>
    {
        /// <summary>
        /// Gets or sets the format string (see <see cref="IFormattable.ToString(string, IFormatProvider)"/>) (used for both parsing and formatting).
        /// </summary>
        public string FormatString { get; set; }

        /// <summary>
        /// Gets or sets the provider used to format and parse the value (see <see cref="IFormattable.ToString(string, IFormatProvider)"/>).
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryFormat(TimeSpan value, out string result)
        {
            result = value.ToString(FormatString, FormatProvider);
            return true;
        }

        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryFormat(object value, out string result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!(value is TimeSpan))
                throw new ArgumentException("Value must be a TimeSpan Type.", nameof(value));

            result = ((TimeSpan)value).ToString(FormatString, FormatProvider);
            return true;
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string str, out TimeSpan result)
        {
            if (string.IsNullOrEmpty(str))
            {
                result = TimeSpan.MinValue;
                return true;
            }

            if (string.IsNullOrEmpty(FormatString))
                return TimeSpan.TryParse(str, FormatProvider, out result);
            else
                return TimeSpan.TryParseExact(str, FormatString, FormatProvider, out result);
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryParse(string str, out object result)
        {
            result = TimeSpan.MinValue;
            TimeSpan val = TimeSpan.MinValue;
            if (!TryParse(str, out val))
                return false;

            result = val;
            return true;
        }
    }
}