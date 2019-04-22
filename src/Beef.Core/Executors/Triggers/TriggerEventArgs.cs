// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the <see cref="Trigger"/> <see cref="EventArgs"/> with optional <see cref="Args"/>.
    /// </summary>
    internal class TriggerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public object Args { get; set; }

        /// <summary>
        /// Gets or sets optional callback for post <see cref="Executor"/> <b>Run</b> notification/processing.
        /// </summary>
        internal Action CompletionCallback { get; set; }
    }
}
