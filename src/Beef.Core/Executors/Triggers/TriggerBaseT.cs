// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents a base class for a trigger with arguments.
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public abstract class TriggerBase<TArgs> : Trigger
    {
        /// <summary>
        /// Executes a single unit of work.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="completionCallback">An optional callback for post <see cref="Executor"/> <b>Run</b> notification/processing.</param>

        protected void Run(TArgs args, Action completionCallback = null)
        {
            Run(this, args, completionCallback);
        }
    }
}
