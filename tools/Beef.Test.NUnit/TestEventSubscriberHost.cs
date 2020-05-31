// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.Events.Subscribe;
using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides a <see cref="EventSubscriberHost"/> for testing <see cref="EventSubscriberBase">Subscribers</see>.
    /// </summary>
    internal class TestEventSubscriberHost : EventSubscriberHost
    {
        private readonly ExecutionContext _ec;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestEventSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="TestEventSubscriberHost"/>.</param>
        internal TestEventSubscriberHost(EventSubscriberHostArgs args) : base(args) => _ec = ExecutionContext.HasCurrent ? ExecutionContext.Current : throw new InvalidOperationException("ExecutionContext.Current must have a value.");

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
        /// Use the existing <see cref="ExecutionContext"/>; i.e. that which was set up prior to <see cref="ReceiveAsync(EventData)"/>.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="event">The event.</param>
        /// <returns>The existing <see cref="ExecutionContext"/>.</returns>
        protected override ExecutionContext CreateExecutionContext(IEventSubscriber subscriber, EventData @event) => _ec;

        /// <summary>
        /// Indicates whether a receive occured and an underlying subsriber was executed.
        /// </summary>
        public bool WasSubscribed { get; private set; }

        /// <summary>
        /// Gets the 
        /// </summary>
        public Result? Result { get; private set; }
    }
}