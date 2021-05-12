// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.RefData.Caching
{
    /// <summary>
    /// Special purpose Reference Data cache loader to allow additional processing to be performed when loading the collection from the data repository.
    /// </summary>
    public class ReferenceDataCacheLoader
    {
        private static Func<ReferenceDataCacheLoader> _create = () => new ReferenceDataCacheLoader();

        /// <summary>
        /// Registers the <see cref="ReferenceDataCacheLoader"/> <paramref name="create"/> function that is invoked to create an instance for each Reference Data cache.
        /// </summary>
        /// <param name="create">The create function.</param>
        /// <remarks>Defaults to a <see cref="ReferenceDataCacheLoader"/> instance that directly invokes the pass <i>loadCollection</i> function.</remarks>
        public static void Register(Func<ReferenceDataCacheLoader> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
        }

        /// <summary>
        /// Creates the <see cref="ReferenceDataCacheLoader"/> instance.
        /// </summary>
        /// <returns>A <see cref="ReferenceDataCacheLoader"/> instance.</returns>
        public static ReferenceDataCacheLoader Create() => _create();

        /// <summary>
        /// Load the collection for in-memory managed cache.
        /// </summary>
        /// <typeparam name="TColl">The <see cref="ReferenceDataCollectionBase{TItem}"/> <see cref="Type"/> of the the collection.</typeparam>
        /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/> for the collection.</typeparam>
        /// <param name="owner">The owning <see cref="IReferenceDataCache{TColl, TItem}"/>.</param>
        /// <param name="loadCollection">The specified function to load the collection from the data repository.</param>
        /// <returns>The collection from the data repository.</returns>
        public virtual async Task<TColl> LoadAsync<TColl, TItem>(IReferenceDataCache<TColl, TItem> owner, Func<Task<TColl>> loadCollection)
            where TColl : ReferenceDataCollectionBase<TItem>, IReferenceDataCollection, new()
            where TItem : ReferenceDataBase, new()
        {
            Check.NotNull(owner, nameof(owner));
            Check.NotNull(loadCollection, nameof(loadCollection));

            var sw = Stopwatch.StartNew();
            var coll = await loadCollection().ConfigureAwait(false);
            sw.Stop();
            Logger.Create<ReferenceDataCacheLoader>().LogDebug("ReferenceData Cache Type '{0}' loaded with {1} item(s) [{3}ms].", typeof(TItem).Name, coll.Count, Thread.CurrentThread.ManagedThreadId, sw.ElapsedMilliseconds);
            return coll;
        }
    }
}