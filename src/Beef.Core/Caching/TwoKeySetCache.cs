// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Generic;

namespace Beef.Caching
{
    /// <summary>
    /// Provides two-way dictionary-backed set-based (full-contents) cache management (i.e. all possible entries are cached in a single request)
    /// to enable two key access (useful for bi-directional value transform).
    /// </summary>
    /// <typeparam name="TKey1">The first key <see cref="Type"/> that is being cache managed.</typeparam>
    /// <typeparam name="TKey2">The second key <see cref="Type"/> that is being cache managed.</typeparam>
    public class TwoKeySetCache<TKey1, TKey2> : CacheCoreBase
    {
        private Dictionary<TKey1, TKey2> _dict1;
        private Dictionary<TKey2, TKey1> _dict2;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeySetCache{TColl, TItem}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="loadCache">The function that is responsible for loading the collection (expects an enumerable <see cref="Tuple{TKey1, TKey2}"/>).</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        protected TwoKeySetCache(Func<object, IEnumerable<Tuple<TKey1, TKey2>>> loadCache = null, string policyKey = null, object data = null) : base(policyKey)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoKeySetCache{TColl, TItem}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="loadCache">The function that is responsible for loading the collection (expects an enumerable <see cref="Tuple{TKey1, TKey2}"/>).</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        protected TwoKeySetCache(ICachePolicy policy, Func<object, IEnumerable<Tuple<TKey1, TKey2>>> loadCache = null, object data = null) : base(policy)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the function to load the cache.
        /// </summary>
        private Func<object, IEnumerable<Tuple<TKey1, TKey2>>> LoadCache { get; set; }

        /// <summary>
        /// Gets or sets the optional data that will be passed into the <see cref="LoadCache"/> operation.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict1 == null ? 0 : _dict1.Count;

        /// <summary>
        /// Gets the caches.
        /// </summary>
        private void GetCache()
        {
            if (LoadCache == null)
                throw new InvalidOperationException("The LoadCache function must be specified for the cache to load.");

            lock (Lock)
            {
                // Refresh cache if expired or no data exists.
                ICachePolicy policy = GetPolicy();
                if (_dict1 != null && !policy.HasExpired())
                    return;

                // Set the cache using the getCache Func<> and resets expiry.
                SetCacheInternal(LoadCache(Data));
                policy.Reset();
            }
        }

        /// <summary>
        /// Gets the cached <see cref="Dictionary{TKey, TValue}"/> for Key 1.
        /// </summary>
        /// <returns>The underlying <see cref="Dictionary{TKey, TValue}"/>.</returns>
        protected Dictionary<TKey1, TKey2> GetCache1()
        {
            GetCache();
            return _dict1;
        }

        /// <summary>
        /// Gets the cached <see cref="Dictionary{TKey, TValue}"/> for Key 2.
        /// </summary>
        /// <returns>The underlying <see cref="Dictionary{TKey, TValue}"/>.</returns>
        protected Dictionary<TKey2, TKey1> GetCache2()
        {
            GetCache();
            return _dict2;
        }

        /// <summary>
        /// Sets the cache proper.
        /// </summary>
        private void SetCacheInternal(IEnumerable<Tuple<TKey1, TKey2>> items)
        {
            if (_dict1 == null)
                _dict1 = new Dictionary<TKey1, TKey2>();

            if (_dict2 == null)
                _dict2 = new Dictionary<TKey2, TKey1>();

            _dict1.Clear();
            _dict2.Clear();
            if (items == null)
                return;

            foreach (var item in items)
            {
                _dict1.Add(item.Item1, item.Item2);
                _dict2.Add(item.Item2, item.Item1);
            }
        }

        /// <summary>
        /// Gets the value from the cache using key 1.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <returns>The value.</returns>
        public TKey2 GetByKey1(TKey1 key1)
        {
            var cache = GetCache1();
            if (!cache.ContainsKey(key1))
                throw new ArgumentException($"Cache does not contain the specified key: {key1.ToString()}.");

            return cache[key1];
        }

        /// <summary>
        /// Gets the value from the cache using key 1. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <param name="key2">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey1(TKey1 key1, out TKey2 key2)
        {
            var cache = GetCache1();
            if (!cache.ContainsKey(key1))
            {
                key2 = default(TKey2);
                return false;
            }

            key2 = cache[key1];
            return true;
        }

        /// <summary>
        /// Determines whether the cache contains key 1.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey1(TKey1 key1)
        {
            return GetCache1().ContainsKey(key1);
        }

        /// <summary>
        /// Gets the value from the cache using key 2.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <returns>The value.</returns>
        public TKey1 GetByKey2(TKey2 key2)
        {
            var cache = GetCache2();
            if (!cache.ContainsKey(key2))
                throw new ArgumentException($"Cache does not contain the specified key: {key2.ToString()}.");

            return cache[key2];
        }

        /// <summary>
        /// Gets the value from the cache using key 2. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <param name="key1">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey2(TKey2 key2, out TKey1 key1)
        {
            var cache = GetCache2();
            if (!cache.ContainsKey(key2))
            {
                key1 = default(TKey1);
                return false;
            }

            key1 = cache[key2];
            return true;
        }

        /// <summary>
        /// Determines whether the cache contains key 2.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey2(TKey2 key2)
        {
            return GetCache2().ContainsKey(key2);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            _dict1.Clear();
            _dict2.Clear();
        }
    }
}
