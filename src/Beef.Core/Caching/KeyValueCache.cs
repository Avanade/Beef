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
    /// Provides a key/value cache.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    public class KeyValueCache<TKey, TValue> : CacheCoreBase
    {
        private readonly ConcurrentDictionary<TKey, CacheValue<TValue>> _dict = new();
        private readonly Func<TKey, TValue>? _get;
        private readonly Func<TKey, Task<TValue>>? _getAsync;
        private readonly bool _cacheDefaultValues;
        private readonly KeyedLock<TKey> _keyLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCache{TKey, TValue}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="get">The function to invoke to get the value where not found in cache.</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="cacheDefaultValues"><c>true</c> indicates that <b>default</b> values returned from the <paramref name="get"/> are to be cached; otherwise, <c>false</c> will
        /// throw an <see cref="ArgumentException"/> indicating that the cache does not contain the specified key.</param>
        public KeyValueCache(Func<TKey, TValue> get, string? policyKey = null, bool cacheDefaultValues = false) : base(policyKey, doNotRegister: true)
        {
            _get = Check.NotNull(get, nameof(get));
            _cacheDefaultValues = cacheDefaultValues;

            // Register an internal policy key with NoCachePolicy to ensure always invoked; then handle internally.
            RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCache{TKey, TValue}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="getAsync">The function to invoke (asynchronously) to get the value where not found in cache.</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="cacheDefaultValues"><c>true</c> indicates that <b>default</b> values returned from the <paramref name="getAsync"/> are to be cached; otherwise, <c>false</c> will
        /// throw an <see cref="ArgumentException"/> indicating that the cache does not contain the specified key.</param>
        public KeyValueCache(Func<TKey, Task<TValue>> getAsync, string? policyKey = null, bool cacheDefaultValues = false) : base(policyKey, doNotRegister: true)
        {
            _getAsync = Check.NotNull(getAsync, nameof(getAsync));
            _cacheDefaultValues = cacheDefaultValues;

            // Register an internal policy key with NoCachePolicy to ensure always invoked; then handle internally.
            RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCache{TKey, TValue}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="get">The function to invoke to get the value where not found in cache.</param>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="cacheDefaultValues"><c>true</c> indicates that <b>default</b> values returned from the <paramref name="get"/> are to be cached; otherwise, <c>false</c> will
        /// throw an <see cref="ArgumentException"/> indicating that the cache does not contain the specified key.</param>
        public KeyValueCache(ICachePolicy policy, Func<TKey, TValue> get, bool cacheDefaultValues = false) : base(policy)
        {
            _get = Check.NotNull(get, nameof(get));
            _cacheDefaultValues = cacheDefaultValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCache{TKey, TValue}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="getAsync">The function to invoke asynchronously to get the value where not found in cache.</param>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="cacheDefaultValues"><c>true</c> indicates that <b>default</b> values returned from the <paramref name="getAsync"/> are to be cached; otherwise, <c>false</c> will
        /// throw an <see cref="ArgumentException"/> indicating that the cache does not contain the specified key.</param>
        public KeyValueCache(ICachePolicy policy, Func<TKey, Task<TValue>> getAsync, bool cacheDefaultValues = false) : base(policy)
        {
            _getAsync = Check.NotNull(getAsync, nameof(getAsync));
            _cacheDefaultValues = cacheDefaultValues;
        }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict.Count;

        /// <summary>
        /// Gets the value from the cache using the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue this[TKey key]
        {
            get { return GetByKey(key); }
        }

        /// <summary>
        /// Gets the value from the cache using the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetByKey(TKey key)
        {
            if (!TryGetByKey(key, out TValue value))
                throw new ArgumentException("Cache does not contain the specified key.");

            return value;
        }

        /// <summary>
        /// Gets the value from the cache using the key. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey(TKey key, out TValue value)
        {
            // Check if it exists; if so then return.
            if (_dict.TryGetValue(key, out CacheValue<TValue> cv) && !cv.Policy.HasExpired())
            {
                value = cv.Value;
                return true;
            }

            // Lock against the key to minimise concurrent gets (which could be expensive).
            TValue v = default!;
            lock (_keyLock.Lock(key))
            {
                if (_dict.TryGetValue(key, out cv) && !cv.Policy.HasExpired())
                {
                    value = cv.Value;
                    return true;
                }

                v = _get != null ? _get(key) : _getAsync!(key).GetAwaiter().GetResult();
                if (EqualityComparer<TValue>.Default.Equals(v, default!))
                {
                    value = default!;
                    if (!_cacheDefaultValues)
                        return false;
                }

                var policy = (ICachePolicy)GetPolicy().Clone();
                policy.Reset();

                cv = _dict.GetOrAdd(key, new CacheValue<TValue> { Policy = policy, Value = value = v });
                return true;
            }
        }

        /// <summary>
        /// Determines whether the cache contains the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key exists and policy has not expired; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(TKey key)
        {
            return _dict.TryGetValue(key, out CacheValue<TValue> cv) && !cv.Policy.HasExpired();
        }

        /// <summary>
        /// Removes the value from the cache using the key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            lock (_keyLock.Lock(key))
            {
                _dict.TryRemove(key, out CacheValue<TValue> _);
                _keyLock.Remove(key);
            }
        }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/> for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="ICachePolicy"/> where cached; otherwise, <c>null</c>.</returns>
        public ICachePolicy GetPolicyByKey(TKey key)
        {
            _dict.TryGetValue(key, out CacheValue<TValue> cv);
            return cv.Policy;
        }

        /// <summary>
        /// Flush (remove) any expired items.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            foreach (var key in _dict.Where(x => ignoreExpiry || x.Value.Policy.IsExpired).Select(x => x.Key).ToArray())
            {
                Remove(key);
            }
        }
    }
}