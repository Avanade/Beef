// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// The <see cref="ICachePolicy"/> manager enables the centralised management of <see cref="ICachePolicy"/> caches.
    /// </summary>
    public static class CachePolicyManager
    {
        private static ICachePolicy _defaultPolicy = new NoExpiryCachePolicy();
        private static readonly object _lock = new object();
        private static ConcurrentDictionary<string, ICachePolicy> _policies = new ConcurrentDictionary<string, ICachePolicy>();
        private static ConcurrentDictionary<string, ICacheCore> _registered = new ConcurrentDictionary<string, ICacheCore>();
        private static Timer _timer;
        private static readonly Random _random = new Random();

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> set to one minute.
        /// </summary>
        public static TimeSpan OneMinute { get; } = new TimeSpan(0, 1, 0);

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> set to five minutes.
        /// </summary>
        public static TimeSpan FiveMinutes { get; } = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> set to ten minutes.
        /// </summary>
        public static TimeSpan TenMinutes { get; } = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> set to one hour.
        /// </summary>
        public static TimeSpan OneHour { get; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> set to one day.
        /// </summary>
        public static TimeSpan OneDay { get; } = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// Indicates whether internal tracing is enabled (and output); related events will be logged.
        /// </summary>
        public static bool IsInternalTracingEnabled { get; set; } = false;

        /// <summary>
        /// Get or sets the default <see cref="ICachePolicy"/> for use when a policy has not previously been set for a <see cref="Type"/>.
        /// </summary>
        /// <remarks>Where not specified a <see cref="NoExpiryCachePolicy"/> is used as the default.</remarks>
        public static ICachePolicy DefaultPolicy
        {
            get => _defaultPolicy;
            set => _defaultPolicy = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Resets the <see cref="ICachePolicy"/> manager to an initial state by <see cref="Unregister(string)">unregistering</see> all previously registered caches.
        /// </summary>
        /// <remarks><i>Caution:</i> where a cache has already been instantied with a policy this will be unregistered; this may result in unintended consequences.</remarks>
        public static void Reset()
        {
            StopFlushTimer();

            foreach (var r in _policies.ToArray())
            {
                Unregister(r.Key);
            }
        }

        /// <summary>
        /// Registers a <see cref="ICacheCore"/> to enable advanced support; i.e. automatic cache flush.
        /// </summary>
        /// <param name="cache">The <see cref="ICacheCore"/> to register.</param>
        /// <param name="overridePolicyKey">The policy key override (defaults to <see cref="ICacheCore.PolicyKey"/>).</param>
        /// <remarks>All <see cref="CacheCoreBase"/> instances are automatically registered.</remarks>
        public static void Register(ICacheCore cache, string overridePolicyKey = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (!_registered.TryAdd(overridePolicyKey ?? cache.PolicyKey, cache))
                throw new InvalidOperationException("Only a single instance of a Cache PolicyKey can be registered.");
        }

        /// <summary>
        /// Gets the <see cref="Register(ICacheCore, string)">registered</see> <see cref="ICacheCore"/> for the specified <paramref name="policyKey"/>.
        /// </summary>
        /// <param name="policyKey">The cache policy key.</param>
        /// <returns>The <see cref="ICacheCore"/> where found; otherwise, <c>null</c>.</returns>
        public static ICacheCore GetRegistered(string policyKey)
        {
            if (string.IsNullOrEmpty(policyKey))
                throw new ArgumentNullException(nameof(policyKey));

            _registered.TryGetValue(policyKey, out ICacheCore cv);
            return cv;
        }

        /// <summary>
        /// Unregisters (removes) the previously <see cref="Register(ICacheCore, string)">registered</see> <see cref="ICacheCore"/> and corresponding policy.
        /// </summary>
        /// <param name="policyKey">The cache policy key.</param>
        /// <remarks>The underlying cache will be flushed (see <see cref="ICacheCore.Flush(bool)"/>) to remove all existing cached data.
        /// <para><i>Caution:</i> where a cache has already been instantied with a policy this will be unregistered; this may result in unintended consequences.</para></remarks>
        public static void Unregister(string policyKey)
        {
            if (string.IsNullOrEmpty(policyKey))
                return;

            _registered.TryRemove(policyKey, out ICacheCore cv);
            _policies.TryRemove(policyKey, out ICachePolicy policy);

            if (cv != null)
                cv.Flush(true);
        }

        /// <summary>
        /// Sets the <paramref name="policy"/> for a specified key.
        /// </summary>
        /// <param name="policyKey">The cache policy key.</param>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        public static void Set(string policyKey, ICachePolicy policy)
        {
            if (string.IsNullOrEmpty(policyKey))
                throw new ArgumentNullException(nameof(policyKey));

            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            _policies.AddOrUpdate(policyKey, policy, (k, p) => policy);
        }

        /// <summary>
        /// Gets the <see name="ICachePolicy"/> for a specified key (uses <see cref="DefaultPolicy"/> where not found).
        /// </summary>
        /// <param name="policyKey">The cache policy key.</param>
        /// <param name="autoCreate"><c>true</c> indicates to automatically create the policy where not found; otherwise, <c>false</c> will result in a <c>null</c> response where not found.</param>
        /// <returns>The <see cref="ICachePolicy"/>.</returns>
        public static ICachePolicy Get(string policyKey, bool autoCreate = true)
        {
            if (autoCreate)
                return _policies.GetOrAdd(policyKey, (ICachePolicy)_defaultPolicy.Clone());
            else
            {
                _policies.TryGetValue(policyKey, out ICachePolicy policy);
                return policy;
            }
        }

        /// <summary>
        /// Sets the <see name="ICachePolicy"/> for the <see cref="DefaultPolicy"/> and <see cref="Type">Types</see> defined within the configuration.
        /// </summary>
        /// <param name="config">The <see cref="CachePolicyConfig"/>.</param>
        public static void SetFromCachePolicyConfig(CachePolicyConfig config)
        {
            CachePolicyConfig.SetCachePolicyManager(config);
        }

        /// <summary>
        /// Starts the timer to manage the frequency in which expired caches will be flushed (see <see cref="CacheCoreBase.OnFlushCache"/>).
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before <see cref="Flush"/> is invoked.</param>
        /// <param name="period">The time interval between invocations of <see cref="Flush"/>.</param>
        /// <remarks>This can be called multiple times to change the timer values as required.</remarks>
        public static void StartFlushTimer(TimeSpan dueTime, TimeSpan period)
        {
            lock (_lock)
            {
                if (_timer == null)
                    _timer = new Timer(TimerElapsed, null, dueTime, period);
                else
                    _timer.Change(dueTime, period);

                Trace(() => Logger.Default.Trace($"CachePolicyManager FlushTimer was started."));
            }
        }

        /// <summary>
        /// Timer has elapsed so the cache should be flushed.
        /// </summary>
        private static void TimerElapsed(Object stateInfo)
        {
            Trace(() => Logger.Default.Trace($"CachePolicyManager FlushTimer elapsed; initiating flush."));
            Flush();
        }

        /// <summary>
        /// Stops the timer managing the frequency in which expired caches will be flushed.
        /// </summary>
        public static void StopFlushTimer()
        {
            if (_timer == null)
                return;

            lock (_lock)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;

                Trace(() => Logger.Default.Trace($"CachePolicyManager FlushTimer was stopped."));
            }
        }

        /// <summary>
        /// Flushes all registered (see <see cref="Register(ICacheCore, string)"/>) caches where they have expired (see <see cref="ICachePolicy.IsExpired"/>).
        /// </summary>
        public static void Flush()
        {
            EnactFlush(false);
        }

        /// <summary>
        /// Flushes all registered (see <see cref="Register(ICacheCore, string)"/>) caches regardless of expiry (see <see cref="ICachePolicy.IsExpired"/>).
        /// </summary>
        public static void ForceFlush()
        {
            EnactFlush(true);
        }

        /// <summary>
        /// Enacts the requested flush.
        /// </summary>
        private static void EnactFlush(bool ignoreExpiry)
        {
            foreach (var cache in _registered.ToArray())
            {
                var policy = Get(cache.Key);
                if (ignoreExpiry || policy.IsExpired)
                {
                    policy.Refresh();
                    cache.Value.Flush(ignoreExpiry);

                    Trace(() => Logger.Default.Trace($"CachePolicyManager Flush '{cache.Key}' ({(ignoreExpiry ? "was forced" : "has expired")})."));
                }
            }
        }

        /// <summary>
        /// Gets an array of all policies.
        /// </summary>
        /// <returns>An array of policies.</returns>
        public static KeyValuePair<string, ICachePolicy>[] GetPolicies()
        {
            return _policies.Select(x => new KeyValuePair<string, ICachePolicy>(x.Key, x.Value)).ToArray();
        }

        /// <summary>
        /// Performs the <paramref name="action"/> where <see cref="IsInternalTracingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="action">The action.</param>
        internal static void Trace(Action action)
        {
            if (IsInternalTracingEnabled)
                action?.Invoke();
        }

        /// <summary>
        /// Add a randomised <paramref name="offset"/> (up to the specified value) to the <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The base time to add the offset to.</param>
        /// <param name="offset">The maximum offset time to add.</param>
        /// <returns>The updated <see cref="DateTime"/> value.</returns>
        public static DateTime AddRandomizedOffsetToTime(DateTime time, TimeSpan? offset)
        {
            if (offset.HasValue)
                return time.AddMilliseconds(_random.NextDouble() * offset.Value.TotalMilliseconds);
            else
                return time;
        }
    }
}
