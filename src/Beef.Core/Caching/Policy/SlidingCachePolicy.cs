// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// A sliding cache policy; in that the cache expires if a request is not made within a specified <see cref="Duration"/> 
    /// otherwise it slides (extends) by the <see cref="Duration"/> until the optional <see cref="MaxDuration"/> is reached.
    /// </summary>
    public class SlidingCachePolicy : ICachePolicy
    {
        private TimeSpan _duration = new TimeSpan(1, 0, 0);
        private TimeSpan? _randomizerOffset;
        private TimeSpan _maxDuration;
        private DateTime? _maxExpiry;

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
        /// Gets or sets the maximum duration before the cache will expire and be flushed.
        /// </summary>
        public TimeSpan MaxDuration
        {
            get { return _maxDuration; }

            set
            {
                if (value.TotalMilliseconds < 0)
                    throw new ArgumentException("A Duration greater than or equal to zero must be specified.");

                _maxDuration = value;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                DateTime now = DateTime.Now;
                if (Expiry != null && now <= Expiry)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed (slides expiry where it has not expired).
        /// </summary>
        bool ICachePolicy.HasExpired()
        {
            var isExpired = IsExpired;
            if (!isExpired)
                Reset();

            Hits++;
            return isExpired;
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
            _maxExpiry = null;
        }

        /// <summary>
        /// Reset the cache expiry. 
        /// </summary>
        public void Reset()
        {
            DateTime now = DateTime.Now;
            DateTime? expiry = now.Add(_duration);

            if (!_maxExpiry.HasValue && _maxDuration.TotalMilliseconds > 0)
                _maxExpiry = now.Add(_maxDuration);

            if (_maxExpiry.HasValue && Expiry > _maxExpiry.Value)
                expiry = _maxExpiry;

            if (_randomizerOffset.HasValue)
                expiry = CachePolicyManager.AddRandomizedOffsetToTime(expiry.Value, _randomizerOffset.Value);

            Expiry = expiry;
            Hits = 0;
        }

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="SlidingCachePolicy"/>.</returns>
        public object Clone()
        {
            return new SlidingCachePolicy { Duration = _duration, RandomizerOffset = _randomizerOffset, MaxDuration = _maxDuration };
        }
    }
}