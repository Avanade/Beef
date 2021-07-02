// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// An absolute cache policy; in that the cache expires after a specified <see cref="Duration"/> since last refresh.
    /// </summary>
    public sealed class AbsoluteCachePolicy : ICachePolicy
    {
        private TimeSpan _duration = new(1, 0, 0);
        private TimeSpan? _randomizerOffset;

        /// <summary>
        /// Gets or sets the cache duration (defaults to 1 hour).
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }

            set
            {
                if (value.TotalMilliseconds <= 0)
                    throw new ArgumentException("A Duration greater than zero must be specified.");

                _duration = value;
            }
        }

        /// <summary>
        /// Gets or sets the randomizer offset; this will be a randomised value up to the value specified added to the <see cref="Duration"/> to minimise simultaneous flushes (defaults to <c>null</c>). 
        /// </summary>
        public TimeSpan? RandomizerOffset
        {
            get { return _randomizerOffset; }

            set
            {
                if (value.HasValue && value.Value.TotalMilliseconds <= 0)
                    throw new ArgumentException("A RandomizerOffset greater than zero must be specified.");

                _randomizerOffset = value;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                DateTime now = Entities.Cleaner.Clean(DateTime.Now);
                if (Expiry != null && now <= Expiry)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        bool ICachePolicy.HasExpired()
        {
            Hits++;
            return IsExpired;
        }

        /// <summary>
        /// Gets the calculated expiry value.
        /// </summary>
        public DateTime? Expiry { get; private set; }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long Hits { get; private set; } = 0;

        /// <summary>
        /// Forces a refresh of the cache.
        /// </summary>
        public void Refresh()
        {
            Expiry = null;
        }

        /// <summary>
        /// Reset the cache expiry. 
        /// </summary>
        public void Reset()
        {
            DateTime now = Entities.Cleaner.Clean(DateTime.Now);
            DateTime? expiry = now.Add(_duration);

            if (_randomizerOffset.HasValue)
                expiry = CachePolicyManager.AddRandomizedOffsetToTime(expiry.Value, _randomizerOffset.Value);

            Expiry = expiry;
            Hits = 0;
        }

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="AbsoluteCachePolicy"/>.</returns>
        public object Clone()
        {
            return new AbsoluteCachePolicy { Duration = _duration, RandomizerOffset = _randomizerOffset };
        }
    }
}