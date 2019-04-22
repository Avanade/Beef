// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Executors
{
    /// <summary>
    /// Represents the result of an <see cref="Executor"/>.
    /// </summary>
    public enum ExecutorResult
    {
        /// <summary>
        /// Executor has not stopped; see the corresponding <see cref="ExecutionState"/>.
        /// </summary>
        Undetermined,

        /// <summary>
        /// Executor successfully executed (completed).
        /// </summary>
        Successful,

        /// <summary>
        /// Executor did <b>not</b> successfully execute.
        /// </summary>
        Unsuccessful
    }
}
