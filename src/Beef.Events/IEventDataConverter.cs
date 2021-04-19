// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Enables the conversion of an <see cref="EventData"/> into a specific messaging <see cref="System.Type"/>.
    /// </summary>
    /// <typeparam name="T">The specific messaging <see cref="System.Type"/>.</typeparam>
    public interface IEventDataConverter<T> where T : class
    {
        /// <summary>
        /// Converts an <see cref="EventData"/> into a <see cref="System.Type"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The converted <see cref="System.Type"/> of <typeparamref name="T"/>.</returns>
        Task<T> ConvertToAsync(EventData @event);

        /// <summary>
        /// Converts an <paramref name="event"/> of <see cref="System.Type"/> <typeparamref name="T"/> into an <see cref="EventData"/>.
        /// </summary>
        /// <param name="event">The event of <see cref="System.Type"/> <typeparamref name="T"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        Task<EventData> ConvertFromAsync(T @event);

        /// <summary>
        /// Converts an <paramref name="event"/> of <see cref="System.Type"/> <typeparamref name="T"/> into an <see cref="EventData{TEventData}"/>.
        /// </summary>
        /// <typeparam name="TEventData">The <see cref="EventData{TEventData}.Value"/> <see cref="Type"/>.</typeparam>
        /// <param name="event">The event of <see cref="System.Type"/> <typeparamref name="T"/>.</param>
        /// <returns>The converted <see cref="EventData{TEventData}"/>.</returns>
        async Task<EventData<TEventData>> ConvertFromAsync<TEventData>(T @event) => (EventData<TEventData>) await ConvertFromAsync(typeof(TEventData), @event).ConfigureAwait(false);

        /// <summary>
        /// Converts an <paramref name="event"/> of <see cref="System.Type"/> <paramref name="valueType"/> into an <see cref="EventData{TEventData}"/>.
        /// </summary>
        /// <param name="valueType">The <see cref="EventData{T}.Value"/> <see cref="Type"/>.</param>
        /// <param name="event">The event of <see cref="System.Type"/> <paramref name="valueType"/>.</param>
        /// <returns>The converted <see cref="EventData"/>.</returns>
        Task<EventData> ConvertFromAsync(Type valueType, T @event);

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="System.Type"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="event">The event of <see cref="System.Type"/> <typeparamref name="T"/>.</param>
        /// <returns>The corresponding <see cref="EventMetadata"/>.</returns>
        Task<EventMetadata?> GetMetadataAsync(T @event);
    }
}