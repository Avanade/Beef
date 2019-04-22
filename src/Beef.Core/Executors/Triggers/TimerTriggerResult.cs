// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the result of a <b>timer</b>-related trigger.
    /// </summary>
    public class TimerTriggerResult
    {
        /// <summary>
        /// Indicates that the <see cref="Executor"/> can be run as a result of the <see cref="Trigger"/>.
        /// </summary>
        /// <remarks>Where the execution should not be run then this should be set to <c>false</c>.</remarks>
        public bool IsExecutorRunEnabled { get; set; } = true;
    }

    /// <summary>
    /// Represents the result of a <b>timer</b>-related trigger with execution <see cref="Args"/> value.
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public class TimerTriggerResult<TArgs> : TimerTriggerResult
    {
        /// <summary>
        /// Get or sets the arguments.
        /// </summary>
        public TArgs Args { get; set; }
    }
}
