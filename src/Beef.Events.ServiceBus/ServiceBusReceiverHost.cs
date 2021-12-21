// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the Azure Service Bus (see <see cref="ServiceBusReceivedMessage"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class ServiceBusReceiverHost : EventSubscriberHost<ServiceBusReceivedMessage, ServiceBusData, ServiceBusReceiverHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiverHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <param name="eventDataConverter">The optional <see cref="IEventDataConverter{T}"/>. Defaults to a <see cref="ServiceBusMessageConverter"/> using a <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public ServiceBusReceiverHost(EventSubscriberHostArgs args, IEventDataConverter<ServiceBusReceivedMessage>? eventDataConverter = null) 
            : base(args, eventDataConverter ?? new ServiceBusReceivedMessageConverter(new NewtonsoftJsonCloudEventSerializer())) { }

        /// <summary>
        /// Creates a <see cref="ServiceBusData"/> instance for a <i>Queue</i> using a similar configuration-based approach as the Azure <i>ServiceBusTrigger</i>.
        /// </summary>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="ServiceBusReceivedMessage"/>.</param>
        /// <param name="queueName">Either a specific queue name, or from configuration where bookended by the '<c>%</c>' character (i.e. '<c>%QueueName%</c>').</param>
        /// <param name="connection">The service bus connection string configuration name; defaults to '<c>AzureWebJobsServiceBus</c>'.</param>
        /// <returns>The <see cref="ServiceBusData"/>.</returns>
        public ServiceBusData CreateServiceBusData(ServiceBusReceivedMessage originating, string queueName, string? connection = "AzureWebJobsServiceBus")
        {
            var config = Args.ServiceProvider?.GetService<IConfiguration>();
            if (config == null)
                throw new InvalidOperationException("Unable to get an instance of IConfiguration via the Args.ServiceProvider.");

            var uri = GetEndpoint(config, connection ?? throw new ArgumentNullException(nameof(connection)));
            return new ServiceBusData(uri.Host, GetValueOrConfig(config, queueName, nameof(queueName)), originating);
        }

        /// <summary>
        /// Creates a <see cref="ServiceBusData"/> instance for a <i>Topic</i> using a similar configuration-based approach as the Azure <i>ServiceBusTrigger</i>.
        /// </summary>
        /// <param name="originating">The <see cref="EventSubscriberData{TOriginating}.Originating"/> <see cref="ServiceBusMessage"/>.</param>
        /// <param name="topicName">Either a specific topic name, or from configuration where bookended by the '<c>%</c>' character (i.e. '<c>%TopicName%</c>').</param>
        /// <param name="subscriptionName">Either a specific subscription name, or from configuration where bookended by the '<c>%</c>' character (i.e. '<c>%SubscriptionName%</c>').</param>
        /// <param name="connection">The service bus connection string configuration name; defaults to '<c>AzureWebJobsServiceBus</c>'.</param>
        /// <returns>The <see cref="ServiceBusData"/>.</returns>
        public ServiceBusData CreateServiceBusData(ServiceBusReceivedMessage originating, string topicName, string subscriptionName, string? connection = "AzureWebJobsServiceBus")
        {
            var config = Args.ServiceProvider?.GetService<IConfiguration>();
            if (config == null)
                throw new InvalidOperationException("Unable to get an instance of IConfiguration via the Args.ServiceProvider.");

            var uri = GetEndpoint(config, connection ?? throw new ArgumentNullException(nameof(connection)));
            return new ServiceBusData(uri.Host, GetValueOrConfig(config, topicName, nameof(topicName)), GetValueOrConfig(config, subscriptionName, nameof(subscriptionName)), originating);
        }

        /// <summary>
        /// Gets the endpoint from the connection string.
        /// </summary>
        private static Uri GetEndpoint(IConfiguration config, string connection)
        {
            var key = "Endpoint=";
            var cs = config.GetValue<string>(connection) ?? throw new ArgumentException($"ServiceBus connection string configuration name '{connection}' does not exist.", nameof(connection));
            if (!string.IsNullOrEmpty(cs))
            {
                var val = cs.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (val != null)
                    return new Uri(val[key.Length..]);
            }

            throw new ArgumentException($"ServiceBus connection string configuration name '{connection}' does not have a valid value.", nameof(connection));
        }

        /// <summary>
        /// Gets the specified value, or from configuration where bookended with the '%' character.
        /// </summary>
        private static string GetValueOrConfig(IConfiguration config, string value, string argName)
        {
            if (Check.NotEmpty(value, nameof(argName)).StartsWith("%") && value.EndsWith("%"))
            {
                var key = value[1..^1];
                return config.GetValue<string>(key) ?? throw new ArgumentException($"ServiceBus queue name configuration key '{key}' does not exist.", argName);
            }
            else
                return value;
        }
    }
}