// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events
{
    /// <summary>
    /// Provides the unhandled <see cref="Exception"/> options of either <see cref="ThrowException"/> or <see cref="Continue"/>.
    /// </summary>
    public enum UnhandledExceptionHandling
    {
        /// <summary>
        /// Stops and bubbles up the unhandled <see cref="Exception"/> allowing the hosting process to determine the appropriate action.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Skips and continues effectively swallowing the <see cref="Exception"/>.
        /// </summary>
        Continue
    }
}