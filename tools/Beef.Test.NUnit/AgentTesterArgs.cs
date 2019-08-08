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
        /// Gets the <see cref="HttpClient"/>.
        /// </summary>
        public HttpClient Client { get; internal set; }

        /// <summary>
        /// Gets the action to run before the request is made.
        /// </summary>
        public Action<HttpRequestMessage> BeforeRequest { get; internal set; }

        /// <summary>
        /// Gets (creates) the <b>Agent</b> instance.
        /// </summary>
        public TAgent Agent => (TAgent)Activator.CreateInstance(typeof(TAgent), Client, BeforeRequest);
    }

    /// <summary>
    /// Provides the <see cref="AgentTester"/> arguments.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    public class AgentTesterRunArgs<TAgent> : AgentTesterRunArgsBase<TAgent> where TAgent : class
    {
        /// <summary>
        /// Gets the executing <see cref="AgentTester"/>.
        /// </summary>
        public AgentTester<TAgent> Tester { get; internal set; }
    }

    /// <summary>
    /// Provides the <see cref="AgentTester{TValue}"/> arguments.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The response <see cref="Type"/>.</typeparam>
    public class AgentTesterRunArgs<TAgent, TValue> : AgentTesterRunArgsBase<TAgent> where TAgent : class
    {
        /// <summary>
        /// Gets the executing <see cref="AgentTester"/>.
        /// </summary>
        public AgentTester<TAgent, TValue> Tester { get; internal set; }
    }
}