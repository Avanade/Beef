// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the <c>CdcExecutor</c> result.
    /// </summary>
    public abstract class CdcExecutorResult
    {
        /// <summary>
        /// Gets or sets the database return code.
        /// </summary>
        public int ReturnCode { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="CdcEnvelope"/>.
        /// </summary>
        public CdcEnvelope? Envelope { get; internal set; }

        /// <summary>
        /// Indicates that a envelope execution was successful and can continue.
        /// </summary>
        public bool EnvelopeExecuted => ReturnCode == 0 && Envelope != null;
    }

    /// <summary>
    /// Represents the typed-<see cref="CdcExecutorResult"/> result.
    /// </summary>
    /// <typeparam name="TCdcEntityWrapperColl">The <typeparamref name="TCdcEntityWrapper"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapper">The entity wrapper <see cref="Type"/>.</typeparam>
    public class CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper> : CdcExecutorResult
        where TCdcEntityWrapperColl : List<TCdcEntityWrapper>, new() where TCdcEntityWrapper : class, ICdcDatabase
    {
        /// <summary>
        /// Gets the resulting <typeparamref name="TCdcEntityWrapperColl"/>.
        /// </summary>
        public TCdcEntityWrapperColl Result { get; } = new TCdcEntityWrapperColl();
    }
}