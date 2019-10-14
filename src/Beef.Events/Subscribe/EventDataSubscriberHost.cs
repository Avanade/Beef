// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides a basic <see cref="EventData"/> <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventDataSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EventDataSubscriberHost"/> using the specified <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventSubscriberHostArgs"/>.</param>
        /// <returns>The <see cref="EventDataSubscriberHost"/>.</returns>
        public static EventDataSubscriberHost Create(EventSubscriberHostArgs args = null) => new EventDataSubscriberHost(args);

        /// <summary>
        /// Creates a new instance of the <see cref="EventDataSubscriberHost"/> using the specified <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/>.</returns>
        public static EventDataSubscriberHost Create(ILogger logger) => new EventDataSubscriberHost(new EventSubscriberHostArgs(logger, Assembly.GetCallingAssembly()));

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDataSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        private EventDataSubscriberHost(EventSubscriberHostArgs args) : base(args) { }

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
                throw new EventSubscriberException($"The {nameof(EventDataSubscriberHost)} does not AllowMultipleMessages; there were {events.Length} event messages.");

            foreach (var @event in events)
            {
                await ReceiveAsync(@event.Subject, @event.Action, (_) => @event);
            }
        }
    }
}