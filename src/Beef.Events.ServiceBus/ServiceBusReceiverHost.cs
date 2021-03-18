// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the Azure Service Bus (see <see cref="AzureServiceBus.Message"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class ServiceBusReceiverHost : EventSubscriberHost<AzureServiceBus.Message, ServiceBusData, ServiceBusReceiverHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiverHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="ServiceBusReceiverHost"/>.</param>
        public ServiceBusReceiverHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets the <see cref="EventData"/> from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The event/message data.</param>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> identified to process.</param>
        /// <returns>The corresponding <see cref="EventData"/>.</returns>
        protected override EventData GetBeefEventData(ServiceBusData data, IEventSubscriber subscriber)
            => subscriber.ValueType == null ? data.Originating.ToBeefEventData() : data.Originating.ToBeefEventData(subscriber.ValueType);
    }
}