// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Events;
using Beef.Events.Subscribe;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the <see cref="EventSubscriberBase">Event Subscriber</see> testing capabilities.
    /// </summary>
    public class EventSubscriberTester
    {
        private readonly EventSubscriberHostArgs _args;
        private SubscriberStatus _expectedStatus;
        private readonly List<(ExpectedEvent expectedEvent, bool useReturnedValue)> _expectedPublished = new List<(ExpectedEvent, bool)>();
        private bool _expectedNonePublished;

        /// <summary>
        /// Create a new <see cref="EventSubscriberTester"/> for a <typeparamref name="TSubscriber"/>.
        /// </summary>
        /// <typeparam name="TSubscriber">The <see cref="EventSubscriberBase"/> that is expected to be executed.</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <returns>An <see cref="EventSubscriberTester"/> instance.</returns>
        public static EventSubscriberTester Create<TSubscriber>(string? username = null) where TSubscriber : EventSubscriberBase, new()
            => new EventSubscriberTester(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), new TSubscriber()).UseLoggerForAuditing(), username);

        /// <summary>
        /// Indicates whether to verify that no events were published as the default behaviour (see <see cref="EventSubscriberTester.ExpectNoEvents"/>).
        /// </summary>
        public static bool DefaultExpectNoEvents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTester"/> class.
        /// </summary>
        private EventSubscriberTester(EventSubscriberHostArgs args, string? username = null)
        {
            TestSetUp.InvokeRegisteredSetUp();
            Beef.Test.NUnit.ExpectEvent.SetUp();

            if (username != null || !ExecutionContext.HasCurrent)
            {
                ExecutionContext.Reset(false);
                ExecutionContext.SetCurrent(AgentTester.CreateExecutionContext.Invoke(username ?? AgentTester.DefaultUsername, args));
            }

            _args = args;
            _expectedNonePublished = DefaultExpectNoEvents;
        }

        /// <summary>
        /// Verifies that the subscriber result has the specified <paramref name="status"/>.
        /// </summary>
        /// <param name="status">The <see cref="SubscriberStatus"/>.</param>
        /// <returns></returns>
        public EventSubscriberTester ExpectResult(SubscriberStatus status)
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
        public EventSubscriberTester ExpectEvent(string template, string action)
        {
            _expectedPublished.Add((new ExpectedEvent(new EventData { Subject = template, Action = action }), false));
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        public EventSubscriberTester ExpectNoEvents()
        {
            _expectedNonePublished = true;
            return this;
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

            // Execute the subscriber host.
            var tesh = new TestEventSubscriberHost(_args);
            var sw = Stopwatch.StartNew();
            await tesh.ReceiveAsync(@event).ConfigureAwait(false);
            sw.Stop();

            // Check expectations.
            if (!tesh.WasSubscribed)
                Assert.Fail("The Subscriber did not Receive the event; check that the Subject and/or Action are as expected.");

            // Log to output.
            Logger.Default.Info("");
            Logger.Default.Info("EVENT SUBSCRIBER TESTER...");
            Logger.Default.Info("");
            Logger.Default.Info($"SUBSCRIBER >");
            Logger.Default.Info($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            Logger.Default.Info($"{tesh.Result}");

            Logger.Default.Info("");
            Logger.Default.Info($"EVENTS PUBLISHED >");
            var events = Beef.Test.NUnit.ExpectEvent.GetEvents();
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

            if (_expectedPublished.Count > 0)
                Beef.Test.NUnit.ExpectEvent.ArePublished(_expectedPublished.Select((v) => v.expectedEvent).ToList());
            else if (_expectedNonePublished)
                Beef.Test.NUnit.ExpectEvent.NonePublished();
        }
    }
}