// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the base class for a trigger with no arguments.
    /// </summary>
    public abstract class TriggerBase : Trigger
    {
        /// <summary>
        /// Executes a single unit of work.
        /// </summary>
        /// <param name="completionCallback">An optional callback for post <see cref="Executor"/> <b>Run</b> notification/processing.</param>
        protected void Run(Action completionCallback = null)
        {
            Run(this, null, completionCallback);
        }
    }
}
