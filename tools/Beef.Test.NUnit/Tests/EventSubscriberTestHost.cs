// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides a <see cref="EventSubscriberHost"/> for testing <see cref="EventSubscriberBase">Subscribers</see>.
    /// </summary>
    [DebuggerStepThrough()]
    public class EventSubscriberTestHost : EventSubscriberHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTestHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberTestHost"/>.</param>
        public EventSubscriberTestHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberTestHost"/> instance (for fluent-style method chaining).</returns>
        public EventSubscriberTestHost UseLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Performs the receive processing for <see cref="EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/> instance to receive/process.</param>
        public async Task ReceiveAsync(EventData @event)
        {
            if (@event == null)
                return;

            var x = new EventDataSubscriberData(@event);

            try
            {
                Result = await ReceiveAsync(new EventDataSubscriberData(@event), (subscriber) => { WasSubscribed = true; return @event; }).ConfigureAwait(false);
            }
            catch (EventSubscriberUnhandledException essex)
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
        public Result? Result { get; internal set; }
    }
}