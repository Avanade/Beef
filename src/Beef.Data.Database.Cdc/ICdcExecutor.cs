// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the core Change Data Capture (CDC) execution capability.
    /// </summary>
    public interface ICdcExecutor
    {
        /// <summary>
        /// Executes the next (new) envelope.
        /// </summary>
        /// <param name="maxQuerySize">The maximum query size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult"/>.</returns>
        public Task<CdcExecutorResult> ExecuteNextAsync(int maxQuerySize, CancellationToken? cancellationToken);

        /// <summary>
        /// Executes any previously incomplete envelope.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult"/>.</returns>
        public Task<CdcExecutorResult> ExecuteIncompleteAsync(CancellationToken? cancellationToken = null);
    }
}