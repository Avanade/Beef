using Beef.Diagnostics;
using Beef.Entities;
using Beef.Events;
using Beef.Grpc;
using Beef.RefData;
using Google.Protobuf;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Base capabilities for <see cref="GrpcAgentTester{TAgent}"/>.
    /// </summary>
    /// <typeparam name="TAgent">The <see cref="GrpcAgentBase"/> <see cref="Type"/>.</typeparam>
    public abstract class GrpcAgentTesterBase<TAgent> : AgentTester where TAgent : GrpcAgentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentTesterBase{TAgent}"/> class with a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        protected GrpcAgentTesterBase(string? username = null, object? args = null) : base(username, args) { }

        /// <summary>
        /// Check the result to make sure it is valid.
        /// </summary>
        /// <param name="result">The <see cref="GrpcAgentResult"/>.</param>
        /// <param name="sw">The <see cref="Stopwatch"/> used to measure <see cref="GrpcServiceAgentBase{TClient}"/> invocation.</param>
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
            if (events.Length == 0)
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
    }

    /// <summary>
    /// Provides the <see cref="GrpcAgentResult"/> testing.
    /// </summary>
    [DebuggerStepThrough()]
    public class GrpcAgentTester<TAgent> : GrpcAgentTesterBase<TAgent> where TAgent : GrpcAgentBase
    {
        private Action<GrpcAgentTester<TAgent>>? _beforeAction;
        private Action<GrpcAgentTester<TAgent>, GrpcAgentResult>? _afterAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentTester{TAgent}"/> class with a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public GrpcAgentTester(string? username = null, object? args = null) : base(username, args) { }

        /// <summary>
        /// An action to perform <b>before</b> the <see cref="Run(Func{GrpcAgentTesterRunArgs{TAgent}, Task{GrpcAgentResult}})"/> or <see cref="RunAsync(Func{GrpcAgentTesterRunArgs{TAgent}, Task{GrpcAgentResult}})"/>.
        /// </summary>
        /// <param name="action">The <b>before</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> Before(Action<AgentTester> action)
        {
            _beforeAction = action;
            return this;
        }

        /// <summary>
        /// An action to perform <b>after</b> the <see cref="Run(Func{GrpcAgentTesterRunArgs{TAgent}, Task{GrpcAgentResult}})"/> or <see cref="RunAsync(Func{GrpcAgentTesterRunArgs{TAgent}, Task{GrpcAgentResult}})"/> and all ignore/expect checks.
        /// </summary>
        /// <param name="action">The <b>after</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> After(Action<GrpcAgentTester<TAgent>, GrpcAgentResult> action)
        {
            _afterAction = action;
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> ExpectStatusCode(HttpStatusCode statusCode)
        {
            SetExpectStatusCode(statusCode);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message will not be checked.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> ExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> ExpectMessages(params string[] messages)
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
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> ExpectEvent(string template, string action)
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
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent> ExpectEvent<T>(string template, string action, T eventValue, params string[] membersToIgnore)
        {
            SetExpectEvent(false, template, action, eventValue, membersToIgnore);
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        public GrpcAgentTester<TAgent> ExpectNoEvents()
        {
            SetExpectNoEvents();
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public GrpcAgentResult Run(Func<GrpcAgentTesterRunArgs<TAgent>, Task<GrpcAgentResult>> func)
        {
            return Task.Run(() => RunAsync(func)).Result;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public async Task<GrpcAgentResult> RunAsync(Func<GrpcAgentTesterRunArgs<TAgent>, Task<GrpcAgentResult>> func)
        {
            Check.NotNull(func, nameof(func));

            _beforeAction?.Invoke(this);
            var sw = Stopwatch.StartNew();
            GrpcAgentResult result = await func(GetRunArgs()).ConfigureAwait(false);
            sw.Stop();
            ResultCheck(result, sw);
            PublishedEventsCheck();
            _afterAction?.Invoke(this, result);
            return result;
        }

        /// <summary>
        /// Gets an <see cref="AgentTesterRunArgs{TAgent}"/> instance.
        /// </summary>
        /// <returns>An <see cref="AgentTesterRunArgs{TAgent}"/> instance.</returns>
        public GrpcAgentTesterRunArgs<TAgent> GetRunArgs() => new GrpcAgentTesterRunArgs<TAgent>(HttpClient, BeforeRequest, this);
    }

    /// <summary>
    /// Provides the <see cref="GrpcAgentResult{TValue}"/> testing.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The response <see cref="GrpcAgentResult{TValue}.Value"/> <see cref="Type"/>.</typeparam>
    [DebuggerStepThrough()]
    public class GrpcAgentTester<TAgent, TValue> : GrpcAgentTesterBase<TAgent> where TAgent : GrpcAgentBase
    {
        private readonly ComparisonConfig _comparisonConfig = GetDefaultComparisonConfig();
        private Action<GrpcAgentTester<TAgent, TValue>>? _beforeAction;
        private Action<GrpcAgentTester<TAgent, TValue>, GrpcAgentResult<TValue>>? _afterAction;
        private bool _isExpectNullValue;
        private Func<GrpcAgentTester<TAgent, TValue>, TValue>? _expectValueFunc;
        private bool _isExpectCreatedBy;
        private string? _changeLogCreatedBy;
        private DateTime? _changeLogCreatedDate;
        private bool _isExpectUpdatedBy;
        private string? _changeLogUpdatedBy;
        private DateTime? _changeLogUpdatedDate;
        private bool _isExpectedETag;
        private string? _previousETag;
        private bool _isExpectedUniqueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentTester{TValue}"/> class with a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public GrpcAgentTester(string? username = null, object? args = null) : base(username, args) { }

        /// <summary>
        /// An action to perform <b>before</b> the <see cref="Run(Func{GrpcAgentTesterRunArgs{TAgent, TValue}, Task{GrpcAgentResult{TValue}}})"/> or <see cref="RunAsync(Func{GrpcAgentTesterRunArgs{TAgent, TValue}, Task{GrpcAgentResult{TValue}}})"/>.
        /// </summary>
        /// <param name="action">The <b>before</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> Before(Action<GrpcAgentTester<TAgent, TValue>> action)
        {
            _beforeAction = action;
            return this;
        }

        /// <summary>
        /// An action to perform <b>after</b> the <see cref="Run(Func{GrpcAgentTesterRunArgs{TAgent, TValue}, Task{GrpcAgentResult{TValue}}})"/> or <see cref="RunAsync(Func{GrpcAgentTesterRunArgs{TAgent, TValue}, Task{GrpcAgentResult{TValue}}})"/> and all ignore/expect checks.
        /// </summary>
        /// <param name="action">The <b>after</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> After(Action<GrpcAgentTester<TAgent, TValue>, GrpcAgentResult<TValue>> action)
        {
            _afterAction = action;
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectStatusCode(HttpStatusCode statusCode)
        {
            SetExpectStatusCode(statusCode);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message will not be checked.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectMessages(params string[] messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        public GrpcAgentTester<TAgent, TValue> ExpectMessages(MessageItemCollection messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect <c>null</c> response value.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectNullValue()
        {
            _isExpectNullValue = true;
            return this;
        }

        /// <summary>
        /// Expect a response comparing the specified <paramref name="valueFunc"/> (and optionally any additional <paramref name="membersToIgnore"/> from the comparison).
        /// </summary>
        /// <param name="valueFunc">The function to generate the response value to compare.</param>
        /// <param name="membersToIgnore">The members to ignore from the comparison.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectValue(Func<GrpcAgentTester<TAgent, TValue>, TValue> valueFunc, params string[] membersToIgnore)
        {
            _expectValueFunc = Check.NotNull(valueFunc, nameof(valueFunc));
            _comparisonConfig.MembersToIgnore.AddRange(membersToIgnore);
            return this;
        }

        /// <summary>
        /// Ignores the <see cref="IChangeLog"/> properties.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> IgnoreChangeLog()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _comparisonConfig.MembersToIgnore.Add("ChangeLog");
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IChangeLog"/> to be implemented for the response with generated values for the underlying <see cref="ChangeLog.CreatedBy"/> and <see cref="ChangeLog.CreatedDate"/> matching the specified values.
        /// </summary>
        /// <param name="createdby">The specific <see cref="ChangeLog.CreatedBy"/> value where specified; otherwise, indicates to check for user running the test (see <see cref="AgentTester.Username"/>).</param>
        /// <param name="createdDateGreaterThan">The <see cref="DateTime"/> in which the <see cref="ChangeLog.CreatedDate"/> should be greater than; where <c>null</c> it will default to <see cref="DateTime.Now"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectChangeLogCreated(string? createdby = null, DateTime? createdDateGreaterThan = null)
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _isExpectCreatedBy = true;
            _changeLogCreatedBy = string.IsNullOrEmpty(createdby) ? Username : createdby;
            _changeLogCreatedDate = createdDateGreaterThan ?? (DateTime?)DateTime.Now.Subtract(new TimeSpan(0, 0, 1));
            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ChangeLog" });
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IChangeLog"/> to be implemented for the response with generated values for the underlying <see cref="ChangeLog.UpdatedBy"/> and <see cref="ChangeLog.UpdatedDate"/> matching the specified values.
        /// </summary>
        /// <param name="updatedby">The specific <see cref="ChangeLog.UpdatedBy"/> value where specified; otherwise, indicates to check for user runing the test (see <see cref="AgentTester.Username"/>).</param>
        /// <param name="updatedDateGreaterThan">The <see cref="TimeSpan"/> in which the <see cref="ChangeLog.UpdatedDate"/> should be greater than; where <c>null</c> it will default to <see cref="DateTime.Now"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectChangeLogUpdated(string? updatedby = null, DateTime? updatedDateGreaterThan = null)
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _isExpectUpdatedBy = true;
            _changeLogUpdatedBy = string.IsNullOrEmpty(updatedby) ? Username : updatedby;
            _changeLogUpdatedDate = updatedDateGreaterThan ?? (DateTime?)DateTime.Now.Subtract(new TimeSpan(0, 0, 1));
            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ChangeLog" });
            return this;
        }

        /// <summary>
        /// Ignores the <see cref="IETag"/> properties.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> IgnoreETag()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IETag).Name) != null, "TValue must implement the interface IETag.");

            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ETag" });
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IETag"/> to be implemented for the response with a generated value for the underlying <see cref="IETag.ETag"/> (different to <paramref name="previousETag"/>).
        /// </summary>
        /// <param name="previousETag">The previous <b>ETag</b> value; expect a value that is different.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Must be non-null and different from the request (where applicable).</remarks>
        public GrpcAgentTester<TAgent, TValue> ExpectETag(string? previousETag = null)
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IETag).Name) != null, "TValue must implement the interface IETag.");

            _isExpectedETag = true;
            _previousETag = previousETag;
            _comparisonConfig.MembersToIgnore.Add("ETag");
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IUniqueKey"/> to be implemented for the response with a generated value for the underlying <see cref="IUniqueKey.UniqueKey"/>.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectUniqueKey()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IUniqueKey).Name) != null, "TValue must implement the interface IUniqueKey.");

            _isExpectedUniqueKey = true;
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. No value comparison will occur. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectEvent(string template, string action)
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
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectEvent<T>(string template, string action, T eventValue, params string[] membersToIgnore)
        {
            SetExpectEvent<T>(false, template, action, eventValue, membersToIgnore);
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. The returned value (<typeparamref name="TValue"/>) will be compared against the <see cref="EventData{TValue}.Value"/>.
        /// Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public GrpcAgentTester<TAgent, TValue> ExpectEventWithValue(string template, string action)
        {
            SetExpectEvent<TValue>(true, template, action, default!);
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        public GrpcAgentTester<TAgent, TValue> ExpectNoEvents()
        {
            SetExpectNoEvents();
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="GrpcAgentResult{TValue}"/>.</returns>
        public GrpcAgentResult<TValue> Run(Func<GrpcAgentTesterRunArgs<TAgent, TValue>, Task<GrpcAgentResult<TValue>>> func)
        {
            return Task.Run(() => RunAsync(func)).Result;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding result.</returns>
        public async Task<GrpcAgentResult<TValue>> RunAsync(Func<GrpcAgentTesterRunArgs<TAgent, TValue>, Task<GrpcAgentResult<TValue>>> func)
        {
            Check.NotNull(func, nameof(func));

            // Execute the before action.
            _beforeAction?.Invoke(this);

            // Execute the function.
            var sw = Stopwatch.StartNew();
            GrpcAgentResult<TValue> result = await func(GetRunArgs()).ConfigureAwait(false);
            sw.Stop();

            // Check expectations.
            ResultCheck(result, sw);

            if (_isExpectedUniqueKey && result.Value is IUniqueKey uk)
                _comparisonConfig.MembersToIgnore.AddRange(uk.UniqueKeyProperties);

            if (_isExpectNullValue && result.HasValue)
                Assert.Fail($"Expected null response value; the following content was returned: 'result.Content'.");

            //if ((_isExpectCreatedBy || _isExpectUpdatedBy || _isExpectedETag || _isExpectedUniqueKey || _expectValueFunc != null) && !result.HasValue)
            //    Assert.Fail($"Expected non-null response content; no response returned.");

            if (_isExpectCreatedBy)
            {
                var cl = result.Value as IChangeLog;
                if (cl == null || cl.ChangeLog == null || string.IsNullOrEmpty(cl.ChangeLog.CreatedBy))
                    Assert.Fail("Expected IChangeLog.CreatedBy to have a non-null value.");
                else if (_changeLogCreatedBy != cl.ChangeLog.CreatedBy)
                    Assert.Fail($"Expected IChangeLog.CreatedBy '{_changeLogCreatedBy}'; actual '{cl.ChangeLog.CreatedBy}'.");

                if (!cl!.ChangeLog!.CreatedDate.HasValue)
                    Assert.Fail("Expected IChangeLog.CreatedDate to have a non-null value.");
                else if (cl.ChangeLog.CreatedDate.Value < _changeLogCreatedDate)
                    Assert.Fail($"Expected IChangeLog.CreatedDate actual '{cl.ChangeLog.CreatedDate.Value}' to be greater than expected '{_changeLogCreatedDate}'.");
            }

            if (_isExpectUpdatedBy)
            {
                var cl = result.Value as IChangeLog;
                if (cl == null || cl.ChangeLog == null || string.IsNullOrEmpty(cl.ChangeLog.UpdatedBy))
                    Assert.Fail("Expected IChangeLog.UpdatedBy to have a non-null value.");
                else if (_changeLogUpdatedBy != cl.ChangeLog.UpdatedBy)
                    Assert.Fail($"Expected IChangeLog.UpdatedBy '{_changeLogUpdatedBy}'; actual was '{cl.ChangeLog.UpdatedBy}'.");

                if (!cl!.ChangeLog!.UpdatedDate.HasValue)
                    Assert.Fail("Expected IChangeLog.UpdatedDate to have a non-null value.");
                else if (cl.ChangeLog.UpdatedDate.Value < _changeLogUpdatedDate)
                    Assert.Fail($"Expected IChangeLog.UpdatedDate actual '{cl.ChangeLog.UpdatedDate.Value}' to be greater than expected '{_changeLogUpdatedDate}'.");
            }

            if (_isExpectedETag)
            {
                var et = result.Value as IETag;
                if (et == null || et.ETag == null)
                    Assert.Fail("Expected IETag.ETag to have a non-null value.");

                if (!string.IsNullOrEmpty(_previousETag) && _previousETag == et!.ETag)
                    Assert.Fail("Expected IETag.ETag value is the same as previous.");
            }

            if (_isExpectedUniqueKey)
            {
                if (!(result.Value is IUniqueKey uk2) || uk2.UniqueKey == null || uk2.UniqueKey.Args == null || uk2.UniqueKey.Args.Any(x => x == null))
                    Assert.Fail("Expected IUniqueKey.Args array to have no null values.");
            }

            if (_expectValueFunc != null)
            {
                var exp = _expectValueFunc(this);
                if (exp == null)
                    throw new InvalidOperationException("ExpectValue function must not return null.");

                // Further configure the comparison configuration.
                _comparisonConfig.TypesToIgnore.AddRange(ReferenceDataManager.Current.GetAllTypes());
                InferAdditionalMembersToIgnore(_comparisonConfig, typeof(TValue));

                // Perform the actual comparison.
                var cl = new CompareLogic(_comparisonConfig);
                var cr = cl.Compare(exp, result.Value);
                if (!cr.AreEqual)
                    Assert.Fail($"Expected vs Actual value mismatch: {cr.DifferencesString}");
            }

            PublishedEventsCheck((ee) => ((EventData<TValue>)ee.EventData).Value = result.Value);

            // Execute the after action.
            _afterAction?.Invoke(this, result);

            return result;
        }

        /// <summary>
        /// Gets an <see cref="AgentTesterRunArgs{TAgent, TValue}"/> instance.
        /// </summary>
        /// <returns>An <see cref="AgentTesterRunArgs{TAgent, TValue}"/> instance.</returns>
        public GrpcAgentTesterRunArgs<TAgent, TValue> GetRunArgs() => new GrpcAgentTesterRunArgs<TAgent, TValue>(HttpClient, BeforeRequest, this);
    }
}