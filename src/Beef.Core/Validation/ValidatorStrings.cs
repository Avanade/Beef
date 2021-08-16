// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Validation
{
    /// <summary>
    /// Provides the standard text format strings.
    /// </summary>
    /// <remarks>For the format defaults within, the '<c>{0}</c>' and '<c>{1}</c>' placeholders represent a property's friendly text and value itself. Any placeholders '<c>{2}</c>', or above, are specific to the underlying valitator.</remarks>
    public static class ValidatorStrings
    {
        /// <summary>
        /// Gets the format string for the compare equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be between {2} and {3}.</i></remarks>
        public static readonly LText BetweenInclusiveFormat = new("Beef.BetweenInclusiveFormat");

        /// <summary>
        /// Gets the format string for the compare equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be between {2} and {3} (exclusive).</i></remarks>
        public static readonly LText BetweenExclusiveFormat = new("Beef.BetweenExclusiveFormat");

        /// <summary>
        /// Gets the format string for the compare equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be equal to {2}.</i></remarks>
        public static readonly LText CompareEqualFormat = new("Beef.CompareEqualFormat");

        /// <summary>
        /// Gets the format string for the compare not equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must not be equal to {2}.</i></remarks>
        public static readonly LText CompareNotEqualFormat = new("Beef.CompareNotEqualFormat");

        /// <summary>
        /// Gets the format string for the compare less than error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be less than {2}.</i></remarks>
        public static readonly LText CompareLessThanFormat = new("Beef.CompareLessThanFormat");

        /// <summary>
        /// Gets the format string for the compare less than or equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be less than or equal to {2}.</i></remarks>
        public static readonly LText CompareLessThanEqualFormat = new("Beef.CompareLessThanEqualFormat");

        /// <summary>
        /// Gets the format string for the compare greater than error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be greater than {2}.</i></remarks>
        public static readonly LText CompareGreaterThanFormat = new("Beef.CompareGreaterThanFormat");

        /// <summary>
        /// Gets the format string for the compare greater than or equal error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be greater than or equal to {2}.</i></remarks>
        public static readonly LText CompareGreaterThanEqualFormat = new("Beef.CompareGreaterThanEqualFormat");

        /// <summary>
        /// Gets the format string for the Maximum digits error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must not exceed {2} digits in total.</i></remarks>
        public static readonly LText MaxDigitsFormat = new("Beef.MaxDigitsFormat");

        /// <summary>
        /// Gets the format string for the Decimal places error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} exceeds the maximum specified number of decimal places ({2}).</i></remarks>
        public static readonly LText DecimalPlacesFormat = new("Beef.DecimalPlacesFormat");

        /// <summary>
        /// Gets the format string for the duplicate error message.
        /// </summary>
        /// <remarks>Defaults to: <i>xxx</i></remarks>
        public static readonly LText DuplicateFormat = new("Beef.DuplicateFormat");

        /// <summary>
        /// Gets the format string for a duplicate value error message; includes ability to specify values.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains duplicates; {2} value '{3}' specified more than once.</i></remarks>
        public static readonly LText DuplicateValueFormat = new("Beef.DuplicateValueFormat");

        /// <summary>
        /// Gets the format string for a duplicate value error message; no values specified.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains duplicates; {2} value specified more than once.</i></remarks>
        public static readonly LText DuplicateValue2Format = new("Beef.DuplicateValue2Format");

        /// <summary>
        /// Gets the format string for the minimum count error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must have at least {2} item(s).</i></remarks>
        public static readonly LText MinCountFormat = new("Beef.MinCountFormat");

        /// <summary>
        /// Gets the format string for the maximum count error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must not exceed {2} item(s).</i></remarks>
        public static readonly LText MaxCountFormat = new("Beef.MaxCountFormat");

        /// <summary>
        /// Gets the format string for the exists error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is not found; a valid value is required.</i></remarks>
        public static readonly LText ExistsFormat = new("Beef.ExistsFormat");

        /// <summary>
        /// Gets the format string for the immutable error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is not allowed to change; please reset value.</i></remarks>
        public static readonly LText ImmutableFormat = new("Beef.ImmutableFormat");

        /// <summary>
        /// Gets the format string for the Mandatory error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is required.</i></remarks>
        public static readonly LText MandatoryFormat = new("Beef.MandatoryFormat");

        /// <summary>
        /// Gets the format string for the must error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is invalid.</i></remarks>
        public static readonly LText MustFormat = new("Beef.MustFormat");

        /// <summary>
        /// Gets the format string for the allow negatives error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must not be negative.</i></remarks>
        public static readonly LText AllowNegativesFormat = new("Beef.AllowNegativesFormat");

        /// <summary>
        /// Gets the format string for the invalid error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is invalid.</i></remarks>
        public static readonly LText InvalidFormat = new("Beef.InvalidFormat");

        /// <summary>
        /// Gets the format string for the minimum length error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must be at least {2} characters in length.</i></remarks>
        public static readonly LText MinLengthFormat = new("Beef.MinLengthFormat");

        /// <summary>
        /// Gets the format string for the maximum length error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} must not exceed {2} characters in length.</i></remarks>
        public static readonly LText MaxLengthFormat = new("Beef.MaxLengthFormat");

        /// <summary>
        /// Gets the format string for the regex error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} is invalid.</i></remarks>
        public static readonly LText RegexFormat = new("Beef.RegexFormat");

        /// <summary>
        /// Gets the format string for the wildcard error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains invalid or non-supported wildcard selection.</i></remarks>
        public static readonly LText WildcardFormat = new("Beef.WildcardFormat");

        /// <summary>
        /// Gets the format string for the collection null item error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains one or more items that are not specified.</i></remarks>
        public static readonly LText CollectionNullItemFormat = new("Beef.CollectionNullItemFormat");

        /// <summary>
        /// Gets the format string for the dictionary null key error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains one or more keys that are not specified.</i></remarks>
        public static readonly LText DictionaryNullKeyFormat = new("Beef.DictionaryNullKeyFormat");

        /// <summary>
        /// Gets the format string for the dictionary null value error message.
        /// </summary>
        /// <remarks>Defaults to: <i>{0} contains one or more values that are not specified.</i></remarks>
        public static readonly LText DictionaryNullValueFormat = new("Beef.DictionaryNullValueFormat");
    }
}