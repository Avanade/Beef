// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Linq;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the base <b>Event</b> processing/publishing capability.
    /// </summary>
    public abstract class EventPublisherBase : IEventPublisher
    {
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
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task PublishAsync(params EventData[] events)
        {
            Check.IsFalse(events.Any(x => string.IsNullOrEmpty(x.Subject)), nameof(events), "EventData must have a Subject.");
            return PublishEventsAsync(events);
        }

        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected abstract Task PublishEventsAsync(params EventData[] events);
    }
}