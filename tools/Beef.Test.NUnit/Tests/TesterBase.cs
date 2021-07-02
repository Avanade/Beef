// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using Beef.Entities;
using Beef.Events;
using Beef.Test.NUnit.Events;
using Beef.Test.NUnit.Logging;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Beef.Test.NUnit.Tests
{
    /// <summary>
    /// Represents the generic base tester.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class TesterBase
    {
        private const string ServiceCollectionKey = "!nt3rn4lS3rv1c3C0ll3ct10n"; // Obfuscated InternalServiceCollection
        private readonly ServiceCollection _serviceCollection = new ServiceCollection();
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTesterBase"/> class.
        /// </summary>
        /// <param name="configureLocalRefData">Indicates whether the pre-set local <see cref="TestSetUp.SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}">reference data</see> is configured.</param>
        /// <param name="inheritServiceCollection">Indicates whether to use the inherit (copy) the tester <see cref="ServiceCollection"/> (advanced use only).</param>
        /// <param name="useCorrelationIdLogger">Indicates whether to use the <see cref="CorrelationIdLogger"/> versus defaulting to <see cref="TestContextLogger"/>.</param>
        protected TesterBase(bool configureLocalRefData = true, bool inheritServiceCollection = false, bool useCorrelationIdLogger = false)
        {
            // Where inheriting then copy the service collection.
            if (inheritServiceCollection && ExecutionContext.HasCurrent && ExecutionContext.Current.Properties.TryGetValue(ServiceCollectionKey, out var sc))
            {
                var coll = (ICollection<ServiceDescriptor>)_serviceCollection;
                foreach (var sd in (ICollection<ServiceDescriptor>)sc)
                {
                    coll.Add(sd);
                }

                return;
            }

            // Where nothing to inherit then create from scratch.
            _serviceCollection.AddSingleton(_ => new CachePolicyManager());
            _serviceCollection.AddLogging(configure =>
            {
                if (useCorrelationIdLogger)
                    configure.AddCorrelationId();
                else
                    configure.AddTestContext();
            });

            _serviceCollection.AddTransient<IWebApiAgentArgs, WebApiAgentArgs>();
            foreach (var kvp in TestSetUp._webApiAgentArgsTypes)
            {
                _serviceCollection.AddTransient(kvp.Key, kvp.Value);
            }

            if (configureLocalRefData)
                TestSetUp.ConfigureDefaultLocalReferenceData(_serviceCollection);
        }

        /// <summary>
        /// Provides the opportunity to further configure the <i>local</i> (non-API) test <see cref="IServiceCollection"/> (see <see cref="LocalServiceProvider"/>).
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        protected void ConfigureLocalServices(Action<IServiceCollection> serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection(_serviceCollection);
                _serviceProvider = null;
            }
        }

        /// <summary>
        /// Gets the <i>local</i> (non-API) test Service Provider (used for the likes of the service agents).
        /// </summary>
        protected internal IServiceProvider LocalServiceProvider => _serviceProvider ??= _serviceCollection.BuildServiceProvider();

        /// <summary>
        /// Prepares the <see cref="ExecutionContext"/> using the <see cref="TestSetUpAttribute"/> configuration, whilst also ensuring that the <see cref="LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        protected internal void PrepareExecutionContext() => PrepareExecutionContext(TestSetUpAttribute.Username, TestSetUpAttribute.Args);

        /// <summary>
        /// Prepares the <see cref="ExecutionContext"/> using the specified <paramref name="username"/>, whilst also ensuring that the <see cref="LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="TestSetUp.CreateExecutionContext"/> function).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>The <see cref="ExecutionContext"/> must be created by the <see cref="AgentTesterBase"/> as the <see cref="ExecutionContext.ServiceProvider"/> must be set to <see cref="LocalServiceProvider"/>.</remarks>
        public void PrepareExecutionContext(string? username, object? args = null)
        {
            ExecutionContext.Reset();
            var ec = TestSetUp.CreateExecutionContext(username ?? TestSetUp.DefaultUsername, args);
            ec.ServiceProvider = LocalServiceProvider;
            ec.Properties.Add(ServiceCollectionKey, _serviceCollection);
            if (string.IsNullOrEmpty(ec.CorrelationId))
                ec.CorrelationId = Guid.NewGuid().ToString();

            ExecutionContext.SetCurrent(ec);
        }

        /// <summary>
        /// Replaces the <see cref="IEventPublisher"/> with a <see cref="ExpectEventPublisher"/> instance (as scoped).
        /// </summary>
        /// <param name="sc">The <see cref="IServiceCollection"/>.</param>
        protected static void ReplaceEventPublisher(IServiceCollection sc)
        {
            sc.Remove<IEventPublisher>();
            sc.AddScoped<IEventPublisher>(_ => new ExpectEventPublisher());
        }

        /// <summary>
        /// Compares the expected versus actual messages and reports the differences.
        /// </summary>
        /// <param name="expectedMessages">The expected messages.</param>
        /// <param name="actualMessages">The actual messages.</param>
        public static void CompareExpectedVsActualMessages(MessageItemCollection? expectedMessages, MessageItemCollection? actualMessages)
        {
            var exp = (from e in expectedMessages ?? new MessageItemCollection()
                       where !(actualMessages ?? new MessageItemCollection()).Any(a => a.Type == e.Type && a.Text == e.Text && (e.Property == null || a.Property == e.Property))
                       select e).ToList();

            var act = (from a in actualMessages ?? new MessageItemCollection()
                       where !(expectedMessages ?? new MessageItemCollection()).Any(e => a.Type == e.Type && a.Text == e.Text && (e.Property == null || a.Property == e.Property))
                       select a).ToList();

            var sb = new StringBuilder();
            if (exp.Count > 0)
            {
                sb.AppendLine(" Expected messages not matched:");
                exp.ForEach(m => sb.AppendLine($"  {m.Type}: {m.Text} {(m.Property != null ? $"[{m.Property}]" : null)}"));
            }

            if (act.Count > 0)
            {
                sb.AppendLine(" Actual messages not matched:");
                act.ForEach(m => sb.AppendLine($"  {m.Type}: {m.Text} {(m.Property != null ? $"[{m.Property}]" : null)}"));
            }

            if (sb.Length > 0)
                Assert.Fail($"Messages mismatch:{System.Environment.NewLine}{sb}");
        }

        /// <summary>
        /// Writes the log message to <see cref="TestContext.Out"/> such that second and subsequent lines are indented.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteTestContextLogMessage(string message)
        {
            var sr = new StringReader(message);
            var first = true;
            while (true)
            {
                var l = sr.ReadLine();
                if (l == null)
                    return;

                if (first)
                    TestContext.Out.WriteLine($"{l}");
                else
                    TestContext.Out.WriteLine($"{new string(' ', 31)}{l}");

                first = false;
            }
        }
    }
}