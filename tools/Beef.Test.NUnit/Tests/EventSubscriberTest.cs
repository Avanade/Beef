// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.Events.Subscribe;
using Beef.Test.NUnit.Events;
using Beef.Test.NUnit.Logging;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Provides the <b>Event Publisher</b> test capabilities (specifically verifying the <see cref="SubscriberStatus"/>).
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    /// <typeparam name="TSubscriber">The <see cref="IEventSubscriber"/> <see cref="Type"/> to test.</typeparam>
    public class EventSubscriberTest<TStartup, TSubscriber> where TStartup : class, new() where TSubscriber : IEventSubscriber
    {
        private readonly EventSubscriberTester<TStartup> _eventSubscriberTester;
        private SubscriberStatus _expectedStatus;
        private readonly List<(ExpectedEvent expectedEvent, bool useReturnedValue)> _expectedPublished = new List<(ExpectedEvent, bool)>();
        private bool _expectedNonePublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTest{TStartup, TSubscriber}"/> class.
        /// </summary>
        /// <param name="eventSubscriberTester">The owning/parent <see cref="EventSubscriberTester{TStartup}"/>.</param>
        public EventSubscriberTest(EventSubscriberTester<TStartup> eventSubscriberTester)
        {
            _eventSubscriberTester = Check.NotNull(eventSubscriberTester, nameof(eventSubscriberTester));
            _expectedNonePublished = TestSetUp.DefaultExpectNoEvents;
        }

        /// <summary>
        /// Gets the unique correlation identifier that is sent via the event to the subscriber.
        /// </summary>
        public string CorrelationId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Verifies that the subscriber result has the specified <paramref name="status"/>.
        /// </summary>
        /// <param name="status">The <see cref="SubscriberStatus"/>.</param>
        /// <returns>The <see cref="EventSubscriberTest{TStartup, TSubscriber}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTest<TStartup, TSubscriber> ExpectResult(SubscriberStatus status)
        {
            _expectedStatus = status;
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventData.Subject"/> and optionally define
        /// <see cref="EventData.Action"/>. No value comparison will occur. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <returns>The <see cref="EventSubscriberTest{TStartup, TSubscriber}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTest<TStartup, TSubscriber> ExpectEvent(string template, string action)
        {
            _expectedPublished.Add((new ExpectedEvent(new EventData { Subject = template, Action = action }), false));
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberTest{TStartup, TSubscriber}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTest<TStartup, TSubscriber> ExpectNoEvents()
        {
            _expectedNonePublished = true;
            return this;
        }

        /// <summary>
        /// Check the published events to make sure they are valid.
        /// </summary>
        /// <param name="eventNeedingValueUpdateAction">Action that will be called where the value needs to be updated.</param>
        private void PublishedEventsCheck(Action<ExpectedEvent>? eventNeedingValueUpdateAction = null)
        {
            try
            {
                if (_expectedPublished.Count > 0)
                {
                    foreach (var ee in _expectedPublished.Where((v) => v.useReturnedValue).Select((v) => v.expectedEvent))
                    {
                        eventNeedingValueUpdateAction?.Invoke(ee);
                    }

                    Events.ExpectEvent.ArePublished(_expectedPublished.Select((v) => v.expectedEvent).ToList(), CorrelationId);
                }
                else if (_expectedNonePublished)
                    Events.ExpectEvent.NonePublished(CorrelationId);
            }
            finally
            {
                ExpectEventPublisher.Remove(CorrelationId);
            }
        }

        /// <summary>
        /// Runs the test for the <paramref name="event"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        public void Run(EventData @event) => Task.Run(() => RunAsync(@event)).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the test for the <paramref name="event"/> asynchronously checking against the expected outcomes.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        public async Task RunAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            @event.CorrelationId = CorrelationId;
            EventSubscriberTestHost? tesh = null;
            Stopwatch? sw = null;

            // Execute the subscriber host.
            await Task.Run(async () =>
            {
                using (var scope = _eventSubscriberTester.ServiceProvider.CreateScope())
                {
                    try
                    {
                        ExecutionContext.Reset(false);
                        tesh = new EventSubscriberTestHost(EventSubscriberHostArgs.Create(scope.ServiceProvider, typeof(TSubscriber)).UseLoggerForAuditing());
                        sw = Stopwatch.StartNew();
                        await tesh.ReceiveAsync(@event).ConfigureAwait(false);
                        sw.Stop();
                    }
                    catch (Exception)
                    {
                        WriteLoggedOutput();
                        throw;
                    }
                };
            }).ConfigureAwait(false);

            // Log to output.
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine("EVENT SUBSCRIBER TESTER...");
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"SUBSCRIBER >");
            TestContext.Out.WriteLine($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            TestContext.Out.WriteLine($"{tesh!.Result?.ToMultiLineString()}");

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"EVENTS PUBLISHED >");
            var events = Events.ExpectEvent.GetEvents(CorrelationId);
            if (events.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var e in events)
                {
                    TestContext.Out.WriteLine($"  Subject: {e.Subject}, Action: {e.Action}");
                }
            }

            WriteLoggedOutput();

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine(new string('=', 80));
            TestContext.Out.WriteLine("");

            // Perform checks.
            if (_expectedStatus != tesh.Result!.Status)
                Assert.Fail($"Expected Status was '{_expectedStatus}'; actual was '{tesh.Result!.Status}'");

            PublishedEventsCheck();
        }

        /// <summary>
        /// Writes the logged output.
        /// </summary>
        private void WriteLoggedOutput()
        {
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"LOGGING >");
            var messages = CorrelationIdLogger.GetMessages(CorrelationId);
            if (messages.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var l in messages)
                {
                    TestContext.Out.WriteLine($"{l}");
                }
            }
        }
    }
}