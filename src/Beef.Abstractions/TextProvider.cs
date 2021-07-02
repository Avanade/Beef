// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
    /// <summary>
    /// Provides access to the global/static <see cref="Current"/> instance.
    /// </summary>
    public static class TextProvider
    {
        private static ITextProvider? _textProvider;
        private static ITextProvider? _backupTextProvider;

        /// <summary>
        /// Sets the <see cref="Current"/> <see cref="ITextProvider"/> instance explicitly.
        /// </summary>
        /// <param name="textProvider">The concrete <see cref="ITextProvider"/> instance.</param>
        public static void SetTextProvider(ITextProvider textProvider) => _textProvider = Check.NotNull(textProvider, nameof(textProvider));

        /// <summary>
        /// Gets the current <see cref="ITextProvider"/> instance using in the following order: <see cref="ExecutionContext.GetService{T}(bool)"/>, the explicit <see cref="SetTextProvider(ITextProvider)"/>, otherwise, <see cref="NullTextProvider"/>. 
        /// </summary>
        public static ITextProvider Current
        {
            get
            {
                var tp = ExecutionContext.GetService<ITextProvider>(false);
                if (tp != null)
                    return tp;

                if (_textProvider != null)
                    return _textProvider;

                return _backupTextProvider ??= new NullTextProvider();
            }
        }
    }

    /// <summary>
    /// Enables the localized text for a passed key.
    /// </summary>
    public interface ITextProvider
    {
        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise, the <see cref="LText.FallbackText"/> where specified. Where nothing found or specified then the key itself will be returned.</returns>
        string GetText(LText key);
    }

    /// <summary>
    /// Provides the localized text for a passed key.
    /// </summary>
    public abstract class TextProviderBase : ITextProvider
    {
        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise, the <see cref="LText.FallbackText"/> where specified. Where nothing found or specified then the key itself will be returned.</returns>
        public string GetText(LText key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (key.KeyAndOrText == null)
                return key.FallbackText ?? throw new InvalidOperationException("LText is in an invalid state; there is no KeyAndOrText or FallbackText specified.");

            return GetTextForKey(key) ?? key.FallbackText ?? key.KeyAndOrText ?? throw new InvalidOperationException("LText is in an invalid state; there is no KeyAndOrText or FallbackText specified.");
        }

        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise, <c>null</c>.</returns>
        protected abstract string GetTextForKey(LText key);
    }

    /// <summary>
    /// Provides a null <see cref="TextProviderBase"/> implementation; the <see cref="GetTextForKey"/> will return the <see cref="LText.KeyAndOrText"/>.
    /// </summary>
    public class NullTextProvider : TextProviderBase
    {
        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise <c>null</c>.</returns>
        protected override string GetTextForKey(LText key) => key?.KeyAndOrText ?? throw new ArgumentNullException(nameof(key));
    }
}