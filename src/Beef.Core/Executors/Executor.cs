// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the core functionality for an executor; to implement inherit from <see cref="ExecutorBase"/> or <see cref="ExecutorBase{TArgs}"/>.
    /// </summary>
    public abstract class Executor
    {
        private ExecutionManager? _executionManager;
        private readonly object _lock = new object();
        private readonly Stack<object> _stack = new Stack<object>();
        private Action? _triggerCallback;
        private Stopwatch? _stopwatch;

        /// <summary>
        /// Repressents a <see cref="Executor.RunWrapperAsync(ExceptionHandling, Func{Task}, object, bool)"/> result.
        /// </summary>
        internal struct RunWrapperResult
        {
            public bool WasSuccessful;
            public Exception Exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Executor"/> class.
        /// </summary>
        internal Executor() { }

        /// <summary>
        /// Configures the <see cref="Executor"/> for a specific <see cref="ExecutionManager"/> instance.
        /// </summary>
        /// <param name="executionManager">The <see cref="ExecutionManager"/>.</param>
        /// <param name="triggerCallback">An optional callback for post <b>Run</b> notification/processing within the invoking <see cref="Triggers.Trigger"/>.</param>
        internal void Configure(ExecutionManager executionManager, Action? triggerCallback)
        {
            lock (_lock)
            {
                if (_executionManager != null)
                    throw new InvalidOperationException($"Executor '{InstanceId}' has already been configured for an ExecutionManager; instance can only be used once.");

                if (State != ExecutionState.NotStarted)
                    throw new InvalidOperationException($"Executor '{InstanceId}' Status is invalid; must be NotStarted.");

                _executionManager = executionManager;
                _triggerCallback = triggerCallback;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ExecutionState"/>.
        /// </summary>
        public ExecutionState State { get; private set; } = ExecutionState.NotStarted;

        /// <summary>
        /// Gets the <see cref="ExecutorResult"/>.
        /// </summary>
        public ExecutorResult Result { get; private set; } = ExecutorResult.Undetermined;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> that led to the <see cref="ExecutorResult.Unsuccessful"/> <see cref="Result"/>.
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Gets the <see cref="Executor"/> instance identifier.
        /// </summary>
        public Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="Executor"/> argument where supported; otherwise, <c>null</c>.
        /// </summary>
        public object? ExecutorArgs { get; internal set; }

        /// <summary>
        /// Gets or sets a return code (represents a means to return a code to the invoker).
        /// </summary>
        public int ReturnCode { get; set; }

        /// <summary>
        /// Executes the <see cref="ExecutionManager"/> <see cref="ExecutionManager.PerRunType"/> function.
        /// </summary>
        /// <param name="args">The <see cref="IExecutorArgs"/>.</param>
        internal void OnPerRunType(IExecutorArgs args)
        {
            _executionManager!.OnPerRunType(args);
        }

        /// <summary>
        /// Performs the <paramref name="action"/> where <see cref="ExecutionManager.IsInternalTracingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Trace(Action action)
        {
            _executionManager!.Trace(action);
        }

        #region Run

        /// <summary>
        /// Runs the <see cref="Executor"/> asynchronously.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        internal Task RunExecutorAsync(object? args)
        {
            return Task.Run(async () =>
            {
                Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' of Type '{GetType().Name}' started."));
                _stopwatch = Stopwatch.StartNew();
                State = ExecutionState.Started;
                if (!(await RunWrapperAsync(ExceptionHandling.Stop, async () => await StartedAsync().ConfigureAwait(false), this, false).ConfigureAwait(false)).WasSuccessful)
                    return;

                if (State != ExecutionState.Started)
                    return;

                Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' running."));
                State = ExecutionState.Running;
                _executionManager!.ExecutorAction(this, args);
            });
        }

        /// <summary>
        /// Wraps the <b>Run</b> <paramref name="func"/> to ensure standard logic is applied.
        /// </summary>
        /// <param name="exceptionHandling">The <see cref="ExceptionHandling"/> option.</param>
        /// <param name="func">The underlying function to execute.</param>
        /// <param name="ecd">The execution context data.</param>
        /// <param name="stopWhenStackEmpty">Indicates whether to stop execution when the execution stack is empty.</param>
        internal async Task<RunWrapperResult> RunWrapperAsync(ExceptionHandling exceptionHandling, Func<Task> func, object ecd, bool stopWhenStackEmpty = true)
        {
            var result = new RunWrapperResult();
            await ExecutorInvoker.Default.InvokeAsync(this, async () =>
            {
#pragma warning disable CS4014, CA1031  // Justification: not awaited by design as the Stop needs to run/complete outside of this current execution task/thread.
                try
                {
                    lock (_lock)
                    {
                        if (State == ExecutionState.Stopped)
                            throw new InvalidOperationException($"RunWrapper cannot be invoked once the Executor '{InstanceId}' has Stopped.");

                        _stack.Push(ecd);
                    }

                    await func().ConfigureAwait(false);
                    result.WasSuccessful = true;
                }
                catch (ExecutorStopException esex)
                {
                    if (esex.StopExecutionManager)
                        _executionManager!.StopExecutionManager(this);
                    else
                    {
                        if (State != ExecutionState.Stopping && State != ExecutionState.Stopped)
                        {
                            Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' StopExecution invoked."));
                            StopAsync(esex.InnerException); // Not awaited by design!
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    if (ecd is ExecutorArgsBase args)
                        args.SetException(ex);

                    if (exceptionHandling == ExceptionHandling.Stop)
                    {
                        Logger.Default.Exception(ex, $"Executor '{InstanceId}' stop as a result of an unhandled exception: {ex.Message}");
                        StopAsync(ex); // Not awaited by design!
                    }
                    else
                        Logger.Default.Exception(ex, $"Executor '{InstanceId}' continued with an unhandled exception: {ex.Message}");
                }
                finally
                {
                    lock (_lock)
                    {
                        _stack.Pop();
                    }

                    if (_stack.Count == 0 && stopWhenStackEmpty)
                        StopAsync(); // Not awaited by design!
                }
#pragma warning restore CS4014
            }).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs each of the items in the collection.
        /// </summary>
        internal async Task<bool> RunItemsAsync<TColl, TItem>(CollectionExecutorCore<TColl, TItem> executor, TColl coll) where TColl : IEnumerable<TItem>
        {
            if (coll == null)
                return true;

            try
            {
                if (executor.IsItemRunParallelismEnabled)
                {
                    var po = new ParallelOptions();
                    if (executor.MaxDegreeOfParallelism.HasValue && executor.MaxDegreeOfParallelism.Value > 0)
                        po.MaxDegreeOfParallelism = executor.MaxDegreeOfParallelism.Value;

                    var plr = Parallel.ForEach<TItem>(coll, po, async (item, state, index) =>
                    {
                        // Before starting another unit of work make sure we are still in a running state.
                        if (state.IsStopped)
                            return;

                        // Where not running (stop requested) check and see whether complete (full exection) of collection is required.
                        if (State != ExecutionState.Running && !executor.CompleteFullExecutionOnStop)
                        {
                            lock (_lock)
                            {
                                state.Stop();
                                return;
                            }
                        }

                        // Execute the unit of work.
                        var ea = new ExecutorItemRunArgs<TItem>(this, index, item);
                        await RunWrapperAsync(executor.ItemExceptionHandling, async () => await executor.RunItemAsync(ea).ConfigureAwait(false), ea).ConfigureAwait(false);
                    });

                    return plr.IsCompleted;
                }
                else
                {
                    // Execute synchronously.
                    long index = 0;
                    foreach (TItem item in coll)
                    {
                        // Before starting another unit of work make sure we are still in a running state. 
                        if (!executor.CompleteFullExecutionOnStop && State != ExecutionState.Running)
                            return false;

                        // Execute the unit of work.
                        var ea = new ExecutorItemRunArgs<TItem>(this, index++, item);
                        await RunWrapperAsync(executor.ItemExceptionHandling, async () => await executor.RunItemAsync(ea).ConfigureAwait(false), ea).ConfigureAwait(false);
                    }

                    return true;
                }
            }
            catch (ExecutorStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var args = new ExecutorCollectionIterateArgs(this);
                args.SetException(ex);
                _executionManager!.OnPerRunType(args);
                throw;
            }
        }

        #endregion

        #region Stop

#pragma warning disable CA1822 // Mark members as static; by-design - want to give illusion they are instance oriented.
        /// <summary>
        /// Stops the executor; being this currently executing executor.
        /// </summary>
        /// <param name="exception">The <see cref="ExecutorResult.Unsuccessful"/> <see cref="Exception"/>; <c>null</c> indicates <see cref="ExecutorResult.Successful"/> (default).</param>
        /// <exception cref="ExecutorStopException">The <see cref="ExecutorStopException"/> is for internal use only: do <b>NOT</b> catch and swallow this exception as it is used internally to manage the stop.</exception>
        public void Stop(Exception? exception = null)
        {
            throw new ExecutorStopException(false, exception);
        }

        /// <summary>
        /// Stops the executor and underlying <see cref="ExecutionManager"/> by invoking the <see cref="ExecutionManager.Stop"/>.
        /// </summary>
        /// <param name="exception">The <see cref="ExecutorResult.Unsuccessful"/> <see cref="Exception"/>; <c>null</c> indicates <see cref="ExecutorResult.Successful"/> (default).</param>
        /// <exception cref="ExecutorStopException">The <see cref="ExecutorStopException"/> is for internal use only: do <b>NOT</b> catch and swallow this exception as it is used internally to manage the stop.</exception>
        public void StopExecutionManager(Exception? exception = null)
        {
            throw new ExecutorStopException(true, exception);
        }
#pragma warning restore CA1822

        /// <summary>
        /// Stops the execution.
        /// </summary>
        /// <param name="exception">The <see cref="ExecutorResult.Unsuccessful"/> <see cref="Exception"/>; <c>null</c> indicates <see cref="ExecutorResult.Successful"/> (default).</param>
        internal async Task StopAsync(Exception? exception = null)
        {
            lock (_lock)
            {
                if (State == ExecutionState.Stopping || State == ExecutionState.Stopped)
                    return;

                State = ExecutionState.Stopping;
                Result = exception == null ? ExecutorResult.Successful : ExecutorResult.Unsuccessful;
                Exception = exception;
            }

            Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' stopping."));

            // Need to wait until the current execution has completed.
            while (true)
            {
                if (_stack.Count < 1)
                    break;

                await Task.Delay(ExecutionManager.ProcessWaitIntervalMilliseconds).ConfigureAwait(false);
            }

            // Run the OnStoppedAsync work.
            var rwr = await RunWrapperAsync(ExceptionHandling.Continue, async () => await StoppedAsync().ConfigureAwait(false), this, false).ConfigureAwait(false);
            if (!rwr.WasSuccessful && Result != ExecutorResult.Unsuccessful)
            {
                Result = ExecutorResult.Unsuccessful;
                Exception = rwr.Exception;
            }

            // Given the processing for the Executor has completed the (optional) trigger callback can be invoked.
            if (rwr.WasSuccessful && _triggerCallback != null && _executionManager!.Trigger.State == Triggers.TriggerState.Running)
            {
                try
                {
                    Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' initiating requested Trigger '{_executionManager.Trigger.InstanceId}' callback."));
                    _triggerCallback.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Default.Exception(ex, $"Executor '{InstanceId}' requested Trigger '{_executionManager.Trigger.InstanceId}' callback encountered an unexpected exception: {ex.ToString()}.");
                    _executionManager.Stop();
                }
            }

            State = ExecutionState.Stopped;
            _stopwatch!.Stop();
            Trace(() => Logger.Default.Trace($"Executor '{InstanceId}' stopped (result: {Result}, elapsed: {_stopwatch.Elapsed})."));
            _executionManager!.ExecutorStopped(this);
        }

        #endregion

        #region Started/Stopped

        /// <summary>
        /// Invokes <see cref="OnStartedAsync"/>.
        /// </summary>
        internal Task StartedAsync()
        {
            return OnStartedAsync();
        }

        /// <summary>
        /// Represents an opportunity to perform pre-work (initialization) before the primary processing starts.
        /// </summary>
        /// <remarks>Any <see cref="Exception"/> thrown will result in an immediate <see cref="Executor.Stop(Exception)"/> for the current execution (no further processing will occur).</remarks>
        protected virtual Task OnStartedAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invokes <see cref="OnStoppedAsync"/>.
        /// </summary>
        internal Task StoppedAsync()
        {
            return OnStoppedAsync();
        }

        /// <summary>
        /// Represents an opportunity to perform post-work (shutdown/cleanup) after the primary processing has stopped (either successfully or unsuccessfully).
        /// </summary>
        /// <remarks>Any <see cref="Exception"/> thrown will be handled as per the <see cref="ExecutionManager.ExceptionHandling"/> setting.</remarks>
        protected virtual Task OnStoppedAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
