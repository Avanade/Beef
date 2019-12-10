// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// A maximum hits cache policy; in that the cache expires after the specified <see cref="MaxHits"/> has been reached.
    /// </summary>
    public sealed class MaxHitsCachePolicy : ICachePolicy
    {
        private int _maxHits = 100;
        private bool _expire = false;

        /// <summary>
        /// Gets or sets the maximum cache hits; defaults to 100.
        /// </summary>
        public int MaxHits
        {
            get { return _maxHits; }

            set
            {
                if (value <= 0)
                    throw new ArgumentException("A MaxHits greater than zero must be specified.");

                _maxHits = value;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (_expire)
                    return true;
                else if (Hits <= _maxHits)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        bool ICachePolicy.HasExpired()
        {
            if (IsExpired)
                return true;

            Hits++;
            return IsExpired;
        }

        /// <summary>
        /// Forces a refresh of the cache.
        /// </summary>
        public void Refresh()
        {
            _expire = true;
        }

        /// <summary>
        /// Reset the cache expiry. 
        /// </summary>
        public void Reset()
        {
            _expire = false;
            Hits = 0;
        }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long Hits { get; private set; } = 0;

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="MaxHitsCachePolicy"/>.</returns>
        public object Clone()
        {
            return new MaxHitsCachePolicy { MaxHits = _maxHits };
        }
    }
}
