// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Generic;

namespace Beef.Caching
{
    /// <summary>
    /// Provides dictionary-backed set-based (full-contents) cache management (i.e. all possible entries are cached in a single request).
    /// </summary>
    /// <typeparam name="TKey">The <see cref="ICollection{TKey}"/> <see cref="Type"/> that is being cache managed.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    public class DictionarySetCache<TKey, TValue> : CacheCoreBase
    {
        private Dictionary<TKey, TValue> _dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySetCache{TColl, TItem}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <see cref="KeyValuePair{TKey, TValue}"/>).</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public DictionarySetCache(Func<object, IEnumerable<KeyValuePair<TKey, TValue>>> loadCache = null, string policyKey = null, object data = null) : base(policyKey)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySetCache{TColl, TItem}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <see cref="KeyValuePair{TKey, TValue}"/>).</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public DictionarySetCache(ICachePolicy policy, Func<object, IEnumerable<KeyValuePair<TKey, TValue>>> loadCache = null, object data = null) : base(policy)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the function to load the cache.
        /// </summary>
        private Func<object, IEnumerable<KeyValuePair<TKey, TValue>>> LoadCache { get; set; }

        /// <summary>
        /// Gets or sets the optional data that will be passed into the <see cref="LoadCache"/> operation.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict == null ? 0 : _dict.Count;

        /// <summary>
        /// Gets the cached <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>The underlying <see cref="Dictionary{TKey, TValue}"/>.</returns>
        protected Dictionary<TKey, TValue> GetCache()
        {
            if (LoadCache == null)
                throw new InvalidOperationException("The LoadCache function must be specified for the cache to load.");

            lock (Lock)
            {
                // Refresh cache if expired or no data exists.
                ICachePolicy policy = GetPolicy();
                if (_dict != null && !policy.HasExpired())
                    return _dict;

                // Set the cache using the getCache Func<> and resets expiry.
                SetCacheInternal(LoadCache(Data));
                policy.Reset();
                return _dict;
            }
        }

        /// <summary>
        /// Explicitly sets the cached <see cref="ICollection{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets
        /// the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        protected void SetCache(IEnumerable<KeyValuePair<TKey, TValue>> items)
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
        private void SetCacheInternal(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (_dict == null)
                _dict = new Dictionary<TKey, TValue>();

            _dict.Clear();
            if (items == null)
                return;

            foreach (var item in items)
            {
                _dict.Add(item.Key, item.Value);
            }
        }

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
            var cache = GetCache();
            if (!cache.ContainsKey(key))
                throw new ArgumentException("Cache does not contain the specified key.");

            return cache[key];
        }

        /// <summary>
        /// Gets the value from the cache using the key. A return value indicates whether the value was found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetByKey(TKey key, out TValue value)
        {
            var cache = GetCache();
            if (!cache.ContainsKey(key))
            {
                value = default;
                return false;
            }

            value = cache[key];
            return true;
        }

        /// <summary>
        /// Determines whether the cache contains the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(TKey key)
        {
            return GetCache().ContainsKey(key);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            if (_dict != null)
                _dict = null;
        }
    }
}