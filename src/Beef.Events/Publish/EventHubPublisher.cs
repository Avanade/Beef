// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System;
using System.Threading.Tasks;
using Beef.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Beef.Events.Publish
{
    /// <summary>
    /// <see cref="Publish(EventData[])">Publishes</see> (sends) the <see cref="EventData"/> array (converted to <see cref="EventHubs.EventData"/>) using the same partition key (see <see cref="GetPartitionKey(EventData[])"/>.
    /// </summary>
    public class EventHubPublisher
    {
        private readonly EventHubs.EventHubClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubPublisher"/> using the specified <see cref="EventHubs.EventHubClient"/> (consider setting the underlying
        /// <see cref="EventHubs.ClientEntity.RetryPolicy"/>) to allow for transient errors).
        /// </summary>
        /// <param name="client">The <see cref="EventHubs.EventHubClient"/>.</param>
        public EventHubPublisher(EventHubs.EventHubClient client) => _client = Check.NotNull(client, nameof(client));

        /// <summary>
        /// Publishes the <paramref name="events"/>.
        /// </summary>
        /// <param name="events">The <see cref="EventData"/> instances.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        public async Task Publish(EventData[] events)
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
                await EventHubPublisherInvoker.Default.InvokeAsync(this, async () => await _client.SendAsync(eventHubEvents, partitionKey));
            }
#pragma warning disable CA1031 // Do not catch general exception types; by-design, is a catch all.
            catch (Exception ex)
#pragma warning restore CA1031 
            {
                OnException(events, partitionKey, ex);
            }
        }

        /// <summary>
        /// Gets the partition key (will use <see cref="ExecutionContext.TenantId"/> by default where set; otherwise, will use the JSON-serialized <see cref="EventData.Key"/> from the first event).
        /// </summary>
        /// <param name="events">The events that will be published.</param>
        /// <returns>The partition key where determined; otherwise, <c>null</c> for round-robin allocation.</returns>
        protected virtual string GetPartitionKey(EventData[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (ExecutionContext.HasCurrent && ExecutionContext.Current.TenantId.HasValue)
                return ExecutionContext.Current.TenantId.Value.ToString();

            return events.Length == 0 || events[0].Key == null ? null : JsonConvert.SerializeObject(events[0].Key);
        }

        /// <summary>
        /// Invoked when the underlying <see cref="EventHubs.EventHubClient.SendAsync(EventHubs.EventData, string)"/> results in an unhandled <see cref="Exception"/>;
        /// by default logs (<see cref="Logger.Exception(Exception)"/>) the <paramref name="exception"/> (in this case the events will <b>not</b> be published; i.e. they will be lost).
        /// </summary>
        /// <param name="events">The events that will be published.</param>
        /// <param name="partitionKey">The partition key (<see cref="GetPartitionKey(EventData[])"/>).</param>
        /// <param name="exception">The unhandled <see cref="Exception"/>.</param>
        protected virtual void OnException(EventData[] events, string partitionKey, Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var subs = string.Join(", ", events.Select(x => x.Subject).Distinct().ToArray());
            Logger.Default.Exception(exception, $"EventHub Publish for Subject(s) '{subs}' failed: {exception.Message}");
        }
    }
}