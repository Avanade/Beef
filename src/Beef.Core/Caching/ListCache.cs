// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Caching
{
    /// <summary>
    /// Provides <see cref="IEnumerable{TList}"/>-backed set-based (full-contents) cache management (i.e. all possible entries are cached in a single request).
    /// </summary>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public class ListCache<TItem> : CacheCoreBase
    {
        IEnumerable<TItem> _cache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCache{TList}"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <typeparamref name="TItem"/>).</param>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public ListCache(Func<object, IEnumerable<TItem>> loadCache = null, string policyKey = null, object data = null) : base(policyKey)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCache{TList}"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        /// <param name="loadCache">The function that is responsible for loading the cache (expects an enumerable <typeparamref name="TItem"/>).</param>
        /// <param name="data">The optional data that will be passed into the <paramref name="loadCache"/> operation.</param>
        public ListCache(ICachePolicy policy, Func<object, IEnumerable<TItem>> loadCache = null, object data = null) : base(policy)
        {
            LoadCache = loadCache;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the function to load the cache.
        /// </summary>
        private Func<object, IEnumerable<TItem>> LoadCache { get; set; }

        /// <summary>
        /// Gets or sets the optional data that will be passed into the <see cref="LoadCache"/> operation.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _cache.Count();

        /// <summary>
        /// Gets the cached <see cref="IEnumerable{TList}"/>.
        /// </summary>
        /// <returns>The underlying <see cref="IEnumerable{TList}"/>.</returns>
        protected IEnumerable<TItem> GetCache()
        {
            lock (Lock)
            {
                // Refresh cache if expired or no data exists.
                ICachePolicy policy = GetPolicy();
                if (_cache != null && !policy.HasExpired())
                    return _cache;

                // Set the cache using the getCache and resets expiry.
                SetCacheInternal(LoadCache(Data));
                policy.Reset();
                return _cache;
            }
        }

        /// <summary>
        /// Explicitly sets the cached <see cref="IEnumerable{TList}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets
        /// the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        protected void SetCache(IEnumerable<TItem> items)
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
        private void SetCacheInternal(IEnumerable<TItem> items)
        {
            var list = new List<TItem>(items);
            _cache = list.ToArray();
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            if (_cache != null)
                _cache = null;
        }
    }
}
