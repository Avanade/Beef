// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Beef.CodeGen.Utility
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
        /// Converts <paramref name="text"/> to camelCase (e.g. 'SomeValue' would return 'someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToCamelCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (!ignoreSpecialNames && TwoCharacterPrefixes.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
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

            if (!ignoreSpecialNames && TwoCharacterPrefixes.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
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

            return char.ToUpper(s[0], CultureInfo.InvariantCulture) + s[1..]; // Make sure the first character is always upper case.
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

            return s.Replace(" ", "_", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with _ and make lowercase.
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

            return s.Replace(" ", "-", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with - and make lowercase.
        }

        /// <summary>
        /// Gets or sets the special prefixes whereby the first two characters will be converted to lowercase versus the standard one.
        /// </summary>
        /// <remarks>Defaults to "ETag" and "OData".</remarks>
        public static string[] TwoCharacterPrefixes { get; set; } = new string[] { "ETag", "OData" };

        /// <summary>
        /// Performs the special case handling.
        /// </summary>
        private static string SpecialCaseHandling(string text) => SpecialCaseHandler == null ? text : SpecialCaseHandler(text);

        /// <summary>
        /// Get or sets the special case handler function that occurs directly after the <see cref="WordSplitPattern"/> <see cref="Regex"/> has been applied.
        /// </summary>
        /// <remarks>Defaults to replace "E Tag" with "ETag", and "O Data" with "OData".</remarks>
        public static Func<string, string>? SpecialCaseHandler { get; set; } =
            new Func<string, string>(text =>
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                var s = text.Replace("E Tag", "ETag", StringComparison.InvariantCulture); 
                return s.Replace("O Data", "OData", StringComparison.InvariantCulture); 
            });
    }
}