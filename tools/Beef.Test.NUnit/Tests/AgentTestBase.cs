// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Events;
using Beef.Test.NUnit.Logging;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the base <b>Agent</b> test capabilities.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [DebuggerStepThrough()]
    public abstract class AgentTestBase<TStartup> : AgentTestCore<TStartup> where TStartup : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestBase{TStartup}"/> class using the specified details.
        /// </summary>
        /// <param name="agentTesterBase">The owning/parent <see cref="Beef.Test.NUnit.Tests.AgentTesterBase"/>.</param>
        /// <param name="username">The username (<c>null</c> indicates to use the existing <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        protected AgentTestBase(AgentTesterBase agentTesterBase, string? username = null, object? args = null) : base(agentTesterBase, username, args) { }

        /// <summary>
        /// Check the result to make sure it is valid.
        /// </summary>
        /// <param name="result">The <see cref="WebApiAgentResult"/>.</param>
        /// <param name="sw">The <see cref="Stopwatch"/> used to measure <see cref="WebApiAgentBase"/> invocation.</param>
        protected void ResultCheck(WebApiAgentResult result, Stopwatch sw)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            // Log to output.
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine("AGENT TESTER...");
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"REQUEST >");
            TestContext.Out.WriteLine($"Request: {result.Request.Method} {result.Request.RequestUri}");

            if (!string.IsNullOrEmpty(Username))
                TestContext.Out.WriteLine($"Username: {Username}");

            TestContext.Out.WriteLine($"Headers: {(result.Request.Headers == null || !result.Request.Headers.Any() ? "none" : "")}");
            if (result.Request.Headers != null && result.Request.Headers.Any())
            {
                foreach (var hdr in result.Request.Headers)
                {
                    var sb = new StringBuilder();
                    foreach (var v in hdr.Value)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        sb.Append(v);
                    }

                    TestContext.Out.WriteLine($"  {hdr.Key}: {sb}");
                }
            }

            JToken? json = null;
            if (result.Request.Content != null)
            {
                // HACK: The Request Content is a forward only stream that is already read; we need to reset this private variable back to the start.
                if (result.Request.Content is StreamContent)
                {
                    var fi = typeof(StreamContent).GetField("_content", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    var ms = (MemoryStream)fi!.GetValue(result.Request.Content)!;
                    ms.Position = 0;
                }

                // Parse out the content.
                try
                {
                    json = JToken.Parse(result.Request.Content.ReadAsStringAsync().Result);
                }
                catch (Exception) { }

                TestContext.Out.WriteLine($"Content: [{result.Request.Content?.Headers?.ContentType?.MediaType ?? "None"}]");
                TestContext.Out.WriteLine(json == null ? result.Request.Content?.ToString() : json.ToString());
            }

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"RESPONSE >");
            TestContext.Out.WriteLine($"HttpStatusCode: {result.StatusCode} ({(int)result.StatusCode})");
            TestContext.Out.WriteLine($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");

            var hdrs = result.Response?.Headers?.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            TestContext.Out.WriteLine($"Headers: {(hdrs == null || !hdrs.Any() ? "none" : "")}");
            if (hdrs != null && hdrs.Any())
            {
                foreach (var hdr in hdrs)
                {
                    TestContext.Out.WriteLine($"  {hdr}");
                }
            }

            TestContext.Out.WriteLine($"Messages: {(result.Messages == null || result.Messages.Count == 0 ? "none" : "")}");

            if (result.Messages != null && result.Messages.Count > 0)
            {
                foreach (var m in result.Messages)
                {
                    TestContext.Out.WriteLine($" {m.Type}: {m.Text} {(m.Property == null ? "" : "(" + m.Property + ")")}");
                }

                TestContext.Out.WriteLine("");
            }

            json = null;
            if (!string.IsNullOrEmpty(result.Content) && result.Response?.Content?.Headers?.ContentType?.MediaType == MediaTypeNames.Application.Json)
            {
                try
                {
                    json = JToken.Parse(result.Content);
                }
                catch (Exception) { /* This is being swallowed by design. */ }
            }

            TestContext.Out.Write($"Content: [{result.Response?.Content?.Headers?.ContentType?.MediaType ?? "none"}]");
            if (json != null)
            {
                TestContext.Out.WriteLine("");
                TestContext.Out.WriteLine(json.ToString());
            }
            else
                TestContext.Out.WriteLine($"{(string.IsNullOrEmpty(result.Content) ? "none" : result.Content)}");

            var content = $"Content: {json ?? (string.IsNullOrEmpty(result.Content) ? "none" : result.Content)}";

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

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"LOGGING >");
            var messages = CorrelationIdLogger.GetMessages(CorrelationId, true);
            if (messages.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var l in messages)
                {
                    TesterBase.WriteTestContextLogMessage(l);
                }
            }

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine(new string('=', 80));
            TestContext.Out.WriteLine("");

            // Perform checks.
            if (_expectedStatusCode.HasValue && _expectedStatusCode != result.StatusCode)
                Assert.Fail($"Expected HttpStatusCode '{_expectedStatusCode} ({(int)_expectedStatusCode})'; actual {result.StatusCode} ({(int)result.StatusCode}).{Environment.NewLine}{Environment.NewLine}{content}");

            if (_expectedErrorType.HasValue && _expectedErrorType != result.ErrorType)
                Assert.Fail($"Expected ErrorType '{_expectedErrorType}'; actual '{result.ErrorType}'.{Environment.NewLine}{Environment.NewLine}{content}");

            if (_expectedErrorMessage != null && _expectedErrorMessage != result.ErrorMessage)
                Assert.Fail($"Expected ErrorMessage '{_expectedErrorMessage}'; actual '{result.ErrorMessage}'.{Environment.NewLine}{Environment.NewLine}{content}");

            if (_expectedMessages != null)
                TesterBase.CompareExpectedVsActualMessages(_expectedMessages, result.Messages);
        }

        /// <summary>
        /// Creates a <typeparamref name="TAgent"/> instance using the <see cref="TesterBase.LocalServiceProvider"/> where found; otherwise, will instantiate directly.
        /// </summary>
        /// <param name="args">The optional <see cref="IWebApiAgentArgs"/>; will default to <see cref="AgentTestCore{TStartup}.CreateAgentArgs"/>.</param>
        /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
        /// <returns>The <typeparamref name="TAgent"/> instance.</returns>
        public TAgent CreateAgent<TAgent>(IWebApiAgentArgs? args = null) where TAgent : WebApiAgentBase
        {
            var agent = AgentTesterBase.LocalServiceProvider.GetService<TAgent>();
            if (agent != null)
                return agent;

            var ctors = typeof(TAgent).GetConstructors();
            if (ctors.Length != 1)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the constructor could not be determined.");

            var pis = ctors[0].GetParameters();
            if (pis.Length != 1)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the constructor must only have a single parameter.");

            var pi = pis[0];
            if (pi.ParameterType != typeof(IWebApiAgentArgs) && !pi.ParameterType.GetInterfaces().Contains(typeof(IWebApiAgentArgs)))
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created; the constructor parameter must implement IWebApiAgentArgs.");

            var obj = Activator.CreateInstance(typeof(TAgent), args ?? CreateAgentArgs(pi.ParameterType));
            if (obj == null)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created.");

            return (TAgent)obj;
        }
    }
}