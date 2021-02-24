// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureEventHubs = Azure.Messaging.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// <see cref="SendEventsAsync(EventData[])">Send</see> the <see cref="EventData"/> array (converted to <see cref="AzureEventHubs.EventData"/>) in multiple batches based on <see cref="EventData.PartitionKey"/>.
    /// </summary>
    public class EventHubProducer : EventPublisherBase
    {
        private readonly EventHubProducerClient _client;
        private readonly EventHubProducerInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubProducer"/> using the specified <see cref="EventHubProducerClient"/> (consider setting the underlying
        /// <see cref="EventHubProducerClientOptions.RetryOptions"/>) to allow for transient errors).
        /// </summary>
        /// <param name="client">The <see cref="EventHubProducerClient"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="EventHubProducerInvoker"/>.</param>
        public EventHubProducer(EventHubProducerClient client, EventHubProducerInvoker? invoker = null)
        {
            _client = Check.NotNull(client, nameof(client));
            _invoker = invoker ?? new EventHubProducerInvoker();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override async Task SendEventsAsync(params EventData[] events)
        {
            if (events == null || events.Length == 0)
                return;

            // Why this logic: https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample04_PublishingEvents.md
            EventDataBatch batch = null!;
            var batches = new List<EventDataBatch>();
            try
            {
                // Build up batches, at least one per partition key.
                foreach (var pk in events.GroupBy(e => e.PartitionKey))
                {
                    batches.Add(batch = pk.Key == null ? await _client.CreateBatchAsync().ConfigureAwait(false) : await _client.CreateBatchAsync(new CreateBatchOptions { PartitionKey = pk.Key }).ConfigureAwait(false));
                    foreach (var ed in pk)
                    {
                        var eh = ed.ToAzureEventHubsEventData();
                        if (!batch.TryAdd(eh))
                        {
                            batches.Add(batch = pk.Key == null ? await _client.CreateBatchAsync().ConfigureAwait(false) : await _client.CreateBatchAsync(new CreateBatchOptions { PartitionKey = pk.Key }).ConfigureAwait(false));
                            if (!batch.TryAdd(eh))
                                throw new InvalidOperationException("The EventData is too large to fit into an EventHubBatch.");
                        }
                    }
                }

                // Send all of the batches.
                foreach (var b in batches)
                {
                    await _invoker.InvokeAsync(this, async () => await _client.SendAsync(b).ConfigureAwait(false)).ConfigureAwait(false);
                }
            }
            finally
            {
                // Must dispose the batch as it has unmanaged resources.
                foreach (var b in batches)
                {
                    b.Dispose();
                }
            }
        }
    }
}