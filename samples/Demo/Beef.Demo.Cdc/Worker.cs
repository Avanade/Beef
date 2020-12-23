using Beef.Data.Database.Cdc;
using Beef.Demo.Cdc.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Beef.Demo.Cdc
{
    public class Worker : CdcBackgroundService<PostsCdcData>
    {
        private readonly IConfiguration _config;

        public Worker(PostsCdcData data, IConfiguration config, IServiceProvider serviceProvider, ILogger<Worker> logger) : base(data, serviceProvider, logger) => _config = config;

        public override string ServiceName => nameof(Worker);

        public override int IntervalSeconds => _config.GetValue<int>("Worker");
    }

    /// <summary>
    /// Represents the base class for Change Data Capture (CDC) background services.
    /// </summary>
    public abstract class CdcBackgroundService<TCdcExecutor> : IHostedService, IDisposable where TCdcExecutor : ICdcExecutor
    {
        private static readonly Random _random = new Random();

        private CancellationTokenSource? _cts;
        private Timer? _timer;
        private Task? _executeTask;

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="IntervalSeconds"/> is less than or equal to zero. Defaults to <b>five</b> minutes.
        /// </summary>
        public static int DefaultIntervalSeconds { get; set; } = 5 * 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcTimedBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcBackgroundService(TCdcExecutor cdcExecutor, IServiceProvider serviceProvider, ILogger logger)
        {
            CdcExecutor = cdcExecutor ?? throw new ArgumentNullException(nameof(cdcExecutor));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// Gets the interval seconds between each <see cref="ExecuteAsync(CancellationToken)"/> execution.
        /// </summary>
        /// <remarks>The interval seconds must be greater than zero otherwise the <see cref="DefaultIntervalSeconds"/> will be used.</remarks>
        public abstract int IntervalSeconds { get; }

        /// <summary>
        /// Gets the <see cref="IntervalSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        private TimeSpan IntervalTimespan => TimeSpan.FromSeconds(IntervalSeconds > 0 ? IntervalSeconds
            : (DefaultIntervalSeconds <= 1 ? throw new InvalidOperationException("The DefaultIntervalSeconds value must be greater than zero.") : DefaultIntervalSeconds));

        /// <summary>
        /// Gets the <typeparamref name="TCdcExecutor"/>.
        /// </summary>
        protected TCdcExecutor CdcExecutor { get; private set; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; private set; }

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

            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext { ServiceProvider = ServiceProvider });

            _executeTask = Task.Run(async () => await ExecuteAsync(_cts!.Token).ConfigureAwait(false));
            _executeTask.Wait();

            Logger.LogInformation($"{ServiceName} execution completed.");
            _timer?.Change(IntervalTimespan, IntervalTimespan);
        }

        /// <summary>
        /// Executes <see cref="ICdcExecutor.ExecuteNextAsync(int, CancellationToken?)"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the long running operations.</returns>
        protected virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            CdcExecutorResult cer;

            do
            {
                cer = await CdcExecutor.ExecuteNextAsync(100, cancellationToken);
            } while (cer.EnvelopeExecuted && !cancellationToken.IsCancellationRequested);
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
        /// Dispose of resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            _timer?.Dispose();
            _cts?.Cancel();
        }
    }
}

#nullable restore