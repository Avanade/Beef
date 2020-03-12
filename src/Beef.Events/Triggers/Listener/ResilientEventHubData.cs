// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.Triggers.Listener
{
    /// <summary>
    /// Represents the "resilient event hub" data; being the <see cref="EventHubs.EventData"/>.
    /// </summary>
    public class ResilientEventHubData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubData"/> class.
        /// </summary>
        /// <param name="eventData"></param>
        public ResilientEventHubData(EventHubs.EventData eventData) => EventData = eventData;

        /// <summary>
        /// Gets or sets the <see cref="EventHubs.EventData"/>.
        /// </summary>
        public EventHubs.EventData EventData { get; }
    }
}