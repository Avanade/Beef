// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents a single (once-only immediate) execution.
    /// </summary>
    /// <typeparam name="TArgs">The executor arguments <see cref="Type"/>.</typeparam>
    public sealed class SingleTrigger<TArgs> : TriggerBase<TArgs>
    {
        private readonly TArgs _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleTrigger{TArgs}"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public SingleTrigger(TArgs args)
        {
            _args = args;
        }

        /// <summary>
        /// Runs the work (once) and then immediately stops.
        /// </summary>
        protected override void OnStarted()
        {
            Run(_args, () => Stop());
        }
    }
}
