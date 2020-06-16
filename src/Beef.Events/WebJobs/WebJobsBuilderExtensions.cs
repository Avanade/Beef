// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

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
        /// Gets (and builds on first access) the <see cref="IConfiguration"/>. Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) 
        /// or User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 2) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 3) appsettings.{environment}.json, 4) appsettings.json, 5) {<paramref name="embeddedFilePrefix"/>}.{environment}.json (embedded resource), and 6) {<paramref name="embeddedFilePrefix"/>}.json (embedded resource).
        /// </summary>
        /// <typeparam name="TStartup">The function startup <see cref="Type"/>.</typeparam>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/>.</param>
        /// <param name="embeddedFilePrefix">The embedded resource name prefix (defaults to "webjobs").</param>
        /// <param name="environmentVariablePrefix">The environment variables prefix.</param>
        /// <param name="configurationBuilder">Action to enable further configuration sources to be added.</param>
        /// <returns>The <see cref="IConfiguration"/> instance.</returns>
        public static IConfiguration GetConfiguration<TStartup>(this IWebJobsBuilder builder, string embeddedFilePrefix = "webjobs",
            string? environmentVariablePrefix = null, Action<ConfigurationBuilder>? configurationBuilder = null)
            where TStartup : class
        {
            return _configs.GetOrAdd(builder, (_) =>
            {
                // Get the current configuration.
                var c = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
                var hostingEnvironment = c.GetValue<string>("AZURE_FUNCTIONS_ENVIRONMENT");

                var cb = new ConfigurationBuilder();
                cb.AddConfiguration(c);

                // Add additional configuration probes.
                cb.AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"{embeddedFilePrefix}.json", true, false)
                    .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"{embeddedFilePrefix}.{hostingEnvironment}.json", true, false)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingEnvironment}.json", true, true)
                    .AddEnvironmentVariables(environmentVariablePrefix);

                configurationBuilder?.Invoke(cb);

                // Build as new and replace existing.
                var config = cb.Build();
                if (string.Compare(hostingEnvironment, "development", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    if (config.GetValue<bool>("UseUserSecrets"))
                        cb.AddUserSecrets<TStartup>();
                }
                else
                {
                    var kvn = config["KeyVaultName"];
                    if (!string.IsNullOrEmpty(kvn))
                    {
                        var astp = new AzureServiceTokenProvider();
                        using var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(astp.KeyVaultTokenCallback));
                        cb.AddAzureKeyVault($"https://{kvn}.vault.azure.net/", kvc, new DefaultKeyVaultSecretManager());
                    }
                }

                // Build again and replace existing.
                config = cb.Build();
                builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
                return config;
            });
        }
    }
}