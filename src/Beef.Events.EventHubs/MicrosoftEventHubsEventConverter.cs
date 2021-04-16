// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using MicrosoftEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Represents an <b>Azure Event Hubs</b> (see <see cref="MicrosoftEventHubs.EventData"/>) <see cref="EventData"/> converter.
    /// </summary>
    public sealed class MicrosoftEventHubsEventConverter : IEventDataConverter<MicrosoftEventHubs.EventData>
    {
        private readonly IEventDataContentSerializer _contentSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftEventHubsEventConverter"/> class.
        /// </summary>
        /// <param name="contentSerializer">The <see cref="IEventDataContentSerializer"/>. Defaults to <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public MicrosoftEventHubsEventConverter(IEventDataContentSerializer? contentSerializer = null) => _contentSerializer = contentSerializer ?? new NewtonsoftJsonCloudEventSerializer();

        /// <summary>
        /// Indicates whether to write to the <see cref="MicrosoftEventHubs.EventData.Properties"/> for the <see cref="EventMetadata"/>.
        /// </summary>
        public bool UseMessagingPropertiesForMetadata { get; set; }

        /// <summary>
        /// Converts a <see cref="MicrosoftEventHubs.EventData"/> to an <see cref="EventData"/>.
        /// </summary>
        /// <param name="event">The <see cref="MicrosoftEventHubs.EventData"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        public async Task<EventData> ConvertFromAsync(MicrosoftEventHubs.EventData @event)
        {
            var ed = (await _contentSerializer.DeserializeAsync(@event.Body.ToArray()).ConfigureAwait(false)) ?? new EventData(null);
            await MergeMetadataAndFixKeyAsync(ed, @event).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Converts a <see cref="MicrosoftEventHubs.EventData"/> to an <see cref="EventData{TEventData}"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="event">The <see cref="MicrosoftEventHubs.EventData"/>.</param>
        /// <returns>The converted <see cref="EventData{TEventData}"/>.</returns>
        public async Task<EventData> ConvertFromAsync(Type valueType, MicrosoftEventHubs.EventData @event)
        {
            var ed = (await _contentSerializer.DeserializeAsync(valueType, @event.Body.ToArray()).ConfigureAwait(false)) ?? AzureEventHubsEventConverter.CreateValueEventData(valueType, await GetMetadataAsync(@event).ConfigureAwait(false));
            await MergeMetadataAndFixKeyAsync(ed, @event).ConfigureAwait(false);
            return ed;
        }

        /// <summary>
        /// Merge in the metadata and fix the key.
        /// </summary>
        private async Task MergeMetadataAndFixKeyAsync(EventData ed, MicrosoftEventHubs.EventData eh)
        {
            if (!UseMessagingPropertiesForMetadata)
                return;

            var md = await GetMetadataAsync(eh).ConfigureAwait(false);
            if (md != null)
            {
                ed.MergeMetadata(md);

                // Always override the key as it doesn't lose the type like the serialized value does.
                if (eh.Properties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                    ed.Key = key;
            }
        }

        /// <summary>
        /// Converts an <see cref="EventData"/> to a <see cref="MicrosoftEventHubs.EventData"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="MicrosoftEventHubs.EventData"/>.</returns>
        public async Task<MicrosoftEventHubs.EventData> ConvertToAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrEmpty(@event.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(@event));

            var bytes = await _contentSerializer.SerializeAsync(@event).ConfigureAwait(false);
            var ehed = new MicrosoftEventHubs.EventData(bytes);

            if (UseMessagingPropertiesForMetadata)
            {
                ehed.Properties.Add(EventMetadata.SubjectAttributeName, @event.Subject);

                if (@event.EventId != null)
                    ehed.Properties.Add(EventMetadata.EventIdAttributeName, @event.EventId);

                if (@event.Action != null)
                    ehed.Properties.Add(EventMetadata.ActionAttributeName, @event.Action);

                if (@event.TenantId != null)
                    ehed.Properties.Add(EventMetadata.TenantIdAttributeName, @event.TenantId);

                if (@event.Source != null)
                    ehed.Properties.Add(EventMetadata.SourceAttributeName, @event.Source);

                if (@event.Key != null)
                    ehed.Properties.Add(EventMetadata.KeyPropertyName, @event.Key);

                if (@event.ETag != null)
                    ehed.Properties.Add(EventMetadata.ETagAttributeName, @event.ETag);

                if (@event.Username != null)
                    ehed.Properties.Add(EventMetadata.UsernameAttributeName, @event.Username);

                if (@event.UserId != null)
                    ehed.Properties.Add(EventMetadata.UserIdAttributeName, @event.UserId);

                if (@event.Timestamp != null)
                    ehed.Properties.Add(EventMetadata.TimestampAttributeName, @event.Timestamp);

                if (@event.CorrelationId != null)
                    ehed.Properties.Add(EventMetadata.CorrelationIdAttributeName, @event.CorrelationId);

                if (@event.PartitionKey != null)
                    ehed.Properties.Add(EventMetadata.PartitionKeyAttributeName, @event.PartitionKey);
            }

            return ehed;
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="MicrosoftEventHubs.EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        public async Task<EventMetadata?> GetMetadataAsync(MicrosoftEventHubs.EventData message)
        {
            if (message == null)
                return null;

            if (UseMessagingPropertiesForMetadata)
            {
                message.Properties.TryGetValue(EventMetadata.SubjectAttributeName, out var subject);
                message.Properties.TryGetValue(EventMetadata.ActionAttributeName, out var action);
                message.Properties.TryGetValue(EventMetadata.CorrelationIdAttributeName, out var correlationId);
                message.Properties.TryGetValue(EventMetadata.PartitionKeyAttributeName, out var partitionKey);
                message.Properties.TryGetValue(EventMetadata.KeyPropertyName, out var key);
                message.Properties.TryGetValue(EventMetadata.ETagAttributeName, out var etag);
                message.Properties.TryGetValue(EventMetadata.UsernameAttributeName, out var username);
                message.Properties.TryGetValue(EventMetadata.UserIdAttributeName, out var userId);

                return new EventMetadata
                {
                    EventId = (message.Properties.TryGetValue(EventMetadata.EventIdAttributeName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null,
                    TenantId = (message.Properties.TryGetValue(EventMetadata.TenantIdAttributeName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null,
                    Subject = (string?)subject,
                    Action = (string?)action,
                    Source = (message.Properties.TryGetValue(EventMetadata.SourceAttributeName, out var src) && src != null && src is Uri) ? (Uri?)src : null,
                    Key = key,
                    ETag = (string)etag,
                    Username = (string?)username,
                    UserId = (string?)userId,
                    Timestamp = (message.Properties.TryGetValue(EventMetadata.TimestampAttributeName, out var time) && time != null && time is DateTime?) ? (DateTime?)time : null,
                    CorrelationId = (string?)correlationId,
                    PartitionKey = (string?)partitionKey
                }; 
            }

            // Try deserializing to get metadata where possible.
            return await _contentSerializer.DeserializeAsync(message.Body.ToArray()).ConfigureAwait(false);
        }
    }
}