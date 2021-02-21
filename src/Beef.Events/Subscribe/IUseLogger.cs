// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the means for the <see cref="EventSubscriberHost"/> to update the <see cref="ILogger"/> value.
    /// </summary>
    public interface IUseLogger
    {
        /// <summary>
        /// Use (set) the <see cref="ILogger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        void UseLogger(ILogger logger);
    }
}