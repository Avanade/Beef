// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the <c>CdcDataOrchestrator</c> result.
    /// </summary>
    public abstract class CdcDataOrchestratorResult
    {
        /// <summary>
        /// Gets or sets the database return code.
        /// </summary>
        public int ReturnCode { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="CdcOutbox"/>.
        /// </summary>
        public CdcOutbox? Outbox { get; internal set; }

        /// <summary>
        /// Indicates that a outbox execution was successful and can continue.
        /// </summary>
        public bool OutboxExecuted => ReturnCode == 0 && Outbox != null;
    }

    /// <summary>
    /// Represents the typed-<see cref="CdcDataOrchestratorResult"/> result.
    /// </summary>
    /// <typeparam name="TCdcEntityWrapperColl">The <typeparamref name="TCdcEntityWrapper"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapper">The entity wrapper <see cref="Type"/>.</typeparam>
    public class CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper> : CdcDataOrchestratorResult
        where TCdcEntityWrapperColl : List<TCdcEntityWrapper>, new() where TCdcEntityWrapper : class, ICdcWrapper
    {
        /// <summary>
        /// Gets the resulting <typeparamref name="TCdcEntityWrapperColl"/>.
        /// </summary>
        public TCdcEntityWrapperColl Result { get; } = new TCdcEntityWrapperColl();
    }
}