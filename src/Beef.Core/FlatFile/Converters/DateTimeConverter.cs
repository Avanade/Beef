// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Globalization;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <see cref="DateTime"/> converter.
    /// </summary>
    public class DateTimeConverter : ITextValueConverter<DateTime>
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
        /// Gets or sets the <see cref="DateTimeStyles"/> used to parse the value.
        /// </summary>
        public DateTimeStyles DateTimeStyles { get; set; }

        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryFormat(DateTime value, out string result)
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

            if (!(value is DateTime))
                throw new ArgumentException("Value must be a DateTime Type.", nameof(value));

            result = ((DateTime)value).ToString(FormatString, FormatProvider);
            return true;
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string str, out DateTime result)
        {
            if (string.IsNullOrEmpty(str))
            {
                result = DateTime.MinValue;
                return true;
            }

            if (string.IsNullOrEmpty(FormatString))
                return DateTime.TryParse(str, FormatProvider, DateTimeStyles, out result);
            else
                return DateTime.TryParseExact(str, FormatString, FormatProvider, DateTimeStyles, out result);
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryParse(string str, out object result)
        {
            result = DateTime.MinValue;
            DateTime val = DateTime.MinValue;
            if (!TryParse(str, out val))
                return false;

            result = val;
            return true;
        }
    }
}