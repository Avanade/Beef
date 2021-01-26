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
        /// Executes the next (new) outbox.
        /// </summary>
        /// <param name="maxQuerySize">The maximum query size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        public Task<CdcDataOrchestratorResult> ExecuteNextAsync(int maxQuerySize, CancellationToken? cancellationToken);

        /// <summary>
        /// Executes any previously incomplete outbox.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        public Task<CdcDataOrchestratorResult> ExecuteIncompleteAsync(CancellationToken? cancellationToken = null);
    }
}