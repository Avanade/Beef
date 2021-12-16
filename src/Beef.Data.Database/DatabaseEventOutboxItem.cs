// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the database <see cref="EventData"/> item for event outbox persistence (enqueue/dequeue).
    /// </summary>
    public class DatabaseEventOutboxItem
    {
        private JsonSerializer? _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxItem"/> class.
        /// </summary>
        public DatabaseEventOutboxItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxItem"/> class from an existing <see cref="Events.EventData"/>.
        /// </summary>
        /// <param name="event">The <see cref="Events.EventData"/>.</param>
        public DatabaseEventOutboxItem(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            _jsonSerializer ??= JsonSerializer.CreateDefault();
            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                _jsonSerializer.Serialize(writer, @event);
            }

            EventId = @event.EventId;
            Subject = @event.Subject;
            Action = @event.Action;
            CorrelationId = @event.CorrelationId;
            TenantId = @event.TenantId;
            PartitionKey = @event.PartitionKey;
            ValueType = @event.ValueType?.AssemblyQualifiedName;
            EventData = stream.ToArray();
        }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.EventId"/>.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.Subject"/>.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.Action"/>.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.CorrelationId"/>.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.TenantId"/>.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata.PartitionKey"/>.
        /// </summary>
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type.AssemblyQualifiedName"/> for the <see cref="EventData{T}.Value"/>; used to aid in the deserialization of the <see cref="EventData"/>.
        /// </summary>
        public string? ValueType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Events.EventData"/> as a JSON serialized <see cref="byte"/> array.
        /// </summary>
        public byte[]? EventData { get; set; }

        /// <summary>
        /// Creates an <see cref="Events.EventData"/> instance from the serialized <see cref="EventData"/> and <see cref="ValueType"/>.
        /// </summary>
        /// <returns>The corresponding <see cref="Events.EventData"/>.</returns>
        public EventData ToEventData()
        {
            if (EventData == null || EventData.Length == 0)
                throw new InvalidOperationException($"The {nameof(EventData)} property must have content to derserialize.");

            _jsonSerializer ??= JsonSerializer.CreateDefault();

            using var stream = new MemoryStream(EventData);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return (EventData)_jsonSerializer.Deserialize(reader, string.IsNullOrEmpty(ValueType) ? typeof(EventData) : typeof(EventData<>).MakeGenericType(Type.GetType(ValueType, true)))!;
        }
    }
}