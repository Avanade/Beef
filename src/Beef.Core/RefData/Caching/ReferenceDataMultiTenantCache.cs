// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.RefData.Caching
{
    /// <summary>
    /// Provides the standard <see cref="ReferenceDataManager"/> instance multi-tenancy cache management using the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/>.
    /// </summary>
    /// <typeparam name="TColl">The <see cref="ReferenceDataCollectionBase{TItem}"/> <see cref="Type"/> of the the collection.</typeparam>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/> for the collection.</typeparam>
    public class ReferenceDataMultiTenantCache<TColl, TItem> : CacheCoreBase, IReferenceDataCache<TColl, TItem>
        where TColl : ReferenceDataCollectionBase<TItem>, IReferenceDataCollection, new()
        where TItem : ReferenceDataBase, new()
    {
        private readonly Func<Task<TColl>> _loadCollection;
        private readonly ConcurrentDictionary<Guid, CacheValue<TColl>> _dict = new();
        private readonly ReferenceDataCacheLoader _loader = ReferenceDataCacheLoader.Create();
        private readonly KeyedLock<Guid> _keyLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataMultiTenantCache{TColl, TItem}"/> class.
        /// </summary>
        /// <param name="loadCollection">The function that loads the collection from the data repository; must infer <see cref="ExecutionContext.TenantId"/> from the <see cref="ExecutionContext"/>.</param>
        public ReferenceDataMultiTenantCache(Func<Task<TColl>> loadCollection) : base(typeof(TItem).FullName, doNotRegister: true)
        {
            _loadCollection = Check.NotNull(loadCollection, nameof(loadCollection));

            // Register an internal policy key with NoCachePolicy to ensure always invoked; then handle internally.
            RegisterInternalPolicyKey();
        }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _dict.Count;

        /// <summary>
        /// Gets the cached <see cref="IReferenceDataCollection"/>.
        /// </summary>
        /// <returns>The <see cref="IReferenceDataCollection"/>.</returns>
        IReferenceDataCollection IReferenceDataCache.GetCollection()
        {
            return GetCollection();
        }

        /// <summary>
        /// Gets the cached <see cref="ReferenceDataCollectionBase{TItem}"/>.
        /// </summary>
        /// <returns>The resultant <see cref="ReferenceDataCollectionBase{TItem}"/>.</returns>
        public TColl GetCollection()
        {
            var tenantId = GetTenantId();
            return GetByTenantId(tenantId);
        }

        /// <summary>
        /// Gets the tenant identifier from the Execution Context.
        /// </summary>
        private static Guid GetTenantId()
        {
            if (!ExecutionContext.HasCurrent)
                return Guid.Empty;

            return ExecutionContext.Current.TenantId ?? Guid.Empty;
        }

        /// <summary>
        /// Gets by the tenant identifier.
        /// </summary>
        private TColl GetByTenantId(Guid tenantId)
        {
            // Check if it exists; if so then return.
            if (_dict.TryGetValue(tenantId, out CacheValue<TColl> cv) && !cv.Policy.HasExpired())
                return cv.Value;

            // Lock against the key to minimise concurrent gets (which could be expensive).
            lock (_keyLock.Lock(tenantId))
            {
                if (_dict.TryGetValue(tenantId, out cv) && !cv.Policy.HasExpired())
                    return cv.Value;

                var coll = GetCollectionInternal();

                var policy = cv.Policy ?? (ICachePolicy)GetPolicy().Clone();
                policy.Reset();

                cv = _dict.GetOrAdd(tenantId, t => new CacheValue<TColl> { Policy = policy, Value = coll });
                return cv.Value;
            }
        }

        /// <summary>
        /// Gets/fills the collection whilst also making the cached data read only.
        /// </summary>
		private TColl GetCollectionInternal()
        {
            TColl coll;
            if (_loadCollection == null)
                coll = new TColl();
            else
            {
                var res = _loader.LoadAsync(this, _loadCollection).GetAwaiter().GetResult();
                if (res != null)
                {
                    foreach (var item in res)
                    {
                        item.MakeReadOnly();
                    }

                    coll = res;
                }
                else
                    coll = new TColl();
            }

            coll.GenerateETag();
            return coll;
        }

        /// <summary>
        /// Sets the cached <see cref="ReferenceDataCollectionBase{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets
        /// the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        void IReferenceDataCache.SetCollection(IEnumerable<ReferenceDataBase> items)
        {
            SetCollection((IEnumerable<TItem>)items);
        }

        /// <summary>
        /// Sets the cached <see cref="ReferenceDataCollectionBase{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        public void SetCollection(IEnumerable<TItem> items)
        {
            var tenantId = GetTenantId();

            lock (_keyLock.Lock(tenantId))
            {
                var coll = GetByTenantId(tenantId);
                coll.Clear();

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        item.MakeReadOnly();
                        coll.Add(item);
                    }
                }

                coll.GenerateETag();
                GetPolicyForTenant(tenantId).Reset();
            }
        }

        /// <summary>
        /// Determines whether the cache contains the tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns><c>true</c> if the tenant exists; otherwise, <c>false</c>.</returns>
        public bool ContainsTenant(Guid tenantId)
        {
            return _dict.TryGetValue(tenantId, out CacheValue<TColl> cv) && !cv.Policy.HasExpired();
        }

        /// <summary>
        /// Removes the tenant from the cache.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        public void Remove(Guid tenantId)
        {
            lock (_keyLock.Lock(tenantId))
            {
                _dict.TryRemove(tenantId, out _);
                _keyLock.Remove(tenantId);
            }
        }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/> for the specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns>The <see cref="ICachePolicy"/> where cached; otherwise, <c>null</c>.</returns>
        public ICachePolicy GetPolicyForTenant(Guid tenantId)
        {
            _dict.TryGetValue(tenantId, out CacheValue<TColl> cv);
            return cv.Policy;
        }

        /// <summary>
        /// Indicates whether the underlying tenant (see <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/>) cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                var p = GetPolicyForTenant(GetTenantId());
                return p == null || p.IsExpired;
            }
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