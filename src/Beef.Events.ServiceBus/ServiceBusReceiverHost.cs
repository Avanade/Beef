// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using MicrosoftServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the Azure Service Bus (see <see cref="MicrosoftServiceBus.Message"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class ServiceBusReceiverHost : EventSubscriberHost<MicrosoftServiceBus.Message, ServiceBusData, ServiceBusReceiverHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiverHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <param name="eventDataConverter">The optional <see cref="IEventDataConverter{T}"/>. Defaults to a <see cref="MicrosoftServiceBusMessageConverter"/> using a <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public ServiceBusReceiverHost(EventSubscriberHostArgs args, IEventDataConverter<MicrosoftServiceBus.Message>? eventDataConverter = null) 
            : base(args, eventDataConverter ?? new MicrosoftServiceBusMessageConverter(new NewtonsoftJsonCloudEventSerializer())) { }
    }
}