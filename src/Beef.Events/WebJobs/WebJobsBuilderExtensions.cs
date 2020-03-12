// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Beef.Events.WebJobs
{
    /// <summary>
    /// Adds extension methods to the <see cref="IWebJobsBuilder"/>.
    /// </summary>
    public static class WebJobsBuilderExtensions
    {
        private static readonly ConcurrentDictionary<IWebJobsBuilder, IConfiguration> _configs = new ConcurrentDictionary<IWebJobsBuilder, IConfiguration>();

        /// <summary>
        /// Adds "resilient event hubs" to the <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/>.</param>
        /// <returns>The <see cref="IWebJobsBuilder"/>.</returns>
        public static IWebJobsBuilder AddResilientEventHubs(this IWebJobsBuilder builder)
        {
            builder.AddExtension<ResilientEventHubExtensionConfigProvider>()
                .BindOptions<ResilientEventHubOptions>();

            return builder;
        }

        /// <summary>
        /// Gets (and builds on first access) the <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/>.</param>
        /// <param name="embeddedAssembly">The embedded resource <see cref="Assembly"/> (defaults to <see cref="Assembly.GetCallingAssembly"/> where not specified).</param>
        /// <param name="embeddedFilePrefix">The embedded resource name prefix (defaults to "webjobs").</param>
        /// <param name="environmentVariablesPrefix">The environment variables prefix.</param>
        /// <param name="configurationBuilder">Action to enable further configuration sources to be added.</param>
        /// <returns>The <see cref="IConfiguration"/> instance.</returns>
        public static IConfiguration GetConfiguration(this IWebJobsBuilder builder, Assembly? embeddedAssembly = null,
            string embeddedFilePrefix = "webjobs", string? environmentVariablesPrefix = null, Action<ConfigurationBuilder>? configurationBuilder = null)
        {
            return _configs.GetOrAdd(builder, (_) =>
            {
                // Get the current configuration.
                var c = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
                var hostingEnvironment = c.GetValue<string>("AZURE_FUNCTIONS_ENVIRONMENT");

                var cb = new ConfigurationBuilder();
                cb.AddConfiguration(c);

                // Add additional configuration probes.
                if (embeddedAssembly != null)
                {
                    cb.AddJsonFile(new EmbeddedFileProvider(embeddedAssembly), $"{embeddedFilePrefix}.json", true, false)
                        .AddJsonFile(new EmbeddedFileProvider(embeddedAssembly), $"{embeddedFilePrefix}.{hostingEnvironment}.json", true, false);
                }

                if (!string.IsNullOrEmpty(environmentVariablesPrefix))
                    cb.AddEnvironmentVariables(environmentVariablesPrefix);

                configurationBuilder?.Invoke(cb);

                // Build as new and replace existing.
                var config = cb.Build();
                builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
                return config;
            });
        }
    }
}