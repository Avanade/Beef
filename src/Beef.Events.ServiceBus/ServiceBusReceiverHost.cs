// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the Azure Service Bus (see <see cref="AzureServiceBus.Message"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class ServiceBusReceiverHost : EventSubscriberHost
    {
        private ServiceBusReceiverHostInvoker? _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiverHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="ServiceBusReceiverHost"/>.</param>
        public ServiceBusReceiverHost(EventSubscriberHostArgs args) : base(args) { }

        /// <summary>
        /// Gets or sets the <see cref="ServiceBusReceiverHostInvoker"/>. Defaults to <see cref="ServiceBusReceiverHostInvoker"/>.
        /// </summary>
        public ServiceBusReceiverHostInvoker Invoker { get => _invoker ??= new ServiceBusReceiverHostInvoker(); set => _invoker = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Use (set) the <see cref="Invoker"/>.
        /// </summary>
        /// <param name="invoker">The <see cref="ServiceBusReceiverHostInvoker"/>.</param>
        /// <returns>The <see cref="ServiceBusReceiverHost"/> instance (for fluent-style method chaining).</returns>
        public ServiceBusReceiverHost UseInvoker(ServiceBusReceiverHostInvoker invoker)
        {
            Invoker = invoker;
            return this;
        }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>The <see cref="ServiceBusReceiverHost"/> instance (for fluent-style method chaining).</returns>
        public ServiceBusReceiverHost UseLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Performs the receive processing for an <see cref="AzureServiceBus.Message"/> instance.
        /// </summary>
        /// <param name="serviceBusName">The Event Hubs name.</param>
        /// <param name="queueName">The Event Hubs partition identifier.</param>
        /// <param name="message">The <see cref="AzureServiceBus.Message"/> instance to receive/process.</param>
        public Task ReceiveAsync(string serviceBusName, string queueName, AzureServiceBus.Message message)
        {
            var sbd = new ServiceBusData(serviceBusName, queueName, message ?? throw new ArgumentNullException(nameof(message)));

            return Invoker.InvokeAsync(this, async () =>
            {
                // Invoke the base EventSubscriberHost.ReceiveAsync to do the actual work!
                var (_, subject, action, _, _, _) = message.GetBeefMetadata();
                await ReceiveAsync(sbd, subject, action, (subscriber) =>
                {
                    // Convert AzureServiceBus.Message to Beef.EventData.
                    try
                    {
                        return subscriber.ValueType == null ? message.ToBeefEventData() : message.ToBeefEventData(subscriber.ValueType);
                    }
                    catch (Exception ex) { throw new EventSubscriberUnhandledException(CreateInvalidEventDataResult(ex)); }
                }).ConfigureAwait(false);
            }, sbd);
        }
    }
}