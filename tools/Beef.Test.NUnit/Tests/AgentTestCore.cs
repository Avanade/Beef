// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using Beef.Test.NUnit.Events;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the core <b>Agent</b> test capabilities.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class AgentTestCore<TStartup> where TStartup : class
    {
        internal HttpStatusCode? _expectedStatusCode;
        internal ErrorType? _expectedErrorType;
        internal string? _expectedErrorMessage;
        internal MessageItemCollection? _expectedMessages;
        internal readonly List<(ExpectedEvent expectedEvent, bool useReturnedValue)> _expectedPublished = new List<(ExpectedEvent, bool)>();
        internal bool _expectedNonePublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestCore{TStartup}"/> class using the specified details.
        /// </summary>
        /// <param name="agentTesterBase">The owning/parent <see cref="Beef.Test.NUnit.Tests.AgentTesterBase"/>.</param>
        /// <param name="username">The username (<c>null</c> indicates to use the existing <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        protected AgentTestCore(AgentTesterBase agentTesterBase, string? username = null, object? args = null)
        {
            AgentTesterBase = agentTesterBase ?? throw new ArgumentNullException(nameof(agentTesterBase));
            if (!ExecutionContext.HasCurrent || ExecutionContext.Current.Username != username)
            {
                if (username == null)
                    AgentTesterBase.PrepareExecutionContext();
                else
                    AgentTesterBase.PrepareExecutionContext(username, args);
            }

            ExecutionContext.Current.CorrelationId = CorrelationId;

            Args = args;
            Username = ExecutionContext.Current.Username;

            if (TestSetUp.DefaultExpectNoEvents)
                SetExpectNoEvents();
        }

        /// <summary>
        /// Gets the owning/parent <see cref="Beef.Test.NUnit.Tests.AgentTesterBase"/>.
        /// </summary>
        protected AgentTesterBase AgentTesterBase { get; private set; }

        /// <summary>
        /// Gets the unique correlation identifier that is sent via the agent to underlying API.
        /// </summary>
        public string CorrelationId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Gets the optional argument.
        /// </summary>
        public object? Args { get; private set; }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        protected void SetExpectStatusCode(HttpStatusCode statusCode) => _expectedStatusCode = statusCode;

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message text will not be checked.</param>
        protected void SetExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            _expectedErrorType = errorType;
            _expectedErrorMessage = errorMessage;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        protected void SetExpectMessages(params string[] messages)
        {
            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            SetExpectMessages(mic);
        }

        /// <summary>
        /// Expect a response with the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        protected void SetExpectMessages(MessageItemCollection messages)
        {
            Check.NotNull(messages, nameof(messages));
            if (_expectedMessages == null)
                _expectedMessages = new MessageItemCollection();

            _expectedMessages.AddRange(messages);
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. No value comparison will occur. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        protected void SetExpectEvent(string template, string action)
        {
            _expectedPublished.Add((new ExpectedEvent(new EventData { Subject = template, Action = action }), false));
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. The <paramref name="eventValue"/> will be compared against the <see cref="EventData{T}.Value"/>. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <typeparam name="T">The event value <see cref="Type"/>.</typeparam>
        /// <param name="useReturnedValue">Indicates whether to use the returned value.</param>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="eventValue">The <see cref="EventData{T}"/> value.</param>
        /// <param name="membersToIgnore">The members to ignore from the <paramref name="eventValue"/> comparison.</param>
        protected void SetExpectEvent<T>(bool useReturnedValue, string template, string action, T eventValue, params string[] membersToIgnore)
        {
            var ee = new ExpectedEvent(new EventData<T> { Subject = template, Action = action, Value = eventValue });
            ee.MembersToIgnore.AddRange(membersToIgnore);
            _expectedPublished.Add((ee, useReturnedValue));
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        protected void SetExpectNoEvents()
        {
            _expectedNonePublished = true;
        }

        /// <summary>
        /// Check the published events to make sure they are valid.
        /// </summary>
        /// <param name="eventNeedingValueUpdateAction">Action that will be called where the value needs to be updated.</param>
        protected void PublishedEventsCheck(Action<ExpectedEvent>? eventNeedingValueUpdateAction = null)
        {
            if (_expectedPublished.Count > 0)
            {
                foreach (var ee in _expectedPublished.Where((v) => v.useReturnedValue).Select((v) => v.expectedEvent))
                {
                    eventNeedingValueUpdateAction?.Invoke(ee);
                }

                ExpectEvent.ArePublished(_expectedPublished.Select((v) => v.expectedEvent).ToList());
            }
            else if (_expectedNonePublished)
                ExpectEvent.NonePublished();

            ExpectEventPublisher.Remove();
        }

        /// <summary>
        /// Creates an <see cref="IWebApiAgentArgs"/> instance using the <see cref="TesterBase.LocalServiceProvider"/>.
        /// </summary>
        /// <returns>An <see cref="IWebApiAgentArgs"/> instance.</returns>
        public IWebApiAgentArgs CreateAgentArgs(Type type)
        {
            if (AgentTesterBase.LocalServiceProvider.GetService(type ?? throw new ArgumentNullException(nameof(type))) is IWebApiAgentArgs args)
                return args;

            return AgentTesterBase.LocalServiceProvider.GetService<IWebApiAgentArgs>() ?? throw new InvalidOperationException($"An instance of {type.Name} was unable to be created.");
        }
    }
}