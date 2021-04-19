// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides a <see cref="CloudEvent"/> <see cref="IEventDataContentSerializer"/> using the <b>Newtonsoft</b> <see cref="JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftJsonCloudEventSerializer : IEventDataContentSerializer
    {
        private const string _beefJsonName = "beef";

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata"/> JSON extension object name. Defaults to `beef`;
        /// </summary>
        public string EventMetadataExtensionName { get; set; } = _beefJsonName;

        /// <summary>
        /// Indicates whether to include the <see cref="EventMetadata"/> as <see cref="CloudEvent"/> <see cref="CloudEvent.GetAttributes">attributes</see>.
        /// </summary>
        /// <remarks>Defaults to <c>true</c>.</remarks>
        public bool IncludeEventMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of <see cref="EventMetadata"/> property names that are serialized/deserialized when <see cref="IncludeEventMetadata"/> is <c>true</c>.
        /// </summary>
        /// <remarks>Defaults to: <see cref="EventMetadata.Subject"/>, <see cref="EventMetadata.Action"/>, <see cref="EventMetadata.TenantId"/>, <see cref="EventMetadata.UserId"/>, <see cref="EventMetadata.Username"/> 
        /// and <see cref="EventMetadata.CorrelationId"/>. An empty array indicates that all are included.</remarks>
        public string[] IncludeEventMetadataProperties { get; set; } = 
            new string[] { nameof(EventMetadata.Subject), nameof(EventMetadata.Action), nameof(EventMetadata.TenantId), nameof(EventMetadata.UserId), nameof(EventMetadata.Username), nameof(EventMetadata.CorrelationId) };

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
                ce.DataContentType = new ContentType(MediaTypeNames.Application.Json);
                ce.Data = @event.GetValue();
            };

            if (IncludeEventMetadata)
            {
                // Add metadata (remove values which are already part of the payload).
                var md = @event.CopyMetadata();
                SetOrNullMetadata(md);

                // Add metadata as an attribute.
                var ces = ce.GetAttributes();
                ces.Add(EventMetadataExtensionName ??= _beefJsonName, md);
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

            if (d.CloudEvent.DataContentType.MediaType != MediaTypeNames.Application.Json)
                throw new InvalidOperationException($"CloudEvent DataContentType.MediaType is '{d.CloudEvent.DataContentType.Name}', it must be '{MediaTypeNames.Application.Json}' to use the '{nameof(NewtonsoftJsonCloudEventSerializer)}'.");

            var ed = (EventData)Activator.CreateInstance(NewtonsoftJsonEventDataSerializer.CreateValueEventDataType(valueType), new object[] { d.Metadata! });
            ed.SetValue(d.CloudEvent.Data is JToken json ? json.ToObject(valueType) : Convert.ChangeType(d.CloudEvent.Data, valueType));
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

            EventMetadata? md = null;
            if (IncludeEventMetadata && ce.GetAttributes().TryGetValue(EventMetadataExtensionName ??= _beefJsonName, out object val) && val != null && val is JToken json)
            {
                md = json.ToObject<EventMetadata>();
                SetOrNullMetadata(md);
            }

            if (md == null)
                md = new EventMetadata();

            if (string.IsNullOrEmpty(md.Subject))
                md.Subject = ce.Type;

            md.Source = ce.Source;
            md.EventId = string.IsNullOrEmpty(ce.Id) ? null : (Guid.TryParse(ce.Id, out var eid) ? eid : (Guid?)null);
            md.Timestamp = ce.Time;

            return (ce, md);
        }

        /// <summary>
        /// Sets or nulls the metadata based on the name inclusion.
        /// </summary>
        private void SetOrNullMetadata(EventMetadata? md)
        {
            if (md == null)
                return;

            md.EventId = SetOrNullValue(nameof(EventMetadata.EventId), md.EventId);
            md.TenantId = SetOrNullValue(nameof(EventMetadata.TenantId), md.TenantId);
            md.Subject = SetOrNullValue(nameof(EventMetadata.Subject), md.Subject);
            md.Action = SetOrNullValue(nameof(EventMetadata.Action), md.Action);
            md.Source = SetOrNullValue(nameof(EventMetadata.Source), md.Source);
            md.Key = SetOrNullValue(nameof(EventMetadata.Key), md.Key);
            md.Username = SetOrNullValue(nameof(EventMetadata.Username), md.Username);
            md.UserId = SetOrNullValue(nameof(EventMetadata.UserId), md.UserId);
            md.Timestamp = SetOrNullValue(nameof(EventMetadata.Timestamp), md.Timestamp);
            md.CorrelationId = SetOrNullValue(nameof(EventMetadata.CorrelationId), md.CorrelationId);
            md.ETag = SetOrNullValue(nameof(EventMetadata.ETag), md.ETag);
            md.PartitionKey = SetOrNullValue(nameof(EventMetadata.PartitionKey), md.PartitionKey);
        }

        /// <summary>
        /// Set the value of null depending on configuration.
        /// </summary>
        private T SetOrNullValue<T>(string name, T value) => 
            IncludeEventMetadataProperties == null || IncludeEventMetadataProperties.Length == 0 || IncludeEventMetadataProperties.Any(x => string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase)) ? value : default!;
    }
}