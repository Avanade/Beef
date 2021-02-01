// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Represents a <c>null</c> event publisher; whereby the events are simply swallowed/discarded on send.
    /// </summary>
    public class NullEventPublisher : EventPublisherBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override Task SendEventsAsync(params EventData[] events) => Task.CompletedTask;
    }
}