// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using Beef.Events;
using Beef.Test.NUnit.Events;
using Beef.Test.NUnit.Logging;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Represents the generic base tester.
    /// </summary>
    public abstract class TesterBase
    {
        private readonly ServiceCollection _serviceCollection = new ServiceCollection();
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterBase"/> class.
        /// </summary>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        protected TesterBase(bool configureLocalRefData = true)
        {
            _serviceCollection.AddLogging(configure => configure.AddTestContext());
            _serviceCollection.AddSingleton(_ => new CachePolicyManager());
            _serviceCollection.AddTransient<IWebApiAgentArgs, WebApiAgentArgs>();

            if (configureLocalRefData)
                TestSetUp.ConfigureDefaultLocalReferenceData(_serviceCollection);
        }

        /// <summary>
        /// Provides the opportunity to further configure the <i>local</i> (non-API) test <see cref="IServiceCollection"/> (see <see cref="LocalServiceProvider"/>).
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        protected void ConfigureLocalServices(Action<IServiceCollection> serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection(_serviceCollection);
                _serviceProvider = null;
            }
        }

        /// <summary>
        /// Gets the <i>local</i> (non-API) test Service Provider (used for the likes of the service agents).
        /// </summary>
        public IServiceProvider LocalServiceProvider => _serviceProvider ??= _serviceCollection.BuildServiceProvider();

        /// <summary>
        /// Prepares the <see cref="ExecutionContext"/> using the <see cref="TestSetUpAttribute"/> configuration, whilst also ensuring that the <see cref="LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        public void PrepareExecutionContext()
        {
            ExecutionContext.Reset(false);
            var ec = TestSetUp.CreateExecutionContext(TestSetUpAttribute.Username, TestSetUpAttribute.Args);
            ec.ServiceProvider = LocalServiceProvider;
            ExecutionContext.SetCurrent(ec);
        }

        /// <summary>
        /// Prepares the <see cref="ExecutionContext"/> using the specified <paramref name="username"/>, whilst also ensuring that the <see cref="LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="TestSetUp.CreateExecutionContext"/> function).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>The <see cref="ExecutionContext"/> must be created by the <see cref="AgentTesterBase"/> as the <see cref="ExecutionContext.ServiceProvider"/> must be set to <see cref="LocalServiceProvider"/>.</remarks>
        public void PrepareExecutionContext(string? username, object? args = null)
        {
            ExecutionContext.Reset(false);
            var ec = TestSetUp.CreateExecutionContext(username ?? TestSetUp.DefaultUsername, args);
            ec.ServiceProvider = LocalServiceProvider;
            ExecutionContext.SetCurrent(ec);
        }

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