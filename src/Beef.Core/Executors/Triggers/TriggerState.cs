// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the current state of the <see cref="Trigger"/>.
    /// </summary>
    public enum TriggerState
    {
        /// <summary>
        /// Trigger has not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Trigger is running.
        /// </summary>
        Running,

        /// <summary>
        /// Trigger is stopping.
        /// </summary>
        Stopping,

        /// <summary>
        /// Trigger is stopped.
        /// </summary>
        Stopped
    }
}
