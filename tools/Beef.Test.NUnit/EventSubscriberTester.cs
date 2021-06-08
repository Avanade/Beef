// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.Test.NUnit.Events;
using Beef.Test.NUnit.Logging;
using Beef.Test.NUnit.Tests;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Manages the testing of an event subscriber with integrated mocking of services as required.
    /// </summary>
    [DebuggerStepThrough]
    public static class EventSubscriberTester
    {
        /// <summary>
        /// Creates an <see cref="EventSubscriberTester{TStartup}"/> to manage the orchestration of one or more <see cref="EventSubscriberBase"/> integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <returns>An <see cref="EventSubscriberTester{TStartup}"/> instance.</returns>
        public static EventSubscriberTester<TStartup> Create<TStartup>() where TStartup : class, new() => new EventSubscriberTester<TStartup>();
    }

    /// <summary>
    /// Manages the testing of an event subscriber with integrated mocking of services as required.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [DebuggerStepThrough]
    public class EventSubscriberTester<TStartup> : TesterBase where TStartup : class, new()
    {
        private SubscriberStatus _expectedStatus;
        private Type? _expectedExceptionType;
        private string? _expectedExceptionMessage;
        private readonly List<(ExpectedEvent expectedEvent, bool useReturnedValue)> _expectedPublished = new List<(ExpectedEvent, bool)>();
        private bool _expectedNonePublished;
        private bool _ignoreEventMismatch;

        private class Fhb : IFunctionsHostBuilder
        {
            public Fhb(IServiceCollection services) => Services = services;

            public IServiceCollection Services { get; private set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTester{TStartup}"/> class.
        /// </summary>
        internal EventSubscriberTester() : base(configureLocalRefData: true)
        {
            // TODO: Come back and revisit.
            // string? environmentVariablePrefix = null, string embeddedFilePrefix = "funcsettings", string environment = TestSetUp.DefaultEnvironment, Action<ConfigurationBuilder>? configurationBuilder = null, Action<IServiceCollection>? services = null
            //_serviceCollection.GetBeefConfiguration<TStartup>(environmentVariablePrefix, embeddedFilePrefix, environment, configurationBuilder);

            ConfigureLocalServices(sc =>
            {
                // Load the service collection from TStartup.Configure(IFunctionsHostBuilder).
                var mi = typeof(TStartup).GetMethod("Configure", new Type[] { typeof(IFunctionsHostBuilder) });
                if (mi == null)
                    throw new InvalidOperationException($"TStartup '{typeof(TStartup).Name}' must implement a 'Configure(IFunctionsHostBuilder)' method.");

                mi.Invoke(new TStartup(), new object[] { new Fhb(sc) });

                // Finish up and build the service provider.
                sc.AddLogging(configure => configure.AddCorrelationId());
                ReplaceEventPublisher(sc);
            });
        }

        /// <summary>
        /// Gets the unique correlation identifier that is sent via the event to the subscriber.
        /// </summary>
        public string CorrelationId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Verifies that the subscriber result has the specified <paramref name="status"/>.
        /// </summary>
        /// <param name="status">The <see cref="SubscriberStatus"/>.</param>
        /// <returns>The <see cref="EventSubscriberTester{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTester<TStartup> ExpectResult(SubscriberStatus status)
        {
            _expectedStatus = status;
            return this;
        }

        /// <summary>
        /// Verifies that the subscriber result is <see cref="SubscriberStatus.UnhandledException"/> and the underlying <see cref="Exception"/> matches the expected <typeparamref name="TException"/> including the <paramref name="expectedExceptionMessage"/>.
        /// </summary>
        /// <typeparam name="TException">The expected <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <returns>The <see cref="EventSubscriberTester{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTester<TStartup> ExpectUnhandledException<TException>(string expectedExceptionMessage) where TException : Exception
        {
            _expectedStatus = SubscriberStatus.UnhandledException;
            _expectedExceptionType = typeof(TException);
            _expectedExceptionMessage = Check.NotNull(expectedExceptionMessage, nameof(expectedExceptionMessage));
            return this;
        }

        /// <summary>
        /// Verifies that the the event is published (in order specified). The expected event can use wildcards for <see cref="EventMetadata.Subject"/> and optionally define
        /// <see cref="EventMetadata.Action"/>. No value comparison will occur. Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <returns>The <see cref="EventSubscriberTester{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTester<TStartup> ExpectEvent(string template, string action)
        {
            _expectedPublished.Add((new ExpectedEvent(new EventData { Subject = template, Action = action }), false));
            return this;
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberTester{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTester<TStartup> ExpectNoEvents()
        {
            _expectedNonePublished = true;
            return this;
        }

        /// <summary>
        /// Ignores (does not verify) that the events that are published must match those finally sent.
        /// </summary>
        /// <returns>The <see cref="AgentTest{TStartup, TAgent}"/> instance to support fluent/chaining usage.</returns>
        public EventSubscriberTester<TStartup> IgnorePublishSendEventMismatch()
        {
            _ignoreEventMismatch = true;
            return this;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        public EventSubscriberTester<TStartup> ConfigureServices(Action<IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(Check.NotNull(serviceCollection, nameof(serviceCollection)));
            return this;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> function.</param>
        public EventSubscriberTester<TStartup> ConfigureServices(Func<IServiceCollection, IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(sc => Check.NotNull(serviceCollection, nameof(serviceCollection))(sc));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public EventSubscriberTester<TStartup> AddSingletonService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public EventSubscriberTester<TStartup> AddSingletonService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public EventSubscriberTester<TStartup> AddScopedService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public EventSubscriberTester<TStartup> AddScopedService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public EventSubscriberTester<TStartup> AddTransientService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public EventSubscriberTester<TStartup> AddTransientService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(Check.NotNull(mock, nameof(mock)).Object));
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

                    Events.ExpectEvent.AreSent(_expectedPublished.Select((v) => v.expectedEvent).ToList(), CorrelationId);
                }
                else if (_expectedNonePublished)
                    Events.ExpectEvent.NoneSent(CorrelationId);

                if (!_ignoreEventMismatch)
                    Events.ExpectEvent.PublishedVersusSent(CorrelationId);
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
        public void Run<TSubscriber>(EventData @event) where TSubscriber : IEventSubscriber => Task.Run(() => RunAsync<TSubscriber>(@event)).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the test for the <paramref name="event"/> asynchronously checking against the expected outcomes.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        public async Task RunAsync<TSubscriber>(EventData @event) where TSubscriber : IEventSubscriber
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            @event.CorrelationId = CorrelationId;
            EventSubscriberTestHost? tesh = null;
            Stopwatch? sw = null;
            Exception? cex = null;

            // Execute the subscriber host.
            using (var scope = LocalServiceProvider.CreateScope())
            {
                try
                {
                    ExecutionContext.Reset();
                    var args = EventSubscriberHostArgs.Create(typeof(TSubscriber)).UseServiceProvider(scope.ServiceProvider);
                    tesh = new EventSubscriberTestHost(args).UseLogger(scope.ServiceProvider.GetService<ILogger<EventSubscriberTester<TStartup>>>());
                    sw = Stopwatch.StartNew();
                    await tesh.ReceiveAsync(@event).ConfigureAwait(false);
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    cex = ex;
                }
            };

            // Log to output.
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine("EVENT SUBSCRIBER TESTER...");
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"SUBSCRIBER >");
            TestContext.Out.WriteLine($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            if (cex == null)
                TestContext.Out.WriteLine($"{tesh!.Result?.ToMultiLineString()}");
            else
            {
                TestContext.Out.WriteLine($"Status: {SubscriberStatus.UnhandledException}");
                TestContext.Out.WriteLine($"Exception: {cex}");
            }

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"EVENTS PUBLISHED >");
            var events = Events.ExpectEvent.GetPublishedEvents(CorrelationId);
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
            events = Events.ExpectEvent.GetSentEvents(CorrelationId);
            if (events.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var e in events)
                {
                    TestContext.Out.WriteLine($"  Subject: {e.Subject ?? "<null>"}, Action: {e.Action ?? "<null>"}, Source: {e.Source?.ToString() ?? "<null>"}");
                }
            }

            WriteLoggedOutput();

            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine(new string('=', 80));
            TestContext.Out.WriteLine("");

            // Perform checks.
            if (cex == null)
            {
                if (_expectedStatus != tesh!.Result!.Status)
                    Assert.Fail($"Expected Status was '{_expectedStatus}'; actual was '{tesh.Result!.Status}'.");
            }
            else
            {
                if (_expectedStatus != SubscriberStatus.UnhandledException)
                    Assert.Fail($"Expected Status was '{_expectedStatus}'; actual was '{SubscriberStatus.UnhandledException}'.");

                if (_expectedExceptionType != null && !ExpectExceptionTest.VerifyExpectedException(_expectedExceptionType, _expectedExceptionMessage!, cex))
                    Assert.Fail($"Expected Exception was '{_expectedExceptionType}' message '{_expectedExceptionMessage}'; actual was '{cex.GetType()}' message '{cex.Message}'.");
            }

            PublishedEventsCheck();
        }

        /// <summary>
        /// Writes the logged output.
        /// </summary>
        private void WriteLoggedOutput()
        {
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"LOGGING >");
            var messages = CorrelationIdLogger.GetMessages(CorrelationId, true);
            if (messages.Count == 0)
                TestContext.Out.WriteLine("  None.");
            else
            {
                foreach (var l in messages)
                {
                    WriteTestContextLogMessage(l);
                }
            }
        }
    }
}