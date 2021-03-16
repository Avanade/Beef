// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents the originating event data for the <see cref="ServiceBusReceiverHost"/>.
    /// </summary>
    public class ServiceBusData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusData"/> class.
        /// </summary>
        /// <param name="serviceBusName">The Event Hubs name.</param>
        /// <param name="queueName">The Event Hubs partition identifier.</param>
        /// <param name="message">The <see cref="AzureServiceBus.Message"/>.</param>
        public ServiceBusData(string serviceBusName, string queueName, AzureServiceBus.Message message)
        {
            ServiceBusName = Check.NotNull(serviceBusName, nameof(serviceBusName));
            QueueName = Check.NotNull(queueName, nameof(queueName));
            Message = Check.NotNull(message, nameof(message));
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
        /// Gets the <see cref="AzureServiceBus.Message"/>.
        /// </summary>
        public AzureServiceBus.Message Message { get; }

        /// <summary>
        /// Gets the invocation attempt counter.
        /// </summary>
        /// <remarks>A value of zero indicates that the attempt count is currently unknown.</remarks>
        public int Attempt { get; internal set; }
    }
}