// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides a basic <see cref="EventData"/> <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventDataSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDataSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        public EventDataSubscriberHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets or sets the <see cref="EventDataSubscriberHostInvoker"/>. Defaults to <see cref="EventDataSubscriberHostInvoker"/>.
        /// </summary>
        public EventDataSubscriberHostInvoker? Invoker { get; set; }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public EventDataSubscriberHost UseLogger(ILogger logger)
        {
            Logger = logger;
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

            if (events.Length != 1 && !Args.AreMultipleMessagesSupported)
                throw new EventSubscriberException($"The '{nameof(EventDataSubscriberHost)}' does not AllowMultipleMessages; there were {events.Length} event messages.");

            if (events.Any(x => string.IsNullOrEmpty(x.Subject)))
                throw new EventSubscriberException($"The '{nameof(EventDataSubscriberHost)}' does not allow event messages where the 'Subject' is not specified.");

            foreach (var @event in events)
            {
                await (Invoker ??= new EventDataSubscriberHostInvoker()).InvokeAsync(this, async () =>
                {
                    await ReceiveAsync(@event, @event.Subject, @event.Action, (_) => @event).ConfigureAwait(false);
                }, @event).ConfigureAwait(false);
            }
        }
    }
}