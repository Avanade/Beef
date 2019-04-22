// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents a single (once-only immediate) execution only.
    /// </summary>
    public sealed class SingleTrigger : TriggerBase
    {
        /// <summary>
        /// Runs the work (once) and then immediately stops.
        /// </summary>
        protected override void OnStarted()
        {
            Run(() => Stop());
        }
    }
}
