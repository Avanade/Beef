// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Business;
using Beef.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        /// Initialize a new instance of the <see cref="DatabaseEventOutboxInvokerArgs"/> class.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        public DatabaseEventOutboxInvokerArgs(IDatabase database, IEventPublisher? eventPublisher)
        {
            Database = Check.NotNull(database, nameof(database));
            EventPublisher = eventPublisher;
        }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        public IEventPublisher? EventPublisher { get; private set; }

        /// <summary>
        /// Gets or sets the unhandled <see cref="Exception"/> handler.
        /// </summary>
        public Action<Exception>? ExceptionHandler { get; set; }
    }

    /// <summary>
    /// Wraps a <b>Data invoke</b> (alternative to <see cref="DataInvoker"/>) enabling enhanced <b>data tier</b> functionality where the database interaction will always include a <see cref="TransactionScope"/> 
    /// of <see cref="TransactionScopeOption.Required"/> and send (enqueue) all pending published <see cref="IEventPublisher.GetEvents">events</see> to the underlying <see cref="DatabaseEventOutboxBase"/>.
    /// </summary>
    public class DatabaseEventOutboxInvoker : InvokerBase<DatabaseEventOutboxInvokerArgs>
    {
        private readonly IDatabase _database;
        private readonly IEventPublisher? _eventPublisher;
        private readonly DatabaseEventOutboxBase? _outboxMapper;

        /// <summary>
        /// Creates a new <see cref="DatabaseEventOutboxInvoker"/> for the specified <paramref name="database"/>.
        /// </summary>
        /// <param name="database">The <see cref="IEventPublisher"/>; where <c>null</c> will automatically </param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>; where <c>null</c> will attempt to get instance from <see cref="ExecutionContext.GetService{T}(bool)"/>.
        /// Where this results in <c>null</c> assumes that <i>no</i> publishing is required.</param>
        /// <param name="outboxMapper">The <see cref="DatabaseEventOutboxBase"/>; where <c>null</c> will attempt to get instance from <see cref="ExecutionContext.GetService{T}(bool)"/>.
        /// Where this results in <c>null</c> an exception will be thrown.</param>
        /// <returns></returns>
        public static DatabaseEventOutboxInvoker Create(IDatabase database, IEventPublisher? eventPublisher = null, DatabaseEventOutboxBase? outboxMapper = null)
            => new DatabaseEventOutboxInvoker(database, eventPublisher, outboxMapper);

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxInvoker"/>.
        /// </summary>
        private DatabaseEventOutboxInvoker(IDatabase database, IEventPublisher? eventPublisher, DatabaseEventOutboxBase? outboxMapper)
        {
            _database = Check.NotNull(database, nameof(database));
            _eventPublisher = eventPublisher ?? ExecutionContext.GetService<IEventPublisher>(false);
            _outboxMapper = outboxMapper;
        }

        #region NoResult

        /// <summary>
        /// Invokes an <paramref name="action"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="action">The function to invoke.</param>
        /// <param name="param">The <see cref="DatabaseEventOutboxInvokerArgs"/> parameter.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected override void WrapInvoke(object caller, Action action, DatabaseEventOutboxInvokerArgs? param, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(action, nameof(action));

            TransactionScope? txn = null;

            try
            {
                txn = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

                action();
                PublishEventsToOutboxAsync(param!).GetAwaiter().GetResult();

                txn?.Complete();
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
        /// Invokes a <paramref name="func"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The <see cref="DatabaseEventOutboxInvokerArgs"/> parameter.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected async override Task WrapInvokeAsync(object caller, Func<Task> func, DatabaseEventOutboxInvokerArgs? param, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            TransactionScope? txn = null;

            try
            {
                txn = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

                await func().ConfigureAwait(false);
                await PublishEventsToOutboxAsync(param!).ConfigureAwait(false);

                txn?.Complete();
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

        #endregion

        #region WithResult

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> synchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The <see cref="DatabaseEventOutboxInvokerArgs"/> parameter.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected override TResult WrapInvoke<TResult>(object caller, Func<TResult> func, DatabaseEventOutboxInvokerArgs? param, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            TransactionScope? txn = null;

            try
            {
                txn = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

                var result = func();
                PublishEventsToOutboxAsync(param!).GetAwaiter().GetResult();

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
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The <see cref="DatabaseEventOutboxInvokerArgs"/> parameter.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected async override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, DatabaseEventOutboxInvokerArgs? param, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            TransactionScope? txn = null;

            try
            {
                txn = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

                var result = await func().ConfigureAwait(false);
                await PublishEventsToOutboxAsync(param!).ConfigureAwait(false);

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

        #endregion

        /// <summary>
        /// Publish (enqueue) the events to the Outbox within the database (scoped within transaction).
        /// </summary>
        private async Task PublishEventsToOutboxAsync(DatabaseEventOutboxInvokerArgs? param)
        {
            if (_eventPublisher == null)
                return;

            var events = _eventPublisher.GetEvents();
            if (events.Length == 0)
                return;

            _eventPublisher.Reset();

            var list = new List<DatabaseEventOutboxItem>();
            foreach (var ed in events)
            {
                list.Add(new DatabaseEventOutboxItem(ed));
            }

            await (_outboxMapper ?? ExecutionContext.GetService<DatabaseEventOutboxBase>(true)!).EnqueueAsync(_database, list).ConfigureAwait(false);
        }
    }
}