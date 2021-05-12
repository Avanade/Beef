// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net.Http;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the default testing capabilities for the <see cref="AgentTesterWaf{TStartup}"/>, <see cref="AgentTest{TStartup, TAgent}"/> and <see cref="AgentTest{TStartup, TAgent, TValue}"/>.
    /// </summary>
    public static class AgentTester
    {
        private static Action<HttpRequestMessage>? _beforeRequest;

        /// <summary>
        /// Registers the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending.
        /// </summary>
        /// <param name="request">The <see cref="Action{HttpRequestMessage}"/>.</param>
        public static void RegisterBeforeRequest(Action<HttpRequestMessage> request) => _beforeRequest = request;

        /// <summary>
        /// Gets or sets the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending.
        /// </summary>
        public static Action<HttpRequestMessage>? GetBeforeRequest() => _beforeRequest;

        /// <summary>
        /// Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) where 'KeyVaultName' config key has value,
        /// 2) User Secrets where 'UseUserSecrets' config key has value (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 4) appsettings.{environment}.json, 5) appsettings.json, 6) webapisettings.{environment}.json (embedded resource within <typeparamref name="TStartup"/> assembly), and 7) webapisettings.json (embedded resource within <typeparamref name="TStartup"/> assembly).
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied). Defaults to <see cref="TestSetUp.DefaultEnvironmentVariablePrefix"/></param>
        /// <param name="environment">The environment to be used by the underlying web host. Defaults to <see cref="TestSetUp.DefaultEnvironment"/>.</param>
        /// <returns>The <see cref="IConfiguration"/>.</returns>
        public static IConfiguration BuildConfiguration<TStartup>(string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment) where TStartup : class
        {
            var cb = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.{environment}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);

            var evp = string.IsNullOrEmpty(environmentVariablePrefix) ? TestSetUp.DefaultEnvironmentVariablePrefix : environmentVariablePrefix;
            if (string.IsNullOrEmpty(evp))
                cb.AddEnvironmentVariables();
            else
                cb.AddEnvironmentVariables(evp.EndsWith("_", StringComparison.InvariantCulture) ? evp : evp + "_");

            var args = Environment.GetCommandLineArgs();
            cb.AddCommandLine(args);

            var config = cb.Build();
            if (config.GetValue<bool>("UseUserSecrets"))
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

            cb.AddCommandLine(args);

            return cb.Build();
        }

        /// <summary>
        /// Creates an <see cref="AgentTesterServer{TStartup}"/> to manage the orchestration of the <see cref="TestServer"/> to execute one or more integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="config">The <see cref="IConfiguration"/>; defaults to <see cref="AgentTester.BuildConfiguration{TStartup}(string?, string?)"/> where <c>null</c>.</param>
        /// <param name="services">The <see cref="Action{IServiceCollection}"/>.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        /// <returns>An <see cref="AgentTesterServer{TStartup}"/> instance.</returns>
        public static AgentTesterServer<TStartup> CreateServer<TStartup>(string? environmentVariablePrefix = null, string environment = TestSetUp.DefaultEnvironment, IConfiguration? config = null, Action<IServiceCollection>? services = null, bool configureLocalRefData = true)
            where TStartup : class => new AgentTesterServer<TStartup>(environmentVariablePrefix, environment, config, services, configureLocalRefData);

        /// <summary>
        /// Creates an <see cref="AgentTesterWaf{TStartup}"/> to manage the orchestration of the <see cref="WebApplicationFactory{TStartup}"/> to execute one or more integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="configuration">The <see cref="Action{IWebHostBuilder}"/>.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        /// <returns>An <see cref="AgentTesterWaf{TStartup}"/> instance.</returns>
        public static AgentTesterWaf<TStartup> CreateWaf<TStartup>(Action<IWebHostBuilder> configuration, bool configureLocalRefData = true) where TStartup : class => new AgentTesterWaf<TStartup>(configuration, null, null, configureLocalRefData);

        /// <summary>
        /// Creates an <see cref="AgentTesterWaf{TStartup}"/> to manage the orchestration of the <see cref="WebApplicationFactory{TStartup}"/> to execute one or more integration tests against enabling specific
        /// <b>API</b> <paramref name="services"/> to be configured/replaced (dependency injection).
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="services">The <see cref="Action{IServiceCollection}"/>.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        /// <returns>An <see cref="AgentTesterWaf{TStartup}"/> instance.</returns>
        public static AgentTesterWaf<TStartup> CreateWaf<TStartup>(Action<IServiceCollection>? services, bool configureLocalRefData = true) where TStartup : class => new AgentTesterWaf<TStartup>(null, null, services, configureLocalRefData);

        /// <summary>
        /// Creates an <see cref="AgentTesterWaf{TStartup}"/> to manage the orchestration of the <see cref="WebApplicationFactory{TStartup}"/> to execute one or more integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="services">The <see cref="Action{IServiceCollection}"/>.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        /// <returns>An <see cref="AgentTesterWaf{TStartup}"/> instance.</returns>
        public static AgentTesterWaf<TStartup> CreateWaf<TStartup>(string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment, Action<IServiceCollection>? services = null, bool configureLocalRefData = true)
            where TStartup : class => new AgentTesterWaf<TStartup>(environmentVariablePrefix, environment, services, configureLocalRefData);
    }
}