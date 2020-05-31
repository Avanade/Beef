// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="EventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubSubscriberHost : EventSubscriberHost
    {
        private Func<IEventSubscriber, EventData, ExecutionContext>? _createFunc;

        /// <summary>
        /// Creates a new instance of the <see cref="EventHubSubscriberHost"/> using the specified <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/>.</returns>
        public static EventHubSubscriberHost Create(EventSubscriberHostArgs args) => new EventHubSubscriberHost(Check.NotNull(args, nameof(args)));

        /// <summary>
        /// Creates a new instance of the <see cref="EventHubSubscriberHost"/> using the specified <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/>.</returns>
        public static EventHubSubscriberHost Create(ILogger logger) => new EventHubSubscriberHost(new EventSubscriberHostArgs(logger, Assembly.GetCallingAssembly()));

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubSubscriberHost"/>.</param>
        private EventHubSubscriberHost(EventSubscriberHostArgs args) : base(args) 
        {
            if (args.AuditWriter == null)
                args.AuditWriter = (result) => throw new EventSubscriberStopException(result);
        }

        /// <summary>
        /// Sets the <see cref="EventSubscriberHost.InvalidEventDataHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (to support fluent-style method chaining).</returns>
        public EventHubSubscriberHost InvalidEventData(ResultHandling handling)
        {
            InvalidEventDataHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="EventSubscriberHost.NotSubscribedHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (to support fluent-style method chaining).</returns>
        public EventHubSubscriberHost NotSubscribed(ResultHandling handling)
        {
            NotSubscribedHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="EventSubscriberHost.DataNotFoundHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (to support fluent-style method chaining).</returns>
        public EventHubSubscriberHost DataNotFound(ResultHandling handling)
        {
            DataNotFoundHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="EventSubscriberHost.InvalidDataHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance (to support fluent-style method chaining).</returns>
        public EventHubSubscriberHost InvalidData(ResultHandling handling)
        {
            InvalidDataHandling = handling;
            return this;
        }

        /// <summary>
        /// Provides a means to override the creation of the <see cref="Beef.ExecutionContext"/>.
        /// </summary>
        /// <param name="createFunc">The function to create the <see cref="Beef.ExecutionContext"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/> instance to support fluent-style method chaining.</returns>
        public EventHubSubscriberHost ExecutionContext(Func<IEventSubscriber, EventData, ExecutionContext> createFunc)
        {
            _createFunc = createFunc;
            return this;
        }

        /// <summary>
        /// Overrides the <see cref="EventSubscriberHost.CreateExecutionContext(IEventSubscriber, EventData)"/>.
        /// </summary>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="ExecutionContext"/>.</returns>
        protected override ExecutionContext CreateExecutionContext(IEventSubscriber subscriber, EventData @event)
        {
            if (_createFunc == null)
                return base.CreateExecutionContext(subscriber, @event);

            return _createFunc(subscriber, @event);
        }

        /// <summary>
        /// Performs the receive processing for an <see cref="EventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="EventHubs.EventData"/> instance to receive/process.</param>
        public async Task ReceiveAsync(EventHubs.EventData @event)
        {
            if (@event == null)
                return;

            // Convert EventHubs.EventData to Beef.EventData..
            var (subject, action, _) = @event.GetBeefMetadata();
            await ReceiveAsync(subject, action, (subscriber) =>
            {
                try
                {
                    return subscriber.ValueType == null ? @event.ToBeefEventData() : @event.ToBeefEventData(subscriber.ValueType);
                }
#pragma warning disable CA1031 // Do not catch general exception types; by design, need this to be a catch-all.
                catch (Exception ex) { throw new InvalidEventDataException(ex); }
#pragma warning restore CA1031
            }).ConfigureAwait(false);
        }
    }
}