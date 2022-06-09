// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Business;
using Beef.Diagnostics;
using Beef.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Beef.Data.Database
{
    /// <summary>
    /// Provides arguments for the <see cref="DatabaseEventOutboxInvoker"/>.
    /// </summary>
    public class DatabaseEventOutboxInvokerArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="IEventPublisher"/>.
        /// </summary>
        /// <remarks>Where <c>null</c> will attempt to get instance from <see cref="ExecutionContext.GetService{T}(bool)"/>; where this then results in <c>null</c> assumes that <i>no</i> publishing is required.</remarks>
        public IEventPublisher? EventPublisher { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseEventOutboxBase"/>.
        /// </summary>
        /// <remarks>Where <c>null</c> will attempt to get instance from <see cref="ExecutionContext.GetService{T}(bool)"/>; where this then results in <c>null</c> a runtime exception will be thrown.</remarks>
        public DatabaseEventOutboxBase? EventOutbox { get; set; }

        /// <summary>
        /// Gets or sets the unhandled <see cref="Exception"/> handler.
        /// </summary>
        public Action<Exception>? ExceptionHandler { get; set; }
    }

    /// <summary>
    /// Wraps a <b>Data invoke</b> (alternative to <see cref="DataInvoker"/>) enabling enhanced <b>data tier</b> functionality where the database interaction will always include a <see cref="TransactionScope"/> 
    /// of <see cref="TransactionScopeOption.Required"/> and send (enqueue) all pending published <see cref="IEventPublisher.GetEvents">events</see> to the underlying <see cref="DatabaseEventOutboxBase"/>.
    /// </summary>
    public class DatabaseEventOutboxInvoker : CoreEx.Abstractions.InvokerBase<object, DatabaseEventOutboxInvokerArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxInvoker"/>.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/>.</param>
        public DatabaseEventOutboxInvoker(IDatabase database) => Database = Check.NotNull(database, nameof(database));

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; }

        /// <inheritdoc/>
        protected async override Task<TResult> OnInvokeAsync<TResult>(object owner, Func<CancellationToken, Task<TResult>> func, DatabaseEventOutboxInvokerArgs? param, CancellationToken cancellationToken)
        {
            TransactionScope? txn = null;

            try
            {
                txn = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

                var result = await func(cancellationToken).ConfigureAwait(false);
                await PublishEventsToOutboxAsync(param).ConfigureAwait(false);

                txn?.Complete();
                return result;
            }
            catch (Exception ex)
            {
                param?.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                txn?.Dispose();
            }
        }

        /// <summary>
        /// Publish (enqueue) the events to the Outbox within the database (scoped within transaction).
        /// </summary>
        private async Task PublishEventsToOutboxAsync(DatabaseEventOutboxInvokerArgs? param)
        {
            var ep = param?.EventPublisher ?? ExecutionContext.GetService<IEventPublisher>(false);
            if (ep == null)
                return;

            var events = ep.GetEvents();
            if (events.Length == 0)
                return;

            ep.Reset();

            var list = new List<DatabaseEventOutboxItem>();
            foreach (var ed in events)
            {
                list.Add(new DatabaseEventOutboxItem(ed));
            }

            await (param?.EventOutbox ?? ExecutionContext.GetService<DatabaseEventOutboxBase>(true)!).EnqueueAsync(Database, list).ConfigureAwait(false);

            Logger.Create<DatabaseEventOutboxInvoker>().LogDebug("There were {count} event(s) enqueued within the database event outbox.", list.Count);
        }
    }
}