// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Beef.Entities
{
    /// <summary>
    /// Provides functions to clean a specified value.
    /// </summary>
    /// <seealso cref="ICleanUp"/>.
    public static class Cleaner
    {
        /// <summary>
        /// Represents the default <see cref="StringTrim"/> value (see <see cref="StringTrim.End"/>).
        /// </summary>
        public const StringTrim DefaultStringTrim = StringTrim.End;

        /// <summary>
        /// Represents the default <see cref="StringTransform"/> value (see <see cref="StringTransform.EmptyToNull"/>).
        /// </summary>
        public const StringTransform DefaultStringTransform = StringTransform.EmptyToNull;

        /// <summary>
        /// Represents the default <see cref="DateTimeTransform"/> value (see <see cref="DateTimeTransform.None"/>).
        /// </summary>
        public const DateTimeTransform DefaultDateTimeTransform = DateTimeTransform.None;

        /// <summary>
        /// Cleans a <see cref="String"/>.
        /// </summary>
        /// <param name="value">The value to clean.</param>
        /// <returns>The cleaned value.</returns>
        /// <remarks>The <paramref name="value"/> will be trimmed and transformed using the respective default <see cref="DefaultStringTrim"/> and 
        /// <see cref="DefaultStringTransform"/> values.</remarks>
        public static string Clean(string value)
        {
            return Clean(value, DefaultStringTrim, DefaultStringTransform);
        }

        /// <summary>
        /// Cleans a <see cref="String"/> and optionally allows <see cref="String.Empty"/>.
        /// </summary>
        /// <param name="value">The value to clean.</param>
        /// <param name="trim">The <see cref="StringTrim"/> (defaults to <see cref="DefaultStringTrim"/>).</param>
        /// <param name="transform">The <see cref="StringTransform"/> (defaults to <see cref="DefaultStringTransform"/>).</param>
        /// <returns>The cleaned value.</returns>
        public static string Clean(string value, StringTrim trim = DefaultStringTrim, StringTransform transform = DefaultStringTransform)
        {
            // Handle a null string.
            if (value == null)
            {
                if (transform == StringTransform.NullToEmpty)
                    return string.Empty;
                else
                    return value;
            }

            // Trim the string.
            string tmp = null;
            switch (trim)
            {
                case StringTrim.Both:
                    tmp = value.Trim();
                    break;

                case StringTrim.Start:
                    tmp = value.TrimStart();
                    break;

                case StringTrim.End:
                    tmp = value.TrimEnd();
                    break;

                default:
                    tmp = value;
                    break;
            }

            // Transform the string.
            switch (transform)
            {
                case StringTransform.EmptyToNull:
                    return (tmp.Length == 0) ? null : tmp;

                case StringTransform.NullToEmpty:
                    return tmp ?? string.Empty;

                default:
                    return tmp;
            }
        }

        /// <summary>
        /// Cleans a value.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to clean.</param>
        /// <returns>The cleaned value.</returns>
        /// <remarks>A <paramref name="value"/> of <see cref="Type"/> <see cref="String"/> will leverage <see cref="Clean{String}"/>, <see cref="Type"/> <see cref="DateTime"/>
        /// leverage <see cref="Clean(DateTime, DateTimeTransform)"/>, and a <see cref="Type"/> of <see cref="IReferenceData"/> are considered special and as such are never cleaned.</remarks>
        public static T Clean<T>(T value)
        {
            if (value is string)
            {
                string val = (string)Convert.ChangeType(value, typeof(string), CultureInfo.CurrentCulture);
                return (T)Convert.ChangeType(Clean(val, DefaultStringTrim, DefaultStringTransform), typeof(T), CultureInfo.CurrentCulture);
            }
            else if (value is DateTime)
            {
                DateTime val = (DateTime)Convert.ChangeType(value, typeof(DateTime), CultureInfo.CurrentCulture);
                return (T)Convert.ChangeType(Clean(val, DefaultDateTimeTransform), typeof(DateTime), CultureInfo.CurrentCulture);
            }
            else if (value is IReferenceData)
            {
                // Reference Data values should not be cleaned.
                return value;
            }

            if (value is ICleanUp ic)
                ic.CleanUp();

            return value;
        }

        /// <summary>
        /// Cleans a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The value to clean.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DefaultDateTimeTransform"/>).</param>
        /// <returns>The cleaned value.</returns>
        public static DateTime Clean(DateTime value, DateTimeTransform transform = DefaultDateTimeTransform)
        {
            switch (transform)
            {
                case DateTimeTransform.DateOnly:
                    return DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

                case DateTimeTransform.DateTimeLocal:
                    if (value == DateTime.MinValue || value == DateTime.MaxValue)
                        return DateTime.SpecifyKind(value, DateTimeKind.Local);
                    else
                        return (value.Kind == DateTimeKind.Local) ? value : TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Local);

                case DateTimeTransform.DateTimeUtc:
                    if (value == DateTime.MinValue || value == DateTime.MaxValue)
                        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
                    else
                        return (value.Kind == DateTimeKind.Utc) ? value : TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Utc);

                case DateTimeTransform.DateTimeUnspecified:
                    return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }

            return value;
        }

        /// <summary>
        /// Cleans a <see cref="Nullable{DateTime}"/> value.
        /// </summary>
        /// <param name="value">The value to clean.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DefaultDateTimeTransform"/>).</param>
        /// <returns>The cleaned value.</returns>
        public static DateTime? Clean(DateTime? value, DateTimeTransform transform = DefaultDateTimeTransform)
        {
            if (value == null || !value.HasValue)
                return value;

            return Clean(value.Value, transform);
        }

        /// <summary>
        /// Cleans a value where it implements <see cref="ICleanUp"/>.
        /// </summary>
        /// <param name="value">The value to clean.</param>
        public static void CleanUp(object value)
        {
            if (value != null && value is ICleanUp)
                ((ICleanUp)value).CleanUp();
        }

        /// <summary>
        /// Indicates whether a value is considered initial.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> indicates that the value is initial; otherwise, <c>false</c>.</returns>
        public static bool IsInitial<T>(T value)
        {
            if (value == null || Comparer<T>.Default.Compare(value, default(T)) == 0)
                return true;

            if (value is ICleanUp ic)
                return ic.IsInitial;

            return false;
        }
    }
}
