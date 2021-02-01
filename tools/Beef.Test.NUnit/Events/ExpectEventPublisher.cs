// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Events
{
    /// <summary>
    /// Represents an <see cref="IEventPublisher"/> to record the published <see cref="Events"/>.
    /// </summary>
    public class ExpectEventPublisher : EventPublisherBase
    {
        private static readonly ConcurrentDictionary<string, List<EventData>> _eventDict = new ConcurrentDictionary<string, List<EventData>>();

        /// <summary>
        /// Gets the events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        /// <param name="removeEvents">Indicates whether to also <see cref="Remove"/> the events.</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetEvents(string? correlationId = null, bool removeEvents = false)
        {
            var list = _eventDict.TryGetValue(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var events) ? events : new List<EventData>();
            if (removeEvents)
                Remove(correlationId);

            return list;
        }

        /// <summary>
        /// Removes the events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        public static void Remove(string? correlationId = null) =>
            _eventDict.TryRemove(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var _);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override Task SendEventsAsync(params EventData[] events)
        {
            if (ExecutionContext.HasCurrent && ExecutionContext.Current.CorrelationId != null)
            {
                var list = _eventDict.GetOrAdd(ExecutionContext.Current.CorrelationId, new List<EventData>());
                list.AddRange(events);
            }

            return Task.CompletedTask;
        }
    }
}