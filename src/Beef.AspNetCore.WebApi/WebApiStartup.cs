// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides helper methods for the <b>ASP.NET Web API</b> startup.
    /// </summary>
    public static class WebApiStartup
    {
        /// <summary>
        /// Builds (creates) the <see cref="IWebHost"/> using the <see cref="WebHost.CreateDefaultBuilder(string[])"/> utilizing the <see cref="ConfigurationBuilder"/>.
        /// </summary>
        /// <typeparam name="TStartup">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="args">The command line args.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with.</param>
        /// <returns>The <see cref="IWebHost"/>.</returns>
        public static IWebHost BuildWebHost<TStartup>(string[] args, string? environmentVariablePrefix = null) where TStartup : class =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((hostingContext, config) => ConfigurationBuilder<TStartup>(config, hostingContext.HostingEnvironment, environmentVariablePrefix))
                   .UseStartup<TStartup>()
                   .Build();

        /// <summary>
        /// Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) 
        /// or User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 2) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 3) appsettings.{environment}.json, 4) appsettings.json, 5) webapisettings.{environment}.json (embedded resource), and 6) webapisettings.json (embedded resource).
        /// </summary>
        /// <typeparam name="TStartup">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with.</param>
        public static void ConfigurationBuilder<TStartup>(IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment, string? environmentVariablePrefix = null) where TStartup : class
        {
            if (configurationBuilder == null)
                throw new ArgumentNullException(nameof(configurationBuilder));

            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));

            configurationBuilder.AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.{hostingEnvironment.EnvironmentName}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables(environmentVariablePrefix);

            var config = configurationBuilder.Build();
            if (hostingEnvironment.IsDevelopment())
            {
                if (config.GetValue<bool>("UseUserSecrets"))
                    configurationBuilder.AddUserSecrets<TStartup>();
            }
            else
            {
                var kvn = config["KeyVaultName"];
                if (!string.IsNullOrEmpty(kvn))
                {
                    var astp = new AzureServiceTokenProvider();
                    using var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(astp.KeyVaultTokenCallback));
                    configurationBuilder.AddAzureKeyVault($"https://{kvn}.vault.azure.net/", kvc, new DefaultKeyVaultSecretManager());
                }
            }
        }

        /// <summary>
        /// Binds (redirects) Beef <see cref="Beef.Diagnostics.Logger"/> to the ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.
        /// </summary>
        /// <param name="logger">The ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.</param>
        /// <param name="args">The Beef <see cref="LoggerArgs"/>.</param>
        /// <remarks>Redirects (binds) the Beef logger to the ASP.NET logger.</remarks>
        public static void BindLogger(ILogger logger, LoggerArgs args)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(args, nameof(args));

#pragma warning disable CA1062 // Validate arguments of public methods; see Check above.
            switch (args.Type)
#pragma warning restore CA1062 
            {
                case LogMessageType.Critical:
                    logger.LogCritical(args.ToString());
                    break;

                case LogMessageType.Info:
                    logger.LogInformation(args.ToString());
                    break;

                case LogMessageType.Warning:
                    logger.LogWarning(args.ToString());
                    break;

                case LogMessageType.Error:
                    logger.LogError(args.ToString());
                    break;

                case LogMessageType.Debug:
                    logger.LogDebug(args.ToString());
                    break;

                case LogMessageType.Trace:
                    logger.LogTrace(args.ToString());
                    break;
            }
        }

        /// <summary>
        /// Gets the named connection string from the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="name">The connection string name.</param>
        /// <returns>The connection string value.</returns>
        public static string GetConnectionString(IConfiguration configuration, string name)
        {
            Check.NotNull(configuration, nameof(configuration));
            Check.NotEmpty(name, nameof(name));

            return configuration.GetConnectionString(name) ?? throw new ArgumentException($"Connection string setting for '{name}' is not defined.", nameof(name));
        }
    }
}