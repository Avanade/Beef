// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Simplifies the testing using an <see cref="AgentTesterServer{TStartup}"/> using system-wide defaults. Automatically manages the instantiation and disposing through the one-time setup and tear-down.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class UsingAgentTesterServer<TStartup> : ITestSetupPrepareExecutionContext where TStartup : class
    {
        private AgentTesterServer<TStartup>? _agentTester;
        private readonly string? _environmentVariablePrefix;
        private readonly string _environment;
        private readonly IConfiguration? _config;
        private readonly Action<IServiceCollection>? _services;
        private readonly bool _configureLocalRefData;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsingAgentTesterServer{TStartup}"/> class.
        /// </summary>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="config">The <see cref="IConfiguration"/>; defaults to <see cref="AgentTester.BuildConfiguration{TStartup}(string?, string?)"/> where <c>null</c>.</param>
        /// <param name="services">An optional action to perform further <see cref="IServiceCollection"/> configuration.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        protected UsingAgentTesterServer(string? environmentVariablePrefix = null, string environment = TestSetUp.DefaultEnvironment, IConfiguration? config = null, Action<IServiceCollection>? services = null, bool configureLocalRefData = true)
        {
            _environmentVariablePrefix = environmentVariablePrefix;
            _environment = environment;
            _config = config;
            _services = services;
            _configureLocalRefData = configureLocalRefData;
        }

        /// <summary>
        /// Gets the underlying <see cref="AgentTesterBase"/>.
        /// </summary>
        AgentTesterBase ITestSetupPrepareExecutionContext.AgentTester => AgentTester;

        /// <summary>
        /// Gets the underling <see cref="AgentTesterServer{TStartup}"/>.
        /// </summary>
        public AgentTesterServer<TStartup> AgentTester => _agentTester ?? throw new InvalidOperationException("The OneTimeSetUp must have been invoked prior to instantiate.");

        /// <summary>
        /// One-time setup. Invokes the <see cref="TestSetUp.Reset(bool, object?)"/> with <c>true</c> and <c>null</c> arguments, and instantiates the <see cref="AgentTester"/> using system-wide defaults.
        /// </summary>
        [OneTimeSetUp]
        public void UsingOneTimeSetUp()
        {
            TestSetUp.Reset(true, null);
            _agentTester = new AgentTesterServer<TStartup>(_environmentVariablePrefix, _environment, _config, _services, _configureLocalRefData);
        }

        /// <summary>
        /// One-time tear-down. Disposes of the <see cref="AgentTester"/>.
        /// </summary>
        [OneTimeTearDown]
        public void UsingOneTimeTearDown() => _agentTester?.Dispose();

        /// <summary>
        /// Prepares (creates) the <see cref="ExecutionContext"/> and ensures that the <see cref="TesterBase.LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="TestSetUp.CreateExecutionContext(string?, object?)"/>).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>The <see cref="ExecutionContext"/> must be created by the <see cref="AgentTesterServer{TStartup}"/> as the <see cref="ExecutionContext.ServiceProvider"/> must be set to <see cref="TesterBase.LocalServiceProvider"/>.</remarks>
        public void PrepareExecutionContext(string? username = null, object? args = null) => AgentTester.PrepareExecutionContext(username, args);

        /// <summary>
        /// Builds the configuration probing as per <see cref="NUnit.AgentTester.BuildConfiguration{TStartup}(string?, string?)"/>.
        /// </summary>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied). Defaults to <see cref="TestSetUp.DefaultEnvironmentVariablePrefix"/></param>
        /// <param name="environment">The environment to be used by the underlying web host. Defaults to <see cref="TestSetUp.DefaultEnvironment"/>.</param>
        /// <returns>The <see cref="IConfiguration"/>.</returns>
        public IConfiguration BuildConfiguration(string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment)
            => NUnit.AgentTester.BuildConfiguration<TStartup>(environmentVariablePrefix, environment);

        /// <summary>
        /// Expects and asserts an <see cref="Exception"/> and its corresponding message.
        /// </summary>
        /// <remarks>Automatically invokes <see cref="PrepareExecutionContext(string?, object?)"/> where <see cref="ExecutionContext.HasCurrent"/> is <c>false</c>.</remarks>
        public ExpectExceptionTest ExpectException
        {
            get
            {
                if (!ExecutionContext.HasCurrent)
                    PrepareExecutionContext();

                return new ExpectExceptionTest();
            }
        }

        /// <summary>
        /// Provides the opportunity to further configure the <i>local</i> (non-API) test <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        protected void ConfigureLocalServices(Action<IServiceCollection> serviceCollection) => AgentTester.ConfigureLocalServices(serviceCollection);
    }
}