// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// A daily cache policy; in that the cache expires after a specified <see cref="Duration"/> in relation to,
    /// and resetting, at the start of the day.
    /// </summary>
    public sealed class DailyCachePolicy : ICachePolicy
    {
        private TimeSpan _duration = new(24, 0, 0);
        private TimeSpan? _randomizerOffset;

        /// <summary>
        /// Gets or sets the cache duration.
        /// </summary>
        /// <remarks><see cref="Duration"/> and <see cref="RandomizerOffset"/> must not exceed 24 hours (defaults to 24 hours).</remarks>
        public TimeSpan Duration
        {
            get { return _duration; }

            set
            {
                if (value.TotalMilliseconds <= 0)
                    throw new ArgumentException("A Duration greater than zero must be specified.");

                if (value.Add(_randomizerOffset ?? TimeSpan.Zero).TotalHours > 24)
                    throw new ArgumentException("Duration and RandomizerOffset must not exceed 24 hours.");

                _duration = value;
            }
        }

        /// <summary>
        /// Gets or sets the cache start of day offset (adds an additional offset to the duration to avoid all caches resetting at the start of the day 00:00:00).
        /// </summary>
        /// <remarks><see cref="Duration"/> and <see cref="RandomizerOffset"/> must not exceed 24 hours (defaults to <c>code</c>).</remarks>
        public TimeSpan? RandomizerOffset
        {
            get { return _randomizerOffset; }

            set
            {
                if (value.HasValue)
                {
                    if (value.Value.TotalMilliseconds <= 0)
                        throw new ArgumentException("A RandomizerOffset greater than zero must be specified.");

                    if (_duration.Add(value.Value).TotalHours > 24)
                        throw new ArgumentException("Duration and RandomizerOffset must not exceed 24 hours.");
                }

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
            Hits = 0;

            // Where the expiry is not set then set.
            DateTime now = Entities.Cleaner.Clean(DateTime.Now);
            if (Expiry == null)
            {
                DateTime time = now.Date;
                while (time < now)
                {
                    time = time.Add(_duration);
                    if (time.DayOfYear != now.DayOfYear)
                        time = now.Date.AddDays(1);
                }

                Expiry = CachePolicyManager.AddRandomizedOffsetToTime(time, _randomizerOffset);
                return;
            }

            // Add duration; if result is tomorrow reset to start of day tomorrow plus offset.
            DateTime temp = now.Add(_duration);
            if (temp.DayOfYear != Expiry.Value.DayOfYear)
                Expiry = CachePolicyManager.AddRandomizedOffsetToTime(temp, _randomizerOffset);
            else
                Expiry = temp;
        }

        /// <summary>
        /// Creates a configured copy of the object.  
        /// </summary>
        /// <returns>A new <see cref="DailyCachePolicy"/>.</returns>
        public object Clone()
        {
            return new DailyCachePolicy { Duration = _duration, RandomizerOffset = _randomizerOffset };
        }
    }
}
