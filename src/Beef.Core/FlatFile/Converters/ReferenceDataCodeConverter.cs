// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> text value converter that enables <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Code"/> conversion.
    /// </summary>
    /// <typeparam name="T">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    public class ReferenceDataCodeConverter<T> : ITextValueConverter<T> where T : ReferenceDataBase
    {
        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryFormat(T value, out string result)
        {
            result = null;
            if (value == null)
                return true;

            if (!value.IsValid)
                return false;

            result = value.Code;
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
            return TryFormat((T)value, out result);
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string str, out T result)
        {
            result = null;
            if (str == null)
                return true;

            result = (T)ReferenceDataManager.Current[typeof(T)].GetByCode(str);
            return result != null && result.IsValid;
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryParse(string str, out object result)
        {
            var tp = TryParse(str, out T res);
            result = res;
            return tp;
        }
    }
}
