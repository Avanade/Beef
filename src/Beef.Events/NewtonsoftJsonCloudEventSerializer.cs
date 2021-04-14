// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CloudNative.CloudEvents;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides a <see cref="CloudEvent"/> <see cref="IEventDataContentSerializer"/> using the <b>Newtonsoft</b> <see cref="JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftJsonCloudEventSerializer : IEventDataContentSerializer
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly string _subjectPropertyName = EventMetadata.SubjectPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _actionPropertyName = EventMetadata.ActionPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _tenantIdPropertyName = EventMetadata.TenantIdPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _correlationIdPropertyName = EventMetadata.CorrelationIdPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _partitionKeyPropertyName = EventMetadata.PartitionKeyPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _keyPropertyName = EventMetadata.KeyPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _etagPropertyName = EventMetadata.ETagPropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _usernamePropertyName = EventMetadata.UsernamePropertyName.ToLowerInvariant().Replace('.', '_');
        private readonly string _userIdPropertyName = EventMetadata.UserIdPropertyName.ToLowerInvariant().Replace('.', '_');

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonCloudEventSerializer"/> class using a default <see cref="JsonSerializer"/>.
        /// </summary>
        public NewtonsoftJsonCloudEventSerializer() => _jsonSerializer = JsonSerializer.CreateDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonCloudEventSerializer"/> class using the specified <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer"/>.</param>
        public NewtonsoftJsonCloudEventSerializer(JsonSerializer jsonSerializer) => _jsonSerializer = Check.NotNull(jsonSerializer, nameof(jsonSerializer));

        /// <summary>
        /// Indicates whether to include the <see cref="EventMetadata"/> as <see cref="CloudEvent"/> <see cref="CloudEvent.GetAttributes">attributes</see>.
        /// </summary>
        /// <remarks>Defaults to <c>true</c>.</remarks>
        public bool IncludeEventMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the default <see cref="CloudEvent.Source"/> where the <see cref="EventMetadata.Source"/> is not specified.
        /// </summary>
        /// <remarks>Defaults to '<c>/notspecified</c>'.</remarks>
        public Uri DefaultSource { get; set; } = new Uri("/notspecified", UriKind.Relative);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<byte[]> SerializeAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrEmpty(@event.Subject))
                throw new ArgumentException("Subject must be specified.", nameof(@event));

            var type = $"{@event.Subject}{(string.IsNullOrEmpty(@event.Action) ? "" : $".{@event.Action}")}";

            var ce = new CloudEvent(type, @event.Source ?? DefaultSource, @event.EventId?.ToString(), @event.Timestamp);
            if (@event.HasValue)
            {
                var sb = new StringBuilder();
                using var writer = new StringWriter(sb);
                _jsonSerializer.Serialize(writer, @event.GetValue());

                ce.DataContentType = new ContentType(MediaTypeNames.Application.Json);
                ce.Data = sb.ToString();
            };

            if (IncludeEventMetadata)
            {
                var ces = ce.GetAttributes();
                ces.Add(_subjectPropertyName, @event.Subject);

                if (@event.Action != null)
                    ces.Add(_actionPropertyName, @event.Action);

                if (@event.TenantId != null)
                    ces.Add(_tenantIdPropertyName, @event.TenantId);

                if (@event.Key != null)
                    ces.Add(_keyPropertyName, @event.Key);

                if (@event.ETag != null)
                    ces.Add(_etagPropertyName, @event.ETag);

                if (@event.Username != null)
                    ces.Add(_usernamePropertyName, @event.Username);

                if (@event.UserId != null)
                    ces.Add(_userIdPropertyName, @event.UserId);

                if (@event.CorrelationId != null)
                    ces.Add(_correlationIdPropertyName, @event.CorrelationId);

                if (@event.PartitionKey != null)
                    ces.Add(_partitionKeyPropertyName, @event.PartitionKey);
            }

            return Task.FromResult(new JsonEventFormatter().EncodeStructuredEvent(ce, out var _));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="bytes"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData?> DeserializeAsync(byte[] bytes)
        {
            var d = Deserialize(bytes);
            if (d.CloudEvent == null)
                return Task.FromResult<EventData?>(null);

            return Task.FromResult<EventData?>(new EventData(d.Metadata));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="valueType"><inheritdoc/></param>
        /// <param name="bytes"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData?> DeserializeAsync(Type valueType, byte[] bytes)
        {
            var d = Deserialize(bytes);
            if (d.CloudEvent?.Data == null)
                return Task.FromResult<EventData?>(null);

            using var reader = new StringReader((string)d.CloudEvent.Data);
            var val = _jsonSerializer.Deserialize(reader, valueType);

            var ed = (EventData)Activator.CreateInstance(NewtonsoftJsonEventDataSerializer.CreateValueEventDataType(valueType), new object[] { d.Metadata! });
            ed.SetValue(val);
            return Task.FromResult<EventData?>(ed);
        }

        /// <summary>
        /// Perform the common deserialization.
        /// </summary>
        private (CloudEvent? CloudEvent, EventMetadata? Metadata) Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return (null, null);

            var ce = new JsonEventFormatter().DecodeStructuredEvent(bytes);
            var em = new EventMetadata
            {
                Subject = ce.Type,
                Source = ce.Source,
                EventId = string.IsNullOrEmpty(ce.Id) ? null : (Guid.TryParse(ce.Id, out var eid) ? eid : (Guid?)null),
                Timestamp = ce.Time
            };

            if (IncludeEventMetadata)
            {
                var ces = ce.GetAttributes();
                em.Subject = ces.TryGetValue(_subjectPropertyName, out var subject) ? (string?)subject : null;
                em.Action = ces.TryGetValue(_actionPropertyName, out var action) ? (string?)action : null;
                em.TenantId = (ces.TryGetValue(_tenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : (Guid.TryParse((string?)tid, out var tid2) ? tid2 : (Guid?)null);
                em.CorrelationId = ces.TryGetValue(_correlationIdPropertyName, out var correlationId) ? (string?)correlationId : null;
                em.PartitionKey = ces.TryGetValue(_partitionKeyPropertyName, out var partitionKey) ? (string?)partitionKey : null;
                em.Key = ces.TryGetValue(_keyPropertyName, out var key) ? key : null;
                em.ETag = ces.TryGetValue(_etagPropertyName, out var etag) ? (string?)etag : null;
                em.Username = ces.TryGetValue(_usernamePropertyName, out var username) ? (string?)username : null;
                em.UserId = ces.TryGetValue(_userIdPropertyName, out var userId) ? (string?)userId : null;
            }

            return (ce, em);
        }
    }
}