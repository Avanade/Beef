// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;

namespace Beef.Events
{
    /// <summary>
    /// Provides the extensions methods for the events capabilities.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Gets (and builds on first access) the <see cref="IConfiguration"/>. 
        /// Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration),
        /// 2) User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 4) local.settings.json, 5) funcsettings.{environment}.json (embedded resource), and 6) funcsettings.json (embedded resource). 
        /// </summary>
        /// <typeparam name="TStartup">The function startup <see cref="Type"/>.</typeparam>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/>.</param>
        /// <param name="embeddedFilePrefix">The embedded resource name prefix (defaults to "funcsettings").</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">Used where the <c>AZURE_FUNCTIONS_ENVIRONMENT</c> cannot be inferred.</param>
        /// <param name="configurationBuilder">Action to enable further configuration sources to be added.</param>
        /// <returns>The <see cref="IConfiguration"/> instance.</returns>
        public static IConfiguration GetBeefConfiguration<TStartup>(this IFunctionsHostBuilder builder, string? environmentVariablePrefix = null, string embeddedFilePrefix = "funcsettings", string? environment = "Development", Action<ConfigurationBuilder>? configurationBuilder = null)
            where TStartup : class
            => GetBeefConfiguration<TStartup>((builder ?? throw new ArgumentNullException(nameof(builder))).Services, environmentVariablePrefix, embeddedFilePrefix, environment, configurationBuilder);

        /// <summary>
        /// Gets (and builds on first access) the <see cref="IConfiguration"/>. 
        /// Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration),
        /// 2) User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 4) local.settings.json, 5) funcsettings.{environment}.json (embedded resource), and 6) funcsettings.json (embedded resource). 
        /// </summary>
        /// <typeparam name="TStartup">The function startup <see cref="Type"/>.</typeparam>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="embeddedFilePrefix">The embedded resource name prefix (defaults to "funcsettings").</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">Used where the <c>AZURE_FUNCTIONS_ENVIRONMENT</c> cannot be inferred.</param>
        /// <param name="configurationBuilder">Action to enable further configuration sources to be added.</param>
        /// <returns>The <see cref="IConfiguration"/> instance.</returns>
        public static IConfiguration GetBeefConfiguration<TStartup>(this IServiceCollection serviceCollection, string? environmentVariablePrefix = null, string embeddedFilePrefix = "funcsettings", string? environment = "Development", Action<ConfigurationBuilder>? configurationBuilder = null)
            where TStartup : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            // Get the current configuration.
            var c = serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
            var hostingEnvironment = c?.GetValue<string>("AZURE_FUNCTIONS_ENVIRONMENT") ?? environment;

            var cb = new ConfigurationBuilder();
            if (c != null)
                cb.AddConfiguration(c);

            // Add additional configuration probes.
            cb.AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"{embeddedFilePrefix}.json", true, false)
              .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"{embeddedFilePrefix}.{hostingEnvironment}.json", true, false)
              .AddJsonFile("local.settings.json", true, true);

            if (string.IsNullOrEmpty(environmentVariablePrefix))
                cb.AddEnvironmentVariables();
            else
                cb.AddEnvironmentVariables(environmentVariablePrefix.EndsWith("_", StringComparison.InvariantCulture) ? environmentVariablePrefix : environmentVariablePrefix + "_");

            configurationBuilder?.Invoke(cb);

            // Build as new and replace existing.
            var config = cb.Build();
            if (string.Compare(hostingEnvironment, "development", StringComparison.InvariantCultureIgnoreCase) == 0 && config.GetValue<bool>("UseUserSecrets"))
                cb.AddUserSecrets<TStartup>();

            var kvn = config["KeyVaultName"];
            if (!string.IsNullOrEmpty(kvn))
            {
                var secretClient = new SecretClient(new Uri($"https://{kvn}.vault.azure.net/"), new DefaultAzureCredential());
                cb.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }

            // Build again and replace existing.
            config = cb.Build();
            serviceCollection.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
            return config;
        }
    }
}