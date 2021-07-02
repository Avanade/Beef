// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.RefData.Caching
{
    /// <summary>
    /// Provides the standard <see cref="ReferenceDataManager"/> instance cache management.
    /// </summary>
    /// <typeparam name="TColl">The <see cref="ReferenceDataCollectionBase{TItem}"/> <see cref="Type"/> of the the collection.</typeparam>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/> for the collection.</typeparam>
    public class ReferenceDataCache<TColl, TItem> : CacheCoreBase, IReferenceDataCache<TColl, TItem>
        where TColl : ReferenceDataCollectionBase<TItem>, IReferenceDataCollection, new()
        where TItem : ReferenceDataBase, new()
    {
        private readonly object _lock = new();
        private TColl? _coll;
        private readonly Func<Task<TColl>> _loadCollection;
        private readonly ReferenceDataCacheLoader _loader = ReferenceDataCacheLoader.Create();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataCache{TColl, TItem}"/> class.
        /// </summary>
        /// <param name="loadCollection">The function that loads the collection from the data repository (<c>null</c> indicates manually updated).</param>
        public ReferenceDataCache(Func<Task<TColl>> loadCollection) : base(typeof(TItem).FullName)
        {
            _loadCollection = loadCollection;
        }

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public override long Count => _coll == null ? 0 : _coll.AllList.Count;

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
            lock (_lock)
            {
                // Refresh ref data when cache has expired or no data exists.
                ICachePolicy policy = GetPolicy();
                if (_coll != null && !policy.HasExpired())
                    return _coll;

                if (!(policy is NoCachePolicy))
                {
                    _coll = GetCollectionInternal();
                    policy.Reset();
                    return _coll;
                }
            }

            // The 'NoCachePolicy' should execute outside of the thread lock - allow concurrency.
            return GetCollectionInternal();
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
                coll = _loader.LoadAsync(this, _loadCollection).GetAwaiter().GetResult() ?? new TColl();

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
        /// <remarks>This provides a means to override the loading of the collection on as needed basis. Also, resets
        /// the cache expiry (<see cref="ICachePolicy.Reset"/>).</remarks>
        public void SetCollection(IEnumerable<TItem> items)
        {
            lock (_lock)
            {
                if (_coll == null)
                    _coll = new TColl();

                _coll.Clear();
                foreach (var item in Check.NotNull(items, nameof(items)))
                {
                    item.MakeReadOnly();
                    _coll.Add(item);
                }

                _coll.GenerateETag();
                GetPolicy().Reset();
            }
        }

        /// <summary>
        /// Flush the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected override void OnFlushCache(bool ignoreExpiry)
        {
            lock (_lock)
            {
                if (_coll != null)
                    _coll = null;
            }
        }

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        public bool IsExpired
        {
            get => (_coll == null) || GetPolicy().IsExpired;
        }
    }
}