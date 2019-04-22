// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the <see cref="Trigger"/> result.
    /// </summary>
    public enum TriggerResult
    {
        /// <summary>
        /// Trigger has not stopped; see the corresponding <see cref="TriggerState"/>.
        /// </summary>
        Undetermined,

        /// <summary>
        /// Trigger successfully executed (completed).
        /// </summary>
        Successful,

        /// <summary>
        /// Trigger did <b>not</b> successfully execute.
        /// </summary>
        Unsuccessful
    }
}
