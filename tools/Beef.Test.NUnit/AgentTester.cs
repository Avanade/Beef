// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Entities;
using Beef.Test.NUnit.Tests;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the default capabilities for the <see cref="AgentTesterWaf{TStartup}"/>, <see cref="AgentTest{TStartup, TAgent}"/> and <see cref="AgentTest{TStartup, TAgent, TValue}"/>.
    /// </summary>
    public static class AgentTester
    {
        private static Func<object?, string> _usernameConverter = (x) => x?.ToString()!;
        private static Func<string?, object?, ExecutionContext> _executionContextCreator = (username, _) => new ExecutionContext { Username = username ?? DefaultUsername };
        private static Action<HttpRequestMessage>? _beforeRequest;
        private static readonly object _lock = new object();
        private static Func<int, object?, bool>? _registeredSetup;
        private static Func<int, object?, Task<bool>>? _registeredSetupAsync;
        private static bool _registeredSetupInvoked;
        private static object? _registeredSetupData;
        private static int _registeredSetupCount;
        private static bool _bypassSetup = false;

        /// <summary>
        /// Static constructor - binds the global logger (<see cref="Beef.Diagnostics.Logger.RegisterGlobal(Action{Diagnostics.LoggerArgs})"/>) to <see cref="TestContext.Out"/>.
        /// </summary>
        static AgentTester() => Beef.Diagnostics.Logger.RegisterGlobal((largs) => TestContext.Out.WriteLine($"{largs}"));

        /// <summary>
        /// Defines the default environment as 'Development'.
        /// </summary>
        public const string DefaultEnvironment = "Development";

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
        /// Sets the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending.
        /// </summary>
        /// <param name="request">The <see cref="Action{HttpRequestMessage}"/>.</param>
        public static void SetBeforeRequest(Action<HttpRequestMessage> request) => _beforeRequest = request;

        /// <summary>
        /// Gets or sets the <see cref="Action{HttpRequestMessage}"/> to perform any additional processing of the request before sending.
        /// </summary>
        public static Action<HttpRequestMessage>? GetBeforeRequest() => _beforeRequest;

        /// <summary>
        /// Gets or sets the default username (defaults to 'Anonymous').
        /// </summary>
        public static string DefaultUsername { get; set; } = "Anonymous";

        /// <summary>
        /// Defines an <b>ETag</b> value that should result in a concurrency error.
        /// </summary>
        public static readonly string ConcurrencyErrorETag = "ZZZZZZZZZZZZ";

        /// <summary>
        /// Indicates whether to verify that no events were published as the default behaviour (see <see cref="AgentTest{TStartup, TAgent}.ExpectNoEvents"/> and <see cref="AgentTest{TStartup, TAgent, TValue}.ExpectNoEvents"/>).
        /// </summary>
        public static bool DefaultExpectNoEvents { get; set; }

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
        /// Builds the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) where 'KeyVaultName' config key has value,
        /// 2) User Secrets where 'UseUserSecrets' config key has value (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (see <paramref name="environmentVariablePrefix"/>),
        /// 4) appsettings.{environment}.json, 5) appsettings.json, 6) webapisettings.{environment}.json (embedded resource within <typeparamref name="TStartup"/> assembly), and 7) webapisettings.json (embedded resource within <typeparamref name="TStartup"/> assembly).
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <returns>The <see cref="IConfiguration"/>.</returns>
        public static IConfiguration BuildConfiguration<TStartup>(string? environmentVariablePrefix = null, string? environment = DefaultEnvironment) where TStartup : class
        {
            var cb = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.{environment}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);

            if (string.IsNullOrEmpty(environmentVariablePrefix))
                cb.AddEnvironmentVariables();
            else
                cb.AddEnvironmentVariables(environmentVariablePrefix.EndsWith("_", StringComparison.InvariantCulture) ? environmentVariablePrefix : environmentVariablePrefix + "_");

            var config = cb.Build();
            if (config.GetValue<bool>("UseUserSecrets"))
                cb.AddUserSecrets<TStartup>();

            var kvn = config["KeyVaultName"];
            if (!string.IsNullOrEmpty(kvn))
            {
                var astp = new AzureServiceTokenProvider();
#pragma warning disable CA2000 // Dispose objects before losing scope; this object MUST NOT be disposed or will result in further error - only a single instance so is OK.
                var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(astp.KeyVaultTokenCallback));
                cb.AddAzureKeyVault($"https://{kvn}.vault.azure.net/", kvc, new DefaultKeyVaultSecretManager());
#pragma warning restore CA2000
            }

            return cb.Build();
        }

        /// <summary>
        /// Registers the synchronous <paramref name="setup"/> that will be invoked each time that <see cref="Reset(bool, object)"/> is invoked to reset.
        /// </summary>
        /// <param name="setup">The setup function to invoke. The first argument is the current count of invocations, and second is the optional data object. The return value is used to set
        /// <see cref="ShouldContinueRunningTests"/>.</param>
        public static void RegisterSetup(Func<int, object?, bool> setup)
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
        public static void RegisterSetup(Func<int, object?, Task<bool>> setupAsync)
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
                Assert.Inconclusive("This test cannot be executed as AgentTester.ShouldContinueRunningTests has been set to false.");
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
                        Logger.Default.Info(null);
                        Logger.Default.Info("Invocation of registered set up action.");
                        Logger.Default.Info(new string('=', 80));

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
                        Logger.Default.Exception(ex, $"This RegisterSetUp function failed to execute successfully: {ex.Message}");
                        Assert.Fail($"This RegisterSetUp function failed to execute successfully: {ex.Message}");
                    }
