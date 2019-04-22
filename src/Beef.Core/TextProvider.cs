// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Reflection;
using System.Resources;

namespace Beef
{
    /// <summary>
    /// Provides the text for a passed key.
    /// </summary>
    /// <remarks>Defaults to the <see cref="DefaultTextProvider"/>.</remarks>
    public abstract class TextProvider
    {
        private static TextProvider _current = new DefaultTextProvider();

        /// <summary>
        /// Creates the <see cref="Current"/> <see cref="TextProvider"/> instance.
        /// </summary>
        /// <param name="textProvider">The concrete <see cref="TextProvider"/> instance.</param>
        public static void Create(TextProvider textProvider)
        {
            _current = textProvider ?? throw new ArgumentNullException("textProvider");
        }

        /// <summary>
        /// Gets the current <see cref="TextProvider"/> instance. 
        /// </summary>
        public static TextProvider Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("The Create method must be invoked before this property can be accessed.");

                return _current;
            }
        }

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
    /// Provides the default <see cref="TextProvider"/> implementation; leverages the default internal resources.
    /// </summary>
    public class DefaultTextProvider : TextProvider
    {
        private static ResourceManager _resourceManager = new ResourceManager("Beef.Strings.Resources", typeof(DefaultTextProvider).GetTypeInfo().Assembly);

        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise <c>null</c>.</returns>
        protected override string GetTextForKey(LText key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return _resourceManager.GetString(key.KeyAndOrText);
        }
    }
}
