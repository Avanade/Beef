// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Reflection;
using System.Resources;

namespace Beef
{
    /// <summary>
    /// Provides the default <see cref="TextProviderBase"/> implementation; leverages the default internal resources (<c>Beef.Strings.Resources</c>).
    /// </summary>
    public class DefaultTextProvider : TextProviderBase
    {
        private static readonly ResourceManager _resourceManager = new("Beef.Strings.Resources", typeof(DefaultTextProvider).GetTypeInfo().Assembly);

        /// <summary>
        /// Gets the text for the passed <see cref="LText"/>.
        /// </summary>
        /// <param name="key">The <see cref="LText"/>.</param>
        /// <returns>The corresponding text where found; otherwise <c>null</c>.</returns>
        protected override string GetTextForKey(LText key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return _resourceManager.GetString(key.KeyAndOrText, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}