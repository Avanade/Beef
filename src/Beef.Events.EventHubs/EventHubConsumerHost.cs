// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AzureEventHubs = Microsoft.Azure.EventHubs;
using AzureConsumer = Microsoft.Azure.EventHubs.Processor;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="AzureEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubConsumerHost : EventSubscriberHost
    {
        private EventHubConsumerHostInvoker? _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubConsumerHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubConsumerHost"/>.</param>
        public EventHubConsumerHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets or sets the <see cref="EventHubConsumerHostInvoker"/>. Defaults to <see cref="EventHubConsumerHostInvoker"/>.
        /// </summary>
        public EventHubConsumerHostInvoker Invoker { get => _invoker ??= new EventHubConsumerHostInvoker(); set => _invoker = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Use (set) the <see cref="Invoker"/>.
        /// </summary>
        /// <param name="invoker">The <see cref="EventHubConsumerHostInvoker"/>.</param>
        /// <returns>The <see cref="EventHubConsumerHost"/> instance (for fluent-style method chaining).</returns>
        public EventHubConsumerHost UseInvoker(EventHubConsumerHostInvoker invoker)
        {
            Invoker = invoker;
            return this;
        }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>The <see cref="EventHubConsumerHost"/> instance (for fluent-style method chaining).</returns>
        public EventHubConsumerHost UseLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Performs the receive processing for an <see cref="AzureEventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="partitionContext">The <see cref="AzureConsumer.PartitionContext"/>.</param>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/> instance to receive/process.</param>
        public Task ReceiveAsync(AzureConsumer.PartitionContext partitionContext, AzureEventHubs.EventData @event)
        {
            if (partitionContext == null)
                throw new ArgumentNullException(nameof(partitionContext));

            var ehd = new EventHubData(partitionContext.EventHubPath, partitionContext.ConsumerGroupName, partitionContext.PartitionId, @event ?? throw new ArgumentNullException(nameof(@event)));

            return Invoker.InvokeAsync(this, async () =>
            {
                // Invoke the base EventSubscriberHost.ReceiveAsync to do the actual work!
                var (_, subject, action, _, _, _) = @event.GetBeefMetadata();
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