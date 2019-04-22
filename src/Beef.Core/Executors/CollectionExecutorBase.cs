// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the base class for a collection executor (<see cref="OnRunCollectionAsync"/>, <see cref="CollectionExecutorCore{TColl, TItem}.OnItemRunAsync(ExecutorItemRunArgs{TItem})"/> and
    /// <see cref="CollectionExecutorCore{TColl, TItem}.OnCompletionRunAsync"/>) that takes no arguments.
    /// </summary>
    /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
    public abstract class CollectionExecutorBase<TColl, TItem> : CollectionExecutorCore<TColl, TItem> where TColl : IEnumerable<TItem>
    {
        /// <summary>
        /// Runs the collection work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorCollectionRunArgs{TArgs}"/>.</param>
        /// <returns>The collection.</returns>
        internal async Task<TColl> RunCollectionAsync(ExecutorCollectionRunArgs args)
        {
            try
            {
                var coll = await OnRunCollectionAsync(args);
                OnPerRunType(args);
                return coll;
            }
            catch (Exception ex)
            {
                if (!args.HasException)
                {
                    args.SetException(ex);
                    OnPerRunType(args);
                }

                throw;
            }
        }

        /// <summary>
        /// Runs the collection unit of work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorCollectionRunArgs{TArgs}"/>.</param>
        /// <returns>The <see cref="IEnumerable{TItem}"/>.</returns>
        protected abstract Task<TColl> OnRunCollectionAsync(ExecutorCollectionRunArgs args);
    }
}
