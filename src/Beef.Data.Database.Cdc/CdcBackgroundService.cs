// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the base capabilities for the Change Data Capture (CDC) background services.
    /// </summary>
    public abstract class CdcBackgroundService : IDisposable
    {
        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="IntervalSeconds"/> is less than or equal to zero. Defaults to <b>five</b> minutes.
        /// </summary>
        public static int DefaultIntervalSeconds { get; set; } = 5 * 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcBackgroundService(IServiceProvider serviceProvider, ILogger logger)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        internal IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// Gets the interval seconds between each execution.
        /// </summary>
        /// <remarks>The interval seconds must be greater than zero otherwise the <see cref="DefaultIntervalSeconds"/> will be used.</remarks>
        public abstract int IntervalSeconds { get; }

        /// <summary>
        /// Gets the <see cref="IntervalSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        protected TimeSpan IntervalTimespan => TimeSpan.FromSeconds(IntervalSeconds > 0 ? IntervalSeconds
            : (DefaultIntervalSeconds <= 1 ? throw new InvalidOperationException("The DefaultIntervalSeconds value must be greater than zero.") : DefaultIntervalSeconds));

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CdcBackgroundService{TCdcDataOrchestrator}"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) { }
    }

    /// <summary>
    /// Represents the base class for Change Data Capture (CDC) background services.
    /// </summary>
    public abstract class CdcBackgroundService<TCdcDataOrchestrator> : CdcBackgroundService, IHostedService where TCdcDataOrchestrator : ICdcDataOrchestrator
    {
        private static readonly Random _random = new Random();

        private CancellationTokenSource? _cts;
        private Timer? _timer;
        private Task? _executeTask;
        private bool _disposed;
        private bool _processIncomplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcBackgroundService(IServiceProvider serviceProvider, ILogger logger) : base(serviceProvider, logger) { }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>The underlying timer start is randomized between zero and one thousand milliseconds; this will minimize multiple services within the host all starting at once.</remarks>
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{ServiceName} service started.");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _timer = new Timer(Execute, null, TimeSpan.FromMilliseconds(_random.Next(0, 1000)), IntervalTimespan);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs the internal execution orchestration.
        /// </summary>
        private void Execute(object? state)
        {
            // Stop the timer as no more work should be initiated until after complete.
            _timer!.Change(Timeout.Infinite, Timeout.Infinite);
            Logger.LogInformation($"{ServiceName} execution triggered by timer.");

            _executeTask = Task.Run(async () => await ScopedExecuteAsync(_cts!.Token).ConfigureAwait(false));
            _executeTask.Wait();

            Logger.LogInformation($"{ServiceName} execution completed.");
            _timer?.Change(IntervalTimespan, IntervalTimespan);
        }

        /// <summary>
        /// Executes the data orchestration for the next envelope and/or incomplete envelope.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the long running operations.</returns>
        private async Task ScopedExecuteAsync(CancellationToken cancellationToken)
        {
            // Create a scope in which to perform the execution.
            using var scope = ServiceProvider.CreateScope();

            // Set up the execution context.
            ExecutionContext.Reset();
            var ec = scope.ServiceProvider.GetService<ExecutionContext>();
            ec.ServiceProvider = scope.ServiceProvider;
            ExecutionContext.SetCurrent(ec);

            // Instantiate data orchestrator and execute.
            var cdo = scope.ServiceProvider.GetService<TCdcDataOrchestrator>() ?? throw new InvalidOperationException($"An instance of {typeof(TCdcDataOrchestrator).Name} could not be instantiated.");
            await ExecuteAsync(cdo, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the data orchestration for the next envelope and/or incomplete envelope.
        /// </summary>
        /// <param name="cdcDataOrchestrator">The <typeparamref name="TCdcDataOrchestrator"/> instance.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the long running operations.</returns>
        protected virtual async Task ExecuteAsync(TCdcDataOrchestrator cdcDataOrchestrator, CancellationToken cancellationToken)
        { 
            CdcDataOrchestratorResult cdor;

            while (true)
            {
                if (_processIncomplete)
                    cdor = await cdcDataOrchestrator.ExecuteIncompleteAsync(cancellationToken).ConfigureAwait(false);
                else
                    cdor = await cdcDataOrchestrator.ExecuteNextAsync(100, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return;

                // Where successful, then the next envelope should be attempted immediately.
                if (cdor.EnvelopeExecuted)
                    _processIncomplete = false;
                else
                {
                    // Nothing found to process so retry later.
                    if (cdor.Envelope == null)
                        return;

                    if (!cdor.Envelope.IsComplete && !_processIncomplete)
                    {
                        _processIncomplete = true;
                        Logger.LogInformation($"Subsequent executions will attempt to complete envelope '{cdor.Envelope.Id}' prior to executing next.");
                    }
                    else
                        return; // Retry later.
                }
            } 
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/>.</returns>
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
        /// Releases the unmanaged resources used by the <see cref="CdcBackgroundService{TCdcDataOrchestrator}"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
                return;

            _disposed = true;
            _timer?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose(disposing);
        }
    }
}