// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents the originating message data for the <see cref="ServiceBusReceiverHost"/>.
    /// </summary>
    /// <remarks><b>Note:</b> this must match the properties for the corresponding Service Bus Receiver or unintended side-effects may occur.</remarks>
    public class ServiceBusData : EventSubscriberData<ServiceBusReceivedMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusData"/> class for a <i>Queue</i>.
        /// </summary>
        /// <param name="serviceBusName">The Service Bus (namespace) host; e.g. '<c>beef.servicebus.windows.net</c>'.</param>
        /// <param name="queueName">The Queue name.</param>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="ServiceBusReceivedMessage"/>.</param>
        public ServiceBusData(string serviceBusName, string queueName, ServiceBusReceivedMessage originating) : base(originating)
        {
            ServiceBusName = Check.NotNull(serviceBusName, nameof(serviceBusName));
            QueueName = Check.NotNull(queueName, nameof(queueName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusData"/> class for a <i>Topic and Subscription</i>.
        /// </summary>
        /// <param name="serviceBusName">The Service Bus (namespace) host; e.g. '<c>beef.servicebus.windows.net</c>'.</param>
        /// <param name="topicName">The Topic name.</param>
        /// <param name="subscriptionName">The Subscription name.</param>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="ServiceBusReceivedMessage"/>.</param>
        public ServiceBusData(string serviceBusName, string topicName, string subscriptionName, ServiceBusReceivedMessage originating) : base(originating)
        {
            ServiceBusName = Check.NotNull(serviceBusName, nameof(serviceBusName));
            QueueName = Check.NotNull(topicName, nameof(topicName));
            SubscriptionName = Check.NotNull(subscriptionName, nameof(subscriptionName));
        }

        /// <summary>
        /// Gets the Service Bus name.
        /// </summary>
        public string ServiceBusName { get; }

        /// <summary>
        /// Gets the Service Bus Queue (or Topic) name.
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Gets the Service Bus Topic Subscription name.
        /// </summary>
        public string? SubscriptionName { get; }

        /// <summary>
        /// Indicates whether the <see cref="ServiceBusData"/> is from a <i>Topic</i>; versus, a <i>Queue</i>.
        /// </summary>
        public bool IsTopic => SubscriptionName != null;
    }
}