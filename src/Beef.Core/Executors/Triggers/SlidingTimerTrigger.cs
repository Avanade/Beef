// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Threading;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents a sliding <see cref="Timer"/>-based trigger; i.e. will pause for the specified <see cref="Interval"/> between each execution, and as such there is no chance
    /// of execution concurrency (see <see cref="TimerTrigger"/>).
    /// </summary>
    public class SlidingTimerTrigger : TriggerBase
    {
        private readonly object _lock = new object();
        private readonly TimerTriggerResult _defaulTriggertResult = new TimerTriggerResult();
        private Timer _timer;
        private int _iterations = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingTimerTrigger"/> class.
        /// </summary>
        /// <param name="interval">The time interval, in milliseconds, between events (defaults to <see cref="TimerTrigger.OneMinuteInterval"/>).</param>
        /// <param name="maxIterations">Defines the maximum number of iterations that the trigger will initiate an execution before stopping; <c>null</c> indicates infinite.</param>
        public SlidingTimerTrigger(int interval = TimerTrigger.OneMinuteInterval, int? maxIterations = null)
        {
            if (interval < 0)
                throw new ArgumentException("The interval value must be greater than or equal to 0.", nameof(interval));

            Interval = interval;

            Check.IsTrue(!maxIterations.HasValue || (maxIterations.HasValue && maxIterations.Value > 0), nameof(maxIterations), "Max iterations where specified must be greater than 0.");
            MaxIterations = maxIterations;
        }

        /// <summary>
        /// Gets the time interval, in milliseconds, between events.
        /// </summary>
        public int Interval { get; private set; }

        /// <summary>
        /// Gets the maximum number of iterations that the trigger will initiate an execution before stopping; <c>null</c> indicates infinite.
        /// </summary>
        public int? MaxIterations { get; private set; }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        protected override void OnStarted()
        {
            // Wait before running to allow initialisation activities to conclude.
            _timer = new Timer((a) => OnTimer(), null, 0, Interval);
        }

        /// <summary>
        /// Timer triggered; run the executor.
        /// </summary>
        private void OnTimer()
        {
            try
            {
                TimerTriggerResult tr = null;
                lock (_lock)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    tr = OnTrigger() ?? throw new InvalidOperationException("OnTrigger override must return a TimerTriggerResult instance.");
                    Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' timer has fired; Executor run enabled: {tr.IsExecutorRunEnabled}."));
                }

                if (tr.IsExecutorRunEnabled)
                    Run(() => RestartTimer());
                else
                    RestartTimer();
            }
            catch (Exception ex)
            {
                Logger.Default.Exception(ex, $"Trigger '{InstanceId}' encountered an exception whilst executing OnTimer: {ex.Message}");
                Stop(ex);
            }
        }

        /// <summary>
        /// Opportunity to perform additional processing to determine whether the <see cref="Executor"/> should be run as a result of the timer.
        /// </summary>
        /// <returns>A <see cref="TimerTriggerResult"/>.</returns>
        /// <remarks>Defaults with <see cref="TimerTriggerResult.IsExecutorRunEnabled"/> set to <c>true</c>.</remarks>
        protected virtual TimerTriggerResult OnTrigger()
        {
            return _defaulTriggertResult;
        }

        /// <summary>
        /// Restarts the timer.
        /// </summary>
        private void RestartTimer()
        {
            lock (_lock)
            {
                if (this.State == TriggerState.Running)
                {
                    if (MaxIterations.HasValue && ++_iterations >= MaxIterations.Value)
                    {
                        Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' maximum number of iterations reached; initiating trigger stop."));
                        Stop();
                    }
                    else
                    {
                        this.Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' restarting timer."));
                        _timer.Change(Interval, Interval);
                    }
                }
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        protected override void OnStopped()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
