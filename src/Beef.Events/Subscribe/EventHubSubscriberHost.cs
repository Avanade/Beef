// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="EventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubSubscriberHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubSubscriberHost"/>.</param>
        public EventHubSubscriberHost(EventSubscriberHostArgs args) : base(args) 
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.AuditWriter == null)
                args.AuditWriter = (result) => throw new EventSubscriberStopException(result);
        }

        /// <summary>
        /// Performs the receive processing for an <see cref="EventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="EventHubs.EventData"/> instance to receive/process.</param>
        public async Task ReceiveAsync(EventHubs.EventData @event)
        {
            if (@event == null)
                return;

            // Convert EventHubs.EventData to Beef.EventData.
            var (_, subject, action, _) = @event.GetBeefMetadata();
            await ReceiveAsync(subject, action, (subscriber) =>
            {
                try
                {
                    return subscriber.ValueType == null ? @event.ToBeefEventData() : @event.ToBeefEventData(subscriber.ValueType);
                }
                catch (Exception ex) { throw new InvalidEventDataException(ex); }
            }).ConfigureAwait(false);
        }
    }
}