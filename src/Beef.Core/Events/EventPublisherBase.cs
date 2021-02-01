// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the base <b>Event</b> processing/publishing capability.
    /// </summary>
    /// <remarks>The key reason for queuing the published events it to promote a single atomic send operation; i.e. all events should be sent together, and either succeed or fail together.</remarks>
    public abstract class EventPublisherBase : IEventPublisher
    {
        private readonly Lazy<ConcurrentQueue<EventData>> _queue = new Lazy<ConcurrentQueue<EventData>>();

        /// <summary>
        /// Gets or sets the prefix for an <see cref="EventData.Subject"/> when creating an <see cref="EventData"/> or <see cref="EventData{T}"/>. <i>Note:</i> the <see cref="PathSeparator"/> will automatically be applied.
        /// </summary>
        public string? EventSubjectPrefix { get; set; }

        /// <summary>
        /// Gets or sets the path seperator <see cref="string"/>.
        /// </summary>
        public string PathSeparator { get; set; } = ".";

        /// <summary>
        /// Gets or sets the template wildcard <see cref="string"/>.
        /// </summary>
        public string TemplateWildcard { get; set; } = "*";

        /// <summary>
        /// Indicates whether the published (queued) events have been sent.
        /// </summary>
        public bool HasBeenSent { get; private set; }

        /// <summary>
        /// Gets the published/queued events.
        /// </summary>
        /// <returns>An <see cref="EventData"/> array.</returns>
        public EventData[] GetEvents()
        {
            var list = new List<EventData>();

            while (_queue.Value.TryDequeue(out var ed))
            {
                list.Add(ed);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Prepends the <see cref="IEventPublisher.EventSubjectPrefix"/> to the <paramref name="subject"/> where specified before creating the <see cref="EventData"/>.
        /// </summary>
        protected virtual string PrependPrefix(string subject) => string.IsNullOrEmpty(EventSubjectPrefix) ? subject : EventSubjectPrefix + PathSeparator + subject;

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public EventData CreateEvent(string subject, string? action = null)
            => EventData.CreateEvent(PrependPrefix(Check.NotEmpty(subject, nameof(subject))), action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public EventData CreateEvent(string subject, string? action = null, params IComparable?[] key)
            => EventData.CreateEvent(PrependPrefix(Check.NotEmpty(subject, nameof(subject))), action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventData.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null) where T : class
            => EventData.CreateValueEvent(value, PrependPrefix(Check.NotEmpty(subject, nameof(subject))), action); 

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null, params IComparable?[] key)
            => EventData.CreateValueEvent(value, PrependPrefix(Check.NotEmpty(subject, nameof(subject))), action, key);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventData.Key"/>).
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public IEventPublisher Publish(string subject, string? action = null) => Publish(new EventData[] { CreateEvent(subject, action) });

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public IEventPublisher Publish(string subject, string? action = null, params IComparable?[] key) => Publish(new EventData[] { CreateEvent(subject, action, key) });

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventData.Key"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public IEventPublisher PublishValue<T>(T value, string subject, string? action = null) where T : class => Publish(new EventData[] { CreateValueEvent(value, subject, action) });

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public IEventPublisher PublishValue<T>(T value, string subject, string? action = null, params IComparable?[] key) => Publish(new EventData[] { CreateValueEvent(value, subject, action, key) });

        /// <summary>
        /// Publishes (queues) one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public IEventPublisher Publish(params EventData[] events)
        {
            if (HasBeenSent)
                throw new InvalidOperationException("No further events can be published (queued) once they have been sent.");

            Check.IsFalse(events.Any(x => string.IsNullOrEmpty(x.Subject)), nameof(events), "EventData must have a Subject.");
            foreach (var ed in events)
            {
                _queue.Value.Enqueue(ed);
            }

            return this;
        }

        /// <summary>
        /// Sends all previously (queued) published events.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task SendAsync()
        {
            if (HasBeenSent)
                throw new InvalidOperationException("Published (queued) events can only be sent once.");

            await SendEventsAsync(GetEvents()).ConfigureAwait(false);
            HasBeenSent = true;
        }

        /// <summary>
        /// Sends none of more previously published (queued) events <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected abstract Task SendEventsAsync(params EventData[] events);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Reset()
        {
            _queue.Value.Clear();
            HasBeenSent = false;
        }
    }
}