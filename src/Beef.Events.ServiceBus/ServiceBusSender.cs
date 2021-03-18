// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// <see cref="SendEventsAsync(EventData[])">Send</see> the <see cref="EventData"/> array (converted to <see cref="AzureServiceBus.ServiceBusMessage"/>) in multiple batches based on <see cref="QueueName"/>.
    /// </summary>
    public class ServiceBusSender : EventPublisherBase
    {
        private readonly AzureServiceBus.ServiceBusClient _client;
        private readonly ServiceBusSenderInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusSender"/> using the specified <see cref="AzureServiceBus.ServiceBusClient"/> where the queue will be inferred from the <see cref="EventData.Subject"/>
        /// using <see cref="CreateQueueName"/> (consider setting the underlying <see cref="AzureServiceBus.ServiceBusClientOptions.RetryOptions"/>) to allow for transient errors).
        /// </summary>
        /// <param name="client">The <see cref="AzureServiceBus.ServiceBusClient"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="ServiceBusSenderInvoker"/>.</param>
        public ServiceBusSender(AzureServiceBus.ServiceBusClient client, ServiceBusSenderInvoker? invoker = null)
        {
            _client = Check.NotNull(client, nameof(client));
            _invoker = invoker ?? new ServiceBusSenderInvoker();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusSender"/> using the specified <see cref="AzureServiceBus.ServiceBusClient"/> and queue name (consider setting the underlying 
        /// <see cref="AzureServiceBus.ServiceBusClientOptions.RetryOptions"/>) to allow for transient errors).
        /// </summary>
        /// <param name="client">The <see cref="AzureServiceBus.ServiceBusClient"/>.</param>
        /// <param name="queueName">The queue name.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="ServiceBusSenderInvoker"/>.</param>
        public ServiceBusSender(AzureServiceBus.ServiceBusClient client, string queueName, ServiceBusSenderInvoker? invoker = null) : this(client, invoker) => QueueName = Check.NotEmpty(queueName, nameof(queueName));

        /// <summary>
        /// Gets the queue name. Where <c>null</c> this indicates that the queue name will be <see cref="CreateQueueName">created</see> (inferred) at runtime
        /// </summary>
        public string? QueueName { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override async Task SendEventsAsync(params EventData[] events)
        {
            if (events == null || events.Length == 0)
                return;

            // Why this logic: https://github.com/Azure/azure-sdk-for-net/tree/Azure.Messaging.ServiceBus_7.1.0/sdk/servicebus/Azure.Messaging.ServiceBus/#send-and-receive-a-batch-of-messages
            var dict = new Dictionary<string, Queue<AzureServiceBus.ServiceBusMessage>>();
            foreach (var @event in events)
            {
                var queueName = QueueName ?? CreateQueueName(@event);
                if (dict.TryGetValue(queueName, out var list))
                    list.Enqueue(@event.ToAzureServiceBusMessage());
                else
                {
                    var queue = new Queue<AzureServiceBus.ServiceBusMessage>();
                    queue.Enqueue(@event.ToAzureServiceBusMessage());
                    dict.Add(queueName, queue);
                }
            }

            // Send to each named queue in batches.
            foreach (var di in dict)
            {
                var sender = _client.CreateSender(di.Key);

                while (di.Value.Count > 0)
                {
                    using var batch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

                    if (batch.TryAddMessage(di.Value.Peek()))
                        di.Value.Dequeue();
                    else
                        throw new InvalidOperationException("The EventData is too large to fit into a ServiceBusMessageBatch.");

                    while (di.Value.Count > 0 && batch.TryAddMessage(di.Value.Peek()))
                    {
                        di.Value.Dequeue();
                    }

                    await sender.SendMessagesAsync(batch);
                }
            }
        }

        /// <summary>
        /// Creates the queue name from the <see cref="EventData.Subject"/>. This is achieved by removing the last part (typically the key) to provide the base path; for example a Subject of
        /// <c>Beef.Demo.Person.1234</c> would result in <c>Beef.Demo.Person</c>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The queue name.</returns>
        public virtual string CreateQueueName(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrEmpty(@event.Subject))
                throw new ArgumentException("The Subject property must be specified.", nameof(@event));

            var parts = @event.Subject.Split(PathSeparator);
            if (parts.Length <= 1)
                return @event.Subject;

            return @event.Subject[0..^(parts.Last().Length + 1)];
        }
    }
}
