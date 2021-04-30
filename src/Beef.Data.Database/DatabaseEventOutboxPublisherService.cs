// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Business;
using Beef.Events;
using Beef.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the <see cref="DatabaseEventOutboxBase"/> publisher service. <para>This service is responsible for <see cref="DatabaseEventOutboxBase.DequeueAsync(IDatabase, int)">dequeueing</see> the previously queued
    /// <see cref="DatabaseEventOutboxItem">event items</see> and publishing the corresonding <see cref="EventData">events</see> using the specified <see cref="IEventPublisher"/>. This will manage the dequeue and event
    /// publish/send transactionally, in that the dequeue will <i>only</i> be committed once all events have been sent successfully. On send failure the database dequeue will be rolled back. This will guarantee all events 
    /// are successfully sent but may result in events potentially being sent multiple times; i.e. guarantee at-least-once sent semantics. The corresponding receiver/consumer(s) are then responsible for ensuring 
    /// at-most-once processing semantics where applicable.</para><para>See also: https://microservices.io/patterns/data/transactional-outbox.html </para>
    /// </summary>
    /// <remarks>The configuration will initially be loaded from configuration where specfied; JSON as follows:
    /// <example>
    /// <code>
    /// "BeefDatabaseEventOutboxPublisherService": {
    ///   "MaxDequeueCount": 10,
    ///   "Interval": "00:05:00",
    ///   "DequeueInterval": "00:00:30"
    /// }
    /// </code>
    /// </example></remarks>
    public class DatabaseEventOutboxPublisherService : TimerHostedServiceBase
    {
        private readonly IDatabase? _database;
        private readonly DatabaseEventOutboxBase _eventOutbox;
        private readonly IEventPublisher? _eventPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxPublisherService"/> class.
        /// </summary>
        /// <param name="serviceProvider"><inheritdoc/></param>
        /// <param name="database">The <see cref="IDatabase"/>; defaults to instance from the <paramref name="serviceProvider"/> per execution scope where not specified.</param>
        /// <param name="eventOutbox">The <see cref="DatabaseEventOutboxBase"/>; defaults to instance from the <paramref name="serviceProvider"/> per execution scope where not specified.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>; defaults to instance from the <paramref name="serviceProvider"/> per execution scope where not specified.</param>
        /// <param name="config"><inheritdoc/></param>
        /// <param name="logger"><inheritdoc/></param>
        /// <param name="overrideExecutionContext"><inheritdoc/></param>
        public DatabaseEventOutboxPublisherService(IServiceProvider serviceProvider, IDatabase? database = null, DatabaseEventOutboxBase? eventOutbox = null, IEventPublisher? eventPublisher = null, IConfiguration? config = null, ILogger? logger = null, Func<ExecutionContext>? overrideExecutionContext = null)
            : base(serviceProvider, logger ?? Check.NotNull(serviceProvider, nameof(serviceProvider)).GetService<ILogger< DatabaseEventOutboxPublisherService>>(), config, overrideExecutionContext) 
        {
            // Where null we will get value when 'executing' from the ExecutionContext.GetService.
            _database = database;
            _eventPublisher = eventPublisher;

            // This we can get here as it must be a singleton; also hook into enqueue event.
            _eventOutbox = eventOutbox ?? serviceProvider.GetService<DatabaseEventOutboxBase>();
            _eventOutbox.OnEnqueue += EventOutbox_OnEnqueue;

            // Default from configuration.
            var c = Config.GetValue<int?>($"Beef{nameof(DatabaseEventOutboxPublisherService)}:{nameof(MaxDequeueCount)}");
            if (c.HasValue)
                MaxDequeueCount = c.Value;

            var ts = Config.GetValue<TimeSpan?>($"Beef{nameof(DatabaseEventOutboxPublisherService)}:{nameof(Interval)}");
            if (ts.HasValue)
                Interval = ts.Value;

            ts = Config.GetValue<TimeSpan?>($"Beef{nameof(DatabaseEventOutboxPublisherService)}:{nameof(DequeueInterval)}");
            if (ts.HasValue)
                DequeueInterval = ts.Value;
        }

        /// <summary>
        /// Gets or sets the maximum dequeue count; defaults to <see cref="DatabaseEventOutboxBase.DefaultMaxDequeueCount"/> where not specified.
        /// </summary>
        public int? MaxDequeueCount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimerHostedServiceBase.Interval"/> override where an <see cref="DatabaseEventOutboxBase.OnEnqueue"/> has occured and a more immediate <see cref="DatabaseEventOutboxBase.DequeueAsync(IDatabase, int)"/> is required.
        /// </summary>
        /// <remarks>This allows the <see cref="TimerHostedServiceBase.Interval"/> to be set to a value large enough to minimize impact on the database where limited event publishing is occuring. Then, when an
        /// <see cref="DatabaseEventOutboxBase.OnEnqueue"/> occurs a <see cref="TimerHostedServiceBase.OneOffIntervalAdjust(TimeSpan, bool)"/> can be issued to advance the time to dequeue/publish/send the events.
        /// <para>This value should still be a value that is large enough to minimize impact on the database where under load.</para><para>Defaults to 30 seconds.</para></remarks>
        public TimeSpan DequeueInterval { get; set; }

        /// <summary>
        /// Set the <see cref="TimerHostedServiceBase.Interval"/> and <see cref="DequeueInterval"/>.
        /// </summary>
        /// <param name="interval">The <see cref="TimerHostedServiceBase.Interval"/>.</param>
        /// <param name="dequeueInterval"></param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        public DatabaseEventOutboxPublisherService SetIntervals(TimeSpan interval, TimeSpan? dequeueInterval)
        {
            Interval = interval;
            if (dequeueInterval.HasValue)
                DequeueInterval = dequeueInterval.Value;

            return this;
        }

        /// <summary>
        /// On enqueue perform the one-off adjustment (where it could make a discernable difference).
        /// </summary>
        private void EventOutbox_OnEnqueue(object sender, EventArgs e)
        {
            if (DequeueInterval < Interval)
                OneOffIntervalAdjust(DequeueInterval, true);
        }

        /// <summary>
        /// Executes the dequeue of the events from the database 
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var maxDequeueCount = MaxDequeueCount ?? DatabaseEventOutboxBase.DefaultMaxDequeueCount;
            var database = _database ?? ExecutionContext.GetService<IDatabase>()!;
            var eventPublisher = _eventPublisher ?? ExecutionContext.GetService<IEventPublisher>()!;

            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Use the DataInvoke to dequeue and publish/send the events within a transaction; i.e. a failure to send must rollback the dequeue. 
                    if (!await DataInvoker.Current.InvokeAsync(this, async () =>
                    {
                        // Get the next tranch of events from the outbox.
                        var events = new List<EventData>();
                        var items = await _eventOutbox.DequeueAsync(database, maxDequeueCount);
                        items.ForEach(item => events.Add(item.ToEventData()));
                        if (events.Count == 0)
                            return false;

                        // Publish and send the events.
                        eventPublisher.Publish(events.ToArray());
                        await eventPublisher.SendAsync();

                        // Continue only where we maxed out the dequeue count.
                        return events.Count == maxDequeueCount;
                    }, new BusinessInvokerArgs { IncludeTransactionScope = true }))
                        return;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Execution failure as a result of an unexpected exception (will continue and retry at next interval): {ex.Message}");
                    return;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"><inheritdoc/></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _eventOutbox.OnEnqueue -= EventOutbox_OnEnqueue;
        }
    }
}