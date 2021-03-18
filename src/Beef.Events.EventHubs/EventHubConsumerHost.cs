// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="AzureEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubConsumerHost : EventSubscriberHost<AzureEventHubs.EventData, EventHubData, EventHubConsumerHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubConsumerHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubConsumerHost"/>.</param>
        public EventHubConsumerHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets the <see cref="EventData"/> from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The event/message data.</param>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> identified to process.</param>
        /// <returns>The corresponding <see cref="EventData"/>.</returns>
        protected override EventData GetBeefEventData(EventHubData data, IEventSubscriber subscriber)
            => subscriber.ValueType == null ? data.Originating.ToBeefEventData() : data.Originating.ToBeefEventData(subscriber.ValueType);
    }
}