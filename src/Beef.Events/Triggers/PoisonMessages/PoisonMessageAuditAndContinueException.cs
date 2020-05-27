// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Subscribe;
using System;

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Represents an internal <see cref="Exception"/> to orchestrate the auditing of a <see cref="PoisonMessage"/> that is is thrown when the <see cref="ResultHandling"/> is 
    /// <see cref="ResultHandling.ContinueWithAudit"/>. This is special case (internal) <i>exception</i> that will be caught by the <b>trigger</b> invoking the <see cref="IPoisonMessagePersistence"/>
    /// <see cref="IPoisonMessagePersistence.SkipAuditAsync(Microsoft.Azure.EventHubs.EventData, string)"/>.
    /// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors; this is for internal use only and only needs the one constructor.
    public class PoisonMessageAuditAndContinueException : Exception
#pragma warning restore CA1032
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoisonMessageAuditAndContinueException"/>.
        /// </summary>
        /// <param name="result">The <see cref="Result"/>.</param>
        internal PoisonMessageAuditAndContinueException(Result result) : base(result.ToString()) => Result = result;

        /// <summary>
        /// Gets the <see cref="Result"/>.
        /// </summary>
        public Result Result { get; private set; }
    }
}