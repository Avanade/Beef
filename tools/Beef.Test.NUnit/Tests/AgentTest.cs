// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using Beef.WebApi;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the <b>Agent</b> test capabilities (specifically verifying the <see cref="WebApiAgentResult"/>).
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    //[DebuggerStepThrough()]
    public class AgentTest<TStartup, TAgent> : AgentTestBase<TStartup> where TStartup : class where TAgent : WebApiAgentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTest{TStartup, TAgent}"/> class with a username.
        /// </summary>
        /// <param name="agentTesterBase">The owning/parent <see cref="AgentTesterBase"/>.</param>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public AgentTest(AgentTesterBase agentTesterBase, string? username = null, object? args = null) : base(agentTesterBase, username, args) { }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectStatusCode(HttpStatusCode statusCode)
        {
            SetExpectStatusCode(statusCode);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message will not be checked.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectMessages(params string[] messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. No value comparison will occur. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectEvent(string template, string action)
        {
            SetExpectEvent(template, action);
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. The <paramref name="eventValue"/> will be compared against the <see cref="EventData{T}.Value"/>. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <typeparam name="T">The event value <see cref="Type"/>.</typeparam>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="eventValue">The <see cref="EventData{T}"/> value.</param>
        /// <param name="membersToIgnore">The members to ignore from the <paramref name="eventValue"/> comparison.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectEvent<T>(string template, string action, T eventValue, params string[] membersToIgnore)
        {
            SetExpectEvent<T>(false, template, action, eventValue, membersToIgnore);
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent> ExpectNoEvents()
        {
            SetExpectNoEvents();
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> with an automatically instantiated <typeparamref name="TAgent"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public WebApiAgentResult Run(Func<TAgent, Task<WebApiAgentResult>> func) => Task.Run(() => RunAsync(func)).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the <paramref name="func"/> with an automatically instantiated <typeparamref name="TAgent"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public async Task<WebApiAgentResult> RunAsync(Func<TAgent, Task<WebApiAgentResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return await RunOverrideAsync(() => func(CreateAgent<TAgent>())).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the <paramref name="func"/> where the agent is self-instantied and executed checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public WebApiAgentResult RunOverride(Func<Task<WebApiAgentResult>> func) => Task.Run(() => RunOverrideAsync(func)).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the <paramref name="func"/> where the agent is self-instantied and executed asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public async Task<WebApiAgentResult> RunOverrideAsync(Func<Task<WebApiAgentResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var sw = Stopwatch.StartNew();
            WebApiAgentResult result = await func().ConfigureAwait(false);
            sw.Stop();
            ResultCheck(result, sw);
            PublishedEventsCheck();
            return result;
        }
    }
}