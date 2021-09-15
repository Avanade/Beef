// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Grpc;
using Beef.Test.NUnit.Events;
using Beef.WebApi;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the base gRPC <b>Agent</b> test capabilities.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [DebuggerStepThrough()]
    public abstract class GrpcAgentTestBase<TStartup> : AgentTestCore<TStartup> where TStartup : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentTestBase{TAgent}"/> class with a username.
        /// </summary>
        /// <param name="agentTesterBase">The owning/parent <see cref="Beef.Test.NUnit.Tests.AgentTesterBase"/>.</param>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        protected GrpcAgentTestBase(AgentTesterBase agentTesterBase, string? username = null, object? args = null) : base(agentTesterBase, username, args) { }

        /// <summary>
        /// Check the result to make sure it is valid.
        /// </summary>
        /// <param name="result">The <see cref="GrpcAgentResult"/>.</param>
        /// <param name="sw">The <see cref="Stopwatch"/> used to measure <see cref="GrpcAgentBase{TClient}"/> invocation.</param>
        protected void ResultCheck(GrpcAgentResult result, Stopwatch sw)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            // Log to output.
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine("GRPC AGENT TESTER...");
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"REQUEST >");
            if (!string.IsNullOrEmpty(Username))
                TestContext.Out.WriteLine($"Username: {Username}");

            TestContext.Out.WriteLine($"gRPC Request: {(result.Request == null ? "null" : $"{result.Request.CalculateSize()}bytes [JSON representation]")}");
            if (result.Request != null)
                TestContext.Out.WriteLine(JsonConvert.SerializeObject(result.Request, Formatting.Indented));

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"RESPONSE >");
            if (result.Status.Detail == null)
                TestContext.Out.WriteLine($"gRPC Status: {result.Status.StatusCode}");
            else
                TestContext.Out.WriteLine($"gRPC Status: {result.Status.StatusCode} ({result.Status.Detail})");

            TestContext.Out.WriteLine($"HttpStatusCode: {result.StatusCode} ({(int)result.StatusCode})");
            TestContext.Out.WriteLine($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            TestContext.Out.WriteLine($"Messages: {(result.Messages == null || result.Messages.Count == 0 ? "none" : "")}");

            if (result.Messages != null && result.Messages.Count > 0)
            {
                foreach (var m in result.Messages)
                {
                    TestContext.Out.WriteLine($" {m.Type}: {m.Text} {(m.Property == null ? "" : "(" + m.Property + ")")}");
                }

                TestContext.Out.WriteLine();
            }

            var bytes = (result.Response is IMessage respm) ? respm.CalculateSize() : 0;

            TestContext.Out.WriteLine($"gRPC Response: {(result.Response == null ? "null" : $"{bytes}bytes [JSON representation]")}");
            if (result.Response != null)
                TestContext.Out.WriteLine(JsonConvert.SerializeObject(result.Response, Formatting.Indented));

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"EVENTS PUBLISHED >");
            var events = ExpectEvent.GetPublishedEvents(CorrelationId);
            if (events.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var e in events)
                {
                    TestContext.Out.WriteLine($"  Subject: {e.Subject ?? "<null>"}, Action: {e.Action ?? "<null>"}, Source: {e.Source?.ToString() ?? "<null>"}");
                }
            }

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"EVENTS SENT (Send invocation count: {Events.ExpectEvent.GetSendCount(CorrelationId)}) >");
            events = ExpectEvent.GetSentEvents(CorrelationId);
            if (events.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var e in events)
                {
                    TestContext.Out.WriteLine($"  Subject: {e.Subject ?? "<null>"}, Action: {e.Action ?? "<null>"}, Source: {e.Source?.ToString() ?? "<null>"}");
                }
            }

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine(new string('=', 80));
            TestContext.Out.WriteLine();

            // Perform checks.
            if (_expectedStatusCode.HasValue && _expectedStatusCode != result.StatusCode)
                Assert.Fail($"Expected HttpStatusCode was '{_expectedStatusCode} ({(int)_expectedStatusCode})'; actual was {result.StatusCode} ({(int)result.StatusCode}).");

            if (_expectedErrorType.HasValue && _expectedErrorType != result.ErrorType)
                Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{result.ErrorType}'.");

            if (_expectedErrorMessage != null && _expectedErrorMessage != result.ErrorMessage)
                Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{result.ErrorMessage}'.");

            if (_expectedMessages != null)
                TesterBase.CompareExpectedVsActualMessages(_expectedMessages, result.Messages);
        }

        /// <summary>
        /// Creates a <typeparamref name="TAgent"/> instance using the <see cref="TesterBase.LocalServiceProvider"/> where found; otherwise, will instantiate directly.
        /// </summary>
        /// <param name="args">The optional <see cref="IWebApiAgentArgs"/>; will default to <see cref="AgentTestCore{TStartup}.CreateAgentArgs"/>.</param>
        /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
        /// <returns>The <typeparamref name="TAgent"/> instance.</returns>
        public TAgent CreateAgent<TAgent>(IWebApiAgentArgs? args = null) where TAgent : GrpcAgentBase
        {
            var agent = AgentTesterBase.LocalServiceProvider.GetService<TAgent>();
            if (agent != null)
                return agent;

            var ctors = typeof(TAgent).GetConstructors();
            if (ctors.Length != 1)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the constructor could not be determined.");

            var pis = ctors[0].GetParameters();
            if (pis.Length != 2)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the constructor must have two parameters.");

            var pi = pis[0];
            if (pi.ParameterType != typeof(IWebApiAgentArgs) && !pi.ParameterType.GetInterfaces().Contains(typeof(IWebApiAgentArgs)))
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the first constructor parameter must implement IWebApiAgentArgs.");

            var pi2 = pis[1];
            if (pi2.ParameterType != typeof(AutoMapper.IMapper))
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the second constructor parameter must be of Type AutoMapper.IMapper.");

            var obj = Activator.CreateInstance(typeof(TAgent), args ?? CreateAgentArgs(pi.ParameterType), AgentTesterBase.LocalServiceProvider.GetService<AutoMapper.IMapper>()
                ?? throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the LocalServiceProvider is unable to construct the required AutoMapper.IMapper parameter instance."));

            if (obj == null)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created.");

            return (TAgent)obj;
        }
    }
}