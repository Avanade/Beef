// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events.Repository
{
    /// <summary>
    /// Represents the core audit record properties; used by the likes of the <see cref="AzureStorageRepository{TData, TAudit}"/>.
    /// </summary>
    public interface IAuditRecord
    {
        /// <summary>
        /// Gets or sets the unique event identifier.
        /// </summary>
        Guid? EventId { get; set; }

        /// <summary>
        /// Indicates whether to skip the poison message and continue processing the next.
        /// </summary>
        bool SkipProcessing { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the enqueue in UTC.
        /// </summary>
        DateTimeOffset EnqueuedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when initially poisoned in UTC.
        /// </summary>
        DateTimeOffset? PoisonedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was skipped in UTC.
        /// </summary>
        DateTimeOffset? SkippedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the number of invocation attempts counter.
        /// </summary>
        int Attempts { get; set; }

        /// <summary>
        /// Gets or sets the event subject.
        /// </summary>
        string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        string? Action { get; set; }

        /// <summary>
        /// Gets or sets the status (see <see cref="Result.Status"/>).
        /// </summary>
        string? Status { get; set; }

        /// <summary>
        /// Gets or sets the reason (see <see cref="Result.Reason"/>).
        /// </summary>
        string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the originating status (see <see cref="Result.Status"/>).
        /// </summary>
        string? OriginatingStatus { get; set; }

        /// <summary>
        /// Gets or sets the originating reason (see <see cref="Result.Reason"/>).
        /// </summary>
        string? OriginatingReason { get; set; }

        /// <summary>
        /// Gets or sets the event body content as a <see cref="string"/>.
        /// </summary>
        string? Body { get; set; }

        /// <summary>
        /// Gets or sets the exception (see <see cref="Result.Exception"/>).
        /// </summary>
        string? Exception { get; set; }
    }
}