// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

#pragma warning disable CA2227, CA1819 // Collection/Array properties should be read only; ignored, as acceptable for a database model.

using Beef.Entities;
using System;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the standard CDC (Change Data Capture) outbox envelope.
    /// </summary>
    public class CdcOutboxEnvelope : IIntIdentifier
    {
        /// <summary>
        /// Gets or sets the outbox envelope identifer.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the outbox envelope created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="Beef.OperationType"/>.
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets the first processed Log Sequence Number (LSN).
        /// </summary>
        public byte[]? FirstLsn { get; set; }

        /// <summary>
        /// Gets or sets the last processed Log Sequence Number (LSN).
        /// </summary>
        public byte[]? LastLsn { get; set; }

        /// <summary>
        /// Indicates whether the changes within the envelope have been marked as completed.
        /// </summary>
        public bool HasBeenCompleted { get; set; }

        /// <summary>
        /// Gets or sets the processed date.
        /// </summary>
        public DateTime? ProcessedDate { get; set; }
    }
}

#pragma warning restore CA2227, CA1819