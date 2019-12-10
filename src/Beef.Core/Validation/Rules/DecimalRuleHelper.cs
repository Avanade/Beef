using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Represents a helper for the <see cref="DecimalRule{TEntity, TProperty}"/>.
    /// </summary>
    public static class DecimalRuleHelper
    {
        /// <summary>
        /// Checks the <paramref name="value"/> for the max digits.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="maxDigits">The maximum digits (including <paramref name="decimalPlaces"/>).</param>
        /// <param name="decimalPlaces">The maximum number of decimal places.</param>
        /// <returns><c>true</c> where valid; otherwise, <c>false</c>.</returns>
        public static bool CheckMaxDigits(decimal value, int maxDigits, int? decimalPlaces = null)
        {
            if (maxDigits < 1)
                throw new ArgumentException("MaxDigits must be 1 or greater.", nameof(maxDigits));

            if (decimalPlaces.HasValue && decimalPlaces.Value < 0)
                throw new ArgumentException("DecimalPlaces cannot be negative.", nameof(decimalPlaces));

            if (value == 0)
                return true;

            return CheckMaxDigits(maxDigits, decimalPlaces, CalcIntegerPartLength(value), CalcFractionalPartLength(value));
        }

        /// <summary>
        /// Checks the max digits.
        /// </summary>
        internal static bool CheckMaxDigits(int maxDigits, int? decimalPlaces, int il, int dp)
        {
            return (il + (decimalPlaces ?? dp)) <= maxDigits;
        }

        /// <summary>
        /// Checks the <paramref name="value"/> to determine whether the fractional-part length is greater than the specified maximum number of decimal places.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="decimalPlaces">The maximum number of decimal places.</param>
        /// <returns><c>true</c> where valid; otherwise, <c>false</c>.</returns>
        public static bool CheckDecimalPlaces(decimal value, int decimalPlaces)
        {
            if (decimalPlaces < 0)
                throw new ArgumentException("DecimalPlaces cannot be negative.", nameof(decimalPlaces));

            if (value == 0)
                return true;

            return CheckDecimalPlaces(decimalPlaces, CalcFractionalPartLength(value));
        }

        /// <summary>
        /// Checks the decimal places.
        /// </summary>
        internal static bool CheckDecimalPlaces(int decimalPlaces, int dp)
        {
            return dp <= decimalPlaces;
        }

        /// <summary>
        /// Calculates the integer-part length for a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The integer-part length.</returns>
        public static int CalcIntegerPartLength(decimal value)
        {
            if (value == 0)
                return 0;

            var floor = (double)Math.Floor(Math.Abs(value));
            if (floor == 0)
                return 0;

            return (int)Math.Floor(Math.Log10(floor)) + 1;
        }

        /// <summary>
        /// Calculates the fractional-part length for a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The fractional-part length.</returns>
        public static int CalcFractionalPartLength(decimal value)
        {
            if (value == 0)
                return 0;

            value = value % 1;
            if (value == 0)
                return 0;

            int count = -1;
            while (value % 10m != 0m)
            {
                value *= 10m;
                count++;
            }

            return count < 0 ? 0 : count;
        }
    }
}