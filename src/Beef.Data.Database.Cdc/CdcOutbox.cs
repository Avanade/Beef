﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using System;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the standard CDC (Change Data Capture) outbox.
    /// </summary>
    public class CdcOutbox : IIntIdentifier
    {
        /// <summary>
        /// Gets or sets the outbox outbox identifer.
        /// </summary>
        [MapperProperty("OutboxId")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the outbox outbox created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Indicates whether the changes within the outbox have been marked as completed.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the processed date.
        /// </summary>
        public DateTime? CompletedDate { get; set; }
    }
}