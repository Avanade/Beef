// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the base <b>Agent Tester</b> arguments.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    public abstract class AgentTesterRunArgsBase<TAgent> where TAgent : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterRunArgsBase{TAgent}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The action to run before the request is made.</param>
        internal AgentTesterRunArgsBase(HttpClient client, Action<HttpRequestMessage> beforeRequest)
        {
            Client = client;
            BeforeRequest = beforeRequest;
        }

        /// <summary>
        /// Gets the <see cref="HttpClient"/>.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Gets the action to run before the request is made.
        /// </summary>
        public Action<HttpRequestMessage> BeforeRequest { get; }

        /// <summary>
        /// Gets (creates) the <b>Agent</b> instance.
        /// </summary>
        public TAgent Agent => (TAgent)Activator.CreateInstance(typeof(TAgent), Client, BeforeRequest)!;
    }

    /// <summary>
    /// Provides the <see cref="AgentTester"/> arguments.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    public class AgentTesterRunArgs<TAgent> : AgentTesterRunArgsBase<TAgent> where TAgent : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterRunArgs{TAgent}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The action to run before the request is made.</param>
        /// <param name="tester">The executing <see cref="AgentTester"/>.</param>
        internal AgentTesterRunArgs(HttpClient client, Action<HttpRequestMessage> beforeRequest, AgentTester<TAgent> tester) : base(client, beforeRequest) => Tester = tester;

        /// <summary>
        /// Gets the executing <see cref="AgentTester"/>.
        /// </summary>
        public AgentTester<TAgent> Tester { get; }
    }

    /// <summary>
    /// Provides the <see cref="AgentTester{TValue}"/> arguments.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The response <see cref="Type"/>.</typeparam>
    public class AgentTesterRunArgs<TAgent, TValue> : AgentTesterRunArgsBase<TAgent> where TAgent : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterRunArgs{TAgent, TValue}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The action to run before the request is made.</param>
        /// <param name="tester">The executing <see cref="AgentTester"/>.</param>
        internal AgentTesterRunArgs(HttpClient client, Action<HttpRequestMessage> beforeRequest, AgentTester<TAgent, TValue> tester) : base(client, beforeRequest) => Tester = tester;

        /// <summary>
        /// Gets the executing <see cref="AgentTester"/>.
        /// </summary>
        public AgentTester<TAgent, TValue> Tester { get; }
    }
}