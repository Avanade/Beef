// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Grpc;
using Beef.RefData;
using Beef.Test.NUnit.Logging;
using Beef.Test.NUnit.Tests;
using Beef.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Manages the orchestration of the <see cref="WebApplicationFactory"/> to execute one or more integration tests against. 
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    public class AgentTesterWaf<TStartup> : AgentTesterBase, IDisposable where TStartup : class
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApplicationFactory{TStartup}"/> class enabling configuration of the <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="IWebHostBuilder"/>.</param>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Class has a Dispose method.")]
        internal AgentTesterWaf(Action<IWebHostBuilder> configuration, string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment, bool configureLocalRefData = true) : base(configureLocalRefData)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var action = new Action<IWebHostBuilder>(whb =>
            {
                configuration(whb);

                whb.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(AgentTester.BuildConfiguration<TStartup>(environmentVariablePrefix, environment));
                });

                whb.ConfigureServices(sc =>
                {
                    sc.AddLogging(configure => { configure.ClearProviders(); configure.AddCorrelationId(); }); 
                });

                whb.ConfigureTestServices(sc =>
                {
                    ReplaceEventPublisher(sc);
                });
            }); 

            WebApplicationFactory = new WebApplicationFactory<TStartup>().WithWebHostBuilder(action);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApplicationFactory{TStartup}"/> class with default <see cref="IWebHostBuilder"/> configuration optionally enabling specific
        /// <b>API</b> <paramref name="services"/> to be configured/replaced (dependency injection).
        /// </summary>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        internal AgentTesterWaf(string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment, Action<IServiceCollection>? services = null, bool configureLocalRefData = true) 
            : this(new Action<IWebHostBuilder>(whb => whb.ConfigureTestServices(sc => services?.Invoke(sc))), environmentVariablePrefix, environment, configureLocalRefData) { }

        /// <summary>
        /// Provides the opportunity to further configure the <i>local</i> (non-API) test <see cref="IServiceCollection"/> (see <see cref="TesterBase.LocalServiceProvider"/>).
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public new AgentTesterWaf<TStartup> ConfigureLocalServices(Action<IServiceCollection> serviceCollection) { base.ConfigureLocalServices(serviceCollection); return this; }

        /// <summary>
        /// Adds the <see cref="IReferenceDataProvider">reference data</see> as a singleton to the <i>local</i> (non-API) <see cref="IServiceCollection"/> (see <see cref="TesterBase.LocalServiceProvider"/>).
        /// </summary>
        /// <typeparam name="TRefService">The <see cref="Type"/> of the service to add.</typeparam>
        /// <typeparam name="TRefProvider">The <see cref="Type"/> of the <i>provider</i> implementation to use.</typeparam>
        /// <typeparam name="TRefAgentService">The <see cref="Type"/> of the <i>agent</i> service to add.</typeparam>
        /// <typeparam name="TRefAgent">The <see cref="Type"/> of the <i>agent</i> implementation to use.</typeparam>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public new AgentTesterWaf<TStartup> ConfigureReferenceData<TRefService, TRefProvider, TRefAgentService, TRefAgent>()
            where TRefService : class, IReferenceDataProvider where TRefProvider : class, TRefService
            where TRefAgentService : class where TRefAgent : WebApiAgentBase, TRefAgentService
        {
            base.ConfigureReferenceData<TRefService, TRefProvider, TRefAgentService, TRefAgent>(); return this;
        }

        /// <summary>
        /// Prepares (creates) the <see cref="ExecutionContext"/> and ensures that the <see cref="TesterBase.LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="TestSetUp.CreateExecutionContext(string?, object?)"/>).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>The <see cref="ExecutionContext"/> must be created by the <see cref="AgentTesterServer{TStartup}"/> as the <see cref="ExecutionContext.ServiceProvider"/> must be set to <see cref="TesterBase.LocalServiceProvider"/>.</remarks>
        public new AgentTesterWaf<TStartup> PrepareExecutionContext(string? username = null, object? args = null) { base.PrepareExecutionContext(username, args); return this; }

        /// <summary>
        /// Gets an <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        public override HttpClient GetHttpClient() => WebApplicationFactory.CreateClient();

        /// <summary>
        /// Registers the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending (overrides the <see cref="AgentTester.RegisterBeforeRequest"/>).
        /// </summary>
        /// <param name="beforeRequest">The before request action.</param>
        /// <returns>This instance to support fluent-style method-chaining.</returns>
        public new AgentTesterWaf<TStartup> RegisterBeforeRequest(Action<HttpRequestMessage> beforeRequest) { base.RegisterBeforeRequest(beforeRequest); return this; }

        /// <summary>
        /// Gets the underlying <see cref="WebApplicationFactory{TStartup}"/>.
        /// </summary>
        public WebApplicationFactory<TStartup> WebApplicationFactory { get; private set; }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AgentTesterWaf{TStartup}"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    WebApplicationFactory.Dispose();

                _disposed = true;
            }
        }

        /// <summary>
        /// Closes and disposes the <see cref="AgentTesterWaf{TStartup}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #region Create

        /// <summary>
        /// Create a new <see cref="AgentTest{TStartup, TAgent}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/> (see <see cref="WebApiAgentBase"/>).</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTest{TStartup, TAgent}"/> instance.</returns>
        public AgentTest<TStartup, TAgent> Test<TAgent>(string? username = null, object? args = null) where TAgent : WebApiAgentBase
        {
            return new AgentTest<TStartup, TAgent>(this, username, args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTest{TStartup, TAgent, TValue}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/> (see <see cref="WebApiAgentBase"/>).</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance</returns>
        public AgentTest<TStartup, TAgent, TValue> Test<TAgent, TValue>(string? username = null, object? args = null) where TAgent : WebApiAgentBase
        {
            return new AgentTest<TStartup, TAgent, TValue>(this, username, args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTest{TStartup, TAgent}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTest{TStartup, TAgent}"/> instance.</returns>
        public AgentTest<TStartup, TAgent> Test<TAgent>(object? userIdentifier, object? args = null) where TAgent : WebApiAgentBase
        {
            return new AgentTest<TStartup, TAgent>(this, TestSetUp.ConvertUsername(userIdentifier), args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTest{TStartup, TAgent, TValue}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance</returns>
        public AgentTest<TStartup, TAgent, TValue> Test<TAgent, TValue>(object userIdentifier, object? args = null) where TAgent : WebApiAgentBase
        {
            return new AgentTest<TStartup, TAgent, TValue>(this, TestSetUp.ConvertUsername(userIdentifier), args);
        }

        #endregion

        #region CreateGrpc

        /// <summary>
        /// Create a new <see cref="GrpcAgentTest{TStartup, TAgent}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/> (see <see cref="GrpcAgentBase"/>).</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="GrpcAgentTest{TStartup, TResult}"/> instance.</returns>
        public GrpcAgentTest<TStartup, TAgent> TestGrpc<TAgent>(string? username = null, object? args = null) where TAgent : GrpcAgentBase
        {
            return new GrpcAgentTest<TStartup, TAgent>(this, username, args);
        }

        /// <summary>
        /// Create a new <see cref="GrpcAgentTest{TStartup, TAgent, TValue}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/> (see <see cref="GrpcAgentBase"/>).</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="GrpcAgentTest{TStartup, TAgent, TValue}"/> instance.</returns>
        public GrpcAgentTest<TStartup, TAgent, TValue> TestGrpc<TAgent, TValue>(string? username = null, object? args = null) where TAgent : GrpcAgentBase
        {
            return new GrpcAgentTest<TStartup, TAgent, TValue>(this, username, args);
        }

        /// <summary>
        /// Create a new <see cref="GrpcAgentTest{TStartup, TAgent}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="GrpcAgentTest{TStartup, TResult}"/> instance.</returns>
        public GrpcAgentTest<TStartup, TAgent> TestGrpc<TAgent>(object? userIdentifier, object? args = null) where TAgent : GrpcAgentBase
        {
            return new GrpcAgentTest<TStartup, TAgent>(this, TestSetUp.ConvertUsername(userIdentifier), args);
        }

        /// <summary>
        /// Create a new <see cref="GrpcAgentTest{TStartup, TAgent, TValue}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="GrpcAgentTest{TStartup, TAgent, TValue}"/> instance</returns>
        public GrpcAgentTest<TStartup, TAgent, TValue> TestGrpc<TAgent, TValue>(object userIdentifier, object? args = null) where TAgent : GrpcAgentBase
        {
            return new GrpcAgentTest<TStartup, TAgent, TValue>(this, TestSetUp.ConvertUsername(userIdentifier), args);
        }

        #endregion
    }
}