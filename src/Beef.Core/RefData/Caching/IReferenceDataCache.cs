// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using System.Collections.Generic;

namespace Beef.RefData.Caching
{
    /// <summary>
    /// Extends <see cref="ICacheCore"/> adding specific Reference Data capabilities.
    /// </summary>
    public interface IReferenceDataCache : ICacheCore
    {
        /// <summary>
        /// Gets the cached <see cref="IReferenceDataCollection"/>.
        /// </summary>
        /// <returns>The <see cref="IReferenceDataCollection"/>.</returns>
        IReferenceDataCollection GetCollection();

        /// <summary>
        /// Sets the cached <see cref="ReferenceDataCollectionBase{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source items.</param>
        void SetCollection(IEnumerable<ReferenceDataBase> items);

        /// <summary>
        /// Indicates whether the cache has expired and must be refreshed.
        /// </summary>
        bool IsExpired { get; }
    }

    /// <summary>
    /// Extends <see cref="ICacheCore"/> adding specific typed Reference Data capabilities.
    /// </summary>
    public interface IReferenceDataCache<TColl, TItem> : IReferenceDataCache
        where TColl : ReferenceDataCollectionBase<TItem>, IReferenceDataCollection, new()
        where TItem : ReferenceDataBase, new()
    {
        /// <summary>
        /// Gets the cached <see cref="ReferenceDataCollectionBase{TItem}"/>.
        /// </summary>
        /// <returns>The <see cref="ReferenceDataCollectionBase{TItem}"/>.</returns>
        new TColl GetCollection();

        /// <summary>
        /// Sets the cached <see cref="ReferenceDataCollectionBase{TItem}"/> with the contents of the passed collection.
        /// </summary>
        /// <param name="items">The source items.</param>
        void SetCollection(IEnumerable<TItem> items);
    }
}
