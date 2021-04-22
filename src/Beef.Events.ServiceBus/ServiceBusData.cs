// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using MicrosoftServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents the originating message data for the <see cref="ServiceBusReceiverHost"/>.
    /// </summary>
    public class ServiceBusData : EventSubscriberData<MicrosoftServiceBus.Message>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusData"/> class.
        /// </summary>
        /// <param name="serviceBusName">The Service Bus (namespace) host; e.g. '<c>beef.servicebus.windows.net</c>'.</param>
        /// <param name="queueName">The Queue name.</param>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="MicrosoftServiceBus.Message"/>.</param>
        public ServiceBusData(string serviceBusName, string queueName, MicrosoftServiceBus.Message originating) : base(originating)
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
    }
}