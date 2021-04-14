// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Mime;
using System.Threading.Tasks;
using MicrosoftServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents an <b>Azure Service Bus</b> (see <see cref="MicrosoftServiceBus.Message"/>) <see cref="EventData"/> converter.
    /// </summary>
    public sealed class MicrosoftServiceBusMessageConverter : IEventDataConverter<MicrosoftServiceBus.Message>
    {
        private readonly IEventDataContentSerializer _contentSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftServiceBusMessageConverter"/> class.
        /// </summary>
        /// <param name="contentSerializer">The <see cref="IEventDataContentSerializer"/>. Defaults to <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public MicrosoftServiceBusMessageConverter(IEventDataContentSerializer? contentSerializer = null) => _contentSerializer = contentSerializer ?? new NewtonsoftJsonCloudEventSerializer();

        /// <summary>
        /// Indicates whether to write to the <see cref="MicrosoftServiceBus.Message.UserProperties"/> for the <see cref="EventMetadata"/>. Defaults to <c>true</c>.
        /// </summary>
        public bool UseMessagingPropertiesForMetadata { get; set; } = true;

        /// <summary>
        /// Converts a <see cref="MicrosoftServiceBus.Message"/> to an <see cref="EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        public async Task<EventData> ConvertFromAsync(MicrosoftServiceBus.Message message)
        {
            var ed = (await _contentSerializer.DeserializeAsync(message.Body).ConfigureAwait(false)) ?? new EventData(null);
            await MergeMetadataAndFixKeyAsync(ed, message).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Converts a <see cref="MicrosoftServiceBus.Message"/> to an <see cref="EventData{TEventData}"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The converted <see cref="EventData{TEventData}"/>.</returns>
        public async Task<EventData> ConvertFromAsync(Type valueType, MicrosoftServiceBus.Message message)
        {
            var ed = await (_contentSerializer.DeserializeAsync(valueType, message.Body).ConfigureAwait(false)) ?? AzureServiceBusMessageConverter.CreateValueEventData(valueType, await GetMetadataAsync(message).ConfigureAwait(false));
            await MergeMetadataAndFixKeyAsync(ed, message).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Merge in the metadata and fix the key.
        /// </summary>
        private async Task MergeMetadataAndFixKeyAsync(EventData ed, MicrosoftServiceBus.Message message)
        {
            if (!UseMessagingPropertiesForMetadata)
                return;

            var md = await GetMetadataAsync(message).ConfigureAwait(false);
            if (md != null)
            {
                ed.MergeMetadata(md);

                // Always override the key as it doesn't lose the type like the serialized value does.
                if (message.UserProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                    ed.Key = key;
            }
        }

        /// <summary>
        /// Converts an <see cref="EventData"/> to a <see cref="MicrosoftServiceBus.Message"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="MicrosoftServiceBus.Message"/>.</returns>
        public async Task<MicrosoftServiceBus.Message> ConvertToAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrEmpty(@event.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(@event));

            var bytes = await _contentSerializer.SerializeAsync(@event).ConfigureAwait(false);
            var msg = new MicrosoftServiceBus.Message(bytes)
            {
                Label = @event.Subject,
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
                msg.UserProperties.Add(EventMetadata.SubjectPropertyName, @event.Subject);

                if (@event.EventId != null)
                    msg.UserProperties.Add(EventMetadata.EventIdPropertyName, @event.EventId);

                if (@event.Action != null)
                    msg.UserProperties.Add(EventMetadata.ActionPropertyName, @event.Action);

                if (@event.TenantId != null)
                    msg.UserProperties.Add(EventMetadata.TenantIdPropertyName, @event.TenantId);

                if (@event.Source != null)
                    msg.UserProperties.Add(EventMetadata.SourcePropertyName, @event.Source);

                if (@event.Key != null)
                    msg.UserProperties.Add(EventMetadata.KeyPropertyName, @event.Key);

                if (@event.ETag != null)
                    msg.UserProperties.Add(EventMetadata.ETagPropertyName, @event.ETag);

                if (@event.Username != null)
                    msg.UserProperties.Add(EventMetadata.UsernamePropertyName, @event.Username);

                if (@event.UserId != null)
                    msg.UserProperties.Add(EventMetadata.UserIdPropertyName, @event.UserId);

                if (@event.Timestamp != null)
                    msg.UserProperties.Add(EventMetadata.TimestampPropertyName, @event.Timestamp);

                if (@event.CorrelationId != null)
                    msg.UserProperties.Add(EventMetadata.CorrelationIdPropertyName, @event.CorrelationId);

                if (@event.PartitionKey != null)
                    msg.UserProperties.Add(EventMetadata.PartitionKeyPropertyName, @event.PartitionKey);
            }

            return msg;
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="MicrosoftServiceBus.Message"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        public async Task<EventMetadata?> GetMetadataAsync(MicrosoftServiceBus.Message message)
        {
            if (message == null)
                return null;

            message.UserProperties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);

            // Where the Subject metadata is defined assume it is correctly configured.
            if (subject != null)
            {
                message.UserProperties.TryGetValue(EventMetadata.ActionPropertyName, out var action);
                message.UserProperties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
                message.UserProperties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);
                message.UserProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key);
                message.UserProperties.TryGetValue(EventMetadata.ETagPropertyName, out var etag);
                message.UserProperties.TryGetValue(EventMetadata.UsernamePropertyName, out var username);
                message.UserProperties.TryGetValue(EventMetadata.UserIdPropertyName, out var userId);

                return new EventMetadata
                {
                    EventId = (message.UserProperties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : Guid.TryParse(message.MessageId, out var mid) ? mid : (Guid?)null,
                    TenantId = (message.UserProperties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null,
                    Subject = (string?)subject ?? message.Label,
                    Action = (string?)action,
                    Source = (message.UserProperties.TryGetValue(EventMetadata.SourcePropertyName, out var src) && src != null && src is Uri) ? (Uri?)tid : null,
                    Key = key,
                    ETag = (string)etag,
                    Username = (string?)username,
                    UserId = (string?)userId,
                    Timestamp = (message.UserProperties.TryGetValue(EventMetadata.TimestampPropertyName, out var time) && time != null && time is DateTime?) ? (DateTime?)time : null,
                    CorrelationId = (string?)correlationId ?? message.CorrelationId,
                    PartitionKey = (string?)partitionKey ?? message.PartitionKey
                };
            }

            // Try deserializing to get metadata where possible.
            return await _contentSerializer.DeserializeAsync(message.Body).ConfigureAwait(false);
        }
    }
}