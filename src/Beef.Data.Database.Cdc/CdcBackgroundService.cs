// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Configuration;
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
        private string? _name;
        private int? _intervalSeconds;
        private int? _maxQuerySize;
        private bool? _continueWithDataLoss;

        /// <summary>
        /// The configuration name for <see cref="IntervalSeconds"/>.
        /// </summary>
        public const string IntervalSecondsName = "IntervalSeconds";

        /// <summary>
        /// The configuration name for <see cref="MaxQuerySize"/>.
        /// </summary>
        public const string MaxQuerySizeName = "MaxQuerySize";

        /// <summary>
        /// The configuration name for <see cref="ContinueWithDataLoss"/>.
        /// </summary>
        public const string ContinueWithDataLossName = "ContinueWithDataLoss";

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="IntervalSeconds"/> is less than or equal to zero. Defaults to <b>one</b> minute.
        /// </summary>
        public static int DefaultIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcBackgroundService(IServiceProvider serviceProvider, IConfiguration config, ILogger logger)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        internal IServiceProvider ServiceProvider { get; private set; }

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
        /// Gets or sets the interval seconds between each execution.
        /// </summary>
        /// <remarks>The interval seconds must be greater than zero otherwise the <see cref="DefaultIntervalSeconds"/> will be used.
        /// <para>Will default to configuration, a) <see cref="ServiceName"/> : <see cref="IntervalSecondsName"/>, then b) <see cref="IntervalSecondsName"/>, where specified.</para></remarks>
        public int? IntervalSeconds 
        { 
            get => _intervalSeconds ?? Config.GetSection(ServiceName)?.GetValue<int?>(IntervalSecondsName) ?? Config.GetValue<int?>(IntervalSecondsName);
            set => _intervalSeconds = value;
        }

        /// <summary>
        /// Gets the <see cref="IntervalSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        protected TimeSpan IntervalTimespan => TimeSpan.FromSeconds(IntervalSeconds.HasValue && IntervalSeconds.Value > 0 ? IntervalSeconds.Value
            : (DefaultIntervalSeconds <= 1 ? throw new InvalidOperationException("The DefaultIntervalSeconds value must be greater than zero.") : DefaultIntervalSeconds));

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="CdcOutbox"/>.
        /// </summary>
        /// <remarks>Where specified overrides the <see cref="ICdcDataOrchestrator.MaxQuerySize"/>.
        /// <para>Will default to configuration, a) <see cref="ServiceName"/> : <see cref="MaxQuerySizeName"/>, then b) <see cref="MaxQuerySizeName"/>, where specified.</para></remarks>
        public virtual int? MaxQuerySize
        {
            get => _maxQuerySize ?? Config.GetSection(ServiceName)?.GetValue<int?>(MaxQuerySizeName) ?? Config.GetValue<int?>(MaxQuerySizeName);
            set => _maxQuerySize = value;
        }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server 
        /// <para>Will default to configuration, a) <see cref="ServiceName"/> : <see cref="ContinueWithDataLossName"/>, then b) <see cref="ContinueWithDataLossName"/>, where specified.</para></remarks>
        public virtual bool? ContinueWithDataLoss
        {
            get => _continueWithDataLoss ?? Config.GetSection(ServiceName)?.GetValue<bool?>(ContinueWithDataLossName) ?? Config.GetValue<bool?>(ContinueWithDataLossName);
            set => _continueWithDataLoss = value;
        }

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
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcBackgroundService(IServiceProvider serviceProvider, IConfiguration config, ILogger logger) : base(serviceProvider, config, logger) { }

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

            Logger.LogInformation($"{ServiceName} execution completed. Retry in {IntervalTimespan}.");
            _timer?.Change(IntervalTimespan, IntervalTimespan);
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
            var ec = scope.ServiceProvider.GetService<ExecutionContext>();
            ec.ServiceProvider = scope.ServiceProvider;
            ExecutionContext.SetCurrent(ec);

            await ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the data orchestration for the next outbox and/or incomplete outbox and continues until all data is processed before stopping (i.e. depending on the volume of data this will be long-running).
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        /// <remarks>This method is provided where a single invocation is required versus executing as a background service using the <see cref="IHostedService"/> capabilties. Note that the
        /// <see cref="CdcBackgroundService.IntervalSeconds"/> is not used when running directly via this method.</remarks>
        public async Task<CdcDataOrchestratorResult> RunAsync(CancellationToken? cancellationToken = null)
        {
            // Set up the execution context.
            ExecutionContext.Reset();
            var ec = ServiceProvider.GetService<ExecutionContext>();
            ec.ServiceProvider = ServiceProvider;
            ExecutionContext.SetCurrent(ec);

            return await ExecuteAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the data orchestration for the next outbox and/or incomplete outbox.
        /// </summary>
        private async Task<CdcDataOrchestratorResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            var cdo = ExecutionContext.GetService<TCdcDataOrchestrator>(throwExceptionOnNull: true)!;

            var mqs = MaxQuerySize;
            if (mqs.HasValue)
                cdo.MaxQuerySize = mqs.Value;

            var cwdl = ContinueWithDataLoss;
            if (cwdl.HasValue)
                cdo.ContinueWithDataLoss = cwdl.Value;

            CdcDataOrchestratorResult cdor;

            while (true)
            {
                if (_processIncomplete)
                    cdor = await cdo.ExecuteIncompleteAsync(cancellationToken).ConfigureAwait(false);
                else
                    cdor = await cdo.ExecuteNextAsync(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return cdor;

                // Where successful, then the next outbox should be attempted immediately.
                if (cdor.OutboxExecuted)
                    _processIncomplete = false;
                else
                {
                    // Nothing found to process so retry later.
                    if (cdor.Outbox == null)
                        return cdor;

                    if (!cdor.Outbox.IsComplete && !_processIncomplete)
                    {
                        _processIncomplete = true;
                        Logger.LogInformation($"Subsequent executions will attempt to complete outbox '{cdor.Outbox.Id}' prior to executing next.");
                    }
                    else
                        return cdor; // Retry later.
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