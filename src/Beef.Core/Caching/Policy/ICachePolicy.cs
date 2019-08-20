// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// Provides the basic policy features for a cache.
    /// </summary>
    public interface ICachePolicy : ICloneable
    {
        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed; this should have no side-effects within the policy.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed; this allows for side-effects within the Policy (e.g. a sliding-style cache may update the expiry time where accessed).
        /// </summary>
        bool HasExpired();

        /// <summary>
        /// Gets the number of cache hits (number of times <see cref="HasExpired"/> has been accessed).
        /// </summary>
        /// <remarks>Where a cache hit results in a refresh the value is reset to zero; as a result this initial load is not accounted for within the hits.</remarks>
        long Hits { get; }

        /// <summary>
        /// Forces a refresh of the cache (expires the cache).
        /// </summary>
        void Refresh();

        /// <summary>
        /// Reset the cache (not expired and zero hits). 
        /// </summary>
        void Reset();
    }
}
