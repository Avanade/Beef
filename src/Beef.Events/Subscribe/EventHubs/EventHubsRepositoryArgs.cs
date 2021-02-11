// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Provides the <b>Event Hubs</b> repository arguments.
    /// </summary>
    public class EventHubsRepositoryArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubsRepositoryArgs"/>.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="eventHubConnectionStringName">The Event Hubs connection string setting name; defaults to 'EventHubConnectionString'.</param>
        /// <param name="consumerGroup">The consumer group, defaults to '$Default'.</param>
        public EventHubsRepositoryArgs(IConfiguration config, string eventHubConnectionStringName = "EventHubConnectionString", string consumerGroup = "$Default") :
            this(config, new EventHubsConnectionStringBuilder(config.GetConnectionStringOrSetting(eventHubConnectionStringName)), consumerGroup) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubsRepositoryArgs"/> using the specified <paramref name="connectionStringBuilder"/>.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="connectionStringBuilder">The <see cref="EventHubsConnectionStringBuilder"/>.</param>
        /// <param name="consumerGroup">The consumer group, defaults to '$Default'.</param>
        public EventHubsRepositoryArgs(IConfiguration config, EventHubsConnectionStringBuilder connectionStringBuilder, string consumerGroup = "$Default")
        {
            if (connectionStringBuilder == null)
                throw new ArgumentNullException(nameof(connectionStringBuilder));

            EventHubPath = EscapeBlobPath(connectionStringBuilder.Endpoint.Host);
            EventHubName = EscapeBlobPath(connectionStringBuilder.EntityPath);
            ConsumerGroup = consumerGroup;
            StorageConnectionString = Check.NotNull(config, nameof(config)).GetWebJobsConnectionString(ConnectionStringNames.Storage);
        }

        /// <summary>
        /// Gets the Event Hubs path.
        /// </summary>
        public string EventHubPath { get; }

        /// <summary>
        /// Gets the Event Hubs name.
        /// </summary>
        public string EventHubName { get; }

        /// <summary>
        /// Gets the consumer group.
        /// </summary>
        public string ConsumerGroup { get; }

        /// <summary>
        /// Gets the Storage connection string.
        /// </summary>
        public string StorageConnectionString { get;  }

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