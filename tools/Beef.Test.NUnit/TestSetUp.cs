// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.RefData;
using Beef.Test.NUnit.Logging;
using Beef.Test.NUnit.Tests;
using Beef.WebApi;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Orchestrates the setup for testing; whilst also providing reusable utility methods.
    /// </summary>
    public sealed class TestSetUp
    {
        private static readonly object _lock = new object();
        private static Func<int, object?, bool>? _registeredSetup;
        private static Func<int, object?, Task<bool>>? _registeredSetupAsync;
        private static bool _registeredSetupInvoked;
        private static object? _registeredSetupData;
        private static int _registeredSetupCount;
        private static bool _bypassSetup = false;
        private static Type? _refServiceType;
        private static Type? _refProviderType;
        private static Type? _refAgentServiceType;
        private static Type? _refAgentType;
        private static Func<object?, string> _usernameConverter = (x) => x?.ToString()!;
        private static Func<string?, object?, ExecutionContext> _executionContextCreator = (username, _) => new ExecutionContext { Username = username ?? DefaultUsername! ?? throw new InvalidOperationException($"{nameof(DefaultUsername)} must not be null.") };
        internal static readonly Dictionary<Type, Type> _webApiAgentArgsTypes = new Dictionary<Type, Type>() { { typeof(IWebApiAgentArgs), typeof(WebApiAgentArgs) } };

        #region Setup

        /// <summary>
        /// Registers the synchronous <paramref name="setup"/> that will be invoked each time that <see cref="Reset(bool, object)"/> is invoked to reset.
        /// </summary>
        /// <param name="setup">The setup function to invoke. The first argument is the current count of invocations, and second is the optional data object. The return value is used to set
        /// <see cref="ShouldContinueRunningTests"/>.</param>
        public static void RegisterSetUp(Func<int, object?, bool> setup)
        {
            lock (_lock)
            {
                if (_registeredSetup != null || _registeredSetupAsync != null)
                    throw new InvalidOperationException("The RegisterSetUp can only be invoked once.");

                _registeredSetup = setup;
                _registeredSetupAsync = null;
            }
        }

        /// <summary>
        /// Registers the asynchronous <paramref name="setupAsync"/> that will be invoked each time that <see cref="Reset(bool, object)"/> is invoked to reset.
        /// </summary>
        /// <param name="setupAsync">The setup function to invoke. The first argument is the current count of invocations, and second is the optional data object. The return value is used to set
        /// <see cref="ShouldContinueRunningTests"/>.</param>
        public static void RegisterSetUp(Func<int, object?, Task<bool>> setupAsync)
        {
            lock (_lock)
            {
                if (_registeredSetup != null || _registeredSetupAsync != null)
                    throw new InvalidOperationException("The RegisterSetUp can only be invoked once.");

                _registeredSetupAsync = setupAsync;
                _registeredSetup = null;
            }
        }

        /// <summary>
        /// Indicates whether tests should continue running; otherwise, set to <c>false</c> for all other remaining tests to return inconclusive.
        /// </summary>
        public static bool ShouldContinueRunningTests { get; set; } = true;

        /// <summary>
        /// Checks the <see cref="ShouldContinueRunningTests"/> and performs an <see cref="Assert"/> <see cref="Assert.Inconclusive(string)"/> where <c>false</c>.
        /// </summary>
        public static void ShouldContinueRunningTestsAssert()
        {
            if (!ShouldContinueRunningTests)
                Assert.Inconclusive("This test cannot be executed as TestSetUp.ShouldContinueRunningTests has been set to false.");
        }

        /// <summary>
        /// Invokes the registered set up action.
        /// </summary>
        internal static void InvokeRegisteredSetUp()
        {
            lock (_lock)
            {
                ShouldContinueRunningTestsAssert();
                if (_bypassSetup)
                    return;

                if (!_registeredSetupInvoked && (_registeredSetup != null || _registeredSetupAsync != null))
                {
                    try
                    {
                        TestContext.Out.WriteLine();
                        TestContext.Out.WriteLine("Invocation of registered set up action.");
                        TestContext.Out.WriteLine(new string('=', 80));

                        if (_registeredSetup != null)
                            ShouldContinueRunningTests = _registeredSetup.Invoke(_registeredSetupCount++, _registeredSetupData);

                        if (_registeredSetupAsync != null)
                            ShouldContinueRunningTests = Task.Run(() => _registeredSetupAsync.Invoke(_registeredSetupCount++, _registeredSetupData)).GetAwaiter().GetResult();

                        if (!ShouldContinueRunningTests)
                            Assert.Fail("This RegisterSetUp function failed to execute successfully.");
                    }
                    catch (AssertionException) { throw; }
#pragma warning disable CA1031 // Do not catch general exception types; by-design, catches them all!
                    catch (Exception ex)
                    {
                        ShouldContinueRunningTests = false;
                        TestContext.Out.WriteLine($"This RegisterSetUp function failed to execute successfully: {ex.Message}{Environment.NewLine}{ex}");
                        Assert.Fail($"This RegisterSetUp function failed to execute successfully: {ex.Message}");
                    }
#pragma warning restore CA1031
                    finally
                    {
                        _registeredSetupInvoked = true;
                        TestContext.Out.WriteLine();
                        TestContext.Out.WriteLine(new string('=', 80));
                        TestContext.Out.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Reset testing to a known initial state; will result in the <see cref="RegisterSetUp(Func{int, object, bool})">registered</see> set up function being executed.
        /// </summary>
        /// <param name="setUpIfAlreadyDone">Indicates whether to perform the setup if already done; defaults to <c>true</c>.</param>
        /// <param name="data">Optional data to be passed to the resgitered set up function.</param>
        public static void Reset(bool setUpIfAlreadyDone = true, object? data = null)
        {
            lock (_lock)
            {
                if (!_registeredSetupInvoked || setUpIfAlreadyDone)
                {
                    ShouldContinueRunningTests = true;
                    _registeredSetupData = data;
                    _registeredSetupInvoked = false;
                    _bypassSetup = false;
                }
            }
        }

        /// <summary>
        /// Reset testing to a known initial state; will <b>not</b> result in <see cref="RegisterSetUp(Func{int, object, bool})">registered</see> set up function being executed.
        /// </summary>
        public static void ResetNoSetup()
        {
            lock (_lock)
            {
                ShouldContinueRunningTests = true;
                _registeredSetupData = null;
                _bypassSetup = true;
            }
        }

        #endregion

        /// <summary>
        /// Defines an <b>ETag</b> value that <i>should</i> result in a concurrency error.
        /// </summary>
        public const string ConcurrencyErrorETag = "ZZZZZZZZZZZZ";

        /// <summary>
        /// Defines the default environment as 'Development'.
        /// </summary>
        public const string DefaultEnvironment = "Development";

        /// <summary>
        /// Indicates whether to verify that no events were published as the default behaviour.
        /// </summary>
        public static bool DefaultExpectNoEvents { get; set; }

        /// <summary>
        /// Gets or sets the default username (defaults to 'Anonymous').
        /// </summary>
        public static string DefaultUsername { get; set; } = "Anonymous";

        /// <summary>
        /// Gets the default environment variable prefix.
        /// </summary>
        public static string? DefaultEnvironmentVariablePrefix { get; set; }

        /// <summary>
        /// Sets the username converter function for when a non-string identifier is specified.
        /// </summary>
        /// <param name="converter">The converter function.</param>
        /// <remarks>The <c>object</c> value is the user identifier.</remarks>
        public static void SetUsernameConverter(Func<object?, string> converter) => _usernameConverter = converter ?? throw new ArgumentNullException(nameof(converter));

        /// <summary>
        /// Converts a username using the function defined by <see cref="SetUsernameConverter(Func{object?, string})"/>.
        /// </summary>
        /// <param name="input">The input user.</param>
        /// <returns>The converted username.</returns>
        public static string ConvertUsername(object? input) => _usernameConverter(input);

        /// <summary>
        /// Sets the <see cref="ExecutionContext"/> creator function for local (non Web API) context.
        /// </summary>
        /// <param name="creator">The creator function.</param>
        public static void SetExecutionContextCreator(Func<string?, object?, ExecutionContext> creator) => _executionContextCreator = creator ?? throw new ArgumentNullException(nameof(creator));

        /// <summary>
        /// Creates an <see cref="ExecutionContext"/> using the function defined by <see cref="SetExecutionContextCreator(Func{string?, object?, ExecutionContext})"/>.
        /// </summary>
        /// <param name="username">The username; defaults to <see cref="DefaultUsername"/> where not specified.</param>
        /// <param name="arg">An optional argument.</param>
        /// <returns>The <see cref="ExecutionContext"/>.</returns>
        public static ExecutionContext CreateExecutionContext(string? username, object? arg) => _executionContextCreator(username, arg);

        /// <summary>
        /// Sets up the default local <see cref="IReferenceDataProvider">reference data</see> provider <see cref="Type"/>s to enable <see cref="ConfigureDefaultLocalReferenceData(IServiceCollection)"/>.
        /// </summary>
        /// <typeparam name="TRefService">The <see cref="Type"/> of the <i>provider</i> service to add.</typeparam>
        /// <typeparam name="TRefProvider">The <see cref="Type"/> of the <i>provider</i> implementation to use.</typeparam>
        /// <typeparam name="TRefAgentService">The <see cref="Type"/> of the <i>agent</i> service to add.</typeparam>
        /// <typeparam name="TRefAgent">The <see cref="Type"/> of the <i>agent</i> implementation to use.</typeparam>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        public static void SetDefaultLocalReferenceData<TRefService, TRefProvider, TRefAgentService, TRefAgent>()
            where TRefService : class, IReferenceDataProvider where TRefProvider : class, TRefService
            where TRefAgentService : class where TRefAgent : WebApiAgentBase, TRefAgentService
        {
            _refServiceType = typeof(TRefService);
            _refProviderType = typeof(TRefProvider);
            _refAgentServiceType = typeof(TRefAgentService);
            _refAgentType = typeof(TRefAgent);
        }

        /// <summary>
        /// Configures the local <see cref="IReferenceDataProvider">reference data</see> provider as defined by <see cref="SetDefaultLocalReferenceData{TRefService, TRefProvider, TRefAgentService, TRefAgent}"/>.
        /// </summary>
        /// <param name="service">The <see cref="IServiceCollection"/>.</param>
        public static void ConfigureDefaultLocalReferenceData(IServiceCollection service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (_refServiceType == null)
                return;

            service.AddSingleton(_refServiceType, _refProviderType)
                   .AddSingleton(_refAgentServiceType, _refAgentType);
        }

        /// <summary>
        /// Gets a default <see cref="ComparisonConfig"/> instance used for the <see cref="AssertCompare{T}(T, T)"/>.
        /// </summary>
        public static ComparisonConfig GetDefaultComparisonConfig() => new ComparisonConfig
        {
            CompareStaticFields = false,
            CompareStaticProperties = false,
            CompareReadOnly = false,
            CompareFields = false,
            MaxDifferences = 100,
            MaxMillisecondsDateDifference = 100
        };

        /// <summary>
        /// Verifies that two values are equal by performing a deep property comparison using the <see cref="GetDefaultComparisonConfig"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AssertCompare<T>(T expected, T actual) => AssertCompare(GetDefaultComparisonConfig(), expected, actual);

        /// <summary>
        /// Verifies that two values are equal by performing a deep property comparison using the specified <paramref name="comparisonConfig"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="comparisonConfig">The <see cref="CompareLogic"/>.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AssertCompare<T>(ComparisonConfig comparisonConfig, T expected, T actual)
        {
            var cl = new CompareLogic(Check.NotNull(comparisonConfig, nameof(comparisonConfig)));
            var cr = cl.Compare(expected, actual);
            if (!cr.AreEqual)
                Assert.Fail($"Expected vs Actual value mismatch: {cr.DifferencesString}");
        }

        /// <summary>
        /// Infer additional members to ignore based on the <paramref name="valueType"/>.
        /// </summary>
        /// <param name="comparisonConfig">The <see cref="ComparisonConfig"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        internal static void InferAdditionalMembersToIgnore(ComparisonConfig comparisonConfig, Type valueType)
        {
            Check.NotNull(comparisonConfig, nameof(comparisonConfig));
            Check.NotNull(valueType, nameof(valueType));

            if (valueType.GetInterface(typeof(IUniqueKey).Name) != null)
                comparisonConfig.MembersToIgnore.AddRange(new string[] { "UniqueKey", "HasUniqueKey", "UniqueKeyProperties" });

            if (valueType.GetInterface(typeof(IChangeTrackingLogging).Name) != null)
                comparisonConfig.MembersToIgnore.Add("ChangeTracking");

            if (valueType.GetInterface(typeof(IEntityCollectionResult).Name) != null)
                comparisonConfig.MembersToIgnore.AddRange(new string[] { "Paging", "ItemType" });
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> instance that logs to the <see cref="System.Console"/>.
        /// </summary>
        /// <returns>The <see cref="ILogger"/> instance.</returns>
        public static ILogger CreateLogger()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddTestContext());
            var logger = services.BuildServiceProvider().GetService<ILogger<TestSetUp>>();
            return logger;
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider"/> instance that also includes an <see cref="ILogger"/>; also sets up and configures <see cref="ExecutionContext.Current"/>.
        /// </summary>
        /// <param name="serviceCollection">An optional action to allow further additions to the underlying <see cref="IServiceCollection"/>.</param>
        /// <param name="createExecutionContext">The function to override the creation of the <see cref="ExecutionContext"/> instance to a custom <see cref="Type"/>; defaults to <see cref="ExecutionContext"/> where not specified.</param>
        /// <returns>An <see cref="IServiceProvider"/>.</returns>
        public static IServiceProvider CreateServiceProvider(Action<IServiceCollection>? serviceCollection = null, Func<IServiceProvider, ExecutionContext>? createExecutionContext = null)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddTestContext());
            services.AddBeefExecutionContext(createExecutionContext);
            serviceCollection?.Invoke(services);
            var sp = services.BuildServiceProvider();
            var ec = sp.GetService<ExecutionContext>();
            ec.ServiceProvider = sp;
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(ec);
            return sp;
        }

        /// <summary>
        /// Adds additional <see cref="IWebApiAgentArgs"/> to <see cref="WebApiAgentArgs"/> equivalent mappings required for dependency injection. This configuration will be used by all testers that 
        /// inherit from <see cref="TesterBase"/>.
        /// </summary>
        /// <typeparam name="TService">The <see cref="IWebApiAgentArgs"/> service interface.</typeparam>
        /// <typeparam name="TImplementation">The <see cref="WebApiAgentArgs"/> service implementation.</typeparam>
        public static void AddWebApiAgentArgsType<TService, TImplementation>() where TService : class, IWebApiAgentArgs where TImplementation : class, TService
        {
            var itype = typeof(TService);
            if (!itype.IsInterface)
                throw new InvalidOperationException("The TService type must be an interface.");

            var stype = typeof(TImplementation);
            if (stype.IsInterface)
                throw new InvalidOperationException("The TImplementation must not be an interface.");

            _webApiAgentArgsTypes[itype] = typeof(TImplementation);
        }
    }
}