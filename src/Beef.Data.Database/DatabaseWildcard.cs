// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Data.Database
{
    /// <summary>
    /// Provide wildcard capabilities for a database.
    /// </summary>
    public class DatabaseWildcard
    {
        /// <summary>
        /// Gets the default database multi (zero or more) wildcard character.
        /// </summary>
        public const char MultiWildcardCharacter = '%';

        /// <summary>
        /// Gets the default database single wildcard character.
        /// </summary>
        public const char SingleWildcardCharacter = '_';

        /// <summary>
        /// Gets the default list of characters that are to be escaped.
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql for more detail on escape characters.</remarks>
        public static readonly char[] DefaultCharactersToEscape = { '%', '_', '[' };

        /// <summary>
        /// Gets the default escaping format string when one of the <see cref="CharactersToEscape"/> is found.
        /// </summary>
        public const string DefaultEscapeFormat =  "[{0}]";

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseWildcard"/> class.
        /// </summary>
        /// <param name="wildcard">The <see cref="Beef.Wildcard"/> configuration.</param>
        /// <param name="multiWildcard">The database multi (zero or more) wildcard character.</param>
        /// <param name="singleWildcard">The database single wildcard character.</param>
        /// <param name="charactersToEscape">The list of characters that are to be escaped (defaults to <see cref="DefaultCharactersToEscape"/>).</param>
        /// <param name="escapeFormat">The escaping format string when one of the <see cref="CharactersToEscape"/> is found (defaults to <see cref="DefaultEscapeFormat"/>).</param>
        public DatabaseWildcard(Wildcard? wildcard = null, char multiWildcard = MultiWildcardCharacter, char singleWildcard = SingleWildcardCharacter,
            char[]? charactersToEscape = null, string? escapeFormat = null)
        {
            Wildcard = wildcard ?? Wildcard.Default ?? Wildcard.MultiAll;
            MultiWildcard = multiWildcard;
            SingleWildcard = singleWildcard;
            CharactersToEscape = new List<char>(charactersToEscape ?? DefaultCharactersToEscape);
            EscapeFormat = escapeFormat ?? DefaultEscapeFormat;

            if (MultiWildcard != char.MinValue && SingleWildcard != char.MinValue && MultiWildcard == SingleWildcard)
                throw new ArgumentException("A Wildcard cannot be configured with the same character for the MultiWildcard and SingleWildcard.", nameof(multiWildcard));

            if (Wildcard.Supported.HasFlag(WildcardSelection.MultiWildcard) && multiWildcard == char.MinValue)
                throw new ArgumentException("A Wildcard that supports MultiWildcard must also define the database MultiWildcard character.");

            if (Wildcard.Supported.HasFlag(WildcardSelection.SingleWildcard) && singleWildcard == char.MinValue)
                throw new ArgumentException("A Wildcard that supports SingleWildcard must also define the database SingleWildcard character.");

            if (CharactersToEscape != null && CharactersToEscape.Count > 0 && string.IsNullOrEmpty(EscapeFormat))
                throw new InvalidOperationException("The EscapeFormat must be provided where CharactersToEscape have been defined.");
        }

        /// <summary>
        /// Gets or sets the underlying <see cref="Beef.Wildcard"/> configuration.
        /// </summary>
        public Wildcard Wildcard { get; private set; }

        /// <summary>
        /// Gets or sets the database multi (zero or more) wildcard character.
        /// </summary>
        public char MultiWildcard { get; private set; }

        /// <summary>
        /// Gets or sets the database single wildcard character.
        /// </summary>
        public char SingleWildcard { get; private set; }

        /// <summary>
        /// Gets or sets the list of characters that are to be escaped.
        /// </summary>
        public List<char> CharactersToEscape { get; private set; }

        /// <summary>
        /// Gets or sets the escaping format when one of the <see cref="CharactersToEscape"/> is found.
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql for more detail on escape characters.</remarks>
        public string EscapeFormat { get; private set; }

        /// <summary>
        /// Replaces the wildcard text with the appropriate database representative characters to enable the corresponding SQL LIKE wildcard.
        /// </summary>
        /// <param name="text">The wildcard text.</param>
        /// <returns>The SQL LIKE wildcard.</returns>
        public string? Replace(string? text)
        {
            var wc = Wildcard ?? Wildcard.Default ?? Wildcard.MultiAll;
            var wr = wc.Parse(text).ThrowOnError();

            if (wr.Selection.HasFlag(WildcardSelection.None) || (wr.Selection.HasFlag(WildcardSelection.Single) && wr.Selection.HasFlag(WildcardSelection.MultiWildcard)))
                return new string(MultiWildcard, 1);

            var sb = new StringBuilder();
            foreach (var c in wr.Text!)
            {
                if (wr.Selection.HasFlag(WildcardSelection.MultiWildcard) && c == Wildcard.MultiWildcardCharacter)
                    sb.Append(MultiWildcard);
                else if (wr.Selection.HasFlag(WildcardSelection.SingleWildcard) && c == Wildcard.SingleWildcardCharacter)
                    sb.Append(SingleWildcard);
                else if (CharactersToEscape != null && CharactersToEscape.Contains(c))
                    sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, EscapeFormat, c));
                else
                    sb.Append(c);
            }

            return Cleaner.Clean(sb.ToString(), StringTrim.None, wc.Transform);
        }
    }
}