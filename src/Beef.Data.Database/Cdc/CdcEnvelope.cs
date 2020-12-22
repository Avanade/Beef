// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using System;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the standard CDC (Change Data Capture) outbox envelope.
    /// </summary>
    public class CdcEnvelope : IIntIdentifier
    {
        /// <summary>
        /// Gets or sets the outbox envelope identifer.
        /// </summary>
        [MapperProperty("EnvelopeId")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the outbox envelope created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Indicates whether the changes within the envelope have been marked as completed.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the processed date.
        /// </summary>
        public DateTime? CompletedDate { get; set; }
    }
}