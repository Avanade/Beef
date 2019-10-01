// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides a basic <see cref="EventData"/> <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventDataSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventSubscriberHostArgs"/>.</param>
        public EventDataSubscriberHost(EventSubscriberHostArgs args = null) : base(args) { }

        /// <summary>
        /// Indicates that multiple messages (<see cref="EventData"/>) can be processed.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHost"/> instance (to support fluent-style method chaining).</returns>
        public EventDataSubscriberHost AllowMultipleMessages()
        {
            AreMultipleMessagesSupported = true;
            return this;
        }

        /// <summary>
        /// Performs the receive processing for one or more <see cref="EventData"/> instances.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> instances to receive/process.</param>
        public async Task ReceiveAsync(params EventData[] events)
        {
            if (events == null || events.Length == 0)
                return;

            if (events.Length != 1 && !AreMultipleMessagesSupported)
                throw new EventSubscriberException($"The {nameof(EventDataSubscriberHost)} does not AllowMultipleMessages; the collection contains '{events.Length}' events.");

            foreach (var @event in events)
            {
                await ReceiveAsync(@event.Subject, @event.Action, () => @event);
            }
        }
    }
}