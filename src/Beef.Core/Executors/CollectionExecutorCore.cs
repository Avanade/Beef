// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the core functionality for a collection executor.
    /// </summary>
    /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
    public abstract class CollectionExecutorCore<TColl, TItem> : Executor where TColl : IEnumerable<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionExecutorCore{TColl, TItem}"/> class.
        /// </summary>
        internal CollectionExecutorCore() { }

        /// <summary>
        /// Indicates whether the item run supports concurrency; in that multiple items from the collection will be executed asychronously.
        /// (defaults to <c>false</c>).
        /// </summary>
        public bool IsItemRunParallelismEnabled { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of concurrent tasks enabled (see <see cref="ParallelOptions.MaxDegreeOfParallelism"/>);
        /// generally, you do not need to modify this setting.
        /// </summary>
        public int? MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Indicates whether to complete the full execution (collection, item(s), completion) on a requested stop (defaults to <c>false</c>).
        /// </summary>
        /// <value><c>true</c> indicates to complete the full execution; otherwise, <c>false</c> to only complete the currently executing units of work (e.g. <see cref="OnItemRunAsync(ExecutorItemRunArgs{TItem})"/>).</value>
        public bool CompleteFullExecutionOnStop { get; set; } = false;

        /// <summary>
        /// Determines whether the <see cref="Executor"/> will <see cref="Executor.Stop"/> or <b>continue</b> when an unhandled <see cref="Exception"/> is encountered during an
        /// <see cref="OnItemRunAsync(ExecutorItemRunArgs{TItem})"/>. 
        /// </summary>
        public ExceptionHandling ItemExceptionHandling { get; set; } = ExceptionHandling.Continue;

        /// <summary>
        /// Invokes the <see cref="OnItemRunAsync(ExecutorItemRunArgs{TItem})"/>.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorItemRunArgs{TItem}"/>.</param>
        internal async Task RunItemAsync(ExecutorItemRunArgs<TItem> args)
        {
            try
            {
                await OnItemRunAsync(args);
                OnPerRunType(args);
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
        /// Runs the <b>item</b> unit of work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorItemRunArgs{TItem}"/>.</param>
        protected abstract Task OnItemRunAsync(ExecutorItemRunArgs<TItem> args);

        /// <summary>
        /// Invokes <see cref="OnCompletionRunAsync"/>.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorCompletionRunArgs"/>.</param>
        internal async Task CompletionRunAsync(ExecutorCompletionRunArgs args)
        {
            try
            {
                await OnCompletionRunAsync(args);
                OnPerRunType(args);
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
        /// Runs after all the items for the collection have run to enable any final completion activities to be performed.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorCompletionRunArgs"/>.</param>
        protected virtual Task OnCompletionRunAsync(ExecutorCompletionRunArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
