// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Caching.Policy
{
    /// <summary>
    /// A no expiry cache policy; in that the cache never automatically expires.
    /// </summary>
    public sealed class NoExpiryCachePolicy : ICachePolicy
    {
        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired { get; private set; } = true;

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        bool ICachePolicy.HasExpired()
        {
            Hits++;
            return IsExpired;
        }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long Hits { get; private set; } = 0;

        /// <summary>
        /// Forces a refresh of the cache.
        /// </summary>
        public void Refresh()
        {
            IsExpired = true;
        }

        /// <summary>
        /// Reset the cache expiry. 
        /// </summary>
        public void Reset()
        {
            IsExpired = false;
            Hits = 0;
        }

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="NoExpiryCachePolicy"/>.</returns>
        public object Clone()
        {
            return new NoExpiryCachePolicy();
        }
    }
}
