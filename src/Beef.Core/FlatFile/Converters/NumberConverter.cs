// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Reflectors;
using System;
using System.Globalization;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Represents a <b>number</b> converter.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to convert; must be numeric.</typeparam>
    public class NumberConverter<T> : ITextValueConverter<T> where T : IFormattable
    {
        private readonly TypeCode _typeCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberConverter{T}"/> class.
        /// </summary>
        public NumberConverter()
        {
            _typeCode = FileColumnReflector.GetTypeCode(typeof(T));
            switch (_typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    break;

                default:
                    throw new InvalidOperationException("Type T must be a numeric Type.");
            }
        }

        /// <summary>
        /// Gets or sets the format string (see <see cref="IFormattable.ToString(string, IFormatProvider)"/>).
        /// </summary>
        public string FormatString { get; set; }

        /// <summary>
        /// Gets or sets the provider used to format and parse the value (see <see cref="IFormattable.ToString(string, IFormatProvider)"/>).
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="NumberStyles"/> used to parse the value.
        /// </summary>
        public NumberStyles NumberStyles { get; set; }

        /// <summary>
        /// Formats the value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The formatted <see cref="string"/>.</param>
        /// <returns><c>true</c> where <paramref name="value"/> was formatted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryFormat(T value, out string result)
        {
            result = ((IFormattable)value).ToString(FormatString, FormatProvider);
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

            if (!(value is T))
                throw new ArgumentException("Value must be a numeric Type.", nameof(value));

            return (TryFormat((T)value, out result));
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string str, out T result)
        {
            result = default(T);
            if (string.IsNullOrEmpty(str))
                return true;

            switch (_typeCode)
            {
                case TypeCode.Decimal:
                    Decimal mval = 0m;
                    if (!Decimal.TryParse(str, NumberStyles, FormatProvider, out mval))
                        return false;

                    result = (T)Convert.ChangeType(mval, typeof(T));
                    return true;

                case TypeCode.Double:
                    Double dval = 0d;
                    if (!Double.TryParse(str, NumberStyles, FormatProvider, out dval))
                        return false;

                    result = (T)Convert.ChangeType(dval, typeof(T));
                    return true;

                case TypeCode.Int16:
                    Int16 i16val = 0;
                    if (!Int16.TryParse(str, NumberStyles, FormatProvider, out i16val))
                        return false;

                    result = (T)Convert.ChangeType(i16val, typeof(T));
                    return true;

                case TypeCode.Int32:
                    Int32 i32val = 0;
                    if (!Int32.TryParse(str, NumberStyles, FormatProvider, out i32val))
                        return false;

                    result = (T)Convert.ChangeType(i32val, typeof(T));
                    return true;

                case TypeCode.Int64:
                    Int64 i64val = 0;
                    if (!Int64.TryParse(str, NumberStyles, FormatProvider, out i64val))
                        return false;

                    result = (T)Convert.ChangeType(i64val, typeof(T));
                    return true;

                case TypeCode.Single:
                    Single sival = 0.0f;
                    if (!Single.TryParse(str, NumberStyles, FormatProvider, out sival))
                        return false;

                    result = (T)Convert.ChangeType(sival, typeof(T));
                    return true;

                case TypeCode.Byte:
                    Byte byval = Byte.MinValue;
                    if (!Byte.TryParse(str, NumberStyles, FormatProvider, out byval))
                        return false;

                    result = (T)Convert.ChangeType(byval, typeof(T));
                    return true;

                case TypeCode.SByte:
                    SByte sbyval = SByte.MinValue;
                    if (!SByte.TryParse(str, NumberStyles, FormatProvider, out sbyval))
                        return false;

                    result = (T)Convert.ChangeType(sbyval, typeof(T));
                    return true;

                case TypeCode.UInt16:
                    UInt16 ui16val = 0;
                    if (!UInt16.TryParse(str, NumberStyles, FormatProvider, out ui16val))
                        return false;

                    result = (T)Convert.ChangeType(ui16val, typeof(T));
                    return true;

                case TypeCode.UInt32:
                    UInt32 ui32val = 0;
                    if (!UInt32.TryParse(str, NumberStyles, FormatProvider, out ui32val))
                        return false;

                    result = (T)Convert.ChangeType(ui32val, typeof(T));
                    return true;

                case TypeCode.UInt64:
                    UInt64 ui64val = 0;
                    if (!UInt64.TryParse(str, NumberStyles, FormatProvider, out ui64val))
                        return false;

                    result = (T)Convert.ChangeType(ui64val, typeof(T));
                    return true;

                default:
                    throw new InvalidOperationException("Type T must be a numeric Type.");
            }
        }

        /// <summary>
        /// Parses a value from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns><c>true</c> where <paramref name="str"/> was converted sucessfully; otherwise, <c>false</c>.</returns>
        bool ITextValueConverter.TryParse(string str, out object result)
        {
            result = default(T);
            if (!TryParse(str, out T val))
                return false;

            result = val;
            return true;
        }
    }
}
