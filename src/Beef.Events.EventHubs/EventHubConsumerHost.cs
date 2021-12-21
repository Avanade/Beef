// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using AzureEventHubs = Azure.Messaging.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the Azure Event Hubs (see <see cref="AzureEventHubs.EventData"/>) <see cref="EventSubscriberHost"/>.
    /// </summary>
    public class EventHubConsumerHost : EventSubscriberHost<AzureEventHubs.EventData, EventHubData, EventHubConsumerHost>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubConsumerHost"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventHubConsumerHost"/>.</param>
        /// <param name="eventDataConverter">The optional <see cref="IEventDataConverter{T}"/>. Defaults to a <see cref="EventHubsEventConverter"/> using a <see cref="NewtonsoftJsonCloudEventSerializer"/>.</param>
        public EventHubConsumerHost(EventSubscriberHostArgs args, IEventDataConverter<AzureEventHubs.EventData>? eventDataConverter = null) 
            : base(args, eventDataConverter ?? new EventHubsEventConverter(new NewtonsoftJsonCloudEventSerializer())) { }


        /// <summary>
        /// Creates a <see cref="EventHubData"/> instance for the specified <paramref name="originating"/> event, <paramref name="partitionContext"/>, <paramref name="eventHubName"/> and <paramref name="consumerGroupName"/>.
        /// </summary>
        /// <param name="originating">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="partitionContext">The <see cref="PartitionContext"/>.</param>
        /// <param name="eventHubName">The event hub name.</param>
        /// <param name="consumerGroupName">The consumer group name; defaults to '<c>$Default</c>'.</param>
        /// <returns>The <see cref="EventHubData"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Defined as non-static same as other method of same name for consistency.")]
        public EventHubData CreateEventHubData(AzureEventHubs.EventData originating, PartitionContext partitionContext, string eventHubName, string? consumerGroupName = null)
            => new EventHubData(eventHubName, consumerGroupName ?? "$Default", (partitionContext ?? throw new ArgumentNullException(nameof(partitionContext))).PartitionId, originating);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originating">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="connection">The event huubs connection string used to infer the <see cref="EventHubData.EventHubName"/>.</param>
        /// <param name="partitionContext">The <see cref="PartitionContext"/>.</param>
        /// <param name="consumerGroupName">The consumer group name; defaults to '<c>$Default</c>'.</param>
        /// <returns>The <see cref="EventHubData"/>.</returns>
        public EventHubData CreateEventHubData(AzureEventHubs.EventData originating, string connection, PartitionContext partitionContext, string? consumerGroupName = null)
            => new EventHubData(GetEventHubName(connection), consumerGroupName ?? "$Default", (partitionContext ?? throw new ArgumentNullException(nameof(partitionContext))).PartitionId, originating);

        /// <summary>
        /// Gets the endpoint from the connection string.
        /// </summary>
        private string GetEventHubName(string connection)
        {
            var config = Args.ServiceProvider?.GetService<IConfiguration>();
            if (config == null)
                throw new InvalidOperationException("Unable to get an instance of IConfiguration via the Args.ServiceProvider.");

            var key = "EntityPath=";
            var cs = config.GetValue<string>(connection) ?? throw new ArgumentException($"EventHubs connection string configuration name '{connection}' does not exist.", nameof(connection));
            if (!string.IsNullOrEmpty(cs))
            {
                var val = cs.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (val != null)
                    return val[key.Length..];
            }

            throw new ArgumentException($"EventHubs connection string configuration name '{connection}' does not have a valid value.", nameof(connection));
        }
    }
}