// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Represents a numeric rule that validates the maximum <see cref="DecimalPlaces"/> (fractional-part length) and <see cref="MaxDigits"/> (being the sum of the integer-part and fractional-part lengths).
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    /// <remarks>Internally converts to the property value to a <see cref="decimal"/>. Floating-point types (<see cref="float"/> and <see cref="double"/>) are generally not supported
    /// as precision might be lost during conversion. For more information on integer- and fractional-part see https://en.wikipedia.org/wiki/Fractional_part. </remarks>
    public class DecimalRule<TEntity, TProperty> : NumericRule<TEntity, TProperty> where TEntity : class
    {
        private int? _maxDigits;
        private int? _decimalPlaces;

        /// <summary>
        /// Gets or sets the maximum digits being the sum of the integer-part and fractional-part (<see cref="DecimalPlaces"/>) lengths.
        /// </summary>
        /// <remarks>For example, to validate a number with the pattern '999.99', then <see cref="MaxDigits"/> would be 5 and
        /// <see cref="DecimalPlaces"/> would be 2. Minimum specified value is 1.</remarks>
        public int? MaxDigits
        {
            get { return _maxDigits; }

            set
            {
                if (value.HasValue && value.Value < 1)
                    throw new ArgumentException("Minimum value (where specified) for MaxDigits is 1.");

                _maxDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum supported number of decimal places (fractional-part length).
        /// </summary>
        /// <remarks>Minimum specified value is 0.</remarks>
        public int? DecimalPlaces
        {
            get { return _decimalPlaces; }

            set
            {
                if (value.HasValue && value.Value < 0)
                    throw new ArgumentException("Minimum value (where specified) for DecimalPlaces is 0.");

                _decimalPlaces = value;
            }
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            // Where the value is null, do nothing; i.e. Nullable<Type>.
            if (Comparer<object>.Default.Compare(context.Value, null) == 0)
                return;

            // Convert numeric to a decimal value.
            decimal value = Convert.ToDecimal(context.Value);

            // Check if negative.
            if (!AllowNegatives && value < 0)
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.AllowNegativesFormat);
                return;
            }

            int il = MaxDigits.HasValue ? CalcIntegerPartLength(value) : 0;
            int dp = MaxDigits.HasValue || DecimalPlaces.HasValue ? CalcFractionalPartLength(value) : 0;

            // Check max digits.
            if (MaxDigits.HasValue && !CheckMaxDigits(MaxDigits.Value, DecimalPlaces, il, dp))
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MaxDigitsFormat, MaxDigits);
                return;
            }

            // Check decimal places.
            if (DecimalPlaces.HasValue && !CheckDecimalPlaces(DecimalPlaces.Value, dp))
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DecimalPlacesFormat, DecimalPlaces);
                return;
            }
        }

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
        private static bool CheckMaxDigits(int maxDigits, int? decimalPlaces, int il, int dp)
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
        private static bool CheckDecimalPlaces(int decimalPlaces, int dp)
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
