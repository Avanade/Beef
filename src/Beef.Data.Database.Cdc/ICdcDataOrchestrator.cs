// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the core Change Data Capture (CDC) data orchestration capability.
    /// </summary>
    public interface ICdcDataOrchestrator
    {
        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="CdcOutbox"/>.
        /// </summary>
        int MaxQuerySize { get; set; }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server </remarks>
        bool ContinueWithDataLoss { get; set; }

        /// <summary>
        /// Executes the next (new) outbox, or reprocesses the last incomplete, then <see cref="CompleteAsync(int, List{CdcTracker})">completes</see> on success.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        /// <remarks>An outbox may be incomplete where there was a previous execution failure.</remarks>
        Task<CdcDataOrchestratorResult> ExecuteAsync(CancellationToken? cancellationToken);

        /// <summary>
        /// Completes an existing outbox updating the corresponding <paramref name="tracking"/> where appropriate.
        /// </summary>
        /// <param name="outboxId">The outbox identifer.</param>
        /// <param name="tracking">The <see cref="CdcTracker"/> list.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        Task<CdcDataOrchestratorResult> CompleteAsync(int outboxId, List<CdcTracker> tracking);
    }
}