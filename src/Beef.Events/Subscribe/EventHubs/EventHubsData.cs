// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AzureEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Represents the originating event data for the <see cref="EventHubSubscriberHost"/>.
    /// </summary>
    public class EventHubsData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubsData"/> class.
        /// </summary>
        /// <param name="eventHubPath">The Event Hubs path.</param>
        /// <param name="consumerGroupName">The Event Hubs consumer group name.</param>
        /// <param name="partitionId">The Event Hubs partition identifier.</param>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        public EventHubsData(string eventHubPath, string consumerGroupName, string partitionId, AzureEventHubs.EventData @event)
        {
            EventHubPath = Check.NotNull(eventHubPath, nameof(eventHubPath));
            ConsumerGroupName = Check.NotNull(consumerGroupName, nameof(consumerGroupName));
            PartitionId = Check.NotNull(partitionId, nameof(partitionId));
            Event = Check.NotNull(@event, nameof(@event));
        }

        /// <summary>
        /// Gets or sets the Event Hubs path.
        /// </summary>
        public string? EventHubPath { get; }

        /// <summary>
        /// Gets or sets the Event Hubs consumer group name.
        /// </summary>
        public string? ConsumerGroupName { get; }

        /// <summary>
        /// Gets or sets the Event Hubs partition identifier.
        /// </summary>
        public string? PartitionId { get; }

        /// <summary>
        /// Gets the <see cref="AzureEventHubs.EventData"/>.
        /// </summary>
        public AzureEventHubs.EventData Event { get; }
    }
}