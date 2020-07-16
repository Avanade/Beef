// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Grpc;
using Beef.WebApi;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the base gRPC <b>Agent</b> test capabilities.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    //[DebuggerStepThrough()]
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
            Logger.Default.Info("");
            Logger.Default.Info("GRPC AGENT TESTER...");
            Logger.Default.Info("");
            Logger.Default.Info($"REQUEST >");
            if (!string.IsNullOrEmpty(Username))
                Logger.Default.Info($"Username: {Username}");

            Logger.Default.Info($"gRPC Request: {(result.Request == null ? "null" : $"{result.Request.CalculateSize()}bytes [JSON representation]")}");
            if (result.Request != null)
                Logger.Default.Info(JsonConvert.SerializeObject(result.Request, Formatting.Indented));

            Logger.Default.Info("");
            Logger.Default.Info($"RESPONSE >");
            if (result.Status.Detail == null)
                Logger.Default.Info($"gRPC Status: {result.Status.StatusCode}");
            else
                Logger.Default.Info($"gRPC Status: {result.Status.StatusCode} ({result.Status.Detail})");

            Logger.Default.Info($"HttpStatusCode: {result.HttpStatusCode} ({(int)result.HttpStatusCode})");
            Logger.Default.Info($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            Logger.Default.Info($"Messages: {(result.Messages == null || result.Messages.Count == 0 ? "none" : "")}");

            if (result.Messages != null && result.Messages.Count > 0)
            {
                foreach (var m in result.Messages)
                {
                    Logger.Default.Info($" {m.Type}: {m.Text} {(m.Property == null ? "" : "(" + m.Property + ")")}");
                }

                Logger.Default.Info(null);
            }

            var bytes = (result.Response is IMessage respm) ? respm.CalculateSize() : 0;

            Logger.Default.Info($"gRPC Response: {(result.Response == null ? "null" : $"{bytes}bytes [JSON representation]")}");
            if (result.Response != null)
                Logger.Default.Info(JsonConvert.SerializeObject(result.Response, Formatting.Indented));

            Logger.Default.Info("");
            Logger.Default.Info($"EVENTS PUBLISHED >");
            var events = ExpectEvent.GetEvents();
            if (events.Count == 0)
                Logger.Default.Info("  None.");
            else
            {
                foreach (var e in events)
                {
                    Logger.Default.Info($"  Subject: {e.Subject}, Action: {e.Action}");
                }
            }

            Logger.Default.Info(null);
            Logger.Default.Info(new string('=', 80));
            Logger.Default.Info(null);

            // Perform checks.
            if (_expectedStatusCode.HasValue && _expectedStatusCode != result.HttpStatusCode)
                Assert.Fail($"Expected HttpStatusCode was '{_expectedStatusCode} ({(int)_expectedStatusCode})'; actual was {result.HttpStatusCode} ({(int)result.HttpStatusCode}).");

            if (_expectedErrorType.HasValue && _expectedErrorType != result.ErrorType)
                Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{result.ErrorType}'.");

            if (_expectedErrorMessage != null && _expectedErrorMessage != result.ErrorMessage)
                Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{result.ErrorMessage}'.");

            if (_expectedMessages != null)
                ExpectValidationException.CompareExpectedVsActual(_expectedMessages, result.Messages);
        }

        /// <summary>
        /// Creates a <typeparamref name="TAgent"/> instance using the <see cref="AgentTesterBase.LocalServiceProvider"/> where found; otherwise, will instantiate directly.
        /// </summary>
        /// <param name="args">The optional <see cref="IWebApiAgentArgs"/>; will default to <see cref="AgentTestCore{TStartup}.CreateAgentArgs"/>.</param>
        /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
        /// <returns>The <typeparamref name="TAgent"/> instance.</returns>
        public TAgent CreateAgent<TAgent>(IWebApiAgentArgs? args = null) where TAgent : GrpcAgentBase
        {
            var agent = AgentTesterBase.LocalServiceProvider.GetService<TAgent>();
            if (agent != null)
                return agent;

            var obj = Activator.CreateInstance(typeof(TAgent), args ?? CreateAgentArgs());
            if (obj == null)
                throw new InvalidOperationException($"An instance of {typeof(TAgent).Name} was unable to be created.");

            return (TAgent)obj;
        }
    }
}