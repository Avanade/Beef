// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Hosting
{
    /// <summary>
    /// Represents a base class for an <see cref="IHostedService"/> based on a <see cref="Interval"/> to <see cref="Execute(object?)"/> work.
    /// </summary>
    /// <remarks>Each timer-based invocation of the <see cref="ExecuteAsync(CancellationToken)"/> will be managed witin the context of a new Dependency Injection (DI)
    /// <see cref="ServiceProviderServiceExtensions.CreateScope">scope</see>.
    /// <para>A <see cref="OneOffIntervalAdjust(TimeSpan, bool)"/> is provided to enable a one-off change to the timer where required.</para></remarks>
    public abstract class TimerHostedServiceBase : IHostedService, IDisposable
    {
        private static readonly Random _random = new();

        private readonly object _lock = new();
        private readonly Func<ExecutionContext>? _overrideExecutionContext;
        private string? _name;
        private CancellationTokenSource? _cts;
        private Timer? _timer;
        private DateTime _lastExecuted = DateTime.MinValue;
        private TimeSpan? _oneOffInterval;
        private Task? _executeTask;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerHostedServiceBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>; defaults to instance from the <paramref name="serviceProvider"/> where not specified.</param>
        /// <param name="overrideExecutionContext">An optional opportunity to create a new <see cref="ExecutionContext"/> instance for the <see cref="ExecuteAsync(CancellationToken)">execution</see> 
        /// <see cref="ServiceProviderServiceExtensions.CreateScope">scope</see>; by default will get instance from the <paramref name="serviceProvider"/>.</param>
        public TimerHostedServiceBase(IServiceProvider serviceProvider, ILogger logger, IConfiguration? config = null, Func<ExecutionContext>? overrideExecutionContext = null)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Config = config ?? ServiceProvider.GetService<IConfiguration>();
            _overrideExecutionContext = overrideExecutionContext;
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider;

        /// <summary>
        /// Gets the <see cref="IConfiguration"/>.
        /// </summary>
        protected IConfiguration Config { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the service name (used for the likes of configuration and logging).
        /// </summary>
        public virtual string ServiceName => _name ??= GetType().Name;

        /// <summary>
        /// Gets or sets the timer interval <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>Defaults to one hour.</remarks>
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Provides an opportunity to make a one-off change to the underlying timer to trigger using the specified interval.
        /// </summary>
        /// <param name="interval">The one-off interval.</param>
        /// <param name="leaveWhereTimeRemainingIsLess">Indicates whether to not adjust the time where the time remaining is less than the interval specified.</param>
        protected void OneOffIntervalAdjust(TimeSpan interval, bool leaveWhereTimeRemainingIsLess = false)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                // Where already executing save the one-off value and use when ready.
                if (_executeTask != null)
                    _oneOffInterval = interval;
                else
                {
                    _oneOffInterval = null;

                    // Where less time remaining than specified and requested to leave then do nothing.
                    if (leaveWhereTimeRemainingIsLess && (DateTime.UtcNow - _lastExecuted) < interval)
                        return;

                    _timer?.Change(interval, interval);
                }
            }
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>The underlying timer start is the <see cref="Interval"/> plus a randomized value between zero and one thousand milliseconds; this will minimize multiple services within the host all starting at once.</remarks>
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogDebug($"{ServiceName} service started. Timer interval {Interval}.");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _timer = new Timer(Execute, null, Interval.Add(TimeSpan.FromMilliseconds(_random.Next(0, 1000))), Interval);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs the internal execution.
        /// </summary>
        private void Execute(object? state)
        {
            // Stop the timer as no more work should be initiated until after complete.
            lock (_lock)
            {
                _timer!.Change(Timeout.Infinite, Timeout.Infinite);
                Logger.LogTrace($"{ServiceName} execution triggered by timer.");

                _executeTask = Task.Run(async () => await ScopedExecuteAsync(_cts!.Token).ConfigureAwait(false));
            }

            _executeTask.Wait();

            // Restart the timer.
            lock (_lock)
            {
                _executeTask = null;
                _lastExecuted = DateTime.UtcNow;

                if (_cts!.IsCancellationRequested)
                    return;

                var interval = _oneOffInterval ?? Interval;
                _oneOffInterval = null;

                Logger.LogTrace($"{ServiceName} execution completed. Retry in {interval}.");
                _timer?.Change(interval, interval);
            }
        }

        /// <summary>
        /// Executes the data orchestration for the next outbox and/or incomplete outbox.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the long running operations.</returns>
        private async Task ScopedExecuteAsync(CancellationToken cancellationToken)
        {
            // Create a scope in which to perform the execution.
            using var scope = ServiceProvider.CreateScope();

            // Set up the "scoped" execution context.
            ExecutionContext.Reset();
            var ec = _overrideExecutionContext?.Invoke() ?? scope.ServiceProvider.GetService<ExecutionContext>();
            ec.ServiceProvider = scope.ServiceProvider;
            ExecutionContext.SetCurrent(ec);

            try
            {
                await ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
                    return;

                Logger.LogCritical(ex, $"{ServiceName} execution failure as a result of an expected exception: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Triggered to perform the work as a result of the <see cref="Interval"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>Each timer-based invocation of the <see cref="ExecuteAsync(CancellationToken)"/> will be managed witin the context of a new Dependency Injection (DI)
        /// <see cref="ServiceProviderServiceExtensions.CreateScope">scope</see>.</remarks>
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{ServiceName} service stop requested.");
            _timer!.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                _cts!.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executeTask ?? Task.CompletedTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }

            Logger.LogInformation($"{ServiceName} service stopped.");
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    _disposed = true;
                    _timer?.Dispose();
                    _cts?.Cancel();
                    _cts?.Dispose();
                }
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TimerHostedServiceBase"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) { }
    }
}