// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Enables the serialization of an <see cref="EventData"/>, as the message content (data), into a corresponding <see cref="byte"/> array.
    /// </summary>
    public interface IEventDataContentSerializer
    {
        /// <summary>
        /// Serializes an <see cref="EventData"/> to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="byte"/> array.</returns>
        Task<byte[]> SerializeAsync(EventData @event);

        /// <summary>
        /// Deserializes a <see cref="byte"/> array into an <see cref="EventData"/>.
        /// </summary>
        /// <param name="bytes">The <see cref="byte"/> array.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        Task<EventData?> DeserializeAsync(byte[] bytes);

        /// <summary>
        /// Deserializes a <see cref="byte"/> array into an <see cref="EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="EventData{T}.Value"/> <see cref="System.Type"/>.</typeparam>
        /// <param name="bytes">The <see cref="byte"/> array.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        async Task<EventData<T>?> DeserializeAsync<T>(byte[] bytes) => (EventData<T>?)await DeserializeAsync(typeof(T), bytes).ConfigureAwait(false);

        /// <summary>
        /// Deserializes a <see cref="byte"/> array into an <see cref="EventData{T}"/> using the specified <paramref name="valueType"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="bytes">The <see cref="byte"/> array.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        Task<EventData?> DeserializeAsync(Type valueType, byte[] bytes);
    }
}