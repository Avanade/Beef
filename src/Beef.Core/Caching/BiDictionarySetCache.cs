// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Generic;

namespace Beef.Caching
{
    /// <summary>
    /// Provides two-way dictionary-backed set-based (full-contents) cache management (i.e. all possible entries are cached in a single request) to enable two key access
    /// (useful for bi-directional transform).
    /// </summary>
    /// <typeparam name="TKey1">The first key <see cref="Type"/> that is being cache managed.</typeparam>
    /// <typeparam name="TKey2">The second key <see cref="Type"/> that is being cache managed.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    public class BiDictionarySetCache<TKey1, TKey2, TValue> : CacheCoreBase
    {
        private Dictionary<TKey1, TValue> _dict1;
        private Dictionary<TKey2, TValue> _dict2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiDictionarySetCache{TKey1, TKey2, TValue}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <see cref="Tuple{TKey1, TKey2, TValue}"/>).</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public BiDictionarySetCache(Func<object, IEnumerable<Tuple<TKey1, TKey2, TValue>>> loadCache = null, string policyKey = null, object data = null) : base(policyKey)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiDictionarySetCache{TKey1, TKey2, TValue}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <see cref="Tuple{TKey1, TKey2, TValue}"/>).</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public BiDictionarySetCache(ICachePolicy policy, Func<object, IEnumerable<Tuple<TKey1, TKey2, TValue>>> loadCache = null, object data = null) : base(policy)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the function to load the cache.
        /// </summary>
        private Func<object, IEnumerable<Tuple<TKey1, TKey2, TValue>>> LoadCache { get; set; }

        /// <summary>
        /// Gets or sets the optional data that will be passed into the <see cref="LoadCache"/> operation.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count =>_dict1 == null ? 0 : _dict1.Count;

        /// <summary>
        /// Gets the cache.
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
        protected Dictionary<TKey1, TValue> GetCache1()
        {
            GetCache();
            return _dict1;
        }

        /// <summary>
        /// Gets the cached <see cref="Dictionary{TKey, TValue}"/> for Key 2.
        /// </summary>
        /// <returns>The underlying <see cref="Dictionary{TKey, TValue}"/>.</returns>
        protected Dictionary<TKey2, TValue> GetCache2()
        {
            GetCache();
            return _dict2;
        }

        /// <summary>
        /// Explicitly sets the cached <see cref="ICollection{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets
        /// the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        protected void SetCache(IEnumerable<Tuple<TKey1, TKey2, TValue>> items)
        {
            lock (Lock)
            {
                SetCacheInternal(items);
                GetPolicy().Reset();
            }
        }

        /// <summary>
        /// Sets the cache proper.
        /// </summary>
        private void SetCacheInternal(IEnumerable<Tuple<TKey1, TKey2, TValue>> items)
        {
            if (_dict1 == null)
                _dict1 = new Dictionary<TKey1, TValue>();

            if (_dict2 == null)
                _dict2 = new Dictionary<TKey2, TValue>();

            _dict1.Clear();
            _dict2.Clear();
            if (items == null)
                return;

            foreach (var item in items)
            {
                _dict1.Add(item.Item1, item.Item3);
                _dict2.Add(item.Item2, item.Item3);
            }
        }

        /// <summary>
        /// Gets the value from the cache using key 1.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetByKey1(TKey1 key1)
        {
            var cache = GetCache1();
            if (!cache.ContainsKey(key1))
                throw new ArgumentException("Cache does not contain the specified key.");

            return cache[key1];
        }

        /// <summary>
        /// Gets the value from the cache using key 1. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key1">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey1(TKey1 key1, out TValue value)
        {
            var cache = GetCache1();
            if (!cache.ContainsKey(key1))
            {
                value = default(TValue);
                return false;
            }

            value = cache[key1];
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
        public TValue GetByKey2(TKey2 key2)
        {
            var cache = GetCache2();
            if (!cache.ContainsKey(key2))
                throw new ArgumentException("Cache does not contain the specified key.");

            return cache[key2];
        }

        /// <summary>
        /// Gets the value from the cache using key 2. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key2">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey1(TKey2 key2, out TValue value)
        {
            var cache = GetCache2();
            if (!cache.ContainsKey(key2))
            {
                value = default(TValue);
                return false;
            }

            value = cache[key2];
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
            if (_dict1 != null)
                _dict1.Clear();

            if (_dict1 != null)
                _dict2.Clear();
        }
    }
}
