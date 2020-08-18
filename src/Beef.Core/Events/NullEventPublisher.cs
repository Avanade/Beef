// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Represents a <c>null</c> event publisher; whereby the events are simply swallowed/discarded.
    /// </summary>
    public class NullEventPublisher : EventPublisherBase
    {
        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected override Task PublishEventsAsync(params EventData[] events) => Task.CompletedTask;
    }
}