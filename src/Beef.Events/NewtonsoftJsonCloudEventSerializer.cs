// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// Indicates whether to include the <see cref="EventMetadata"/> as <see cref="CloudEvent"/> <see cref="CloudEvent.SetAttributeFromString(string, string)">attributes</see>.
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

            var ce = new CloudEvent
            {
                Type = type,
                Source = @event.Source ?? DefaultSource,
                Id = @event.EventId?.ToString(),
                Time = @event.Timestamp == null ? (DateTimeOffset?)null : new DateTimeOffset(@event.Timestamp.Value, TimeSpan.Zero)
            };

            if (@event.HasValue)
            {
                ce.DataContentType = MediaTypeNames.Application.Json;
                ce.Data = @event.GetValue();
            };

            if (IncludeEventMetadata)
                SetMetadataAttributes(ce, @event);

            return Task.FromResult(new JsonEventFormatter().EncodeStructuredModeMessage(ce, out var _).ToArray());
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

            if (d.CloudEvent.DataContentType != MediaTypeNames.Application.Json)
                throw new InvalidOperationException($"CloudEvent DataContentType.MediaType is '{d.CloudEvent.DataContentType}'; it must be '{MediaTypeNames.Application.Json}' to use the '{nameof(NewtonsoftJsonCloudEventSerializer)}'.");

            var ed = (EventData)Activator.CreateInstance(NewtonsoftJsonEventDataSerializer.CreateValueEventDataType(valueType), new object[] { d.Metadata! });
            ed.SetValue(d.CloudEvent.Data is JToken json ? json.ToObject(valueType) 
                : (d.CloudEvent.Data is string str ? TypeDescriptor.GetConverter(valueType).ConvertFromInvariantString(str) : Convert.ChangeType(d.CloudEvent.Data, valueType)));

            return Task.FromResult<EventData?>(ed);
        }

        /// <summary>
        /// Perform the common deserialization.
        /// </summary>
        private (CloudEvent? CloudEvent, EventMetadata? Metadata) Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return (null, null);

            var rom = new ReadOnlyMemory<byte>(bytes);
            var ce = new JsonEventFormatter().DecodeStructuredModeMessage(rom, new ContentType(MediaTypeNames.Application.Json), null);

            var md = new EventMetadata();
            if (IncludeEventMetadata)
                GetMetadataAttributes(ce, md);

            if (string.IsNullOrEmpty(md.Subject))
                md.Subject = ce.Type;

            md.Source = ce.Source;
            md.EventId = string.IsNullOrEmpty(ce.Id) ? null : (Guid.TryParse(ce.Id, out var eid) ? eid : (Guid?)null);
            md.Timestamp = ce.Time?.UtcDateTime;

            return (ce, md);
        }

        /// <summary>
        /// Sets the metadata attributes based on configuration.
        /// </summary>
        private void SetMetadataAttributes(CloudEvent ce, EventMetadata? md)
        {
            if (md == null)
                return;

            SetMetadataAttribute(ce, nameof(EventMetadata.TenantId), md.TenantId?.ToString());
            SetMetadataAttribute(ce, nameof(EventMetadata.Subject), md.Subject);
            SetMetadataAttribute(ce, nameof(EventMetadata.Action), md.Action);
            SetMetadataAttribute(ce, nameof(EventMetadata.Key), md.Key?.ToString());
            SetMetadataAttribute(ce, nameof(EventMetadata.Username), md.Username);
            SetMetadataAttribute(ce, nameof(EventMetadata.UserId), md.UserId);
            SetMetadataAttribute(ce, nameof(EventMetadata.CorrelationId), md.CorrelationId);
            SetMetadataAttribute(ce, nameof(EventMetadata.ETag), md.ETag);
            SetMetadataAttribute(ce, nameof(EventMetadata.PartitionKey), md.PartitionKey);
        }

        /// <summary>
        /// Sets the metadata attribute based on configuration.
        /// </summary>
        private void SetMetadataAttribute<T>(CloudEvent ce, string name, T value)
        {
            if (Comparer<T>.Default.Compare(value, default(T)!) == 0)
                return;

            if (IncludeEventMetadataProperties == null || IncludeEventMetadataProperties.Length == 0 || IncludeEventMetadataProperties.Any(x => string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase)))
                ce[name.ToLowerInvariant()] = value;
        }

        /// <summary>
        /// Gets the configured metadata attributes.
        /// </summary>
        private void GetMetadataAttributes(CloudEvent ce, EventMetadata md)
        {
            var tenantId = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.TenantId));
            if (Guid.TryParse(tenantId, out var guid))
                md.TenantId = guid;

            md.Subject = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.Subject));
            md.Action = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.Action));
            md.Key = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.Key));
            md.Username = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.Username));
            md.UserId = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.UserId));
            md.CorrelationId = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.CorrelationId));
            md.ETag = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.ETag));
            md.PartitionKey = GetMetadataAttribute<string?>(ce, nameof(EventMetadata.PartitionKey));
        }

        /// <summary>
        /// Gets the configured metadata attribute.
        /// </summary>
        private static T GetMetadataAttribute<T>(CloudEvent ce, string name)
        {
            var val = ce[name.ToLowerInvariant()];
            if (val == null)
                return default!;
            else
                return (T)val;
        }
    }
}