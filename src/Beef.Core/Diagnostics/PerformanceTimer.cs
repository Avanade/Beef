// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Diagnostics;

namespace Beef.Diagnostics
{
    /// <summary>
    /// Encapsulates a <see cref="Stopwatch"/> to measure performance.
    /// </summary>
    public abstract class PerformanceTimer : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTimer"/> class.
        /// </summary>
        protected PerformanceTimer()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        /// <summary>
        /// Gets the underlying <see cref="Stopwatch"/>.
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        /// <summary>
        /// Stops the <see cref="PerformanceTimer"/> and invokes <see cref="OnStopped"/>.
        /// </summary>
        public void Stop()
        {
            Stopwatch.Stop();
            OnStopped();
            Stopwatch = null;
        }

        /// <summary>
        /// Provides a means to add functionality on the <see cref="Stop"/>.
        /// </summary>
        protected virtual void OnStopped()
        {
        }

        /// <summary>
        /// Automatically invokes <see cref="Stop"/> where not previously initiated manually.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PerformanceTimer"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Stopwatch != null)
                Stop();
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~PerformanceTimer()
        {
            Dispose(false);
        }
    }
}
