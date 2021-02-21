// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureEventHubs = Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="AzureEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubSubscriberHost : EventSubscriberHost
    {
        private EventHubSubscriberHostInvoker? _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubSubscriberHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubSubscriberHost"/>.</param>
        public EventHubSubscriberHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets or sets the <see cref="EventHubSubscriberHostInvoker"/>. Defaults to <see cref="EventHubSubscriberHostInvoker"/>.
        /// </summary>
        public EventHubSubscriberHostInvoker Invoker { get => _invoker ??= new EventHubSubscriberHostInvoker(); set => _invoker = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Use (set) the <see cref="Invoker"/>.
        /// </summary>
        /// <param name="invoker">The <see cref="EventHubSubscriberHostInvoker"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (for fluent-style method chaining).</returns>
        public EventHubSubscriberHost UseInvoker(EventHubSubscriberHostInvoker invoker)
        {
            Invoker = invoker;
            return this;
        }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (for fluent-style method chaining).</returns>
        public EventHubSubscriberHost UseLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Performs the receive processing for an <see cref="AzureEventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="partitionContext">The <see cref="AzureEventHubs.Processor.PartitionContext"/>.</param>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/> instance to receive/process.</param>
        public Task ReceiveAsync(AzureEventHubs.Processor.PartitionContext partitionContext, AzureEventHubs.EventData @event)
        {
            if (partitionContext == null)
                throw new ArgumentNullException(nameof(partitionContext));

            var ehd = new EventHubsData(partitionContext.EventHubPath, partitionContext.ConsumerGroupName, partitionContext.PartitionId, @event ?? throw new ArgumentNullException(nameof(@event)));

            return Invoker.InvokeAsync(this, async () =>
            {
                // Invoke the base EventSubscriberHost.ReceiveAsync to do the actual work!
                var (_, subject, action, _) = @event.GetBeefMetadata();
                await ReceiveAsync(ehd, subject, action, (subscriber) =>
                {
                    // Convert AzureEventHubs.EventData to Beef.EventData.
                    try
                    {
                        return subscriber.ValueType == null ? @event.ToBeefEventData() : @event.ToBeefEventData(subscriber.ValueType);
                    }
                    catch (Exception ex) { throw new EventSubscriberUnhandledException(CreateInvalidEventDataResult(ex)); }
                }).ConfigureAwait(false);
            }, ehd);
        }
    }
}