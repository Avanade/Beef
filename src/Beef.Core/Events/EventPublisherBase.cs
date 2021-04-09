// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
        private readonly Lazy<ConcurrentQueue<EventData>> _queue = new();

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
        /// Gets or sets the <see cref="EventData.Subject"/> format.
        /// </summary>
        public EventStringFormat SubjectFormat { get; set; } = EventStringFormat.None;

        /// <summary>
        /// Gets or sets the <see cref="EventData.Action"/> format.
        /// </summary>
        public EventStringFormat ActionFormat { get; set; } = EventStringFormat.None;

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
        /// Publishes (queues) one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>This method will also perform the appropriate <see cref="SubjectFormat"/> and <see cref="ActionFormat"/> on the <paramref name="events"/>, as well as applying the <see cref="EventSubjectPrefix"/>.</remarks>
        public virtual IEventPublisher Publish(params EventData[] events)
        {
            Check.IsFalse(events.Any(x => string.IsNullOrEmpty(x.Subject)), nameof(events), "EventData must have a Subject.");
            foreach (var ed in events)
            {
                ed.Subject = Format(PrependPrefix(Check.NotEmpty(ed.Subject, nameof(EventData.Subject))), SubjectFormat);
                ed.Action = Format(ed.Action, ActionFormat);
                _queue.Value.Enqueue(ed);
            }

            return this;
        }

        /// <summary>
        /// Prepends the <see cref="IEventPublisher.EventSubjectPrefix"/> to the <paramref name="subject"/> where specified before creating the <see cref="EventData"/>.
        /// </summary>
        protected virtual string PrependPrefix(string subject) => string.IsNullOrEmpty(EventSubjectPrefix) ? subject : EventSubjectPrefix + PathSeparator + subject;

        /// <summary>
        /// Format the string.
        /// </summary>
        private static string? Format(string? text, EventStringFormat? format) => string.IsNullOrEmpty(text) ? text : format switch
        {
            EventStringFormat.Uppercase => text.ToUpperInvariant()!,
            EventStringFormat.Lowercase => text.ToLowerInvariant()!,
            _ => text
        };

        /// <summary>
        /// Sends all previously (queued) published events.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Once sent also automatically invokes the <see cref="Reset"/>.</remarks>
        public async Task SendAsync()
        {
            await SendEventsAsync(GetEvents()).ConfigureAwait(false);
            Reset();
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
        public void Reset() => _queue.Value.Clear();
    }
}