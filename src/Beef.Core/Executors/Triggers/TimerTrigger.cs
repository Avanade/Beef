// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Threading;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents a <see cref="Timer"/>-based trigger; i.e. will always trigger at the specified <see cref="Interval"/> regardless of execution time. As such there is a
    /// possibility of execution concurrency as a result; to avoid concurrency use the <see cref="SlidingTimerTrigger"/>.
    /// </summary>
    public class TimerTrigger : TriggerBase
    {
        /// <summary>
        /// Defines a one second interval.
        /// </summary>
        public const int OneSecondInterval = 1000;

        /// <summary>
        /// Defines a one minute interval.
        /// </summary>
        public const int OneMinuteInterval = 60 * OneSecondInterval;

        /// <summary>
        /// Defines a one hour interval.
        /// </summary>
        public const int OneHourInterval = 60 * OneMinuteInterval;

        /// <summary>
        /// Defines a one day interval.
        /// </summary>
        public const int OneDayInterval = 24 * OneHourInterval;

        private readonly object _lock = new object();
        private readonly TimerTriggerResult _triggerResult = new TimerTriggerResult { IsExecutorRunEnabled = true };
        private Timer _timer;
        private long _iterations = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTrigger"/> class with a specified <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">The time interval, in milliseconds, between events (defaults to <see cref="OneMinuteInterval"/>).</param>
        /// <param name="maxIterations">Defines the maximum number of iterations that the trigger will initiate an execution before stopping; <c>null</c> indicates infinite.</param>
        public TimerTrigger(int interval = OneMinuteInterval, long? maxIterations = null)
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
        public long? MaxIterations { get; private set; }

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
                lock (_lock)
                {
                    var tr = OnTrigger() ?? throw new InvalidOperationException("OnTrigger override must return a TimerTriggerResult instance.");
                    Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' timer has fired; Executor run enabled: {tr.IsExecutorRunEnabled}."));
                    if (tr.IsExecutorRunEnabled)
                        Run();

                    if (MaxIterations.HasValue && ++_iterations >= MaxIterations.Value)
                    {
                        Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' maximum number of iterations reached; initiating trigger stop."));
                        Stop();
                    }
                }
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
            return _triggerResult;
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
