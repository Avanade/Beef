// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides helper methods for the <b>ASP.NET Web API</b> startup.
    /// </summary>
    public static class WebApiStartup
    {
        /// <summary>
        /// Creates the <see cref="IWebHost"/> using the <see cref="WebHost.CreateDefaultBuilder(string[])"/> utilizing the standardized <see cref="ConfigurationBuilder"/>.
        /// </summary>
        /// <typeparam name="TStartup">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="args">The command line args.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with.</param>
        /// <returns>The <see cref="IWebHost"/>.</returns>
        public static IWebHostBuilder CreateWebHost<TStartup>(string[] args, string? environmentVariablePrefix = null) where TStartup : class =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((hostingContext, config) => ConfigurationBuilder<TStartup>(config, args, hostingContext.HostingEnvironment, environmentVariablePrefix))
                   .UseStartup<TStartup>();

        /// <summary>
        /// Creates and builds the <see cref="IWebHost"/> using the <see cref="WebHost.CreateDefaultBuilder(string[])"/> utilizing the <see cref="ConfigurationBuilder"/>.
        /// </summary>
        /// <typeparam name="TStartup">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="args">The command line args.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with.</param>
        /// <returns>The <see cref="IWebHost"/>.</returns>
        public static IWebHost BuildWebHost<TStartup>(string[] args, string? environmentVariablePrefix = null) where TStartup : class =>
            CreateWebHost<TStartup>(args, environmentVariablePrefix).Build();

        /// <summary>
        /// Builds the configuration probing; will probe in the following order: 1) Command-line arguments, 2) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration),
        /// 3) User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 4) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 5) appsettings.{environment}.json, 6) appsettings.json, 7) webapisettings.{environment}.json (embedded resource), and 8) webapisettings.json (embedded resource).
        /// </summary>
        /// <typeparam name="TStartup">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <param name="args">The command line args.</param>
        /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        public static void ConfigurationBuilder<TStartup>(IConfigurationBuilder configurationBuilder, string[] args, IWebHostEnvironment hostingEnvironment, string? environmentVariablePrefix = null) where TStartup : class
        {
            if (configurationBuilder == null)
                throw new ArgumentNullException(nameof(configurationBuilder));

            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));

            configurationBuilder.AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.{hostingEnvironment.EnvironmentName}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true);

            if (string.IsNullOrEmpty(environmentVariablePrefix))
                configurationBuilder.AddEnvironmentVariables();
            else
                configurationBuilder.AddEnvironmentVariables(environmentVariablePrefix.EndsWith("_", StringComparison.InvariantCulture) ? environmentVariablePrefix : environmentVariablePrefix + "_");

            configurationBuilder.AddCommandLine(args);

            var config = configurationBuilder.Build();
            if (hostingEnvironment.IsDevelopment() && config.GetValue<bool>("UseUserSecrets"))
                configurationBuilder.AddUserSecrets<TStartup>();

            var kvn = config["KeyVaultName"];
            if (!string.IsNullOrEmpty(kvn))
            {
                var astp = new AzureServiceTokenProvider();
#pragma warning disable CA2000 // Dispose objects before losing scope; this object MUST NOT be disposed or will result in further error - only a single instance so is OK.
                var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(astp.KeyVaultTokenCallback));
                configurationBuilder.AddAzureKeyVault($"https://{kvn}.vault.azure.net/", kvc, new DefaultKeyVaultSecretManager());
#pragma warning restore CA2000
            }

            configurationBuilder.AddCommandLine(args);
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