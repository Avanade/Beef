// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
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
        /// Gets the <see cref="CdcOutbox"/> where <see cref="IsSuccessful"/>.
        /// </summary>
        public CdcOutbox? Outbox { get; internal set; }

        /// <summary>
        /// Indicates that the outbox execution is successful and can continue.
        /// </summary>
        public bool IsSuccessful => Exception  == null;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> where <b>not</b> <see cref="IsSuccessful"/>
        /// </summary>
        public Exception? Exception { get; internal set; }

        /// <summary>
        /// Gets the events that were published/sent.
        /// </summary>
        public EventData[]? Events { get; internal set; }
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