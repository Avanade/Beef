// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Beef
{
    /// <summary>
    /// Provides a standardised approach to parsing and validating wildcard text.
    /// </summary>
    public class Wildcard
    {
        /// <summary>
        /// Gets the standard .NET multi (zero or more) wildcard character.
        /// </summary>
        public const char MultiWildcardCharacter = '*';

        /// <summary>
        /// Gets the standard .NET single wildcard character.
        /// </summary>
        public const char SingleWildcardCharacter = '?';

        /// <summary>
        /// Gets the space ' ' character.
        /// </summary>
        public const char SpaceCharacter = ' ';

        /// <summary>
        /// Gets the <see cref="WildcardSelection.MultiBasic"/> <see cref="Wildcard"/> using only the <see cref="MultiWildcardCharacter"/>.
        /// </summary>
        public static Wildcard MultiBasic { get; } = new Wildcard(WildcardSelection.MultiBasic, MultiWildcardCharacter);

        /// <summary>
        /// Gets the <see cref="WildcardSelection.MultiAll"/> <see cref="Wildcard"/> using only the <see cref="MultiWildcardCharacter"/>.
        /// </summary>
        public static Wildcard MultiAll { get; } = new Wildcard(WildcardSelection.MultiAll, MultiWildcardCharacter);

        /// <summary>
        /// Gets the <see cref="WildcardSelection.BothAll"/> <see cref="Wildcard"/> using both the <see cref="MultiWildcardCharacter"/> and <see cref="SingleWildcardCharacter"/>.
        /// </summary>
        public static Wildcard BothAll { get; } = new Wildcard(WildcardSelection.BothAll, MultiWildcardCharacter, SingleWildcardCharacter);

        /// <summary>
        /// Gets or sets the default <see cref="Wildcard"/> settings (defaults to <see cref="WildcardSelection.MultiAll"/>.
        /// </summary>
        public static Wildcard Default { get; set; } = MultiAll;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wildcard"/> class.
        /// </summary>
        /// <param name="supported">The supported <see cref="WildcardSelection"/>.</param>
        /// <param name="multiWildcard">The .NET multi (zero or more) wildcard character (defaults to <see cref="MultiWildcardCharacter"/>).</param>
        /// <param name="singleWildcard">The .NET single wildcard character (defaults to <see cref="char.MinValue"/>).</param>
        /// <param name="charactersNotAllowed">The list of characters that are not allowed.</param>
        /// <param name="transform">The <see cref="StringTransform"/> option for the wildcard text.</param>
        /// <param name="spaceTreatment">The <see cref="WildcardSpaceTreatment"/> that defines the treatment of embedded space ' ' characters within the wildcard.</param>
        public Wildcard(WildcardSelection supported, char multiWildcard = MultiWildcardCharacter, char singleWildcard = char.MinValue, char[] charactersNotAllowed = null,
            WildcardSpaceTreatment spaceTreatment = WildcardSpaceTreatment.None, StringTransform transform = StringTransform.EmptyToNull)
        {
            if (supported == WildcardSelection.Undetermined || supported.HasFlag(WildcardSelection.InvalidCharacter))
                throw new ArgumentException("A Wildcard cannot be configured with Undetermined and/or InvalidCharacter supported selection(s).", nameof(supported));

            if (multiWildcard != char.MinValue && singleWildcard != char.MinValue && multiWildcard == singleWildcard)
                throw new ArgumentException("A Wildcard cannot be configured with the same character for the MultiWildcard and SingleWildcard.", nameof(multiWildcard));

            if (charactersNotAllowed != null && (charactersNotAllowed.Contains(multiWildcard) || charactersNotAllowed.Contains(singleWildcard)))
                throw new ArgumentException("A Wildcard cannot be configured with either the MultiWildcard or SingleWildcard in the CharactersNotAllowed list.", nameof(charactersNotAllowed));

            if (supported.HasFlag(WildcardSelection.MultiWildcard) && multiWildcard == char.MinValue)
                throw new ArgumentException("A Wildcard that supports MultiWildcard must also define the MultiWildcard character.");

            if (supported.HasFlag(WildcardSelection.SingleWildcard) && singleWildcard == char.MinValue)
                throw new ArgumentException("A Wildcard that supports SingleWildcard must also define the SingleWildcard character.");

            Supported = supported;
            MultiWildcard = multiWildcard;
            SingleWildcard = singleWildcard;
            CharactersNotAllowed = charactersNotAllowed;
            SpaceTreatment = spaceTreatment;
            Transform = transform;
        }

        /// <summary>
        /// Gets the supported <see cref="WildcardSelection"/>.
        /// </summary>
        public WildcardSelection Supported { get; private set; }

        /// <summary>
        /// Gets the .NET multi (zero or more) wildcard character.
        /// </summary>
        /// <remarks>A value of <see cref="char.MinValue"/> indicates no multi wildcard support.</remarks>
        public char MultiWildcard { get; private set; }

        /// <summary>
        /// Gets the .NET single wildcard character.
        /// </summary>
        /// <remarks>A value of <see cref="char.MinValue"/> indicates no single wildcard support.</remarks>
        public char SingleWildcard { get; private set; }

        /// <summary>
        /// Gets the list of characters that are not allowed.
        /// </summary>
        public char[] CharactersNotAllowed { get; private set; }

        /// <summary>
        /// Gets the <see cref="StringTransform"/> option for the wildcard text.
        /// </summary>
        public StringTransform Transform { get; private set; }

        /// <summary>
        /// Gets the <see cref="WildcardSpaceTreatment"/> that defines the treatment of embedded space ' ' characters within the wildcard.
        /// </summary>
        public WildcardSpaceTreatment SpaceTreatment { get; private set; }

        /// <summary>
        /// Validates the wildcard text against what is <see cref="Supported"/> to ensure validity.
        /// </summary>
        /// <param name="text">The wildcard text.</param>
        /// <returns><c>true</c> indicates that the text is valid; otherwise, <c>false</c> for invalid.</returns>
        /// <remarks>Note that leading and trailing spaces are ignored.</remarks>
        public bool Validate(string text)
        {
            return !Parse(text).HasError;
        }

        /// <summary>
        /// Validates the <paramref name="selection"/> against what is <see cref="Supported"/> to ensure validity.
        /// </summary>
        /// <param name="selection">The <see cref="WildcardSelection"/> to validate.</param>
        /// <returns><c>true</c> indicates that the selection is valid; otherwise, <c>false</c> for invalid.</returns>
        public bool Validate(WildcardSelection selection)
        {
            if ((selection.HasFlag(WildcardSelection.None) && !Supported.HasFlag(WildcardSelection.None)) ||
                (selection.HasFlag(WildcardSelection.Equal) && !Supported.HasFlag(WildcardSelection.Equal)) ||
                (selection.HasFlag(WildcardSelection.Single) && !Supported.HasFlag(WildcardSelection.Single)) ||
                (selection.HasFlag(WildcardSelection.StartsWith) && !Supported.HasFlag(WildcardSelection.StartsWith)) ||
                (selection.HasFlag(WildcardSelection.EndsWith) && !Supported.HasFlag(WildcardSelection.EndsWith)) ||
                (selection.HasFlag(WildcardSelection.Contains) && !Supported.HasFlag(WildcardSelection.Contains)) ||
                (selection.HasFlag(WildcardSelection.Embedded) && !Supported.HasFlag(WildcardSelection.Embedded)) ||
                (selection.HasFlag(WildcardSelection.MultiWildcard) && !Supported.HasFlag(WildcardSelection.MultiWildcard)) ||
                (selection.HasFlag(WildcardSelection.SingleWildcard) && !Supported.HasFlag(WildcardSelection.SingleWildcard)) ||
                (selection.HasFlag(WildcardSelection.AdjacentWildcards) && !Supported.HasFlag(WildcardSelection.AdjacentWildcards)) ||
                (selection.HasFlag(WildcardSelection.InvalidCharacter)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Parses the wildcard text to ensure validitity returning a <see cref="WildcardResult"/>.
        /// </summary>
        /// <param name="text">The wildcard text.</param>
        /// <returns>The corresponding <see cref="WildcardResult"/>.</returns>
        public WildcardResult Parse(string text)
        {
            text = Cleaner.Clean(text, StringTrim.Both, Transform);
            if (string.IsNullOrEmpty(text))
                return new WildcardResult { Wildcard = this, Selection = WildcardSelection.None, Text = text };

            var sb = new StringBuilder();
            var wr = new WildcardResult { Wildcard = this, Selection = WildcardSelection.Undetermined };

            if (CharactersNotAllowed != null && CharactersNotAllowed.Count() > 0 && text.IndexOfAny(CharactersNotAllowed) >= 0)
                wr.Selection |= WildcardSelection.InvalidCharacter;

            var hasMulti = SpaceTreatment == WildcardSpaceTreatment.MultiWildcardWhenOthers && Supported.HasFlag(WildcardSelection.MultiWildcard) && text.IndexOf(MultiWildcardCharacter) >= 0;
            var hasTxt = false;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var isMulti = c == MultiWildcard;
                var isSingle = c == SingleWildcard;

                if (isMulti)
                {
                    wr.Selection |= WildcardSelection.MultiWildcard;

                    // Skip adjacent multi's as they are redundant.
                    for (int j = i + 1; j < text.Length; j++)
                    {
                        if (text[j] == MultiWildcard)
                        {
                            i = j;
                            continue;
                        }

                        break;
                    }
                }

                if (isSingle)
                    wr.Selection |= WildcardSelection.SingleWildcard;

                if (isMulti || isSingle)
                {
                    if (text.Length == 1)
                        wr.Selection |= WildcardSelection.Single;
                    else if (i == 0)
                        wr.Selection |= WildcardSelection.EndsWith;
                    else if (i == text.Length - 1)
                        wr.Selection |= WildcardSelection.StartsWith;
                    else
                    {
                        if (hasTxt || isSingle)
                            wr.Selection |= WildcardSelection.Embedded;
                        else
                            wr.Selection |= WildcardSelection.EndsWith;
                    }

                    if (i < text.Length - 1 && (text[i + 1] == MultiWildcard || text[i + 1] == SingleWildcard))
                        wr.Selection |= WildcardSelection.AdjacentWildcards;
                }
                else
                {
                    hasTxt = true;
                    if (c == SpaceCharacter && (SpaceTreatment == WildcardSpaceTreatment.Compress || SpaceTreatment == WildcardSpaceTreatment.MultiWildcardAlways || SpaceTreatment == WildcardSpaceTreatment.MultiWildcardWhenOthers))
                    {
                        // Compress adjacent spaces.
                        bool skipChar = SpaceTreatment != WildcardSpaceTreatment.Compress && text[i - 1] == MultiWildcardCharacter;
                        for (int j = i + 1; j < text.Length; j++)
                        {
                            if (text[j] == SpaceCharacter)
                            {
                                i = j;
                                continue;
                            }

                            break;
                        }

                        if (skipChar || (SpaceTreatment != WildcardSpaceTreatment.Compress && text[i + 1] == MultiWildcardCharacter))
                            continue;

                        if (SpaceTreatment == WildcardSpaceTreatment.MultiWildcardAlways || (SpaceTreatment == WildcardSpaceTreatment.MultiWildcardWhenOthers && hasMulti))
                        {
                            c = MultiWildcardCharacter;
                            wr.Selection |= WildcardSelection.MultiWildcard;
                            wr.Selection |= WildcardSelection.Embedded;
                        }
                    }
                }

                sb.Append(c);
            }

            if (!hasTxt && wr.Selection == (WildcardSelection.StartsWith | WildcardSelection.MultiWildcard))
            {
                wr.Selection |= WildcardSelection.Single;
                wr.Selection ^= WildcardSelection.StartsWith;
            }

            if (hasTxt && wr.Selection.HasFlag(WildcardSelection.StartsWith) && wr.Selection.HasFlag(WildcardSelection.EndsWith) && !wr.Selection.HasFlag(WildcardSelection.Embedded))
            {
                wr.Selection |= WildcardSelection.Contains;
                wr.Selection ^= WildcardSelection.StartsWith;
                wr.Selection ^= WildcardSelection.EndsWith;
            }

            if (wr.Selection == WildcardSelection.Undetermined)
                wr.Selection |= WildcardSelection.Equal;

            wr.Text = sb.Length == 0 ? null : sb.ToString();
            return wr;
        }
    }

    /// <summary>
    /// Represents the <see cref="Wildcard"/> <see cref="Wildcard.Parse"/> result.
    /// </summary>
    public class WildcardResult
    {
        /// <summary>
        /// Gets the originating <see cref="Beef.Wildcard"/> configuration.
        /// </summary>
        internal Wildcard Wildcard { get; set; }

        /// <summary>
        /// Gets the resulting <see cref="WildcardSelection"/>.
        /// </summary>
        public WildcardSelection Selection { get; internal set; }

        /// <summary>
        /// Gets the updated wildcard text.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Indicates whether the <see cref="Text"/> contains one or more non-<see cref="Wildcard.Supported"/> errors.
        /// </summary>
        public bool HasError => !Wildcard.Validate(Selection);

        /// <summary>
        /// Gets the <see cref="Text"/> with all the wildcard characters removed.
        /// </summary>
        public string GetTextWithoutWildcards()
        {
            var s = Text;
            if (Selection.HasFlag(WildcardSelection.MultiWildcard))
                s = s.Replace(new string(Wildcard.MultiWildcard, 1), string.Empty);

            if (Selection.HasFlag(WildcardSelection.SingleWildcard))
                s = s.Replace(new string(Wildcard.SingleWildcard, 1), string.Empty);

            return s;
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> where result <see cref="HasError"/> is <c>true</c>.
        /// </summary>
        /// <returns>The current instance to enable method-chaining.</returns>
        public WildcardResult ThrowOnError()
        {
            if (HasError)
                throw new InvalidOperationException("Wildcard selection text is not supported.");

            return this;
        }

        /// <summary>
        /// Creates the corresponding <see cref="Regex"/> for the wildcard text.
        /// </summary>
        /// <param name="ignoreCase">Indicates whether the regular expression should ignore case (default) or not.</param>
        /// <returns></returns>
        public Regex CreateRegex(bool ignoreCase = true)
        {
            ThrowOnError();

            if (Selection.HasFlag(WildcardSelection.Single))
                return CreateRegex(Selection.HasFlag(WildcardSelection.MultiWildcard) ? "^.*$" : "^.$", ignoreCase);

            var p = Regex.Escape(Text);
            if (Selection.HasFlag(WildcardSelection.MultiWildcard))
                p = p.Replace("\\*", ".*");

            if (Selection.HasFlag(WildcardSelection.SingleWildcard))
                p = p.Replace("\\?", ".");

            return CreateRegex($"^{p}$", ignoreCase);
        }

        /// <summary>
        /// Create the <see cref="Regex"/>.
        /// </summary>
        private Regex CreateRegex(string pattern, bool ignoreCase)
        {
            return ignoreCase ? new Regex(pattern, RegexOptions.IgnoreCase) : new Regex(pattern);
        }

        /// <summary>
        /// Gets the corresponding <b>regular expression</b> pattern for the wildcard text.
        /// </summary>
        /// <returns>The corresponding <see cref="Regex"/> pattern.</returns>
        /// <exception cref="InvalidOperationException">Throws an <see cref="InvalidOperationException"/> where result <see cref="HasError"/> is <c>true</c>.</exception>
        public string GetRegexPattern()
        {
            ThrowOnError();

            if (Selection.HasFlag(WildcardSelection.Single))
                return Selection.HasFlag(WildcardSelection.MultiWildcard) ? "^.*$" : "^.$";

            var p = Regex.Escape(Text);
            if (Selection.HasFlag(WildcardSelection.MultiWildcard))
                p = p.Replace("\\*", ".*");

            if (Selection.HasFlag(WildcardSelection.SingleWildcard))
                p = p.Replace("\\?", ".");

            return $"^{p}$";
        }
    }

    /// <summary>
    /// Represents the wildcard selection.
    /// </summary>
    [Flags]
    public enum WildcardSelection
    {
        /// <summary>
        /// Indicates that the wildcard selection is undetermined.
        /// </summary>
        Undetermined,

        /// <summary>
        /// Indicates that there was no selection; i.e the text was null or empty (see <see cref="string.IsNullOrEmpty(string)"/>).
        /// </summary>
        None = 1,

        /// <summary>
        /// Indicates that no wildcard characters were found and an equal operation should be performed.
        /// </summary>
        Equal = 2,

        /// <summary>
        /// Indicates a single wildcard character (e.g. '*' or '?').
        /// </summary>
        Single = 4,

        /// <summary>
        /// Indicates the selection contains a starts with operation (e.g. 'xxx*', 'xxx?', 'xx*x*', etc).
        /// </summary>
        StartsWith = 8,

        /// <summary>
        /// Indicates the selection contains an ends with operation (e.g. '*xxx', '?xxx', '?x*xx', etc).
        /// </summary>
        EndsWith = 16,

        /// <summary>
        /// Indicates the selection contains both a <see cref="StartsWith"/> and <see cref="EndsWith"/> (with no <see cref="Embedded"/>) operation (e.g. '*xxx*', '?xxx*', etc).
        /// </summary>
        Contains = 32,

        /// <summary>
        /// Indicates the selection contains an embedded operation (e.g. 'xx*xx', '*xx*xx*', 'xx?xx*', etc).
        /// </summary>
        Embedded = 64,

        /// <summary>
        /// Indicates the selection contains at least one instance of the <see cref="Wildcard.MultiWildcard"/> character.
        /// </summary>
        MultiWildcard = 128,

        /// <summary>
        /// Indicates the selection contains at least one instance of the <see cref="Wildcard.SingleWildcard"/> character.
        /// </summary>
        SingleWildcard = 256,

        /// <summary>
        /// Indicates the selection contains adjacent (side-by-side) wildcard characters (e.g. '*?', 'xx**xx', 'xxx**', etc.
        /// </summary>
        AdjacentWildcards = 512,

        /// <summary>
        /// Indicates the selection contains one or more invalid characters (see <see cref="Wildcard.CharactersNotAllowed"/>).
        /// </summary>
        InvalidCharacter = 1024,

        /// <summary>
        /// Represents the <see cref="MultiWildcard"/> and <see cref="SingleWildcard"/> <b>all</b> selections; excludes <see cref="InvalidCharacter"/> and <see cref="Undetermined"/>.
        /// </summary>
        BothAll = None | Equal | Single | StartsWith | EndsWith | Contains | Embedded | MultiWildcard | SingleWildcard | AdjacentWildcards,

        /// <summary>
        /// Represents the <see cref="MultiWildcard"/> <b>basic</b> selections; includes <see cref="Equal"/>, <see cref="StartsWith"/>, <see cref="EndsWith"/>, <see cref="Contains"/> and <see cref="AdjacentWildcards"/>.
        /// </summary>
        MultiBasic = None | Equal | Single | StartsWith | EndsWith | Contains | MultiWildcard | AdjacentWildcards,

        /// <summary>
        /// Represents the <see cref="MultiWildcard"/> <b>all</b> selections; includes <see cref="MultiBasic"/> and <see cref="Embedded"/>.
        /// </summary>
        MultiAll = MultiBasic | Embedded
    }

    /// <summary>
    /// Defines the treatment of embedded <see cref="Wildcard.SpaceCharacter"/> within the wildcard.
    /// </summary>
    /// <remarks>Note: leading and trailing spaces are <i>always</i> removed.</remarks>
    public enum WildcardSpaceTreatment
    {
        /// <summary>
        /// Indicates that no treatment is to be performed; leave as is.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that multiple adjacent embedded <see cref="Wildcard.SpaceCharacter"/> are compressed into a single space.
        /// </summary>
        /// <remarks>Example: 'XX   XX' -> 'XX XX'.</remarks>
        Compress,

        /// <summary>
        /// Indicates that the embedded <see cref="Wildcard.SpaceCharacter"/> are <i>always</i> compressed and converted to the multi-wildcard character.
        /// </summary>
        /// <remarks>Examples: 'XX   XX' -> 'XX*XX', 'XX   XX*' -> 'XX*XX*'.</remarks>
        MultiWildcardAlways,

        /// <summary>
        /// Indicates that the embedded <see cref="Wildcard.SpaceCharacter"/> are compressed and converted to the multi-wildcard character only where <i>other</i> multi-wildcards are found.
        /// </summary>
        /// <remarks>Examples: 'XX   XX' -> 'XX   XX', 'XX   XX*' -> 'XX*XX*'.</remarks>
        MultiWildcardWhenOthers
    }
}