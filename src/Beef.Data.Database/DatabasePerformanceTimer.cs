// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Diagnostics;

namespace Beef.Data.Database
{
    /// <summary>
    /// Measures code and database command execution performance and provides a means to perform an <see cref="OnTimerStopped(Action{DatabaseCommand, Stopwatch})">action</see> with the result (e.g. log).
    /// </summary>
    /// <remarks>The code and database command execution includes the processing time for the returned <see cref="DatabaseRecord"/> results.</remarks>
    public class DatabasePerformanceTimer : PerformanceTimer
    {
        static private Action<DatabaseCommand, Stopwatch> _action;
        private readonly DatabaseCommand _databaseCommand;
 
        /// <summary>
        /// Sets the action to be performed when the underlying timer is <see cref="PerformanceTimer.Stop">stopped</see>.
        /// </summary>
        /// <param name="action">The action that is called when the timer is stopped.</param>
        public static void OnTimerStopped(Action<DatabaseCommand, Stopwatch> action)
        {
            _action = Check.NotNull(action, nameof(action));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePerformanceTimer"/> class.
        /// </summary>
        public DatabasePerformanceTimer(DatabaseCommand databaseCommand)
        {
            _databaseCommand = Check.NotNull(databaseCommand, nameof(databaseCommand));
        }

        /// <summary>
        /// Overrides the <see cref="PerformanceTimer.OnStopped"/> to invoke the action.
        /// </summary>
        protected override void OnStopped()
        {
            base.OnStopped();
            _action?.Invoke(_databaseCommand, base.Stopwatch);
        }
    }
}
