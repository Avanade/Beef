// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Publish;
using Beef.Events.Subscribe;
using Beef.Events.Subscribe.EventHubs;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
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
        /// Adds a transient service to instantiate a new <see cref="EventHubSubscriberHost"/> instance using the specified <paramref name="args"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <param name="addSubscriberTypeServices">Indicates whether to add all the <see cref="EventSubscriberHostArgs.GetSubscriberTypes"/> as scoped services (defaults to <c>true</c>).</param>
        /// <param name="additional">Optional (additional) opportunity to further configure the instantiated <see cref="EventHubSubscriberHost"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefEventHubSubscriberHost(this IServiceCollection services, EventSubscriberHostArgs args, bool addSubscriberTypeServices = true, Action<IServiceProvider, EventHubSubscriberHost>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            services.AddTransient(sp =>
            {
                var ehsh = new EventHubSubscriberHost(args);
                args.UseServiceProvider(sp);
                additional?.Invoke(sp, ehsh);
                return ehsh;
            });

            if (addSubscriberTypeServices)
            {
                foreach (var type in args.GetSubscriberTypes())
                {
                    services.TryAddScoped(type);
                }
            }

            return services;
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> <see cref="EventHubPublisher"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="retryPolicy">The <see cref="RetryPolicy"/>; defaults to <see cref="RetryPolicy.Default"/> where not specified.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefEventHubEventPublisher(this IServiceCollection services, string connectionString, RetryPolicy? retryPolicy = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher>(_ =>
            {
                var ehc = EventHubClient.CreateFromConnectionString(Check.NotEmpty(connectionString, nameof(connectionString)));
                ehc.RetryPolicy = retryPolicy ?? RetryPolicy.Default;
                return new EventHubPublisher(ehc);
            });
        }

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
                var astp = new AzureServiceTokenProvider();
#pragma warning disable CA2000 // Dispose objects before losing scope; this object MUST NOT be disposed or will result in further error - only a single instance so is OK.
                var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(astp.KeyVaultTokenCallback));
                cb.AddAzureKeyVault($"https://{kvn}.vault.azure.net/", kvc, new DefaultKeyVaultSecretManager());
#pragma warning restore CA2000
            }

            // Build again and replace existing.
            config = cb.Build();
            serviceCollection.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
            return config;
        }
    }
}