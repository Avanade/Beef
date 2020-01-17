// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.Listener
{
    /// <summary>
    /// Represents the "resilient event hub" listener responsible for the triggering, and transient-fault-handling, of the function.
    /// </summary>
    internal class ResilientEventHubListener : IListener, IEventProcessorFactory
    {
        private readonly EventProcessorHost _host;
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ResilientEventHubOptions _options;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private bool _started;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubListener"/> class.
        /// </summary>
        /// <param name="host">The <see cref="EventProcessorHost"/>.</param>
        /// <param name="executor">The <see cref="ITriggeredFunctionExecutor"/> (being the function to invoke).</param>
        /// <param name="options">The <see cref="ResilientEventHubOptions"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ResilientEventHubListener(EventProcessorHost host, ITriggeredFunctionExecutor executor, ResilientEventHubOptions options, IConfiguration config, ILogger logger)
        {
            _host = host;
            _executor = executor;
            _options = options;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _host.RegisterEventProcessorFactoryAsync(this, _options.EventProcessorOptions).ConfigureAwait(false);
            _started = true;
        }

        /// <summary>
        /// Cancel the listener.
        /// </summary>
        public void Cancel() => StopAsync(CancellationToken.None).Wait();

        /// <summary>
        /// Stop the listener.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_started)
                await _host.UnregisterEventProcessorAsync().ConfigureAwait(false);

            _started = false;
        }

        /// <summary>
        /// Dispose of any and all resources.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Creates an <see cref="IEventProcessor"/> (see <see cref="ResilientEventHubProcessor"/>) instance.
        /// </summary>
        /// <param name="context">The <see cref="PartitionContext"/>.</param>
        /// <returns>The <see cref="IEventProcessor"/>.</returns>
        public IEventProcessor CreateEventProcessor(PartitionContext context) => new ResilientEventHubProcessor(_executor, _options, _config, _logger);
    }
}