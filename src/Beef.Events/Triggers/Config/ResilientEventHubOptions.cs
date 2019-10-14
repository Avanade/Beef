// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Beef.Events.Triggers.Config
{
    /// <summary>
    /// Represents the "resilient event hub" options.
    /// </summary>
    public class ResilientEventHubOptions
    {
        private readonly ConcurrentDictionary<string, string> _eventHubs = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the <see cref="EventProcessorOptions"/>.
        /// </summary>
        public EventProcessorOptions EventProcessorOptions { get; } = EventProcessorOptions.DefaultOptions;

        /// <summary>
        /// Gets the <see cref="PartitionManagerOptions"/>.
        /// </summary>
        public PartitionManagerOptions PartitionManagerOptions { get; } = new PartitionManagerOptions();

        /// <summary>
        /// Gets or sets the maximum retry <see cref="TimeSpan"/> (default is 15 minutes).
        /// </summary>
        public TimeSpan MaxRetryTimespan { get; set; } = new TimeSpan(0, 15, 0);

        /// <summary>
        /// Determine (gets or sets) whether a possible <b>poison message</b> has been encountered and should be logged after the specified retry count (must be between 1 and 10, defaults to 6).
        /// </summary>
        /// <remarks>It is recommended that a poison message is not identifed (logged) immediately to allow a number of retries to occur in case the <see cref="Exception"/> is genuinely transient in nature.</remarks>
        public int LogPoisonMessageAfterRetryCount { get; set; } = 6;

        /// <summary>
        /// Gets or sets the event hub endpoint path.
        /// </summary>
        public string EventHubPath { get; set; }

        /// <summary>
        /// Gets or set the event hub name.
        /// </summary>
        public string EventHubName { get; set; }

        /// <summary>
        /// Gets or sets the function name.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets or sets the runtime function type information: namespace.class.method.
        /// </summary>
        public string FunctionType { get; set; }

        /// <summary>
        /// Adds the named event hub with the specified credentials.
        /// </summary>
        /// <param name="eventHubName">The event hub name.</param>
        /// <param name="eventHubConnectionString">The event hub recevier connection string.</param>
        public void AddEventHub(string eventHubName, string eventHubConnectionString)
        {
            _eventHubs.TryAdd(eventHubName ?? throw new ArgumentNullException(nameof(eventHubName)), eventHubConnectionString ?? throw new ArgumentNullException(nameof(eventHubConnectionString)));
        }

        /// <summary>
        /// Gets (instantiates) the <see cref="EventProcessorHost"/> for the specified <paramref name="eventHub"/> and <paramref name="consumerGroup"/>.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="eventHub">The event hub name.</param>
        /// <param name="consumerGroup">The consumer group.</param>
        /// <returns>The <see cref="EventProcessorHost"/>.</returns>
        public EventProcessorHost GetEventProcessorHost(IConfiguration config, string eventHub, string consumerGroup)
        {
            if (!_eventHubs.TryGetValue(eventHub, out string connectionString))
                throw new InvalidOperationException($"The specified Event Hub '{eventHub}' does not exist.");

            // Build the storage connection string.
            var storageConnectionString = config.GetWebJobsConnectionString(ConnectionStringNames.Storage);

            EventHubsConnectionStringBuilder cs = new EventHubsConnectionStringBuilder(connectionString);
            if (cs.EntityPath != null)
            {
                eventHub = cs.EntityPath;
                cs.EntityPath = null;
            }

            EventHubPath = EscapeBlobPath(cs.Endpoint.Host);
            EventHubName = EscapeBlobPath(eventHub);
            var blobPrefix = $"{EventHubPath}/{EventHubName}/";

            var maxRetryMinutes = config.GetValue<int>("MaxRetryMinutes");
            if (maxRetryMinutes > 0 && maxRetryMinutes <= 60 * 24)
                MaxRetryTimespan = TimeSpan.FromMinutes(maxRetryMinutes);

            var logPoisonCount = config.GetValue<int>("LogPoisonMessageAfterRetryCount");
            if (logPoisonCount > 0 && logPoisonCount <= 10)
                LogPoisonMessageAfterRetryCount = logPoisonCount;

            return new EventProcessorHost(
                    hostName: Guid.NewGuid().ToString(),
                    eventHubPath: eventHub,
                    consumerGroupName: consumerGroup,
                    eventHubConnectionString: cs.ToString(),
                    storageConnectionString: storageConnectionString,
                    leaseContainerName: "azure-webjobs-eventhub",
                    storageBlobPrefix: blobPrefix)
            {
                PartitionManagerOptions = PartitionManagerOptions
            };
        }

        /// <summary>
        /// Escape the blob path (copied from https://github.com/Azure/azure-functions-eventhubs-extension/blob/master/src/Microsoft.Azure.WebJobs.Extensions.EventHubs/Config/EventHubOptions.cs).
        /// </summary>
        internal static string EscapeBlobPath(string path)
        {
            StringBuilder sb = new StringBuilder(path.Length);
            foreach (char c in path)
            {
                if (c >= 'a' && c <= 'z')
                    sb.Append(c); // Already lower case.
                else if (c == '-' || c == '_' || c == '.')
                    sb.Append(c); // Common characters. 
                else if (c >= 'A' && c <= 'Z')
                    sb.Append((char)(c - 'A' + 'a')); // ToLower
                else if (c >= '0' && c <= '9')
                    sb.Append(c); // Numeric as is.
                else
                    sb.Append(EscapeStorageCharacter(c));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escape the unknown character and normalise.
        /// </summary>
        private static string EscapeStorageCharacter(char character)
        {
            var ordinalValue = (ushort)character;
            if (ordinalValue < 0x100)
                return string.Format(CultureInfo.InvariantCulture, ":{0:X2}", ordinalValue);
            else
                return string.Format(CultureInfo.InvariantCulture, "::{0:X4}", ordinalValue);
        }
    }
}