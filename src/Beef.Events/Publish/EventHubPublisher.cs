// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System;
using System.Threading.Tasks;
using Beef.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Beef.Events.Publish
{
    /// <summary>
    /// <see cref="SendEventsAsync(EventData[])">Send</see> the <see cref="EventData"/> array (converted to <see cref="EventHubs.EventData"/>) using the same partition key (see <see cref="GetPartitionKey(EventData[])"/>.
    /// </summary>
    public class EventHubPublisher : EventPublisherBase
    {
        private readonly EventHubs.EventHubClient _client;
        private readonly EventHubPublisherInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubPublisher"/> using the specified <see cref="EventHubs.EventHubClient"/> (consider setting the underlying
        /// <see cref="EventHubs.ClientEntity.RetryPolicy"/>) to allow for transient errors).
        /// </summary>
        /// <param name="client">The <see cref="EventHubs.EventHubClient"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="EventHubPublisherInvoker"/>.</param>
        public EventHubPublisher(EventHubs.EventHubClient client, EventHubPublisherInvoker? invoker = null)
        {
            _client = Check.NotNull(client, nameof(client));
            _invoker = invoker ?? new EventHubPublisherInvoker();
        }

        /// <summary>
        /// Indicates whether any <see cref="Exception"/> thrown during the publish should be swallowed (e.g. log and continue processing). Defaults to <c>false</c>.
        /// </summary>
        public bool SwallowException { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override async Task SendEventsAsync(params EventData[] events)
        {
            if (events == null || events.Length == 0)
                return;

            var partitionKey = GetPartitionKey(events);
            var eventHubEvents = new EventHubs.EventData[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                eventHubEvents[i] = events[i].ToEventHubsEventData();
            }

            try
            {
                await _invoker.InvokeAsync(this, async () => await _client.SendAsync(eventHubEvents, partitionKey).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnException(events, partitionKey, ex);
                if (!SwallowException)
                    throw;
            }
        }

        /// <summary>
        /// Gets the partition key (will use <see cref="ExecutionContext.TenantId"/> by default where set; otherwise, will use the JSON-serialized <see cref="EventData.Key"/> from the first event).
        /// </summary>
        /// <param name="events">The events that will be published.</param>
        /// <returns>The partition key where determined; otherwise, <c>null</c> for round-robin allocation.</returns>
        protected virtual string? GetPartitionKey(EventData[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (ExecutionContext.HasCurrent && ExecutionContext.Current.TenantId.HasValue)
                return ExecutionContext.Current.TenantId.Value.ToString();

            return events.Length == 0 || events[0].Key == null ? null : JsonConvert.SerializeObject(events[0].Key);
        }

        /// <summary>
        /// Invoked when the underlying <see cref="EventHubs.EventHubClient.SendAsync(EventHubs.EventData, string)"/> results in an unhandled <see cref="Exception"/>;
        /// by default logs the <paramref name="exception"/> (in this case the events will <b>not</b> be published; i.e. they will be lost).
        /// </summary>
        /// <param name="events">The events that will be published.</param>
        /// <param name="partitionKey">The partition key (<see cref="GetPartitionKey(EventData[])"/>).</param>
        /// <param name="exception">The unhandled <see cref="Exception"/>.</param>
        protected virtual void OnException(EventData[] events, string? partitionKey, Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var subs = string.Join(", ", events.Select(x => x.Subject).Distinct().ToArray());
            Logger.Create<EventHubPublisher>().LogError(exception, $"EventHub Publish for Subject(s) '{subs}' failed: {exception.Message}");
        }
    }
}