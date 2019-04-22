// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> text value converter that enables <see cref="ReferenceDataBase"/> mapping <see cref="ReferenceDataBase.SetMapping{T}(string, T)"/> conversion.
    /// </summary>
    /// <typeparam name="T">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    public abstract class ReferenceDataMappingConverter<T> : ITextValueConverter<T> where T : ReferenceDataBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataMappingConverter{T}"/> class.
        /// </summary>
        /// <param name="name">The <see cref="ReferenceDataBase"/> mapping name.</param>
        protected ReferenceDataMappingConverter(string name)
        {
            Name = !string.IsNullOrEmpty(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> mapping name.
        /// </summary>
        public string Name { get; private set; }

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

            if (!value.TryGetMapping(Name, out IComparable mval))
                throw new InvalidOperationException($"The ReferenceData does not containing a '{Name}' Mapping property/value.");

            result = (string)mval;
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

            result = (T)ReferenceDataManager.Current[typeof(T)].GetByMappingValue(Name, str);
            return result != null;
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
