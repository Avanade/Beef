// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Beef.Caching
{
    /// <summary>
    /// Provides multi-key cache management over underlying data caches.
    /// </summary>
    public class MultiKeyCache<TKey, TCache> : CacheCoreBase
        where TCache : ICacheCore
    {
        private readonly Func<TKey, ICachePolicy, TCache> _createCache;
        private readonly ConcurrentDictionary<TKey, TCache> _dict = new();
        private readonly KeyedLock<TKey> _keyLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyCache{TKey, TCache}"/> class.
        /// </summary>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="createCache">The function that creates the cache for the specified <typeparamref name="TKey"/> that should also use the passed <see cref="ICachePolicy"/>.</param>
        /// <param name="doNotRegister">Indicates that the automatic <see cref="CachePolicyManager.Register"/> should not occur (inheriting class must perform).</param>
        public MultiKeyCache(Func<TKey, ICachePolicy, TCache> createCache, string? policyKey = null, bool doNotRegister = false) : base(policyKey, doNotRegister: true)
        {
            _createCache = Check.NotNull(createCache, nameof(createCache));

            // Register an internal policy key with NoCachePolicy to ensure always invoked; then handle internally.
            if (!doNotRegister)
                RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict.Count;

        /// <summary>
        /// Gets <typeparamref name="TCache"/> by the specified key (automatically creates where not found).
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The underlying cache.</returns>
        public TCache GetCache(TKey key)
        {
            // Check if it exists; if so then return.
            if (_dict.TryGetValue(key, out TCache cache) && !cache.GetPolicy()!.HasExpired())
                return cache;

            lock (_keyLock.Lock(key))
            {
                if (_dict.TryGetValue(key, out cache) && !cache.GetPolicy()!.HasExpired())
                    return cache;

                var policy = (ICachePolicy)GetPolicy().Clone();
                cache = _createCache(key, policy);
                if (cache == null)
                    throw new InvalidOperationException("The CreateCache function must create an instance of the Cache.");

                policy.Reset();
                return _dict.GetOrAdd(key, cache);
            }
        }

        /// <summary>
        /// Gets <typeparamref name="TCache"/> by the specified key where found.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cache">When this method returns, contains the cache associated with the specified key, if the key is found; otherwise, <c>null</c>. This parameter is
        /// passed uninitialized.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetCache(TKey key, out TCache cache)
        {
            return _dict.TryGetValue(key, out cache);
        }

        /// <summary>
        /// Determines whether the cache contains the key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns><c>true</c> if the tenant exists; otherwise, <c>false</c>.</returns>
        public bool Contains(TKey key)
        {
            return _dict.TryGetValue(key, out TCache cache) && !cache.GetPolicy()!.HasExpired();
        }

        /// <summary>
        /// Removes the cache for the specified key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        public void Remove(TKey key)
        {
            lock (_keyLock.Lock(key))
            {
                if (_dict.TryRemove(key, out TCache cache))
                    cache.Dispose();
            }
        }

        /// <summary>
        /// Flush (remove) any expired items.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            foreach (var cv in _dict.Where(x => ignoreExpiry || x.Value.GetPolicy()!.IsExpired).ToArray())
            {
                Remove(cv.Key);
            }
        }
    }
}