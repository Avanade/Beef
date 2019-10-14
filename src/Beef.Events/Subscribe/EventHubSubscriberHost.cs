// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="EventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubSubscriberHost : EventSubscriberHost
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EventHubSubscriberHost"/> using the specified <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <returns>The <see cref="EventHubSubscriberHost"/>.</returns>
        public static EventHubSubscriberHost Create(EventSubscriberHostArgs args) => new EventHubSubscriberHost(args);

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
        private EventHubSubscriberHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Performs the receive processing for an <see cref="EventHubs.EventData"/> instance.
        /// </summary>
        /// <param name="event">The <see cref="EventHubs.EventData"/> instance to receive/process.</param>
        public async Task ReceiveAsync(EventHubs.EventData @event)
        {
            if (@event == null)
                return;

            var (subject, action, _) = @event.GetBeefMetadata();

            await ReceiveAsync(subject, action, (subscriber) => subscriber.ValueType == null ? @event.ToBeefEventData() : @event.ToBeefEventData(subscriber.ValueType));
        }
    }
}