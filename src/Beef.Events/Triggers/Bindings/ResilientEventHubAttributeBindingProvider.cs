// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.Bindings
{
    /// <summary>
    /// Provides the <see cref="ResilientEventHubBinding"/> capability.
    /// </summary>
    internal class ResilientEventHubAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _config;
        private readonly INameResolver _nameResolver;
        private readonly IOptions<ResilientEventHubOptions> _options;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubAttributeBindingProvider"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="nameResolver">The <see cref="INameResolver"/>.</param>
        /// <param name="options">The <see cref="ResilientEventHubOptions"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ResilientEventHubAttributeBindingProvider(IConfiguration config, INameResolver nameResolver, IOptions<ResilientEventHubOptions> options, ILogger logger)
        {
            _config = config;
            _nameResolver = nameResolver;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Create the <see cref="ITriggerBinding"/> (being the <see cref="ResilientEventHubBinding"/>).
        /// </summary>
        /// <param name="context">The <see cref="TriggerBindingProviderContext"/>.</param>
        /// <returns></returns>
        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            // Get the required attribuite.
            var att = context.Parameter.GetCustomAttribute<ResilientEventHubTriggerAttribute>(false);
            if (att == null)
                return Task.FromResult<ITriggerBinding?>(null);

            // Resolve the attribute parameters.
            var resolvedEventHubName = _nameResolver.ResolveWholeString(att.EventHubName);
            var resolvedConsumerGroup = _nameResolver.ResolveWholeString(att.ConsumerGroup ?? PartitionReceiver.DefaultConsumerGroupName);

            if (!string.IsNullOrWhiteSpace(att.Connection))
            {
                var connectionString = _config.GetConnectionStringOrSetting(_nameResolver.ResolveWholeString(att.Connection));
                _options.Value.AddEventHub(resolvedEventHubName, connectionString);
            }

            // Capture the name of the function being executed.
            _options.Value.FunctionName = context.Parameter.Member.GetCustomAttribute<FunctionNameAttribute>().Name;
            _options.Value.FunctionType = context.Parameter.Member.DeclaringType.FullName + "." + context.Parameter.Member.Name;

            // Create the event hub processor.
            var host = _options.Value.GetEventProcessorHost(_config, resolvedEventHubName, resolvedConsumerGroup);

            // Create and return the binding.
            return Task.FromResult<ITriggerBinding?>(new ResilientEventHubBinding(host, context.Parameter, _options.Value, _config, _logger));
        }
    }
}