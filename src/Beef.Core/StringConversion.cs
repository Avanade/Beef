// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Beef
{
    /// <summary>
    /// Provides special case string conversions.
    /// </summary>
    public static class StringConversion
    {
        /// <summary>
        /// The <see cref="Regex"/> expression pattern for splitting strings into words.
        /// </summary>
        public const string WordSplitPattern = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        /// <summary>
        /// Pluralizes the <paramref name="text"/> (English).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The pluralized text.</returns>
        public static string? ToPlural(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            throw new NotSupportedException();
        }

        /// <summary>
        /// Converts <paramref name="text"/> to camelCase (e.g. 'SomeValue' would return 'someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToCamelCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (!ignoreSpecialNames && (text.StartsWith("ETag", StringComparison.InvariantCultureIgnoreCase) || text.StartsWith("OData", StringComparison.InvariantCultureIgnoreCase)))
                return $"{char.ToLower(text[0], CultureInfo.InvariantCulture)}{char.ToLower(text[1], CultureInfo.InvariantCulture)}{text[2..]}";
            else
                return char.ToLower(text[0], CultureInfo.InvariantCulture) + text[1..];
        }

        /// <summary>
        /// Converts <paramref name="text"/> to _camelCase (e.g. 'SomeValue' would return '_someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPrivateCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return "_" + ToCamelCase(text, ignoreSpecialNames);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to PascalCase (e.g. 'someValue' would return 'SomeValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPascalCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (!ignoreSpecialNames && (text.StartsWith("ETag", StringComparison.InvariantCultureIgnoreCase) || text.StartsWith("OData", StringComparison.InvariantCultureIgnoreCase)))
                return $"{char.ToUpper(text[0], CultureInfo.InvariantCulture)}{char.ToUpper(text[1], CultureInfo.InvariantCulture)}{text[2..]}";
            else
                return char.ToUpper(text[0], CultureInfo.InvariantCulture) + text[1..];
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Sentence Case ('someValueXML' would return 'Some Value XML'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSentenceCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

            return char.ToUpper(s[0], CultureInfo.InvariantCulture) + s.Substring(1); // Make sure the first character is always upper case.
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Snake Case ('someValueXML' would return 'some_value_xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSnakeCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

#pragma warning disable CA1308 // Normalize strings to uppercase; lowercase is correct!
            return s.Replace(" ", "_", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with _ and make lowercase.
#pragma warning restore CA1308 
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Kebab Case ('someValueXML' would return 'some-value-xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToKebabCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

#pragma warning disable CA1308 // Normalize strings to uppercase; lowercase is correct!
            return s.Replace(" ", "-", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with - and make lowercase.
#pragma warning restore CA1308 
        }

        /// <summary>
        /// Special case handling.
        /// </summary>
        private static string SpecialCaseHandling(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text.Replace("E Tag", "ETag", StringComparison.InvariantCulture); // Special case where we will put back together.
            return s.Replace("O Data", "OData", StringComparison.InvariantCulture); // Special case where we will put back together.
        }

        /// <summary>
        /// Converts a text to past tense (English only).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPastTense(string? text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 3 || text.EndsWith("ed", StringComparison.InvariantCultureIgnoreCase))
                return text;

            // Ends with the letter e, then remove the final e and add the -ed suffix: tie->tied, like->liked, agree->agreed.
            if (text[^1] == 'e')
                return text + "d";

            // Ends with the letter y preceded by a consonant, then change the y to an i and add the -ed suffix: apply->applied, pry->pried, study->studied.
            if (text[^1] == 'y' && !IsVowel(text[^2]))
                return text[0..^2] + "ied";

            // Ends with a single consonant other than w or y preceded by a single vowel, then double the final consonant and add the -ed suffix: drop->dropped, admit->admitted, concur->concured.
            if (!IsVowel(text[^1]) && text[^1] != 'w' && text[^1] != 'y' && IsVowel(text[^2]) && !IsVowel(text[^3]))
                return text + text[^1] + "ed";

            // Ends with the letter c, then add the letter k followed by the -ed suffix: frolic->frolicked, picnic->picnicked.
            if (text[^1] == 'c')
                return text + "ked";

            // Add the -ed suffix.
            return text + "ed";
        }

        /// <summary>
        /// Determine whether the character is a vowel (English only).
        /// </summary>
        public static bool IsVowel(char c) => char.IsLetter(c) && (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u');
    }
}
