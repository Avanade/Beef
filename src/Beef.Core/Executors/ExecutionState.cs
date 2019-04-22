// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Executors
{
    /// <summary>
    /// Represents the current state of an <see cref="ExecutionManager"/> or <see cref="Executor"/>.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// Executor has not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Executor has started.
        /// </summary>
        Started,

        /// <summary>
        /// Executor is running.
        /// </summary>
        Running,

        /// <summary>
        /// Executor is stopping.
        /// </summary>
        Stopping,

        /// <summary>
        /// Executor is stopped.
        /// </summary>
        Stopped
    }
}
