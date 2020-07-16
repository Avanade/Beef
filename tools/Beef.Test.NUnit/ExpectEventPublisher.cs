// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
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
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetEvents(string? correlationId = null) =>
            _eventDict.TryGetValue(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var events) ? events : new List<EventData>();

        /// <summary>
        /// Removes the events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        public static void Remove(string? correlationId = null) =>
            _eventDict.TryRemove(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var _);

        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected override Task PublishEventsAsync(params EventData[] events)
        {
            var list = _eventDict.GetOrAdd(ExecutionContext.Current.CorrelationId ?? throw new InvalidOperationException("The ExecutionContext.CorrelationId must be set for the ExpectEventPublisher to function conrrectly."), new List<EventData>());
            list.AddRange(events);
            return Task.CompletedTask;
        }
    }
}