// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
    /// <summary>
    /// Represents the <b>localization text</b> key/identifier to be used by the <see cref="TextProvider"/>.
    /// </summary>
    public class LText
    {
        /// <summary>
        /// Gets or sets the <see cref="Int32"/> key/identifier format to convert to a standardised <see cref="string"/>.
        /// </summary>
        public static string IntKeyFormat { get; set; } = "{0:000000}";

        /// <summary>
        /// Initializes a new instance of the <see cref="LText"/> with a <paramref name="keyAndOrText"/> and optional <paramref name="fallbackText"/>.
        /// </summary>
        /// <param name="keyAndOrText">The key and/or text.</param>
        /// <param name="fallbackText">The fallback text to be used when the <paramref name="keyAndOrText"/> is not found by the <see cref="TextProvider"/>.</param>
        /// <remarks>At least one of the arguments must be specified.</remarks>
        public LText(string? keyAndOrText, string? fallbackText = null)
        {
            if (string.IsNullOrEmpty(keyAndOrText) && string.IsNullOrEmpty(fallbackText))
                throw new ArgumentException("At least one of the arguments must be specified.");

            KeyAndOrText = keyAndOrText;
            FallbackText = fallbackText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LText"/> with an <see cref="Int32"/> key and optional <paramref name="fallbackText"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="fallbackText">The fallback text to be used when not found by the <see cref="TextProvider"/>.</param>
        public LText(int key, string? fallbackText = null)
        {
            KeyAndOrText = key <= 0 ? throw new ArgumentException("Key must be a positive integer.", nameof(key)) : key.ToString(IntKeyFormat, System.Globalization.CultureInfo.InvariantCulture);
            FallbackText = fallbackText;
        }

        /// <summary>
        /// Gets the key and/or text (where the key is not found, it will used as the text; unless a <see cref="FallbackText"/> is specified.
        /// </summary>
        public string? KeyAndOrText { get; private set; }

        /// <summary>
        /// Gets or sets the optional fallback text to be used when the <see cref="KeyAndOrText"/> is not found by the <see cref="TextProvider"/> (where not specified that <see cref="KeyAndOrText"/> becomes the fallback text).
        /// </summary>
        public string? FallbackText { get; set; }

        /// <summary>
        /// Returns the <see cref="LText"/> as a <see cref="string"/> (see <see cref="TextProvider"/> <see cref="TextProvider.Current"/> <see cref="TextProviderBase.GetText(LText)"/>).
        /// </summary>
        /// <returns>The <see cref="LText"/> string value.</returns>
        public override string ToString() => this;

        /// <summary>
        /// An implicit cast from an <see cref="LText"/> to a <see cref="string"/> (see <see cref="TextProvider"/> <see cref="TextProvider.Current"/> <see cref="TextProviderBase.GetText(LText)"/>).
        /// </summary>
        /// <param name="text">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise, the <see cref="LText.FallbackText"/> where specified. Where nothing found or specified then the key itself will be returned.</returns>
        public static implicit operator string(LText text) => TextProvider.Current.GetText(text);

        /// <summary>
        /// An implicit cast from a text <see cref="string"/> to an <see cref="LText"/> value updating the <see cref="KeyAndOrText"/>.
        /// </summary>
        /// <param name="keyAndOrText">The key and/or text.</param>
        public static implicit operator LText(string keyAndOrText) => new(keyAndOrText);
    }
}