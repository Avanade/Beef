// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors
{
    /// <summary>
    /// Internal exception used to orchestrate the <see cref="Executor"/> <see cref="Executor.Stop"/> method. This exception should not be caught as it is used internally; swallowing exception will
    /// result in unexpected side effects.
    /// </summary>
    internal class ExecutorStopException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorStopException"/> class.
        /// </summary>
        /// <param name="stopExecutionManager">Indicates whether all executions should be stopped via the <see cref="ExecutionManager"/> or just the executing instance (default).</param>
        /// <param name="stopException">The underlying stop <see cref="Exception"/> (optional).</param>
        internal ExecutorStopException(bool stopExecutionManager = false, Exception stopException = null) 
            : base("The ExecutorStopException is for internal use only: do NOT catch and swallow this exception as it is used internally to manage a Stop.", stopException)
        {
            StopExecutionManager = stopExecutionManager;
        }

        /// <summary>
        /// Indicates whether all executions should be stopped via the <see cref="ExecutionManager"/> or just the executing instance (default).
        /// </summary>
        internal bool StopExecutionManager { get; private set; }
    }
}
