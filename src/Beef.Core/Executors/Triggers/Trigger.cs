// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Threading.Tasks;

namespace Beef.Executors.Triggers
{
    /// <summary>
    /// Represents the core (internal) trigger capabilities. Basically, this class enables <see cref="TriggerBase"/> and <see cref="TriggerBase{TInput}"/>.
    /// </summary>
    public abstract class Trigger : IDisposable
    {
        private readonly object _lock = new object();
        private ExecutionManager _executionManager;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trigger"/> class.
        /// </summary>
        internal Trigger() { }

        /// <summary>
        /// Configures the <see cref="Trigger"/> for a specific <see cref="Executors.ExecutionManager"/> instance.
        /// </summary>
        /// <param name="executionManager">The <see cref="Executors.ExecutionManager"/>.</param>
        internal void Configure(ExecutionManager executionManager)
        {
            lock (_lock)
            {
                if (_executionManager != null)
                    throw new InvalidOperationException($"Trigger '{InstanceId}' has already been configured for an ExecutionManager; instance can only be used once.");

                if (State != TriggerState.NotStarted)
                    throw new InvalidOperationException($"Trigger '{InstanceId}' Status is invalid; must be NotStarted.");

                _executionManager = executionManager;
            }
        }

        /// <summary>
        /// Gets the current <see cref="TriggerState"/>.
        /// </summary>
        public TriggerState State { get; private set; } = TriggerState.NotStarted;

        /// <summary>
        /// Gets the <see cref="TriggerResult"/>.
        /// </summary>
        public TriggerResult Result { get; private set; } = TriggerResult.Undetermined;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> that led to the <see cref="TriggerResult.Unsuccessful"/> <see cref="Result"/>.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the <see cref="Trigger"/> instance identifier.
        /// </summary>
        public Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Performs the <paramref name="action"/> where <see cref="ExecutionManager.IsInternalTracingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Trace(Action action)
        {
            _executionManager.Trace(action);
        }

        /// <summary>
        /// Starts the trigger.
        /// </summary>
        internal void Start()
        {
            if (State != TriggerState.NotStarted)
                throw new InvalidOperationException($"Trigger '{InstanceId}' Status is invalid; must have a value of NotStarted.");

            State = TriggerState.Running;
            Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' of Type '{GetType().Name}' started."));

            try
            {
                OnStarted();
            }
#pragma warning disable CA1031 // Do not catch general exception types; by-design, needs to fail!
            catch (Exception ex)
            {
                Logger.Default.Exception(ex, $"Trigger '{InstanceId}' encountered an exception whilst executing OnStarted: {ex.Message}");
                Stop(ex);
            }
#pragma warning restore CA1031
        }

        /// <summary>
        /// Trigger has started.
        /// </summary>
        protected abstract void OnStarted();

        /// <summary>
        /// Stops the trigger. (results in <see cref="TriggerResult.Successful"/>).
        /// </summary>
        /// <param name="exception">The <see cref="TriggerResult.Unsuccessful"/> <see cref="Exception"/>; <c>null</c> indicates <see cref="TriggerResult.Successful"/> (default).</param>
        protected void Stop(Exception exception = null)
        {
            StopExecution(true, exception);
        }

        /// <summary>
        /// Stops the execution.
        /// </summary>
        /// <param name="raiseOnStopExecution">Indicates whether to raise the <see cref="OnStopExecution"/> event.</param>
        /// <param name="exception">The <see cref="Exception"/> that led to the <see cref="TriggerResult.Unsuccessful"/>; <c>null</c> indicates <see cref="TriggerResult.Successful"/>.</param>
        internal void StopExecution(bool raiseOnStopExecution, Exception exception = null)
        {
            if (State == TriggerState.Stopping || State == TriggerState.Stopped)
                return;

            if (State != TriggerState.Running)
                throw new InvalidOperationException($"Trigger '{InstanceId}' Status is invalid; must be Running.");

            State = TriggerState.Stopping;
            Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' stopping."));

            Result = exception == null ? TriggerResult.Successful : TriggerResult.Unsuccessful;
            Exception = exception;

            if (Result == TriggerResult.Successful)
            {
                try
                {
                    OnStopped();
                }
#pragma warning disable CA1031 // Do not catch general exception types; by-design needs to catch-all.
                catch (Exception ex)
                {
                    Result = TriggerResult.Unsuccessful;
                    Exception = ex;
                    Logger.Default.Exception(ex, $"Trigger '{InstanceId}' encountered an exception whilst executing OnStopped/OnStopExecution: {ex.Message}");
                }
#pragma warning restore CA1031 
            }

            if (raiseOnStopExecution)
                OnStopExecution?.Invoke(this, EventArgs.Empty);

            State = TriggerState.Stopped;
            Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' stopped (result: {Result})."));
        }

        /// <summary>
        /// Occurs when the <see cref="Stop"/> is executed.
        /// </summary>
        internal event EventHandler OnStopExecution;

        /// <summary>
        /// Represents an opportunity to perform shutdown/cleanup work after the underlying <see cref="Trigger"/> has stopped.
        /// </summary>
        protected virtual void OnStopped() { }

        /// <summary>
        /// Raises the <see cref="OnRunAsync"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="completionCallback">An optional callback for post <see cref="Executor"/> <b>Run</b> notification/processing.</param>
        internal void Run(object sender, object args, Action completionCallback)
        {
            Check.NotNull(sender, nameof(sender));
            if (State == TriggerState.Stopped || State == TriggerState.Stopping)
                return;
            else if (State != TriggerState.Running)
                throw new InvalidOperationException($"Trigger '{InstanceId}' Status is invalid; must have a value of Running.");

            Trace(() => Logger.Default.Trace($"Trigger '{InstanceId}' triggering Executor to run."));
            Task.Run(async () => await (OnRunAsync?.Invoke(new TriggerEventArgs { Args = args, CompletionCallback = completionCallback })).ConfigureAwait(false)).Wait();
        }

        /// <summary>
        /// Action to run when the trigger initiated a <b>Run</b>.
        /// </summary>
        internal Func<TriggerEventArgs, Task> OnRunAsync { get; set; }

        /// <summary>
        /// Release/dispose of all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ExecutionManager"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Trigger()
        {
            Dispose(false);
        }
    }
}
