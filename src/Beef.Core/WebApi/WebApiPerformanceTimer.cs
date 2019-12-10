// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Diagnostics;

namespace Beef.WebApi
{
    /// <summary>
    /// Measures code and Web API HTTP execution performance and provides a means to perform an <see cref="OnTimerStopped(Action{string, Stopwatch})">action</see> with the result (e.g. log).
    /// </summary>
    /// <remarks>The code and Web API HTTP execution includes the processing time for the returned results.</remarks>
    public class WebApiPerformanceTimer : PerformanceTimer
    {
        static private Action<string, Stopwatch> _action;

        /// <summary>
        /// Sets the action to be performed when the underlying timer is <see cref="PerformanceTimer.Stop">stopped</see>.
        /// </summary>
        /// <param name="action">The action that is called when the timer is stopped (passes the <see cref="Url"/>).</param>
        public static void OnTimerStopped(Action<string, Stopwatch> action)
        {
            _action = Check.NotNull(action, nameof(action));
        }

#pragma warning disable CA1054 // Uri parameters should not be strings; by design.
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPerformanceTimer"/> class with a specified <see cref="Url"/>.
        /// </summary>
        /// <param name="url">The <see cref="Url"/>.</param>
        public WebApiPerformanceTimer(string url)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            Url = url;
        }

#pragma warning disable CA1056 // Uri properties should not be strings; by design.
        /// <summary>
        /// Gets or sets the URL string.
        /// </summary>
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Overrides the <see cref="PerformanceTimer.OnStopped"/> to invoke the action on stop.
        /// </summary>
        protected override void OnStopped()
        {
            base.OnStopped();
            _action?.Invoke(Url, base.Stopwatch);
        }
    }
}
