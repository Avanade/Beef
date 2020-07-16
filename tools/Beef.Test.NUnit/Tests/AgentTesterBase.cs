// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using Beef.Events;
using Beef.RefData;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the base agent tester capabilities.
    /// </summary>
    public abstract class AgentTesterBase
    {
        private readonly ServiceCollection _serviceCollection = new ServiceCollection();
        private IServiceProvider? _serviceProvider;
        private Action<HttpRequestMessage>? _beforeRequest;
        private bool _beforeRequestOverridden;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterBase"/> class.
        /// </summary>
        protected AgentTesterBase()
        {
            _serviceCollection.AddSingleton(_ => new CachePolicyManager());
            _serviceCollection.AddTransient(_ => GetHttpClient());
            _serviceCollection.AddTransient(_ => GetBeforeRequest());
            _serviceCollection.AddTransient<IWebApiAgentArgs, WebApiAgentArgs>();
        }

        /// <summary>
        /// Gets the <i>local</i> (non-API) test Service Provider (used for the likes of the service agents).
        /// </summary>
        public IServiceProvider LocalServiceProvider => _serviceProvider ??= _serviceCollection.BuildServiceProvider();

        /// <summary>
        /// Provides the opportunity to further configure the <i>local</i> (non-API) test <see cref="IServiceCollection"/> (see <see cref="LocalServiceProvider"/>).
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public void ConfigureLocalServices(Action<IServiceCollection> serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection(_serviceCollection);
                _serviceProvider = null;
            }
        }

        /// <summary>
        /// Adds the <see cref="IReferenceDataProvider">reference data</see> as a singleton to the <i>local</i> (non-API) <see cref="IServiceCollection"/> (see <see cref="LocalServiceProvider"/>).
        /// </summary>
        /// <typeparam name="TRefService">The <see cref="Type"/> of the <i>provider</i> service to add.</typeparam>
        /// <typeparam name="TRefProvider">The <see cref="Type"/> of the <i>provider</i> implementation to use.</typeparam>
        /// <typeparam name="TRefAgentService">The <see cref="Type"/> of the <i>agent</i> service to add.</typeparam>
        /// <typeparam name="TRefAgent">The <see cref="Type"/> of the <i>agent</i> implementation to use.</typeparam>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public void ConfigureReferenceData<TRefService, TRefProvider, TRefAgentService, TRefAgent>()
            where TRefService : class, IReferenceDataProvider where TRefProvider : class, TRefService
            where TRefAgentService : class where TRefAgent : WebApiAgentBase, TRefAgentService
        {
            _serviceCollection.AddSingleton<TRefService, TRefProvider>();
            _serviceCollection.AddSingleton<TRefAgentService, TRefAgent>();
            _serviceProvider = null;
        }

        /// <summary>
        /// Prepares the <see cref="ExecutionContext"/> and ensures that the <see cref="LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="AgentTester.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="AgentTester.CreateExecutionContext"/> function).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public void Prepare(string? username = null, object? args = null)
        {
            AgentTester.InvokeRegisteredSetUp();

            ExecutionContext.Reset(false);
            var ec = AgentTester.CreateExecutionContext(username ?? AgentTester.DefaultUsername, args);
            ec.ServiceProvider = LocalServiceProvider;
            ExecutionContext.SetCurrent(ec);
        }

        /// <summary>
        /// Registers the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending (overrides the <see cref="AgentTester.SetBeforeRequest(Action{HttpRequestMessage})"/>).
        /// </summary>
        /// <param name="beforeRequest">The before request action.</param>
        /// <returns>This instance to support fluent-style method-chaining.</returns>
        public void SetBeforeRequest(Action<HttpRequestMessage> beforeRequest)
        {
            _beforeRequest = beforeRequest;
            _beforeRequestOverridden = true;
        }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending.
        /// </summary>
        public Action<HttpRequestMessage>? GetBeforeRequest() => _beforeRequestOverridden ? _beforeRequest : AgentTester.GetBeforeRequest();

        /// <summary>
        /// Gets an <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        public abstract HttpClient GetHttpClient();

        /// <summary>
        /// Replaces the <see cref="IEventPublisher"/> with the <see cref="ExpectEvent.EventPublisher"/> instance (as a singleton).
        /// </summary>
        /// <param name="sc">The <see cref="IServiceCollection"/>.</param>
        protected static void ReplaceEventPublisher(IServiceCollection sc)
        {
            sc.Remove<IEventPublisher>();
            sc.AddSingleton<IEventPublisher>(_ => ExpectEvent.EventPublisher);
        }
    }
}