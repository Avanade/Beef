// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents the originating event data for the <see cref="ServiceBusReceiverHost"/>.
    /// </summary>
    public class ServiceBusData : EventSubscriberData<AzureServiceBus.Message>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusData"/> class.
        /// </summary>
        /// <param name="serviceBusName">The Event Hubs name.</param>
        /// <param name="queueName">The Event Hubs partition identifier.</param>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="AzureServiceBus.Message"/>.</param>
        public ServiceBusData(string serviceBusName, string queueName, AzureServiceBus.Message originating) : base(originating)
        {
            ServiceBusName = Check.NotNull(serviceBusName, nameof(serviceBusName));
            QueueName = Check.NotNull(queueName, nameof(queueName));
        }

        /// <summary>
        /// Gets the Service Bus name.
        /// </summary>
        public string ServiceBusName { get; }

        /// <summary>
        /// Gets the Service Bus Queue name.
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> metadata.
        /// </summary>
        protected override EventMetadata GetEventMetadata() => Originating.GetEventMetadata();
    }
}