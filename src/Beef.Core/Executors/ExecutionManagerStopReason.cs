// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Executors.Triggers;
using System;
using System.Threading;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the reason for the <see cref="ExecutionManager"/> stopping.
    /// </summary>
    public enum ExecutionManagerStopReason
    {
        /// <summary>
        /// The <see cref="ExecutionManager"/> has not stopped.
        /// </summary>
        NotStopped,

        /// <summary>
        /// The <see cref="Trigger"/> initiated the stop.
        /// </summary>
        TriggerStop,

        /// <summary>
        /// The <see cref="Trigger"/> had an unexpected <see cref="Exception"/>.
        /// </summary>
        TriggerException,

        /// <summary>
        /// An <see cref="Executor"/> initiated the stop.
        /// </summary>
        ExecutorStop,

        /// <summary>
        /// An <see cref="Executor"/> had an unexpected <see cref="Exception"/>.
        /// </summary>
        ExecutorExceptionStop,

        /// <summary>
        /// The <see cref="ExecutionManager"/> initiated the stop.
        /// </summary>
        ExecutionManagerStop,

        /// <summary>
        /// The <see cref="ExecutionManager"/> initiated the stop as a result of a <see cref="CancellationToken"/>.
        /// </summary>
        ExecutionManagerCancellationTokenStop,

        /// <summary>
        /// The <see cref="ExecutionManager"/> had an unexpected <see cref="Exception"/>.
        /// </summary>
        ExecutionManagerException
    }
}
