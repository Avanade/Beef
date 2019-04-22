// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors
{
    /// <summary>
    /// Determines whether processing will <see cref="Stop"/> or <see cref="Continue"/> when an unhandled <see cref="Exception"/> is encountered. 
    /// </summary>
    public enum ExceptionHandling
    {
        /// <summary>
        /// Logs the <see cref="Exception"/> and initiates a <see cref="Executor.Stop"/> (results in an <see cref="ExecutorResult.Unsuccessful"/> <see cref="Executor.Result"/>).
        /// </summary>
        Stop,

        /// <summary>
        /// Log the <see cref="Exception"/> and continue (results in an <see cref="ExecutorResult.Successful"/> <see cref="Executor.Result"/>).
        /// </summary>
        Continue
    }
}