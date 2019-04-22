// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;

namespace Beef.Caching
{
    /// <summary>
    /// Provides the core features for a cache.
    /// </summary>
    public interface ICacheCore : IDisposable
    {
        /// <summary>
        /// Gets the unqiue cache policy key.
        /// </summary>
        string PolicyKey { get; }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/>.
        /// </summary>
        /// <returns>The <see cref="ICachePolicy"/>.</returns>
        ICachePolicy GetPolicy();

        /// <summary>
        /// Flush the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/>.</param>
        void Flush(bool ignoreExpiry);

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        long Count { get; }
    }
}
