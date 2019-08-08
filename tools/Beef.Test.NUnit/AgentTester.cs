// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Entities;
using Beef.RefData;
using Beef.WebApi;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the base <b>Agent</b> testing capabilities.
    /// </summary>
    [DebuggerStepThrough()] 
    public abstract class AgentTester
    {
        private static readonly object _lock = new object();
        private static TestServer _testServer;
        private static IConfiguration _configuration;
        private static Action<HttpRequestMessage> _beforeRequest;

        private HttpStatusCode? _expectedStatusCode;
        private ErrorType? _expectedErrorType;
        private string _expectedErrorMessage;
        private MessageItemCollection _expectedMessages;

        /// <summary>
        /// Defines the default environment as 'Development'.
        /// </summary>
        public const string DefaultEnvironment = "Development";

        /// <summary>
        /// Gets or sets the default username (defaults to 'Anonymous').
        /// </summary>
        public static string DefaultUsername { get; set; } = "Anonymous";

        /// <summary>
        /// Defines an <b>ETag</b> value that should result in a concurrency error.
        /// </summary>
        public static readonly string ConcurrencyErrorETag = "ZZZZZZZZZZZZ";

        #region StartupTestServer

        /// <summary>
        /// Starts up the <see cref="TestServer"/> using the specified <b>API startup</b> <see cref="Type"/> and building the underlying configuration.
        /// </summary>
        /// <typeparam name="TStartup">The startup <see cref="Type"/>.</typeparam>
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="addEnvironmentVariables">Indicates whether to add support for environment variables (defaults to <c>true</c>).</param>
        /// <param name="environmentVariablesPrefix">Override the environment variables prexfix.</param>
        /// <param name="testServerConfig">An optional <see cref="Action{TestServer}"/> to further configure the underlying <see cref="TestServer"/>.</param>
        public static void StartupTestServer<TStartup>(string environment = DefaultEnvironment, bool addEnvironmentVariables = true, string environmentVariablesPrefix = null, Action<TestServer> testServerConfig = null) where TStartup : class
        {
            var cb = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(TStartup).Assembly), $"webapisettings.{environment}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);

            if (addEnvironmentVariables)
                cb.AddEnvironmentVariables(environmentVariablesPrefix);

            StartupTestServer<TStartup>(cb.Build(), environment, testServerConfig);
        }

        /// <summary>
        /// Starts up the <see cref="TestServer"/> using the specified <b>API startup</b> <see cref="Type"/> and <paramref name="config"/>.
        /// </summary>
        /// <typeparam name="TStartup">The startup <see cref="Type"/>.</typeparam>
        /// <param name = "config" > The <see cref="IConfiguration"/>.</param> 
        /// <param name="environment">The environment to be used by the underlying web host.</param>
        /// <param name="testServerConfig">An optional <see cref="Action{TestServer}"/> to further configure the underlying <see cref="TestServer"/>.</param>
        public static void StartupTestServer<TStartup>(IConfiguration config, string environment = DefaultEnvironment, Action<TestServer> testServerConfig = null) where TStartup : class
        {
            lock (_lock)
            {
                if (_testServer != null)
                    throw new InvalidOperationException("The TestServer can only be started up once only.");

                var whb = new WebHostBuilder()
                    .UseEnvironment(environment)
                    .UseConfiguration(config)
                    .UseStartup<TStartup>();

                var testServer = new TestServer(whb);
                testServerConfig?.Invoke(testServer);

                _configuration = config;
                _testServer = testServer;
            }
        }

        /// <summary>
        /// Gets the <see cref="TestServer"/>.
        /// </summary>
        public static TestServer TestServer
        {
            get
            {
                lock (_lock)
                {
                    if (_testServer != null)
                        return _testServer;

                    throw new InvalidOperationException("TestServer is not running; use StartupTestServer method to start.");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IConfiguration"/>.
        /// </summary>
        public static IConfiguration Configuration
        {
            get
            {
                lock (_lock)
                {
                    if (_testServer != null && _configuration != null)
                        return _configuration;

                    throw new InvalidOperationException("TestServer is not running; use StartupTestServer method to start.");
                }
            }
        }

        #endregion

        #region AssertCompare

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
        public static void AssertCompare<T>(T expected, T actual)
        {
            AssertCompare(GetDefaultComparisonConfig(), expected, actual);
        }

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

        #endregion

        #region RegisterBeforeRequest

        /// <summary>
        /// Registers the <see cref="Action{HttpRequestMessage}"/> to perform an additional processing of the request before sending.
        /// </summary>
        /// <param name="beforeRequest">The before request action.</param>
        public static void RegisterBeforeRequest(Action<HttpRequestMessage> beforeRequest)
        {
            _beforeRequest = beforeRequest;
        }

        /// <summary>
        /// Executes the before request action where registered (see <see cref="RegisterBeforeRequest(Action{HttpRequestMessage})"/>.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        protected static void BeforeRequest(HttpRequestMessage requestMessage)
        {
            _beforeRequest?.Invoke(requestMessage);
        }

        #endregion

        #region Create

        /// <summary>
        /// Create a new <see cref="AgentTester{TAgent}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTester{TResult}"/> instance.</returns>
        public static AgentTester<TAgent> Create<TAgent>(string username = null, object args = null) where TAgent : class
        {
            return new AgentTester<TAgent>(username, args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTester{TAgent, TValue}"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTester{TValue}"/> instance</returns>
        public static AgentTester<TAgent, TValue> Create<TAgent, TValue>(string username = null, object args = null) where TAgent : class
        {
            return new AgentTester<TAgent, TValue>(username, args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTester{TAgent}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="UsernameConverter"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTester{TResult}"/> instance.</returns>
        public static AgentTester<TAgent> Create<TAgent>(object userIdentifier, object args = null) where TAgent : class
        {
            return new AgentTester<TAgent>(UsernameConverter?.Invoke(userIdentifier), args);
        }

        /// <summary>
        /// Create a new <see cref="AgentTester{TAgent, TValue}"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="UsernameConverter"/>).
        /// </summary>
        /// <typeparam name="TAgent">The <b>Agent</b> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The response value <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>An <see cref="AgentTester{TValue}"/> instance</returns>
        public static AgentTester<TAgent, TValue> Create<TAgent, TValue>(object userIdentifier, object args = null) where TAgent : class
        {
            return new AgentTester<TAgent, TValue>(UsernameConverter?.Invoke(userIdentifier), args);
        }

        /// <summary>
        /// Gets or sets the username converter function for when a non-string identifier is specified.
        /// </summary>
        /// <remarks>The <c>object</c> value is the user identifier.</remarks>
        public static Func<object, string> UsernameConverter { get; set; } = (x) => x?.ToString();

        /// <summary>
        /// Gets or sets the function for creating the <see cref="ExecutionContext"/> where there is no current instance.
        /// </summary>
        /// <remarks>The <c>string</c> is the <see cref="Username"/> and the <c>object</c> is the optional <see cref="Args"/>.</remarks>
        public static Func<string, object, ExecutionContext> CreateExecutionContext { get; set; } = (username, _) => new ExecutionContext { Username = username };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTester"/> class using the specified details.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>; otherwise, create using <see cref="CreateExecutionContext"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        protected AgentTester(string username = null, object args = null)
        {
            TestSetUp.InvokeRegisteredSetUp();

            if (username != null || !ExecutionContext.HasCurrent)
            {
                ExecutionContext.Reset(false);
                ExecutionContext.SetCurrent(CreateExecutionContext.Invoke(username ?? DefaultUsername, args));
            }

            Args = args;
            Username = ExecutionContext.Current.Username;
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Gets the optional argument.
        /// </summary>
        public object Args { get; private set; }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        protected void SetExpectStatusCode(HttpStatusCode statusCode)
        {
            _expectedStatusCode = statusCode;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message text will not be checked.</param>
        protected void SetExpectErrorType(ErrorType errorType, string errorMessage = null)
        {
            _expectedErrorType = errorType;
            _expectedErrorMessage = errorMessage;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTester"/> instance to support fluent/chaining usage.</returns>
        protected void SetExpectMessages(params string[] messages)
        {
            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            SetExpectMessages(mic);
        }

        /// <summary>
        /// Expect a response with the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The <see cref="AgentTester"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        protected void SetExpectMessages(MessageItemCollection messages)
        {
            Check.NotNull(messages, nameof(messages));
            if (_expectedMessages == null)
                _expectedMessages = new MessageItemCollection();

            _expectedMessages.AddRange(messages);
        }

        /// <summary>
        /// Check the result to make sure it is valid.
        /// </summary>
        /// <param name="result">The <see cref="WebApiAgentResult"/>.</param>
        /// <param name="sw">The <see cref="Stopwatch"/> used to measure <see cref="WebApiServiceAgentBase"/> invocation.</param>
        protected void ResultCheck(WebApiAgentResult result, Stopwatch sw)
        {
            Check.NotNull(result, nameof(result));

            // Log to output.
            Logger.Default.Info("");
            Logger.Default.Info("AGENT TESTER...");
            Logger.Default.Info("");
            Logger.Default.Info($"REQUEST >");
            Logger.Default.Info($"Request: {result.Request.Method} {result.Request.RequestUri}");

            if (!string.IsNullOrEmpty(Username))
                Logger.Default.Info($"Username: {Username}");

            Logger.Default.Info($"Headers: {(result.Request.Headers == null || !result.Request.Headers.Any() ? "none" : "")}");
            if (result.Request.Headers != null && result.Request.Headers.Any())
            {
                foreach (var hdr in result.Request.Headers)
                {
                    var sb = new StringBuilder();
                    foreach (var v in hdr.Value)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        sb.Append(v);
                    }

                    Logger.Default.Info($"  {hdr.Key}: {sb}");
                }
            }

            JToken json = null;
            if (result.Request.Content != null)
            {
                try
                {
                    json = JToken.Parse(result.Request.Content.ReadAsStringAsync().Result);
                }
                catch (Exception) { }

                Logger.Default.Info($"Content [{result.Request.Content?.Headers?.ContentType?.MediaType}]:");
                Logger.Default.Info(json == null ? result.Request.Content.ToString() : json.ToString());
            }

            Logger.Default.Info("");
            Logger.Default.Info($"RESPONSE >");
            Logger.Default.Info($"HttpStatusCode: {result.StatusCode} ({(int)result.StatusCode})");
            Logger.Default.Info($"Elapsed (ms): {(sw == null ? "none" : sw.ElapsedMilliseconds.ToString())}");

            var hdrs = result.Response?.Headers?.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Logger.Default.Info($"Headers: {(hdrs == null || !hdrs.Any() ? "none" : "")}");
            if (hdrs != null && hdrs.Any())
            {
                foreach (var hdr in hdrs)
                {
                    Logger.Default.Info($"  {hdr}");
                }
            }

            Logger.Default.Info($"Messages: {(result.Messages == null || result.Messages.Count == 0 ? "none" : "")}");

            if (result.Messages != null && result.Messages.Count > 0)
            {
                foreach (var m in result.Messages)
                {
                    Logger.Default.Info($" {m.Type}: {m.Text} {(m.Property == null ? "" : "(" + m.Property + ")")}");
                }

                Logger.Default.Info(null);
            }

            json = null;
            if (!string.IsNullOrEmpty(result.Content) && result.Response?.Content?.Headers?.ContentType?.MediaType == "application/json")
            {
                try
                {
                    json = JToken.Parse(result.Content);
                }
                catch (Exception) { /* This is being swallowed by design. */ }
            }

            TestContext.Out.Write($"Content: ");
            if (json != null)
            {
                Logger.Default.Info(null);
                Logger.Default.Info(json.ToString());
            }
            else
                Logger.Default.Info($"{(string.IsNullOrEmpty(result.Content) ? "none" : result.Content)}");

            Logger.Default.Info(null);
            Logger.Default.Info(new string('=', 80));
            Logger.Default.Info(null);

            // Perform checks.
            if (_expectedStatusCode.HasValue && _expectedStatusCode != result.StatusCode)
                Assert.Fail($"Expected HttpStatusCode was '{_expectedStatusCode} ({(int)_expectedStatusCode})'; actual was {result.StatusCode} ({(int)result.StatusCode}).");

            if (_expectedErrorType.HasValue && _expectedErrorType != result.ErrorType)
                Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{result.ErrorType}'.");

            if (_expectedErrorMessage != null && _expectedErrorMessage != result.ErrorMessage)
                Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{result.ErrorMessage}'.");

            if (_expectedMessages != null)
                ExpectValidationException.CompareExpectedVsActual(_expectedMessages, result.Messages);
        }
    }

    /// <summary>
    /// Provides the <see cref="WebApiAgentResult"/> testing.
    /// </summary>
    [DebuggerStepThrough()]
    public class AgentTester<TAgent> : AgentTester where TAgent : class
    {
        private Action<AgentTester<TAgent>> _beforeAction;
        private Action<AgentTester<TAgent>, WebApiAgentResult> _afterAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTester{TAgent}"/> class with a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public AgentTester(string username = null, object args = null) : base(username, args) { }

        /// <summary>
        /// An action to perform <b>before</b> the <see cref="Run(Func{AgentTesterRunArgs{TAgent}, Task{WebApiAgentResult}})"/> or <see cref="RunAsync(Func{AgentTesterRunArgs{TAgent}, Task{WebApiAgentResult}})"/>.
        /// </summary>
        /// <param name="action">The <b>before</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester Before(Action<AgentTester> action)
        {
            _beforeAction = action;
            return this;
        }

        /// <summary>
        /// An action to perform <b>after</b> the <see cref="Run(Func{AgentTesterRunArgs{TAgent}, Task{WebApiAgentResult}})"/> or <see cref="RunAsync(Func{AgentTesterRunArgs{TAgent}, Task{WebApiAgentResult}})"/> and all ignore/expect checks.
        /// </summary>
        /// <param name="action">The <b>after</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent> After(Action<AgentTester<TAgent>, WebApiAgentResult> action)
        {
            _afterAction = action;
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent> ExpectStatusCode(HttpStatusCode statusCode)
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
        public AgentTester<TAgent> ExpectErrorType(ErrorType errorType, string errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent> ExpectMessages(params string[] messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The <see cref="AgentTester{TAgent}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        public AgentTester<TAgent> ExpectMessages(MessageItemCollection messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public WebApiAgentResult Run(Func<AgentTesterRunArgs<TAgent>, Task<WebApiAgentResult>> func)
        {
            return Task.Run(() => RunAsync(func)).Result;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="Task{TResult}"/>.</returns>
        public async Task<WebApiAgentResult> RunAsync(Func<AgentTesterRunArgs<TAgent>, Task<WebApiAgentResult>> func)
        {
            Check.NotNull(func, nameof(func));

            _beforeAction?.Invoke(this);
            var sw = Stopwatch.StartNew();
            WebApiAgentResult result = await func(GetRunArgs());
            sw.Stop();
            ResultCheck(result, sw);
            _afterAction?.Invoke(this, result);
            return result;
        }

        /// <summary>
        /// Gets an <see cref="AgentTesterRunArgs{TAgent}"/> instance.
        /// </summary>
        /// <returns>An <see cref="AgentTesterRunArgs{TAgent}"/> instance.</returns>
        public AgentTesterRunArgs<TAgent> GetRunArgs() => new AgentTesterRunArgs<TAgent> { Tester = this, Client = TestServer.CreateClient(), BeforeRequest = BeforeRequest };
    }

    /// <summary>
    /// Provides the <see cref="WebApiAgentResult{TValue}"/> testing.
    /// </summary>
    /// <typeparam name="TAgent">The agent <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The response <see cref="WebApiAgentResult{TValue}.Value"/> <see cref="Type"/>.</typeparam>
    [DebuggerStepThrough()]
    public class AgentTester<TAgent, TValue> : AgentTester where TAgent : class
    {
        private readonly ComparisonConfig _comparisonConfig = GetDefaultComparisonConfig();
        private Action<AgentTester<TAgent, TValue>> _beforeAction;
        private Action<AgentTester<TAgent, TValue>, WebApiAgentResult<TValue>> _afterAction;
        private bool _isExpectNullValue;
        private Func<AgentTester<TAgent, TValue>, TValue> _expectValueFunc;
        private bool _isExpectCreatedBy;
        private string _changeLogCreatedBy;
        private DateTime? _changeLogCreatedDate;
        private bool _isExpectUpdatedBy;
        private string _changeLogUpdatedBy;
        private DateTime? _changeLogUpdatedDate;
        private bool _isExpectedETag;
        private string _previousETag;
        private bool _isExpectedUniqueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTester{TValue}"/> class with a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        public AgentTester(string username = null, object args = null) : base(username, args) { }

        /// <summary>
        /// An action to perform <b>before</b> the <see cref="Run(Func{AgentTesterRunArgs{TAgent, TValue}, Task{WebApiAgentResult{TValue}}})"/> or <see cref="RunAsync(Func{AgentTesterRunArgs{TAgent, TValue}, Task{WebApiAgentResult{TValue}}})"/>.
        /// </summary>
        /// <param name="action">The <b>before</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> Before(Action<AgentTester<TAgent, TValue>> action)
        {
            _beforeAction = action;
            return this;
        }

        /// <summary>
        /// An action to perform <b>after</b> the <see cref="Run(Func{AgentTesterRunArgs{TAgent, TValue}, Task{WebApiAgentResult{TValue}}})"/> or <see cref="RunAsync(Func{AgentTesterRunArgs{TAgent, TValue}, Task{WebApiAgentResult{TValue}}})"/> and all ignore/expect checks.
        /// </summary>
        /// <param name="action">The <b>after</b> action.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> After(Action<AgentTester<TAgent, TValue>, WebApiAgentResult<TValue>> action)
        {
            _afterAction = action;
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The expected <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> ExpectStatusCode(HttpStatusCode statusCode)
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
        public AgentTester<TAgent, TValue> ExpectErrorType(ErrorType errorType, string errorMessage = null)
        {
            SetExpectErrorType(errorType, errorMessage);
            return this;
        }

        /// <summary>
        /// Expect a response with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> ExpectMessages(params string[] messages)
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
        public AgentTester<TAgent, TValue> ExpectMessages(MessageItemCollection messages)
        {
            SetExpectMessages(messages);
            return this;
        }

        /// <summary>
        /// Expect <c>null</c> response value.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> ExpectNullValue()
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
        public AgentTester<TAgent, TValue> ExpectValue(Func<AgentTester<TAgent, TValue>, TValue> valueFunc, params string[] membersToIgnore)
        {
            _expectValueFunc = Check.NotNull(valueFunc, nameof(valueFunc));
            _comparisonConfig.MembersToIgnore.AddRange(membersToIgnore);
            return this;
        }

        /// <summary>
        /// Ignores the <see cref="IChangeLog"/> properties.
        /// </summary>
        /// <returns>The <see cref="AgentTester{TAgent, TValue}"/> instance to support fluent/chaining usage.</returns>
        public AgentTester<TAgent, TValue> IgnoreChangeLog()
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
        public AgentTester<TAgent, TValue> ExpectChangeLogCreated(string createdby = null, DateTime? createdDateGreaterThan = null)
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
        public AgentTester<TAgent, TValue> ExpectChangeLogUpdated(string updatedby = null, DateTime? updatedDateGreaterThan = null)
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
        public AgentTester<TAgent, TValue> IgnoreETag()
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
        public AgentTester<TAgent, TValue> ExpectETag(string previousETag = null)
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
        public AgentTester<TAgent, TValue> ExpectUniqueKey()
        {
            Check.IsTrue(typeof(TValue).GetInterface(typeof(IUniqueKey).Name) != null, "TValue must implement the interface IUniqueKey.");

            _isExpectedUniqueKey = true;
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding <see cref="WebApiAgentResult{TValue}"/>.</returns>
        public WebApiAgentResult<TValue> Run(Func<AgentTesterRunArgs<TAgent, TValue>, Task<WebApiAgentResult<TValue>>> func)
        {
            return Task.Run(() => RunAsync(func)).Result;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> asynchonously checking against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The corresponding result.</returns>
        public async Task<WebApiAgentResult<TValue>> RunAsync(Func<AgentTesterRunArgs<TAgent, TValue>, Task<WebApiAgentResult<TValue>>> func)
        {
            Check.NotNull(func, nameof(func));

            // Execute the before action.
            _beforeAction?.Invoke(this);

            // Execute the function.
            var sw = Stopwatch.StartNew();
            WebApiAgentResult<TValue> result = await func(GetRunArgs());
            sw.Stop();

            // Check expectations.
            ResultCheck(result, sw);

            if (_isExpectedUniqueKey && result.Value is IUniqueKey uk)
                _comparisonConfig.MembersToIgnore.AddRange(uk.UniqueKeyProperties);

            if (_isExpectNullValue && result.HasValue)
                Assert.Fail($"Expected null response value; the following content was returned: '{result.Content}'.");

            if (_expectValueFunc != null && !result.HasValue)
                Assert.Fail($"Expected non-null response content; no response returned.");

            if (_isExpectCreatedBy)
            {
                var cl = result.Value as IChangeLog;
                if (cl.ChangeLog == null || string.IsNullOrEmpty(cl.ChangeLog.CreatedBy))
                    Assert.Fail("Expected IChangeLog.UpdatedBy to have a non-null value.");
                else if (_changeLogCreatedBy != cl.ChangeLog.CreatedBy)
                    Assert.Fail($"Expected IChangeLog.UpdatedBy '{_changeLogCreatedBy}'; actual '{cl.ChangeLog.CreatedBy}'.");

                if (!cl.ChangeLog.CreatedDate.HasValue)
                    Assert.Fail("Expected IChangeLog.UpdatedDate to have a non-null value.");
                else if (cl.ChangeLog.CreatedDate.Value < _changeLogCreatedDate)
                    Assert.Fail($"Expected IChangeLog.UpdatedDate actual '{cl.ChangeLog.CreatedDate.Value}' to be greater than expected '{_changeLogCreatedDate}'.");
            }

            if (_isExpectUpdatedBy)
            {
                var cl = result.Value as IChangeLog;
                if (cl.ChangeLog == null || string.IsNullOrEmpty(cl.ChangeLog.UpdatedBy))
                    Assert.Fail("Expected IChangeLog.UpdatedBy to have a non-null value.");
                else if (_changeLogUpdatedBy != cl.ChangeLog.UpdatedBy)
                    Assert.Fail($"Expected IChangeLog.UpdatedBy '{_changeLogUpdatedBy}'; actual was '{cl.ChangeLog.UpdatedBy}'.");

                if (!cl.ChangeLog.UpdatedDate.HasValue)
                    Assert.Fail("Expected IChangeLog.UpdatedDate to have a non-null value.");
                else if (cl.ChangeLog.UpdatedDate.Value < _changeLogUpdatedDate)
                    Assert.Fail($"Expected IChangeLog.UpdatedDate actual '{cl.ChangeLog.UpdatedDate.Value}' to be greater than expected '{_changeLogUpdatedDate}'.");
            }

            if (_isExpectedETag)
            {
                var et = result.Value as IETag;
                if (et.ETag == null)
                    Assert.Fail("Expected IETag.ETag to have a non-null value.");

                if (!string.IsNullOrEmpty(_previousETag) && _previousETag == et.ETag)
                    Assert.Fail("Expected IETag.ETag value is the same as previous.");
            }

            if (_isExpectedUniqueKey)
            {
                uk = result.Value as IUniqueKey;
                if (uk.UniqueKey.Args.Any(x => x == null))
                    Assert.Fail("Expected IUniqueKey.Args array to have no null values.");
            }

            if (_expectValueFunc != null)
            {
                var exp = _expectValueFunc(this);
                if (exp == null)
                    throw new InvalidOperationException("ExpectValue function must not return null.");

                // Further configure the comparison configuration.
                if (ReferenceDataManager.HasCurrent)
                    _comparisonConfig.TypesToIgnore.AddRange(ReferenceDataManager.Current.GetAllTypes());

                if (typeof(TValue).GetInterface(typeof(IUniqueKey).Name) != null)
                    _comparisonConfig.MembersToIgnore.AddRange(new string[] { "UniqueKey", "HasUniqueKey", "UniqueKeyProperties" });

                if (typeof(TValue).GetInterface(typeof(IChangeTrackingLogging).Name) != null)
                    _comparisonConfig.MembersToIgnore.Add("ChangeTracking");

                if (typeof(TValue).GetInterface(typeof(IEntityCollectionResult).Name) != null)
                    _comparisonConfig.MembersToIgnore.AddRange(new string[] { "Paging", "ItemType" });

                // Perform the actual configuration.
                var cl = new CompareLogic(_comparisonConfig);
                var cr = cl.Compare(exp, result.Value);
                if (!cr.AreEqual)
                    Assert.Fail($"Expected vs Actual value mismatch: {cr.DifferencesString}");
            }

            // Execute the after action.
            _afterAction?.Invoke(this, result);

            return result;
        }

        /// <summary>
        /// Gets an <see cref="AgentTesterRunArgs{TAgent, TValue}"/> instance.
        /// </summary>
        /// <returns>An <see cref="AgentTesterRunArgs{TAgent, TValue}"/> instance.</returns>
        public AgentTesterRunArgs<TAgent, TValue> GetRunArgs() => new AgentTesterRunArgs<TAgent, TValue> { Tester = this, Client = TestServer.CreateClient(), BeforeRequest = BeforeRequest };
    }
}
