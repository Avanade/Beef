// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.Events.Subscribe;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides a <see cref="EventSubscriberHost"/> for testing <see cref="EventSubscriberBase">Subscribers</see>.
    /// </summary>
    public class EventSubscriberTestHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTestHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberTestHost"/>.</param>
        public EventSubscriberTestHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Performs the receive processing for <see cref="EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/> instance to receive/process.</param>
        public async Task ReceiveAsync(EventData @event)
        {
            if (@event == null)
                return;

            try
            {
                Result = await ReceiveAsync(@event.Subject, @event.Action, (subscriber) => { WasSubscribed = true; return @event; }).ConfigureAwait(false);
            }
            catch (EventSubscriberStopException essex)
            {
                Result = essex.Result;
            }
        }

        /// <summary>
        /// Indicates whether a receive occured and an underlying subsriber was executed.
        /// </summary>
        public bool WasSubscribed { get; private set; }

        /// <summary>
        /// Gets the corresponding <see cref="Result"/>.
        /// </summary>
        public Result? Result { get; private set; }
    }
}