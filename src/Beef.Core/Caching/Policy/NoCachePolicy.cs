// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Caching.Policy
{
    /// <summary>
    /// A no caching policy; i.e. data is never cached.
    /// </summary>
    /// <remarks>This is a special <see cref="System.Type"/> that does not cache and is always set to expired (see <see cref="IsExpired"/>).</remarks>
    public sealed class NoCachePolicy : ICachePolicy
    {
        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed (always <c>true</c>).
        /// </summary>
        public bool IsExpired
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed (always <c>true</c>).
        /// </summary>
        bool ICachePolicy.HasExpired() => IsExpired;

        /// <summary>
        /// Forces a refresh of the cache.
        /// </summary>
        public void Refresh()
        {
        }

        /// <summary>
        /// Reset the cache expiry. 
        /// </summary>
        public void Reset()
        {
        }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long Hits => 0;

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="NoCachePolicy"/>.</returns>
        public object Clone()
        {
            return new NoCachePolicy();
        }
    }
}
