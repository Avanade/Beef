// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides an <see cref="EventData"/> <see cref="IEventDataContentSerializer"/> using the <b>Newtonsoft</b> <see cref="JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftJsonEventDataSerializer : IEventDataContentSerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonEventDataSerializer"/> class using a default <see cref="JsonSerializer"/>.
        /// </summary>
        public NewtonsoftJsonEventDataSerializer() => _jsonSerializer = JsonSerializer.CreateDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonEventDataSerializer"/> class using the specified <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer"/>.</param>
        public NewtonsoftJsonEventDataSerializer(JsonSerializer jsonSerializer) => _jsonSerializer = Check.NotNull(jsonSerializer, nameof(jsonSerializer));

        /// <summary>
        /// Indicates whether the <see cref="EventData.GetValue"/> is serialized only; or alternatively, the complete <see cref="EventData"/> (default).
        /// </summary>
        public bool SerializeValueOnly { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<byte[]> SerializeAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (SerializeValueOnly && !@event.HasValue)
                return Task.FromResult(Array.Empty<byte>());

            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                if (SerializeValueOnly)
                    _jsonSerializer.Serialize(writer, @event.GetValue());
                else
                    _jsonSerializer.Serialize(writer, @event);
            }

            return Task.FromResult(stream.ToArray());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="bytes"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData?> DeserializeAsync(byte[] bytes)
        {
            if (SerializeValueOnly)
                return Task.FromResult<EventData?>(new EventData(null));

            if (bytes == null || bytes.Length == 0)
                return Task.FromResult<EventData?>(null);

            using var stream = new MemoryStream(bytes);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return Task.FromResult((EventData?)_jsonSerializer.Deserialize(reader, typeof(EventData)));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="valueType"><inheritdoc/></param>
        /// <param name="bytes"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData?> DeserializeAsync(Type valueType, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Task.FromResult<EventData?>(null);

            using var stream = new MemoryStream(bytes);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            if (SerializeValueOnly)
            {
                var val = _jsonSerializer.Deserialize(reader, valueType);
                var ed = (EventData)Activator.CreateInstance(CreateValueEventDataType(valueType), new object[] { (EventMetadata)null! });
                if (val != null)
                    ed.SetValue(val);

                return Task.FromResult<EventData?>(ed);
            }
            
            return Task.FromResult((EventData?)_jsonSerializer.Deserialize(reader, CreateValueEventDataType(valueType)));
        }

        /// <summary>
        /// Create the <see cref="EventData{T}"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{Type}.Value"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="Type"/>.</returns>
        internal static Type CreateValueEventDataType(Type valueType) => typeof(EventData<>).MakeGenericType(Check.NotNull(valueType, nameof(valueType)));
    }
}