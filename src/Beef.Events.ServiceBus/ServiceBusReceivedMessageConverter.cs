// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents an <b>Azure Service Bus</b> (see <see cref="AzureServiceBus.ServiceBusReceivedMessage"/>) <see cref="EventData"/> converter.
    /// </summary>
    public sealed class ServiceBusReceivedMessageConverter : IEventDataConverter<AzureServiceBus.ServiceBusReceivedMessage>
    {
        private readonly IEventDataContentSerializer _contentSerializer;

        /// <summary>
        /// Creates a new instance of <see cref="EventData{T}"/> using the specified <paramref name="valueType"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="metadata">The corresponding <see cref="EventMetadata"/>.</param>
        /// <returns>A new instance of <see cref="EventData{T}"/></returns>
        internal static EventData CreateValueEventData(Type valueType, EventMetadata? metadata) => (EventData)Activator.CreateInstance(typeof(EventData<>).MakeGenericType(new Type[] { valueType }), new object[] { metadata! });

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceivedMessageConverter"/> class.
        /// </summary>
        /// <param name="contentSerializer">The <see cref="IEventDataContentSerializer"/>. Defaults to <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        /// <param name="useApplicationPropertiesForMetadata">Indicates whether to use the <see cref="AzureServiceBus.ServiceBusMessage.ApplicationProperties"/> for <see cref="EventMetadata"/>.</param>
        public ServiceBusReceivedMessageConverter(IEventDataContentSerializer? contentSerializer = null, bool useApplicationPropertiesForMetadata = true)
        {
            _contentSerializer = contentSerializer ?? new NewtonsoftJsonCloudEventSerializer();
            UseApplicationPropertiesForMetadata = useApplicationPropertiesForMetadata;
        }

        /// <summary>
        /// Indicates whether to write to the <see cref="AzureServiceBus.ServiceBusMessage.ApplicationProperties"/> for the <see cref="EventMetadata"/>.
        /// </summary>
        public bool UseApplicationPropertiesForMetadata { get; set; }

        /// <summary>
        /// Converts a <see cref="AzureServiceBus.ServiceBusMessage"/> to an <see cref="EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        public async Task<EventData> ConvertFromAsync(AzureServiceBus.ServiceBusReceivedMessage message)
        {
            var ed = (await _contentSerializer.DeserializeAsync(message.Body.ToArray()).ConfigureAwait(false)) ?? new EventData(null);
            await MergeMetadataAndFixKeyAsync(ed, message).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Converts a <see cref="AzureServiceBus.ServiceBusMessage"/> to an <see cref="EventData{TEventData}"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The converted <see cref="EventData{TEventData}"/>.</returns>
        public async Task<EventData> ConvertFromAsync(Type valueType, AzureServiceBus.ServiceBusReceivedMessage message)
        {
            var ed = await (_contentSerializer.DeserializeAsync(valueType, message.Body.ToArray()).ConfigureAwait(false)) ?? CreateValueEventData(valueType, await GetMetadataAsync(message).ConfigureAwait(false));
            await MergeMetadataAndFixKeyAsync(ed, message).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Merge in the metadata and fix the key.
        /// </summary>
        private async Task MergeMetadataAndFixKeyAsync(EventData ed, AzureServiceBus.ServiceBusReceivedMessage message)
        {
            if (!UseApplicationPropertiesForMetadata)
                return;

            var md = await GetMetadataAsync(message).ConfigureAwait(false);
            if (md != null)
            {
                ed.MergeMetadata(md);

                // Always override the key as it doesn't lose the type like the serialized value does.
                if (message.ApplicationProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                    ed.Key = key;
            }
        }

        /// <summary>
        /// Converts an <see cref="EventData"/> to a <see cref="AzureServiceBus.ServiceBusReceivedMessage"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="AzureServiceBus.ServiceBusReceivedMessage"/>.</returns>
        /// <remarks>This will throw a <see cref="NotSupportedException"/> as a <see cref="AzureServiceBus.ServiceBusReceivedMessage"/> should only be received; not created directly.</remarks>
        public Task<AzureServiceBus.ServiceBusReceivedMessage> ConvertToAsync(EventData @event) => throw new NotSupportedException();

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="AzureServiceBus.ServiceBusReceivedMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        public async Task<EventMetadata?> GetMetadataAsync(AzureServiceBus.ServiceBusReceivedMessage message)
        {
            if (message == null)
                return null;

            message.ApplicationProperties.TryGetValue(EventMetadata.SubjectAttributeName, out var subject);

            // Where the Subject metadata is defined assume it is correctly configured.
            if (subject != null)
            {
                message.ApplicationProperties.TryGetValue(EventMetadata.ActionAttributeName, out var action);
                message.ApplicationProperties.TryGetValue(EventMetadata.CorrelationIdAttributeName, out var correlationId);
                message.ApplicationProperties.TryGetValue(EventMetadata.PartitionKeyAttributeName, out var partitionKey);
                message.ApplicationProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key);
                message.ApplicationProperties.TryGetValue(EventMetadata.ETagAttributeName, out var etag);
                message.ApplicationProperties.TryGetValue(EventMetadata.UsernameAttributeName, out var username);
                message.ApplicationProperties.TryGetValue(EventMetadata.UserIdAttributeName, out var userId);

                return new EventMetadata
                {
                    EventId = (message.ApplicationProperties.TryGetValue(EventMetadata.EventIdAttributeName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : Guid.TryParse(message.MessageId, out var mid) ? mid : (Guid?)null,
                    TenantId = (message.ApplicationProperties.TryGetValue(EventMetadata.TenantIdAttributeName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null,
                    Subject = (string?)subject ?? message.Subject,
                    Action = (string?)action,
                    Source = (message.ApplicationProperties.TryGetValue(EventMetadata.SourceAttributeName, out var src) && src != null && src is Uri) ? (Uri?)src : null,
                    Key = key,
                    ETag = (string)etag,
                    Username = (string?)username,
                    UserId = (string?)userId,
                    Timestamp = (message.ApplicationProperties.TryGetValue(EventMetadata.TimestampAttributeName, out var time) && time != null && time is DateTime?) ? (DateTime?)time : null,
                    CorrelationId = (string?)correlationId ?? message.CorrelationId,
                    PartitionKey = (string?)partitionKey ?? message.PartitionKey
                }; 
            }

            // Try deserializing to get metadata where possible.
            return await _contentSerializer.DeserializeAsync(message.Body.ToArray()).ConfigureAwait(false);
        }
    }
}