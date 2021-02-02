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
    public sealed class ExpectEventPublisher : EventPublisherBase
    {
        private static readonly ConcurrentDictionary<string, List<EventData>> _sentEventDict = new ConcurrentDictionary<string, List<EventData>>();
        private static readonly ConcurrentDictionary<string, List<EventData>> _publishedEventDict = new ConcurrentDictionary<string, List<EventData>>();

        /// <summary>
        /// Gets the sent events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetSentEvents(string? correlationId = null)
            => _sentEventDict.TryGetValue(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var events) ? events : new List<EventData>();

        /// <summary>
        /// Gets the published events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetPublishedEvents(string? correlationId = null)
            => _publishedEventDict.TryGetValue(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var events) ? events : new List<EventData>();

        /// <summary>
        /// Removes the events for the specified <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        public static void Remove(string? correlationId = null)
        {
            _sentEventDict.TryRemove(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var _);
            _publishedEventDict.TryRemove(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var _);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override IEventPublisher Publish(params EventData[] events)
        {
            if (ExecutionContext.HasCurrent && ExecutionContext.Current.CorrelationId != null)
            {
                var list = _publishedEventDict.GetOrAdd(ExecutionContext.Current.CorrelationId, new List<EventData>());
                list.AddRange(events);
            }

            return base.Publish(events);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override Task SendEventsAsync(params EventData[] events)
        {
            if (ExecutionContext.HasCurrent && ExecutionContext.Current.CorrelationId != null)
            {
                var list = _sentEventDict.GetOrAdd(ExecutionContext.Current.CorrelationId, new List<EventData>());
                list.AddRange(events);
            }

            return Task.CompletedTask;
        }
    }
}