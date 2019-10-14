// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Represents a poison message table entity.
    /// </summary>
    public class PoisonMessage : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoisonMessage"/> class.
        /// </summary>
        public PoisonMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoisonMessage"/> class with a specified <paramref name="partitionKey"/> and <paramref name="rowKey"/>.
        /// </summary>
        /// <param name="partitionKey">The <see cref="TableEntity.PartitionKey"/>.</param>
        /// <param name="rowKey">The <see cref="TableEntity.RowKey"/>.</param>
        public PoisonMessage(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }

        /// <summary>
        /// Gets or sets the offset of the data relative to the Event Hub partition stream.
        /// </summary>
        public string Offset { get; set; }
        
        /// <summary>
        /// Gets or sets the logical sequence number of the event within the partition stream of the Event Hub.
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Indicates whether to skip the poison message and continue processing the next.
        /// </summary>
        public bool SkipMessage { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the sent time in UTC.
        /// </summary>
        public DateTime EnqueuedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when initially poisoned in UTC.
        /// </summary>
        public DateTime PoisonedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was skipped in UTC.
        /// </summary>
        public DateTime? SkippedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the runtime function type information: namespace.class.method.
        /// </summary>
        public string FunctionType { get; set; }
        
        /// <summary>
        /// Gets or sets the function name.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.EventHubs.EventData.Body"/> content as a <see cref="string"/>.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Exception"/> as a <see cref="string"/>.
        /// </summary>
        public string Exception { get; set; }
    }
}