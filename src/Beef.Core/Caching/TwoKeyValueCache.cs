// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Caching
{
    /// <summary>
    /// Provides a two-key/value cache.
    /// </summary>
    /// <typeparam name="TKey1">The first key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TKey2">The second key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    /// <remarks>To ensure the most effective concurrency throughput and consistency <i>Key1</i> is the primary and is used for all cross-thread locking scenarios; as such,
    /// <i>Key2</i> is considered secondary. Therefore, it is possible that if an access it made for both <i>Key1</i> and <i>Key2</i> concurrently each will result in a <i>Value</i> get;
    /// in this case the value for <i>Key1</i> is always used.</remarks>
    public class TwoKeyValueCache<TKey1, TKey2, TValue> : CacheCoreBase
    {
        private readonly ConcurrentDictionary<TKey1, CacheValue> _dict1 = new();
        private readonly ConcurrentDictionary<TKey2, CacheValue> _dict2 = new();
        private readonly KeyedLock<TKey1> _keyLock1 = new(); // this is the primary key used for *all* concurrency management.
        private readonly KeyedLock<TKey2> _keyLock2 = new(); // this is a secondary key, used to minimise get2 concurrency only.

        private readonly Func<TKey1, (bool hasValue, TKey2 key2, TValue value)>? _get1;
        private readonly Func<TKey1, Task<(bool hasValue, TKey2 key2, TValue value)>>? _getAsync1;
        private readonly Func<TKey2, (bool hasValue, TKey1 key1, TValue value)>? _get2;
        private readonly Func<TKey2, Task<(bool hasValue, TKey1 key1, TValue value)>>? _getAsync2;

        /// <summary>
        /// Represents the cached <see cref="Value"/>, corresponding <see cref="Policy"/> and <see cref="Key1"/> and <see cref="Key2"/>.
        /// </summary>
        private struct CacheValue
        {
            public TKey1 Key1;
            public TKey2 Key2;
            public ICachePolicy Policy;
            public TValue Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeyValueCache{TKey1, TKey2, TValue}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="get1">The function to invoke to get the value (by key1) where not found in cache.</param>
        /// <param name="get2">The function to invoke to get the value (by key2) where not found in cache.</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        public TwoKeyValueCache(Func<TKey1, (bool hasValue, TKey2 key2, TValue value)> get1, Func<TKey2, (bool hasValue, TKey1 key1, TValue value)> get2, string? policyKey = null) : base(policyKey, doNotRegister: true)
        {
            _get1 = Check.NotNull(get1, nameof(get1));
            _get2 = Check.NotNull(get2, nameof(get2));

            // Register an internal policy key with NoCachePolicy to ensure always invoked; then handle internally.
            RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeyValueCache{TKey1, TKey2, TValue}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="getAsync1">The function to invoke (asynchronously) to get the value (by key1) where not found in cache.</param>
        /// <param name="getAsync2">The function to invoke (asynchronously) to get the value (by key2) where not found in cache.</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        public TwoKeyValueCache(Func<TKey1, Task<(bool hasValue, TKey2 key2, TValue value)>> getAsync1, Func<TKey2, Task<(bool hasValue, TKey1 key1, TValue value)>> getAsync2, string? policyKey = null) : base(policyKey, doNotRegister: true)
        {
            _getAsync1 = Check.NotNull(getAsync1, nameof(getAsync1));
            _getAsync2 = Check.NotNull(getAsync2, nameof(getAsync2));

            // Register an internally policy key with NoCachePolicy to ensure always invoked; then handle internally.
            RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeyValueCache{TKey1, TKey2, TValue}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="get1">The function to invoke to get the value (by key1) where not found in cache.</param>
        /// <param name="get2">The function to invoke to get the value (by key2) where not found in cache.</param>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        public TwoKeyValueCache(ICachePolicy policy, Func<TKey1, (bool hasValue, TKey2 key2, TValue value)> get1, Func<TKey2, (bool hasValue, TKey1 key1, TValue value)> get2) : base(policy)
        {
            _get1 = Check.NotNull(get1, nameof(get1));
            _get2 = Check.NotNull(get2, nameof(get2));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeyValueCache{TKey1, TKey2, TValue}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="getAsync1">The function to invoke asynchronously to get the value (by key1) where not found in cache.</param>
        /// <param name="getAsync2">The function to invoke asynchronously to get the value (by key2) where not found in cache.</param>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        public TwoKeyValueCache(ICachePolicy policy, Func<TKey1, Task<(bool hasValue, TKey2 key2, TValue value)>> getAsync1, Func<TKey2, Task<(bool hasValue, TKey1 key1, TValue value)>> getAsync2) : base(policy)
        {
            _getAsync1 = Check.NotNull(getAsync1, nameof(getAsync1));
            _getAsync2 = Check.NotNull(getAsync2, nameof(getAsync2));
        }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict1.Count;

        #region Key1

        /// <summary>
        /// Gets the value from the cache using the specified key1.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetByKey1(TKey1 key1)
        {
            if (!TryGetByKey1(key1, out TValue value))
                throw new ArgumentException("Cache does not contain the specified key.");

            return value;
        }

        /// <summary>
        /// Gets the value from the cache using the specified key1. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey1(TKey1 key1, out TValue value)
        {
            // Check if it exists; if so then return.
            if (_dict1.TryGetValue(key1, out CacheValue cv) && !cv.Policy.HasExpired())
            {
                value = cv.Value;
                return true;
            }

            // Lock against the key to minimise concurrent gets (which could be expensive).
            lock (_keyLock1.Lock(key1))
            {
                // Check existence again, and use if found.
                if (_dict1.TryGetValue(key1, out cv) && !cv.Policy.HasExpired())
                {
                    value = cv.Value;
                    return true;
                }
                
                // Get the value by key1.
                var r = _get1 != null ? _get1(key1) : _getAsync1!(key1).GetAwaiter().GetResult();
                if (r.hasValue)
                {
                    var policy = (ICachePolicy)GetPolicy().Clone();
                    policy.Reset();

                    cv = new CacheValue { Key1 = key1, Key2 = r.key2, Policy = policy, Value = value = r.value };

                    // Make sure that the cache does not have a copy with different key combination.
                    if (_dict2.TryGetValue(cv.Key2, out var cv2) && Comparer<TKey1>.Default.Compare(cv2.Key1, cv.Key1) != 0)
                        throw new ArgumentException("An element with the same key2 already exists in the cache.");

                    _dict1.GetOrAdd(cv.Key1, cv);
                    _dict2.AddOrUpdate(cv.Key2, cv, (_, __) => cv);
                    return true;
                }
            }

            // Nothing found.
            Remove1(cv.Key1);
            value = default!;
            return false;
        }

        /// <summary>
        /// Determines whether the cache contains the key1.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey1(TKey1 key)
        {
            return _dict1.TryGetValue(key, out CacheValue cv) && !cv.Policy.HasExpired();
        }

        /// <summary>
        /// Removes the value from the cache using the key1.
        /// </summary>
        /// <param name="key1">The key.</param>
        public void Remove1(TKey1 key1)
        {
            if (!_dict1.TryGetValue(key1, out CacheValue cv))
                return;

            lock (_keyLock1.Lock(cv.Key1))
            {
                Remove(cv.Key1, cv.Key2);
            }
        }

        /// <summary>
        /// Internal remove logic; assumed locking is performed by the consumer.
        /// </summary>
        private void Remove(TKey1 key1, TKey2 key2)
        {
            _dict2.TryRemove(key2, out _);
            _keyLock2.Remove(key2);

            _dict1.TryRemove(key1, out _);
            _keyLock1.Remove(key1);
        }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/> for the specified key1.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="ICachePolicy"/> where cached; otherwise, <c>null</c>.</returns>
        public ICachePolicy GetPolicyByKey1(TKey1 key)
        {
            _dict1.TryGetValue(key, out CacheValue cv);
            return cv.Policy;
        }

        #endregion

        #region Key2

        /// <summary>
        /// Gets the value from the cache using the specified key2.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetByKey2(TKey2 key2)
        {
            if (!TryGetByKey2(key2, out TValue value))
                throw new ArgumentException("Cache does not contain the specified key.");

            return value;
        }

        /// <summary>
        /// Gets the value from the cache using the key2. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey2(TKey2 key2, out TValue value)
        {
            // Check if it exists; if so then return.
            if (_dict2.TryGetValue(key2, out CacheValue cv) && !cv.Policy.HasExpired())
            {
                value = cv.Value;
                return true;
            }

            // Does not exist so get result to access key 1 for primary locking purposes. We lock key2 briefly to minimise concurrency (expense) on the actual get.
            (bool hasValue, TKey1 key1, TValue value) r;
            lock (_keyLock2.Lock(key2))
            {
                if (_dict2.TryGetValue(key2, out cv) && !cv.Policy.HasExpired())
                {
                    value = cv.Value;
                    return true;
                }

                r = _get2 != null ? _get2(key2) : _getAsync2!(key2).GetAwaiter().GetResult();

                // Exit where no data found.
                if (!r.hasValue)
                {
                    Remove1(cv.Key1);
                    value = default!;
                    return false;
                }

                // Lock against key1 to control concurrency.
                lock (_keyLock1.Lock(r.key1))
                {
                    // Where a value now exists use.
                    if (_dict2.TryGetValue(key2, out cv) && !cv.Policy.HasExpired())
                    {
                        value = cv.Value;
                        return true;
                    }

                    var policy = (ICachePolicy)GetPolicy().Clone();
                    policy.Reset();

                    cv = new CacheValue { Key1 = r.key1, Key2 = key2, Policy = policy, Value = value = r.value };

                    // Make sure that the cache does not have a copy with different key combination.
                    if (_dict1.TryGetValue(cv.Key1, out var cv2) && Comparer<TKey1>.Default.Compare(cv2.Key1, cv.Key1) != 0)
                        throw new ArgumentException("An element with the same key1 already exists in the cache.");

                    _dict2.GetOrAdd(cv.Key2, cv);
                    _dict1.AddOrUpdate(cv.Key1, cv, (_, __) => cv);
                    return true;
                }
            }
        }

        /// <summary>
        /// Determines whether the cache contains the key2.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey2(TKey2 key)
        {
            return _dict2.TryGetValue(key, out CacheValue cv) && !cv.Policy.HasExpired();
        }

        /// <summary>
        /// Removes the value from the cache using the key2.
        /// </summary>
        /// <param name="key2">The key.</param>
        public void Remove2(TKey2 key2)
        {
            if (!_dict2.TryGetValue(key2, out CacheValue cv))
                return;

            Remove1(cv.Key1);
        }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/> for the specified key2.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="ICachePolicy"/> where cached; otherwise, <c>null</c>.</returns>
        public ICachePolicy GetPolicyByKey2(TKey2 key)
        {
            _dict2.TryGetValue(key, out CacheValue cv);
            return cv.Policy;
        }

        #endregion

        /// <summary>
        /// Flush (remove) any expired items.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            foreach (var key1 in _dict1.Where(x => ignoreExpiry || x.Value.Policy.IsExpired).Select(x => x.Value.Key1).ToArray())
            {
                Remove1(key1);
            }
        }
    }
}