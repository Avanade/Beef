// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Represents an <see cref="EventData"/> to <see cref="EventData"/> pass-through converter.
    /// </summary>
    public class EventDataConverter : IEventDataConverter<EventData>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData> ConvertFromAsync(EventData @event) => Task.FromResult(@event);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <param name="valueType"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData> ConvertFromAsync(Type valueType, EventData @event) => Task.FromResult(@event);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventData> ConvertToAsync(EventData @event) => Task.FromResult(@event);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task<EventMetadata?> GetMetadataAsync(EventData @event) => Task.FromResult<EventMetadata?>(@event);
    }
}