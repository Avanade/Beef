// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureEventHubs = Azure.Messaging.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Represents the originating event data for the <see cref="EventHubConsumerHost"/>.
    /// </summary>
    public class EventHubData : EventSubscriberData<AzureEventHubs.EventData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubData"/> class.
        /// </summary>
        /// <param name="eventHubName">The Event Hubs name.</param>
        /// <param name="consumerGroupName">The Event Hubs consumer group name.</param>
        /// <param name="partitionId">The Event Hubs partition identifier.</param>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="AzureEventHubs.EventData"/>.</param>
        public EventHubData(string eventHubName, string consumerGroupName, string partitionId, AzureEventHubs.EventData originating) : base(originating)
        {
            EventHubName = Check.NotNull(eventHubName, nameof(eventHubName));
            ConsumerGroupName = Check.NotNull(consumerGroupName, nameof(consumerGroupName));
            PartitionId = Check.NotNull(partitionId, nameof(partitionId));
        }

        /// <summary>
        /// Gets or sets the Event Hubs path.
        /// </summary>
        public string EventHubName { get; }

        /// <summary>
        /// Gets or sets the Event Hubs consumer group name.
        /// </summary>
        public string ConsumerGroupName { get; }

        /// <summary>
        /// Gets or sets the Event Hubs partition identifier.
        /// </summary>
        public string PartitionId { get; }
    }
}