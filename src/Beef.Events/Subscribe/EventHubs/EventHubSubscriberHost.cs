// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureEventHubs = Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="AzureEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubSubscriberHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubSubscriberHost"/>.</param>
        public EventHubSubscriberHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets or sets the <see cref="EventHubSubscriberHostInvoker"/>. Defaults to <see cref="EventHubSubscriberHostInvoker"/>.
        /// </summary>
        public EventHubSubscriberHostInvoker? Invoker { get; set; }

        /// <summary>
        /// Performs the receive processing for an <see cref="AzureEventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/> instance to receive/process.</param>
        public Task ReceiveAsync(AzureEventHubs.EventData @event)
        {
            if (@event == null)
                return Task.CompletedTask;

            return (Invoker ??= new EventHubSubscriberHostInvoker()).InvokeAsync(this, async () =>
            {
                // Invoke the base EventSubscriberHost.ReceiveAsync to do the actual work!
                var (_, subject, action, _) = @event.GetBeefMetadata();
                await ReceiveAsync(@event, subject, action, (subscriber) =>
                {
                    // Convert AzureEventHubs.EventData to Beef.EventData.
                    try
                    {
                        return subscriber.ValueType == null ? @event.ToBeefEventData() : @event.ToBeefEventData(subscriber.ValueType);
                    }
                    catch (Exception ex) { throw new EventSubscriberUnhandledException(Result.InvalidEventData(ex)); }
                }).ConfigureAwait(false);
            }, @event);
        }
    }
}