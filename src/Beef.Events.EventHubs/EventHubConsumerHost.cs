// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using MicrosoftEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="MicrosoftEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubConsumerHost : EventSubscriberHost<MicrosoftEventHubs.EventData, EventHubData, EventHubConsumerHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubConsumerHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubConsumerHost"/>.</param>
        /// <param name="eventDataConverter">The optional <see cref="IEventDataConverter{T}"/>. Defaults to a <see cref="MicrosoftEventHubsEventConverter"/> using a <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public EventHubConsumerHost(EventSubscriberHostArgs args, IEventDataConverter<MicrosoftEventHubs.EventData>? eventDataConverter = null) 
            : base(args, eventDataConverter ?? new MicrosoftEventHubsEventConverter(new NewtonsoftJsonCloudEventSerializer())) { }
    }
}