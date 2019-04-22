// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a text value converter.
    /// </summary>
    public interface ITextValueConverter
    {
        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        bool TryFormat(object value, out string result);

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool TryParse(string str, out object result);
    }

    /// <summary>
    /// Represents a text value converter for a specified <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
    public interface ITextValueConverter<T> : ITextValueConverter
    {
        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        bool TryFormat(T value, out string result);

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool TryParse(string str, out T result);
    }
}
