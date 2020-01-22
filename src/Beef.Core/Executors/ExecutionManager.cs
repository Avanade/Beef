// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Executors.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Executors
{
    /// <summary>
    /// Provides the <see cref="Executor"/> create and run capabilities.
    /// </summary>
    public class ExecutionManager : IDisposable
    {
        /// <summary>
        /// Gets the interval (milliseconds) to wait for a process to complete.
        /// </summary>
        public const int ProcessWaitIntervalMilliseconds = 10;

        private readonly Func<Executor> _createExecutor;
        private ManualResetEvent _waiter;
        private readonly object _lock = new object();
        private readonly Dictionary<Guid, Executor> _executors = new Dictionary<Guid, Executor>();
        private Stopwatch _stopwatch;
        private Exception _ctorException;
        private bool _disposed;

        /// <summary>
        /// Indicates whether internal tracing is enabled (and output) for all executors.
        /// </summary>
        public static bool IsInternalTracingEnabledForAll { get; set; } = false;

        #region Create with Function

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <paramref name="executorWork"/> function once only (<see cref="SingleTrigger"/>) with no arguments.
        /// </summary>
        /// <param name="executorWork">The function to perform the underlying work (no <see cref="ExecutorBase"/> instance required).</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create(Func<ExecutorRunArgs, Task> executorWork)
        {
            return Create(() => new ExecutorFunc(executorWork), new SingleTrigger());
        }

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <paramref name="executorWork"/> function with a <paramref name="trigger"/> and no arguments.
        /// </summary>
        /// <param name="executorWork">The function to perform the underlying work (no <see cref="ExecutorBase"/> instance required).</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create(Func<ExecutorRunArgs, Task> executorWork, TriggerBase trigger)
        {
            return Create(() => new ExecutorFunc(executorWork), trigger);
        }

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <paramref name="executorWork"/> function once only (<see cref="SingleTrigger"/>) with arguments.
        /// </summary>
        /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
        /// <param name="executorWork">The function to perform the underlying work (no <see cref="ExecutorBase"/> instance required).</param>
        /// <param name="args">The arguments.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager CreateWithArgs<TArgs>(Func<ExecutorRunArgs<TArgs>, Task> executorWork, TArgs args)
        {
            return Create(() => new ExecutorFunc<TArgs>(executorWork), new SingleTrigger<TArgs>(args));
        }

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <paramref name="executorWork"/> function with a <paramref name="trigger"/> and arguments.
        /// </summary>
        /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
        /// <param name="executorWork">The function to perform the underlying work (no <see cref="ExecutorBase"/> instance required).</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager CreateWithArgs<TArgs>(Func<ExecutorRunArgs<TArgs>, Task> executorWork, TriggerBase<TArgs> trigger)
        {
            return Create(() => new ExecutorFunc<TArgs>(executorWork), trigger);
        }

        #endregion

        #region Create with ExecutorBase

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <see cref="ExecutorBase"/> created by the <paramref name="executorCreate"/> function once only (<see cref="SingleTrigger"/>) with no arguments.
        /// </summary>
        /// <param name="executorCreate">The function to create the executor with no arguments.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create(Func<ExecutorBase> executorCreate)
        {
            return Create(executorCreate, new SingleTrigger());
        }

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <see cref="ExecutorBase"/> created by the <paramref name="executorCreate"/> function with a <paramref name="trigger"/> and no arguments.
        /// </summary>
        /// <param name="executorCreate">The function to create the executor with no arguments.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create(Func<ExecutorBase> executorCreate, TriggerBase trigger)
        {
            if (executorCreate == null)
                throw new ArgumentNullException(nameof(executorCreate));

            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            return new ExecutionManager(trigger, () => executorCreate.Invoke(), async (executor, args) =>
            {
                var exe = executor as ExecutorBase;
                var ea = new ExecutorRunArgs(exe);
                await exe.RunWrapperAsync(ExceptionHandling.Stop, async () =>
                {
                    await exe.RunAsync(ea).ConfigureAwait(false);
                }, ea).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Create an <see cref="ExecutionManager"/> to run the <see cref="ExecutorBase{TArgs}"/> created by the <paramref name="executorCreate"/> function once only (<see cref="SingleTrigger{TArgs}"/>) with arguments.
        /// </summary>
        /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
        /// <param name="executorCreate">The function to create the executor with arguments.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TArgs>(Func<ExecutorBase<TArgs>> executorCreate, TArgs args)
        {
            return Create(executorCreate, new SingleTrigger<TArgs>(args));
        }

        /// <summary>
        /// Creates an <see cref="ExecutionManager"/> to run the <see cref="ExecutorBase{TArgs}"/> created by the <paramref name="executorCreate"/> function with a <paramref name="trigger"/> and arguments.
        /// </summary>
        /// <param name="executorCreate">The function to create the executor with arguments.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TArgs>(Func<ExecutorBase<TArgs>> executorCreate, TriggerBase<TArgs> trigger)
        {
            if (executorCreate == null)
                throw new ArgumentNullException(nameof(executorCreate));

            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            return new ExecutionManager(trigger, () => executorCreate.Invoke(), async (executor, args) =>
            {
                var exe = executor as ExecutorBase<TArgs>;
                exe.ExecutorArgs = ((TriggerEventArgs)args).Args;
                var ea = new ExecutorRunArgs<TArgs>(exe);
                await exe.RunWrapperAsync(ExceptionHandling.Stop, async () =>
                {
                    await exe.RunAsync(ea).ConfigureAwait(false);
                }, ea).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Creates an <see cref="ExecutionManager"/> to run the <see cref="CollectionExecutorBase{TColl, TItem}"/> created by the <paramref name="executorCreate"/> function once only (<see cref="SingleTrigger"/>) with no arguments.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
        /// <param name="executorCreate">The function to create the executor with no arguments.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TColl, TItem>(Func<CollectionExecutorBase<TColl, TItem>> executorCreate) where TColl : IEnumerable<TItem>
        {
            return Create<TColl, TItem>(executorCreate, new SingleTrigger());
        }

        /// <summary>
        /// Creates an <see cref="ExecutionManager"/> to run the <see cref="CollectionExecutorBase{TColl, TItem}"/> created by the <paramref name="executorCreate"/> function with a <paramref name="trigger"/> and no arguments.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
        /// <param name="executorCreate">The function to create the executor with no arguments.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>The <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TColl, TItem>(Func<CollectionExecutorBase<TColl, TItem>> executorCreate, TriggerBase trigger) where TColl : IEnumerable<TItem>
        {
            if (executorCreate == null)
                throw new ArgumentNullException(nameof(executorCreate));

            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            return new ExecutionManager(trigger, () => executorCreate.Invoke(), async (executor, args) =>
            {
                var exe = executor as CollectionExecutorBase<TColl, TItem>;
                await exe.RunWrapperAsync(ExceptionHandling.Stop, async () =>
                {
                    await exe.CompletionRunAsync(new ExecutorCompletionRunArgs(exe, await exe.RunItemsAsync(exe, await exe.RunCollectionAsync(new ExecutorCollectionRunArgs(exe)).ConfigureAwait(false)).ConfigureAwait(false))).ConfigureAwait(false);
                }, exe).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Creates an <see cref="ExecutionManager"/> to run the <see cref="CollectionExecutorBase{TColl, TItem, TArgs}"/> created by the <paramref name="executorCreate"/> function once only (<see cref="SingleTrigger{TArgs}"/>) with arguments.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
        /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
        /// <param name="executorCreate">The function to create the executor with arguments.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TColl, TItem, TArgs>(Func<CollectionExecutorBase<TColl, TItem, TArgs>> executorCreate, TArgs args) where TColl : IEnumerable<TItem>
        {
            return Create<TColl, TItem, TArgs>(executorCreate, new SingleTrigger<TArgs>(args));
        }

        /// <summary>
        /// Creates an <see cref="ExecutionManager"/> to run the <see cref="CollectionExecutorBase{TColl, TItem, TArgs}"/> created by the <paramref name="executorCreate"/> function with a <paramref name="trigger"/> and arguments.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The collection item <see cref="Type"/>.</typeparam>
        /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
        /// <param name="executorCreate">The function to create the executor with arguments.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>An <see cref="ExecutionManager"/>.</returns>
        public static ExecutionManager Create<TColl, TItem, TArgs>(Func<CollectionExecutorBase<TColl, TItem, TArgs>> executorCreate, TriggerBase<TArgs> trigger) where TColl : IEnumerable<TItem>
        {
            return new ExecutionManager(trigger, () => executorCreate.Invoke(), async (executor, args) =>
            {
                var exe = executor as CollectionExecutorBase<TColl, TItem, TArgs>;
                exe.ExecutorArgs = ((TriggerEventArgs)args).Args;
                await exe.RunWrapperAsync(ExceptionHandling.Stop, async () =>
                {
                    await exe.CompletionRunAsync(new ExecutorCompletionRunArgs(exe, await exe.RunItemsAsync(exe, await exe.RunCollectionAsync(new ExecutorCollectionRunArgs<TArgs>(exe)).ConfigureAwait(false)).ConfigureAwait(false))).ConfigureAwait(false);
                }, exe).ConfigureAwait(false);
            });
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/>.
        /// </summary>
        private ExecutionManager(Trigger trigger, Func<Executor> createExecutor, Action<Executor, object> executorAction)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            _createExecutor = createExecutor ?? throw new ArgumentNullException(nameof(createExecutor));
            ExecutorAction = executorAction ?? throw new ArgumentNullException(nameof(executorAction));

            // Wire up the Trigger to this ExecutionManager (one-to-one relationship).
            Trigger.Configure(this);
            Trigger.OnRunAsync = TriggerOnRunAsync;
            Trigger.OnStopExecution += (sender, e) => { StopExecution(ExecutionManagerStopReason.TriggerStop); };
        }

        /// <summary>
        /// Gets the <see cref="Trigger"/>.
        /// </summary>
        public Trigger Trigger { get; private set; }

        /// <summary>
        /// Gets the executor action as defined in the <b>Create</b> methods (i.e. the configured work).
        /// </summary>
        internal Action<Executor, object> ExecutorAction { get; private set; }

        /// <summary>
        /// Gets the <see cref="ExecutionState"/>.
        /// </summary>
        public ExecutionState Status { get; private set; } = ExecutionState.NotStarted;

        /// <summary>
        /// Gets the <see cref="ExecutionManagerStopReason"/>.
        /// </summary>
        public ExecutionManagerStopReason StopReason { get; private set; } = ExecutionManagerStopReason.NotStopped;

        /// <summary>
        /// Gets the <see cref="Executor"/> that initiated the stop (<see cref="ExecutionManagerStopReason.ExecutorStop"/> or <see cref="ExecutionManagerStopReason.ExecutorExceptionStop"/>).
        /// </summary>
        public Executor StopExecutor { get; private set; }

        /// <summary>
        /// Gets the last <see cref="Executor"/> that was executed.
        /// </summary>
        public Executor LastExecutor { get; private set; }

        /// <summary>
        /// Indicates whether the execution was stopped as a result of an <see cref="ExecutionException"/>.
        /// </summary>
        public bool HadExecutionException => StopReason == ExecutionManagerStopReason.ExecutionManagerException || StopReason == ExecutionManagerStopReason.TriggerException || StopReason == ExecutionManagerStopReason.ExecutorExceptionStop;

        /// <summary>
        /// Gets the <see cref="Exception"/> that led to the <see cref="Stop"/> (as per the <see cref="StopReason"/>).
        /// </summary>
        public Exception ExecutionException
        {
            get
            {
                return StopReason switch
                {
                    ExecutionManagerStopReason.ExecutionManagerException => _ctorException,
                    ExecutionManagerStopReason.TriggerException => Trigger.Exception,
                    ExecutionManagerStopReason.ExecutorExceptionStop => StopExecutor.Exception,
                    _ => null,
                };
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ExecutionManager"/> will <see cref="Executor.Stop"/> or <b>continue</b> when an unhandled <see cref="Exception"/> is encountered during an
        /// execution (see <see cref="Executor"/> <see cref="Executor.Result"/> of <see cref="ExecutorResult.Unsuccessful"/>). Defaults to <see cref="ExceptionHandling.Continue"/>. 
        /// </summary>
        /// <remarks>When an <see cref="Executor"/> is <see cref="ExecutionState.Stopped"/> and was <see cref="ExecutorResult.Unsuccessful"/> and the the <see cref="Beef.Executors.ExceptionHandling"/> is
        /// <see cref="ExceptionHandling.Stop"/> then the <see cref="ExecutionManager"/> <see cref="Stop"/> will be invoked; which in turn stops the trigger and then waits for any outstanding
        /// execution work to complete.</remarks>
        public ExceptionHandling ExceptionHandling { get; set; } = ExceptionHandling.Continue;

        /// <summary>
        /// Gets the <see cref="ExecutionManager"/> instance identifier.
        /// </summary>
        public Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the number of <see cref="Executor"/> instances (count) that were created during the lifetime of the <see cref="ExecutionManager"/>.
        /// </summary>
        public long ExecutorCount { get; private set; } = 0;

        /// <summary>
        /// Indicates whether internal tracing is enabled (and output) overridding the <see cref="IsInternalTracingEnabledForAll"/> value.
        /// </summary>
        public bool? IsInternalTracingEnabled { get; set; }

        /// <summary>
        /// Performs the <paramref name="action"/> where <see cref="IsInternalTracingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="action">The action.</param>
        internal void Trace(Action action)
        {
            if ((IsInternalTracingEnabled.HasValue && IsInternalTracingEnabled.Value) || (!IsInternalTracingEnabled.HasValue && IsInternalTracingEnabledForAll))
                action?.Invoke();
        }

        /// <summary>
        /// Occurs after executing each <see cref="Executor"/> <see cref="ExecutorRunType"/> has run. This is intended for adding the likes of standardised logging only.
        /// This is <b>NOT</b> intended for the execution of specific processing logic as unintended side-effects may occur. Defaults to the <see cref="ExecutionManagerLogger.Default"/>
        /// <see cref="ExecutionManagerLogger.ExecutionManagerLogger"/>.
        /// </summary>
        /// <remarks>Any exceptions thrown during the processing of the <b>Run</b> will be captured and reported by <see cref="IExecutorBaseArgs.Exception"/>, then immediately 
        /// thrown to bubble up into the standard exception processing within the <see cref="Executor"/>. The <see cref="PerRunType"/> should not throw exceptions as these will
        /// be automatically swallowed and standard processing will continue.</remarks>
        public Action<IExecutorArgs> PerRunType { get; set; } = ExecutionManagerLogger.Default.LogExecutionRun;

        /// <summary>
        /// Executes the <see cref="ExecutionManager.PerRunType"/> action.
        /// </summary>
        /// <param name="args">The <see cref="IExecutorArgs"/>.</param>
        internal void OnPerRunType(IExecutorArgs args)
        {
            try
            {
                PerRunType?.Invoke(args);
            }
#pragma warning disable CA1031 // Do not catch general exception types; by-design logs and swallows.
            catch (Exception ex)
            {
                Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' AfterRun action threw an Exception (this exception was swallowed by design and otherwise not logged): {ex.ToString()}"));
            }
#pragma warning restore CA1031
        }

        /// <summary>
        /// Indicates whether work is currently being performed (<see cref="ExecutorBase.OnRunAsync(ExecutorRunArgs)"/> 
        /// or <see cref="ExecutorBase{TArgs}.OnRunAsync(ExecutorRunArgs{TArgs})"/>) versus waiting for the <see cref="Triggers.Trigger"/> to initiate/fire.
        /// </summary>
        public bool IsDoingWork
        {
            get
            {
                lock (_lock)
                {
                    return _executors.Count > 0;
                }
            }
        }

        /// <summary>
        /// Creates and starts a <see cref="Task"/> to run the underlying <see cref="Executor"/> (asynchronously).
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task RunAsync()
        {
            return RunAsync(CancellationToken.None);
        }

        /// <summary>
        /// Runs the underlying <see cref="Executor"/> (synchronously).
        /// </summary>
        /// <returns>The <see cref="ExecutionManager"/>.</returns>
        public ExecutionManager Run()
        {
            RunAsync().Wait();
            return this;
        }

        /// <summary>
        /// Creates and starts a <see cref="Task"/> to run the underlying <see cref="Executor"/> with a specified <see cref="CancellationToken"/> (asynchronously).
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task RunAsync(CancellationToken cancellationToken)
        {
            Check.NotNull(cancellationToken, nameof(cancellationToken));
            Check.IsFalse(cancellationToken.IsCancellationRequested, "CancellationToken must not be in a cancelled state.");
            _stopwatch = Stopwatch.StartNew();

            return Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (_waiter != null)
                            throw new InvalidOperationException("An Executor can only be Run once; create a new instance.");

                        _waiter = new ManualResetEvent(false);
                    }

                    cancellationToken.Register(() => CancellationTokenStop());
                    Start();
                    _waiter.WaitOne();
                    Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' exiting."));
                }
                finally
                {
                    if (_waiter != null)
                    {
                        _waiter.Dispose();
                        _waiter = null;
                    }
                }
            });
        }

        /// <summary>
        /// Starts the execution.
        /// </summary>
        internal void Start()
        {
            Status = ExecutionState.Started;
            Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' started."));
            Trigger.Start();
        }

        /// <summary>
        /// Stops the execution.
        /// </summary>
        /// <remarks>Stops the trigger and then waits for any outstanding execution work to complete.</remarks>
        public void Stop()
        {
            Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' Stop invoked."));
            StopExecution(ExecutionManagerStopReason.ExecutionManagerStop).Wait();
        }

        /// <summary>
        /// Stops the execution as a result of a cancel.
        /// </summary>
        private void CancellationTokenStop()
        {
            Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' CancellationToken Stop invoked."));
            StopExecution(ExecutionManagerStopReason.ExecutionManagerCancellationTokenStop);
        }

        /// <summary>
        /// Stoped the execution as a result of a <see cref="Executor.StopExecutionManager"/>.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal void StopExecutionManager(Executor executor)
        {
            Trace(() => Logger.Default.Trace($"Executor '{executor.InstanceId}' StopExecutionManager invoked."));
            StopExecution(ExecutionManagerStopReason.ExecutorStop, executor);
        }

        /// <summary>
        /// Stops the execution.
        /// </summary>
        private Task StopExecution(ExecutionManagerStopReason stopReason, Executor stopExecutor = null)
        {
            if (Status == ExecutionState.NotStarted)
                throw new InvalidOperationException($"ExecutionManager '{InstanceId}' must be in a Started or Running state to Stop.");
            else if (Status == ExecutionState.Stopping || Status == ExecutionState.Stopped)
                return Task.CompletedTask;

            Status = ExecutionState.Stopping;
            Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' stopping."));

            // Stop Trigger first so no new Executors get started.
            if (Trigger.State == TriggerState.Running)
            {
                Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' requesting Trigger '{Trigger.InstanceId}' to Stop."));
                Trigger.StopExecution(false);
            }

            // By design: needs to run on seperate thread to allow current Execution to complete.
            return Task.Run(() =>
            {
                // Tell each running executor to stop.
                List<Task> tasks = new List<Task>();
                lock (_lock)
                {
                    var exes = _executors.Select(x => x.Value).ToArray();
                    if (exes.Length > 0)
                        Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' requesting {exes.Length} Executor(s) to Stop."));

                    // Need the executors moved to an array first as the StopAsync will update the _executors dictionary.
                    foreach (var exe in _executors.Select(x => x.Value).ToArray())
                    {
                        tasks.Add(exe.StopAsync());
                    }
                }

                // Need to wait until the current execution has completed.
                if (tasks.Count > 0)
                {
                    Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' waiting for {tasks.Count} Executor(s) to Stop."));
                    try
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
#pragma warning disable CA1031 // Do not catch general exception types; by-design, intended to log and swallow.
                    catch (Exception ex)
                    {
                        // Too late to do anything about it; just keep on keeping on.
                        Logger.Default.Exception(ex, $"ExecutionManager '{InstanceId}' caught (and swallowed) an exception whilst waiting for {tasks.Count} Executor(s) to Stop.");
                    }
#pragma warning restore CA1031

                    // This is a safety net; should not have any outstanding Executors at this point.
                    while (_executors.Count > 0)
                    {
                        Task.Delay(ProcessWaitIntervalMilliseconds).Wait();
                    }
                }

                _stopwatch.Stop();
                Status = ExecutionState.Stopped;
                StopReason = (stopReason == ExecutionManagerStopReason.TriggerStop && Trigger.Exception != null) ? ExecutionManagerStopReason.TriggerException : stopReason;
                StopExecutor = stopExecutor;
                Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' stopped (reason: {StopReason}, executors: {ExecutorCount}, elapsed: {_stopwatch.Elapsed})."));

                // Signals work stopped.
                _waiter.Set();
            });
        }

        /// <summary>
        /// Wraps the <b>Run</b> <paramref name="func"/> to ensure standard logic is applied.
        /// </summary>
        private Task RunWrapperAsync(Func<Task> func, Executor executor)
        {
            return ExecutorInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (Status == ExecutionState.Stopped)
                            throw new InvalidOperationException("RunWrapper cannot be invoked once the ExecutionManager has Stopped.");

                        if (_executors.Count == 0)
                            Status = ExecutionState.Running;

                        Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' created Executor '{executor.InstanceId}'."));
                        _executors.Add(executor.InstanceId, executor);
                    }

                    await func().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lock (_lock)
                    {
                        if (_executors.Count == 1)
                            Logger.Default.Exception(ex, $"ExecutionManager '{InstanceId}' encountered an unexpected exception: {ex.ToString()}.");

                        ExecutorStopped(executor);
                        throw;
                    }
                }
                finally
                {
                    LastExecutor = executor;
                }
            });
        }

        /// <summary>
        /// Call back to advise that the Executor has stopped.
        /// </summary>
        /// <param name="executor">The executor.</param>
        internal void ExecutorStopped(Executor executor)
        {
            lock (_lock)
            {
                Trace(() => Logger.Default.Trace($"ExecutionManager '{InstanceId}' acknowledges Executor '{executor.InstanceId}' stopped."));
                _executors.Remove(executor.InstanceId);
            }
        }

        /// <summary>
        /// Create and run an executor as a result of the trigger firing.
        /// </summary>
        private async Task TriggerOnRunAsync(TriggerEventArgs args)
        {
            if (Status != ExecutionState.Started && Status != ExecutionState.Running)
                return;

            Executor executor = null;

            try
            {
                executor = _createExecutor() ?? throw new InvalidOperationException($"ExecutionManager '{InstanceId}' invoked CreateExecutor function; it must return an Executor instance.");
                executor.Configure(this, args.CompletionCallback);
                lock (_lock)
                {
                    ExecutorCount++;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types; by-design to log and bubble internally.
            catch (Exception ex)
            {
                Logger.Default.Exception(ex, $"ExecutionManager '{InstanceId}' unable to create Executor instance: {ex.Message}.");
                _ctorException = ex;
                await StopExecution(ExecutionManagerStopReason.ExecutionManagerException).ConfigureAwait(false);
                return;
            }
#pragma warning restore CA1031

            await RunWrapperAsync(() => executor.RunExecutorAsync(args), executor).ConfigureAwait(false);

            if (executor.Result == ExecutorResult.Unsuccessful && ExceptionHandling == ExceptionHandling.Stop)
            {
                await StopExecution(ExecutionManagerStopReason.ExecutorExceptionStop, executor).ConfigureAwait(false);
                return;
            }
        }

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

            if (disposing)
            {
                if (_waiter != null)
                    _waiter.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~ExecutionManager()
        {
            Dispose(false);
        }
    }
}