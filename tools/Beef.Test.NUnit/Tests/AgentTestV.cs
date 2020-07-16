// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using Beef.RefData;
using Beef.WebApi;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the <b>Agent</b> test capabilities (specifically verifying the <see cref="WebApiAgentResult{TValue}"/>).
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The response <see cref="WebApiAgentResult{TValue}.Value"/> <see cref="Type"/>.</typeparam>
    //[DebuggerStepThrough()]
    public class AgentTest<TStartup, TAgent, TValue> : AgentTestBase<TStartup> where TStartup : class where TAgent : WebApiAgentBase
    {
        private readonly ComparisonConfig _comparisonConfig = AgentTester.GetDefaultComparisonConfig();
        private bool _isExpectNullValue;
        private Func<AgentTest<TStartup, TAgent, TValue>, TValue>? _expectValueFunc;
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
        /// Initializes a new instance of the <see cref="AgentTest{TStartup, TAgent, TValue}"/> class with a username.
        /// </summary>
        /// <param name="agentTesterBase">The owning/parent <see cref="AgentTesterBase"/>.</param>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public AgentTest(AgentTesterBase agentTesterBase, string? username = null, object? args = null) : base(agentTesterBase, username, args) { }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectStatusCode(HttpStatusCode statusCode)
        {
            SetExpectStatusCode(statusCode);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message will not be checked.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectMessages(params string[] messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        public AgentTest<TStartup, TAgent, TValue> ExpectMessages(MessageItemCollection messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect <c>null</c> response value.
        /// </summary>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectNullValue()
        {
            _isExpectNullValue = true;
            return this;
        }

        /// <summary>
        /// Expect a response comparing the specified <paramref name="valueFunc"/> (and optionally any additional <paramref name="membersToIgnore"/> from the comparison).
        /// </summary>
        /// <param name="valueFunc">The function to generate the response value to compare.</param>
        /// <param name="membersToIgnore">The members to ignore from the comparison.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectValue(Func<AgentTest<TStartup, TAgent, TValue>, TValue> valueFunc, params string[] membersToIgnore)
        {
            _expectValueFunc = Check.NotNull(valueFunc, nameof(valueFunc));
            _comparisonConfig.MembersToIgnore.AddRange(membersToIgnore);
            return this;
        }

        /// <summary>
        /// Ignores the <see cref="IChangeLog"/> properties.
        /// </summary>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> IgnoreChangeLog()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _comparisonConfig.MembersToIgnore.Add("ChangeLog");
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IChangeLog"/> to be implemented for the response with generated values for the underlying <see cref="ChangeLog.CreatedBy"/> and <see cref="ChangeLog.CreatedDate"/> matching the specified values.
        /// </summary>
        /// <param name="createdby">The specific <see cref="ChangeLog.CreatedBy"/> value where specified (can include wildcards); otherwise, indicates to check for user running the test (see <see cref="AgentTestCore{TStartup}.Username"/>).</param>
        /// <param name="createdDateGreaterThan">The <see cref="DateTime"/> in which the <see cref="ChangeLog.CreatedDate"/> should be greater than; where <c>null</c> it will default to <see cref="DateTime.Now"/>.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectChangeLogCreated(string? createdby = null, DateTime? createdDateGreaterThan = null)
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _isExpectCreatedBy = true;
            _changeLogCreatedBy = string.IsNullOrEmpty(createdby) ? Username : createdby;
            _changeLogCreatedDate = createdDateGreaterThan ?? (DateTime?)Cleaner.Clean(DateTime.Now.Subtract(new TimeSpan(0, 0, 1)));
            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ChangeLog" });
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IChangeLog"/> to be implemented for the response with generated values for the underlying <see cref="ChangeLog.UpdatedBy"/> and <see cref="ChangeLog.UpdatedDate"/> matching the specified values.
        /// </summary>
        /// <param name="updatedby">The specific <see cref="ChangeLog.UpdatedBy"/> value where specified (can include wildcards); otherwise, indicates to check for user runing the test (see <see cref="AgentTestCore{TStartup}.Username"/>).</param>
        /// <param name="updatedDateGreaterThan">The <see cref="TimeSpan"/> in which the <see cref="ChangeLog.UpdatedDate"/> should be greater than; where <c>null</c> it will default to <see cref="DateTime.Now"/>.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectChangeLogUpdated(string? updatedby = null, DateTime? updatedDateGreaterThan = null)
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IChangeLog).Name) != null, "TValue must implement the interface IChangeLog.");

            _isExpectUpdatedBy = true;
            _changeLogUpdatedBy = string.IsNullOrEmpty(updatedby) ? Username : updatedby;
            _changeLogUpdatedDate = updatedDateGreaterThan ?? (DateTime?)Cleaner.Clean(DateTime.Now.Subtract(new TimeSpan(0, 0, 1)));
            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ChangeLog" });
            return this;
        }

        /// <summary>
        /// Ignores the <see cref="IETag"/> properties.
        /// </summary>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> IgnoreETag()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IETag).Name) != null, "TValue must implement the interface IETag.");

            _comparisonConfig.MembersToIgnore.AddRange(new string[] { "ETag" });
            return this;
        }

        /// <summary>
        /// Expects the <see cref="IETag"/> to be implemented for the response with a generated value for the underlying <see cref="IETag.ETag"/> (different to <paramref name="previousETag"/>).
        /// </summary>
        /// <param name="previousETag">The previous <b>ETag</b> value; expect a value that is different.</param>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Must be non-null and different from the request (where applicable).</remarks>
        public AgentTest<TStartup, TAgent, TValue> ExpectETag(string? previousETag = null)
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
        /// <returns>The <see cref="AgentTest{TStartup, TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectUniqueKey()
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
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectEvent(string template, string action)
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
        public AgentTest<TStartup, TAgent, TValue> ExpectEvent<T>(string template, string action, T eventValue, params string[] membersToIgnore)
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
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTest<TStartup, TAgent, TValue> ExpectEventWithValue(string template, string action)
        {
            SetExpectEvent<TValue>(true, template, action, default!);
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        public AgentTest<TStartup, TAgent, TValue> ExpectNoEvents()
        {
            SetExpectNoEvents();
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> with an automatically instantiated <typeparamref name="TAgent"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="WebApiAgentResult{TValue}"/>.</returns>
        public WebApiAgentResult<TValue> Run(Func<TAgent, Task<WebApiAgentResult<TValue>>> func) => Task.Run(() => RunAsync(func)).Result;

        /// <summary>
        /// Runs the <paramref name="func"/> with an automatically instantiated <typeparamref name="TAgent"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding result.</returns>
        public async Task<WebApiAgentResult<TValue>> RunAsync(Func<TAgent, Task<WebApiAgentResult<TValue>>> func)
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
        public WebApiAgentResult<TValue> RunOverride(Func<Task<WebApiAgentResult<TValue>>> func) => Task.Run(() => RunOverrideAsync(func)).Result;

        /// <summary>
        /// Runs the <paramref name="func"/> where the agent is self-instantied and executed asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TValue>> RunOverrideAsync(Func<Task<WebApiAgentResult<TValue>>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // Execute the function.
            var sw = Stopwatch.StartNew();
            WebApiAgentResult<TValue> result = await func().ConfigureAwait(false);
            sw.Stop();

            // Check expectations.
            ResultCheck(result, sw);

            if (_isExpectedUniqueKey && result.Value is IUniqueKey uk)
                _comparisonConfig.MembersToIgnore.AddRange(uk.UniqueKeyProperties);

            if (_isExpectNullValue && result.HasValue)
                Assert.Fail($"Expected null response value; the following content was returned: '{result.Content}'.");

            if ((_isExpectCreatedBy || _isExpectUpdatedBy || _isExpectedETag || _isExpectedUniqueKey || _expectValueFunc != null) && !result.HasValue)
                Assert.Fail($"Expected non-null response content; no response returned.");

            if (_isExpectCreatedBy)
            {
                var cl = result.Value as IChangeLog;
                if (cl == null || cl.ChangeLog == null || string.IsNullOrEmpty(cl.ChangeLog.CreatedBy))
                    Assert.Fail("Expected IChangeLog.CreatedBy to have a non-null value.");
                else
                {
                    var wcr = Wildcard.BothAll.Parse(_changeLogCreatedBy).ThrowOnError();
                    if (!wcr.CreateRegex().IsMatch(cl.ChangeLog.CreatedBy))
                        Assert.Fail($"Expected IChangeLog.CreatedBy '{_changeLogCreatedBy}'; actual '{cl.ChangeLog.CreatedBy}'.");
                }

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
                else
                {
                    var wcr = Wildcard.BothAll.Parse(_changeLogUpdatedBy).ThrowOnError();
                    if (!wcr.CreateRegex().IsMatch(cl.ChangeLog.UpdatedBy))
                        Assert.Fail($"Expected IChangeLog.UpdatedBy '{_changeLogUpdatedBy}'; actual was '{cl.ChangeLog.UpdatedBy}'.");
                }

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
                _comparisonConfig.AttributesToIgnore.AddRange(new Type[] { typeof(ReferenceDataInterfaceAttribute) });
                AgentTester.InferAdditionalMembersToIgnore(_comparisonConfig, typeof(TValue));

                // Perform the actual comparison.
                var cl = new CompareLogic(_comparisonConfig);
                var cr = cl.Compare(exp, result.Value);
                if (!cr.AreEqual)
                    Assert.Fail($"Expected vs Actual value mismatch: {cr.DifferencesString}");
            }

            PublishedEventsCheck((ee) => ((EventData<TValue>)ee.EventData).Value = result.Value);

            return result;
        }
    }
}