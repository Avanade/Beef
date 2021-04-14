// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Mime;
using System.Threading.Tasks;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents an <b>Azure Service Bus</b> (see <see cref="AzureServiceBus.ServiceBusMessage"/>) <see cref="EventData"/> converter.
    /// </summary>
    public sealed class AzureServiceBusMessageConverter : IEventDataConverter<AzureServiceBus.ServiceBusMessage>
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
        /// Initializes a new instance of the <see cref="AzureServiceBusMessageConverter"/> class.
        /// </summary>
        /// <param name="contentSerializer">The <see cref="IEventDataContentSerializer"/>. Defaults to <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        /// <param name="useAttributes">Indicates whether to use the <see cref="AzureServiceBus.ServiceBusMessage.ApplicationProperties"/> for <see cref="EventMetadata"/>.</param>
        public AzureServiceBusMessageConverter(IEventDataContentSerializer? contentSerializer = null, bool useAttributes = true) => _contentSerializer = contentSerializer ?? new NewtonsoftJsonCloudEventSerializer();

        /// <summary>
        /// Indicates whether to write to the <see cref="AzureServiceBus.ServiceBusMessage.ApplicationProperties"/> for the <see cref="EventMetadata"/>. Defaults to <c>true</c>.
        /// </summary>
        public bool UseMessagingPropertiesForMetadata { get; set; } = true;

        /// <summary>
        /// Converts a <see cref="AzureServiceBus.ServiceBusMessage"/> to an <see cref="EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        public async Task<EventData> ConvertFromAsync(AzureServiceBus.ServiceBusMessage message)
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
        public async Task<EventData> ConvertFromAsync(Type valueType, AzureServiceBus.ServiceBusMessage message)
        {
            var ed = await (_contentSerializer.DeserializeAsync(valueType, message.Body.ToArray()).ConfigureAwait(false)) ?? CreateValueEventData(valueType, await GetMetadataAsync(message).ConfigureAwait(false));
            await MergeMetadataAndFixKeyAsync(ed, message).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Merge in the metadata and fix the key.
        /// </summary>
        private async Task MergeMetadataAndFixKeyAsync(EventData ed, AzureServiceBus.ServiceBusMessage message)
        {
            if (!UseMessagingPropertiesForMetadata)
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
        /// Converts an <see cref="EventData"/> to a <see cref="AzureServiceBus.ServiceBusMessage"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="AzureServiceBus.ServiceBusMessage"/>.</returns>
        public async Task<AzureServiceBus.ServiceBusMessage> ConvertToAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrEmpty(@event.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(@event));

            var bytes = await _contentSerializer.SerializeAsync(@event).ConfigureAwait(false);
            var msg = new AzureServiceBus.ServiceBusMessage(new BinaryData(bytes))
            {
                Subject = @event.Subject,
                ContentType = MediaTypeNames.Application.Json
            };

            if (@event.EventId != null)
                msg.MessageId = @event.EventId?.ToString();

            if (@event.CorrelationId != null)
                msg.CorrelationId = @event.CorrelationId;

            if (@event.PartitionKey != null)
                msg.PartitionKey = @event.PartitionKey;

            if (UseMessagingPropertiesForMetadata)
            {
                msg.ApplicationProperties.Add(EventMetadata.SubjectPropertyName, @event.Subject);

                if (@event.EventId != null)
                    msg.ApplicationProperties.Add(EventMetadata.EventIdPropertyName, @event.EventId);

                if (@event.Action != null)
                    msg.ApplicationProperties.Add(EventMetadata.ActionPropertyName, @event.Action);

                if (@event.TenantId != null)
                    msg.ApplicationProperties.Add(EventMetadata.TenantIdPropertyName, @event.TenantId);

                if (@event.Source != null)
                    msg.ApplicationProperties.Add(EventMetadata.SourcePropertyName, @event.Source);

                if (@event.Key != null)
                    msg.ApplicationProperties.Add(EventMetadata.KeyPropertyName, @event.Key);

                if (@event.ETag != null)
                    msg.ApplicationProperties.Add(EventMetadata.ETagPropertyName, @event.ETag);

                if (@event.Username != null)
                    msg.ApplicationProperties.Add(EventMetadata.UsernamePropertyName, @event.Username);

                if (@event.UserId != null)
                    msg.ApplicationProperties.Add(EventMetadata.UserIdPropertyName, @event.UserId);

                if (@event.Timestamp != null)
                    msg.ApplicationProperties.Add(EventMetadata.TimestampPropertyName, @event.Timestamp);

                if (@event.CorrelationId != null)
                    msg.ApplicationProperties.Add(EventMetadata.CorrelationIdPropertyName, @event.CorrelationId);

                if (@event.PartitionKey != null)
                    msg.ApplicationProperties.Add(EventMetadata.PartitionKeyPropertyName, @event.PartitionKey);
            }

            return msg;
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="AzureServiceBus.ServiceBusMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        public async Task<EventMetadata?> GetMetadataAsync(AzureServiceBus.ServiceBusMessage message)
        {
            if (message == null)
                return null;

            message.ApplicationProperties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);

            // Where the Subject metadata is defined assume it is correctly configured.
            if (subject != null)
            {
                message.ApplicationProperties.TryGetValue(EventMetadata.ActionPropertyName, out var action);
                message.ApplicationProperties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
                message.ApplicationProperties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);
                message.ApplicationProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key);
                message.ApplicationProperties.TryGetValue(EventMetadata.ETagPropertyName, out var etag);
                message.ApplicationProperties.TryGetValue(EventMetadata.UsernamePropertyName, out var username);
                message.ApplicationProperties.TryGetValue(EventMetadata.UserIdPropertyName, out var userId);

                return new EventMetadata
                {
                    EventId = (message.ApplicationProperties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : Guid.TryParse(message.MessageId, out var mid) ? mid : (Guid?)null,
                    TenantId = (message.ApplicationProperties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null,
                    Subject = (string?)subject ?? message.Subject,
                    Action = (string?)action,
                    Source = (message.ApplicationProperties.TryGetValue(EventMetadata.SourcePropertyName, out var src) && src != null && src is Uri) ? (Uri?)tid : null,
                    Key = key,
                    ETag = (string)etag,
                    Username = (string?)username,
                    UserId = (string?)userId,
                    Timestamp = (message.ApplicationProperties.TryGetValue(EventMetadata.TimestampPropertyName, out var time) && time != null && time is DateTime?) ? (DateTime?)time : null,
                    CorrelationId = (string?)correlationId ?? message.CorrelationId,
                    PartitionKey = (string?)partitionKey ?? message.PartitionKey
                }; 
            }

            // Try deserializing to get metadata where possible.
            return await _contentSerializer.DeserializeAsync(message.Body.ToArray()).ConfigureAwait(false);
        }
    }
}