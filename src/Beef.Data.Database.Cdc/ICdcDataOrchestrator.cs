// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
        /// Executes the next (new) outbox.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        Task<CdcDataOrchestratorResult> ExecuteNextAsync(CancellationToken? cancellationToken);

        /// <summary>
        /// Executes any previously incomplete outbox.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        Task<CdcDataOrchestratorResult> ExecuteIncompleteAsync(CancellationToken? cancellationToken);
    }
}