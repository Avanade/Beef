// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beef.Events.Triggers.Config
{
    /// <summary>
    /// Represents the "resilient event hubs" extension configuration provider.
    /// </summary>
    [Extension("ResilientEventHubs")]
#pragma warning disable CA1812 // Apparently never instatiated; by-design, it is actually!
    internal class ResilientEventHubExtensionConfigProvider : IExtensionConfigProvider
#pragma warning restore CA1812 
    {
        private readonly IConfiguration _config;
        private readonly IOptions<ResilientEventHubOptions> _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly INameResolver _nameResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubExtensionConfigProvider"/>.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="options">The <see cref="IOptions{ResilientEventHubOptions}"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="nameResolver">The <see cref="INameResolver"/>.</param>
        public ResilientEventHubExtensionConfigProvider(IConfiguration config, IOptions<ResilientEventHubOptions> options, ILoggerFactory loggerFactory, INameResolver nameResolver)
        {
            _config = config;
            _options = options;
            _loggerFactory = loggerFactory;
            _nameResolver = nameResolver;
        }

        /// <summary>
        /// Initialize the <see cref="ResilientEventHubExtensionConfigProvider"/>.
        /// </summary>
        /// <param name="context">The <see cref="ExtensionConfigContext"/>.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register the trigger binding provider including a trigger-category logger.
            var bindingProvider = new ResilientEventHubAttributeBindingProvider(_config, _nameResolver, _options, _loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("ResilientEventHub")));

            context.AddBindingRule<ResilientEventHubTriggerAttribute>()
                .BindToTrigger(bindingProvider);
        }
    }
}