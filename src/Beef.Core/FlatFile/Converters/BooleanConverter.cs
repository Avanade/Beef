// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <see cref="bool"/> converter (see <see cref="TrueValues"/> and <see cref="FalseValues"/> for configuration).
    /// </summary>
    public class BooleanConverter : ITextValueConverter<bool>
    {
        /// <summary>
        /// The default list of <c>true</c> values ('Y', 'T', '1', 'True', 'Yes' and 'X').
        /// </summary>
        public static readonly string[] DefaultTrueValues = new string[] { "Y", "T", "1", "True", "Yes", "X" };

        /// <summary>
        /// The default list of <c>false</c> values ('N', 'F', '0', 'False', 'No' and '').
        /// </summary>
        public static readonly string[] DefaultFalseValues = new string[] { "N", "F", "0", "False", "No", "" };

        /// <summary>
        /// Gets or sets the list of valid <c>true</c> values (defaults to <see cref="DefaultTrueValues"/>).
        /// </summary>
        /// <remarks>When performing a <see cref="TryFormat(bool, out string)"/> the first value in the array is always used.</remarks>
        public List<string> TrueValues { get; } = new List<string>(DefaultTrueValues);

        /// <summary>
        /// Gets or sets the list of valid <c>false</c> values (defaults to <see cref="DefaultFalseValues"/>).
        /// </summary>
        /// <remarks>When performing a <see cref="TryFormat(bool, out string)"/> the first value in the array is always used.</remarks>
        public List<string> FalseValues { get; } = new List<string>(DefaultFalseValues);

        /// <summary>
        /// Gets or sets the <see cref="StringComparer"/> for comparisons (defaults to <see cref="StringComparer.Ordinal"/>);
        /// </summary>
        public StringComparer StringComparer { get; set; } = StringComparer.Ordinal;

        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryFormat(bool value, out string result)
        {
            result = value ? TrueValues[0] : FalseValues[0];
            return true;
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string str, out bool result)
        {
            result = false;
            if (FalseValues.Contains(str, StringComparer))
                return true;

            if (!TrueValues.Contains(str, StringComparer))
                return false;

            result = true;
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
            return TryFormat((bool)value, out result);
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryParse(string str, out object result)
        {
            result = false;
            if (!TryParse(str, out bool res))
                return false;

            result = res;
            return true;
        }
    }
}