#pragma warning restore CA1031
                    finally
                    {
                        _registeredSetupInvoked = true;
                        Logger.Default.Info(null);
                        Logger.Default.Info(new string('=', 80));
                        Logger.Default.Info(null);
                    }
                }
            }
        }

        /// <summary>
        /// Reset testing to a known initial state; will result in the <see cref="RegisterSetup(Func{int, object, bool})">registered</see> set up function being executed.
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
        /// Reset testing to a known initial state; will <b>not</b> result in <see cref="RegisterSetup(Func{int, object, bool})">registered</see> set up function being executed.
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

        /// <summary>
        /// Creates an <see cref="AgentTesterServer{TStartup}"/> to manage the orchestration of the <see cref="TestServer"/> to execute one or more integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied).</param>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="config">The <see cref="IConfiguration"/>; defaults to <see cref="AgentTester.BuildConfiguration{TStartup}(string?, string?)"/> where <c>null</c>.</param>
        /// <param name="configureServices">An optional action to perform further <see cref="IServiceCollection"/> configuration.</param>
        /// <returns>An <see cref="AgentTesterServer{TStartup}"/> instance.</returns>
        public static AgentTesterServer<TStartup> CreateServer<TStartup>(string? environmentVariablePrefix = null, string environment = AgentTester.DefaultEnvironment, IConfiguration? config = null, Action<WebHostBuilderContext, IServiceCollection>? configureServices = null)
            where TStartup : class => new AgentTesterServer<TStartup>(environmentVariablePrefix, environment, config, configureServices);

        /// <summary>
        /// Creates an <see cref="AgentTesterWaf{TStartup}"/> to manage the orchestration of the <see cref="WebApplicationFactory{TStartup}"/> to execute one or more integration tests against.
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="configuration">The <see cref="Action{IWebHostBuilder}"/>.</param>
        /// <returns>An <see cref="AgentTesterWaf{TStartup}"/> instance.</returns>
        public static AgentTesterWaf<TStartup> CreateWaf<TStartup>(Action<IWebHostBuilder> configuration) where TStartup : class => new AgentTesterWaf<TStartup>(configuration);

        /// <summary>
        /// Creates an <see cref="AgentTesterWaf{TStartup}"/> to manage the orchestration of the <see cref="WebApplicationFactory{TStartup}"/> to execute one or more integration tests against enabling specific
        /// <b>API</b> <paramref name="services"/> to be configured/replaced (dependency injection).
        /// </summary>
        /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
        /// <param name="services">The <see cref="Action{IServiceCollection}"/>.</param>
        /// <returns>An <see cref="AgentTesterWaf{TStartup}"/> instance.</returns>
        public static AgentTesterWaf<TStartup> CreateWaf<TStartup>(Action<IServiceCollection>? services = null) where TStartup : class => new AgentTesterWaf<TStartup>(services);
    }
}