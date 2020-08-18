// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Grpc;
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
    public abstract class AgentTesterBase : TesterBase
    {
        private Action<HttpRequestMessage>? _beforeRequest;
        private bool _beforeRequestOverridden;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterBase"/> class.
        /// </summary>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        protected AgentTesterBase(bool configureLocalRefData = true) : base(configureLocalRefData) 
        {
            ConfigureLocalServices(sc =>
            {
                sc.AddTransient(_ => GetHttpClient());
                sc.AddTransient(_ => GetBeforeRequest());
                sc.AddBeefAgentServices();
                sc.AddBeefGrpcAgentServices();
            });
        }

        /// <summary>
        /// Adds the <see cref="IReferenceDataProvider">reference data</see> as a singleton to the <i>local</i> (non-API) <see cref="IServiceCollection"/> (see <see cref="TesterBase.LocalServiceProvider"/>).
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
            ConfigureLocalServices(sc =>
            {
                sc.AddSingleton<TRefService, TRefProvider>();
                sc.AddSingleton<TRefAgentService, TRefAgent>();
            });
        }

        /// <summary>
        /// Registers the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending (overrides the <see cref="AgentTester.RegisterBeforeRequest(Action{HttpRequestMessage})"/>).
        /// </summary>
        /// <param name="beforeRequest">The before request action.</param>
        /// <returns>This instance to support fluent-style method-chaining.</returns>
        public void RegisterBeforeRequest(Action<HttpRequestMessage> beforeRequest)
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
    }
}