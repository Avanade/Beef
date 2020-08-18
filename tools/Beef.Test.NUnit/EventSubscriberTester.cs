// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Subscribe;
using Beef.Test.NUnit.Logging;
using Beef.Test.NUnit.Tests;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Manages the orchestration of the subscriber tester to execute one or more integration tests against. 
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    public class EventSubscriberTester<TStartup> : TesterBase where TStartup : class, new()
    {
        private class Fhb : IFunctionsHostBuilder
        {
            public Fhb(IServiceCollection services) => Services = services;

            public IServiceCollection Services { get; private set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberTester{TStartup}"/> class.
        /// </summary>
        public EventSubscriberTester() : base(false)
        {
            // TODO: Come back and revisit.
            // string? environmentVariablePrefix = null, string embeddedFilePrefix = "funcsettings", string environment = TestSetUp.DefaultEnvironment, Action<ConfigurationBuilder>? configurationBuilder = null, Action<IServiceCollection>? services = null
            //_serviceCollection.GetBeefConfiguration<TStartup>(environmentVariablePrefix, embeddedFilePrefix, environment, configurationBuilder);

            var sc = new ServiceCollection();

            var mi = typeof(TStartup).GetMethod("Configure", new Type[] { typeof(IFunctionsHostBuilder) });
            if (mi == null)
                throw new InvalidOperationException($"TStartup '{typeof(TStartup).Name}' must implement a 'Configure(IFunctionsHostBuilder)' method.");

            mi.Invoke(new TStartup(), new object[] { new Fhb(sc) });

            //services?.Invoke(_serviceCollection);
            sc.AddLogging(configure => configure.AddCorrelationId());
            ReplaceEventPublisher(sc);
            ServiceProvider = sc.BuildServiceProvider();
        }

        /// <summary>
        /// Gets the <see cref="ServiceProvider"/>.
        /// </summary>
        internal ServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Creates a new <see cref="EventSubscriberTest{TStartup, TSubscriber}"/> for the specified <typeparamref name="TSubscriber"/>.
        /// </summary>
        /// <typeparam name="TSubscriber">The <see cref="IEventSubscriber"/> <see cref="Type"/> to test.</typeparam>
        /// <returns>The <see cref="EventSubscriberTest{TStartup, TSubscriber}"/>.</returns>
        public EventSubscriberTest<TStartup, TSubscriber> Test<TSubscriber>() where TSubscriber : IEventSubscriber => new EventSubscriberTest<TStartup, TSubscriber>(this);
    }

    /// <summary>
    /// Provides the default testing capabilities for the <see cref="EventSubscriberTester{TStartup}"/>.
    /// </summary>
    public static class EventSubscriberTester
    {
        /// <summary>
        /// Creates an <see cref="EventSubscriberTester{TStartup}"/> to manage the orchestration of one or more <see cref="EventSubscriberBase"/> integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <returns>An <see cref="EventSubscriberTester{TStartup}"/> instance.</returns>
        public static EventSubscriberTester<TStartup> Create<TStartup>() where TStartup : class, new() => new EventSubscriberTester<TStartup>();
    }
}