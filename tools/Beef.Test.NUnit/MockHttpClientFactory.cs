// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.RefData;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the <see cref="IHttpClientFactory"/> mocking.
    /// </summary>
    public class MockHttpClientFactory
    {
        private readonly Dictionary<string, MockHttpClient> _mockClients = new Dictionary<string, MockHttpClient>();

        /// <summary>
        /// Creates the <see cref="MockHttpClientFactory"/>.
        /// </summary>
        /// <returns>The <see cref="MockHttpClientFactory"/>.</returns>
        public static MockHttpClientFactory Create() => new MockHttpClientFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpClientFactory"/> class.
        /// </summary>
        private MockHttpClientFactory() { }

        /// <summary>
        /// Gets the <see cref="Mock"/> <see cref="IHttpClientFactory"/>.
        /// </summary>
        internal Mock<IHttpClientFactory> HttpClientFactory { get; } = new Mock<IHttpClientFactory>();

        /// <summary>
        /// Creates the <see cref="MockHttpClient"/> with the specified logical <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The logical name of the client.</param>
        /// <param name="baseAddress">The base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.</param>
        /// <returns>The <see cref="MockHttpClient"/>.</returns>
        /// <remarks>Only a single client can be created per logical name.</remarks>
        public MockHttpClient CreateClient(string name, Uri? baseAddress = null)
        {
            if (_mockClients.ContainsKey(name ?? throw new ArgumentNullException(nameof(name))))
                throw new ArgumentException("This named client has already been defined.", nameof(name));

            var mc = new MockHttpClient(this, name, baseAddress);
            _mockClients.Add(name, mc);
            return mc;
        }

        /// <summary>
        /// Replaces (or adds) the singleton <see cref="IHttpClientFactory"/> within the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="sc">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> to support fluent-style method-chaining.</returns>
        public IServiceCollection ReplaceSingleton(IServiceCollection sc)
        {
            sc.ReplaceSingleton(HttpClientFactory.Object);
            return sc;
        }

        /// <summary>
        /// Gets the logically named mocked <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="name">The logical name of the client.</param>
        /// <returns>The <see cref="HttpClient"/> where it exists; otherwise; <c>null</c>.</returns>
        public HttpClient? GetHttpClient(string name) => _mockClients.GetValueOrDefault(name ?? throw new ArgumentNullException(nameof(name)))?.HttpClient;
    }

    /// <summary>
    /// Provides the <see cref="System.Net.Http.HttpClient"/> (more specifically <see cref="HttpMessageHandler"/>) mocking.
    /// </summary>
    public class MockHttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpClient"/> class.
        /// </summary>
        /// <param name="factory">The <see cref="MockHttpClientFactory"/>.</param>
        /// <param name="name">The logical name of the client.</param>
        /// <param name="baseAddress">The base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.</param>
        internal MockHttpClient(MockHttpClientFactory factory, string name, Uri? baseAddress)
        {
            HttpClient = new HttpClient(MessageHandler.Object) { BaseAddress = baseAddress ?? new Uri("https://unittest") };
            factory.HttpClientFactory.Setup(x => x.CreateClient(It.Is<string>(x => x == name))).Returns(() => HttpClient);
        }

        /// <summary>
        /// Gets the <see cref="Mock"/> <see cref="HttpMessageHandler"/>.
        /// </summary>
        internal Mock<HttpMessageHandler> MessageHandler { get; } = new Mock<HttpMessageHandler>();

        /// <summary>
        /// Gets the mocked <see cref="HttpClient"/>.
        /// </summary>
        internal HttpClient HttpClient { get; set; }

        /// <summary>
        /// Creates a new <see cref="MockHttpClientRequest"/> for the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="method">The <see cref="HttpMethod"/>.</param>
        /// <param name="requestUri">The string that represents the request <see cref="Uri"/>.</param>
        /// <returns>The <see cref="MockHttpClientRequest"/>.</returns>
        public MockHttpClientRequest Request(HttpMethod method, string requestUri) => new MockHttpClientRequest(this, method, requestUri);
    }

    /// <summary>
    /// Provides the <see cref="HttpRequestMessage"/> configuration for mocking.
    /// </summary>
    public class MockHttpClientRequest
    {
        private readonly MockHttpClient _client;
        private readonly HttpMethod _method;
        private readonly string _requestUri;
        private IMockHttpClientRequestAssert? _requestAssert;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpClientRequest"/> class.
        /// </summary>
        /// <param name="client">The <see cref="MockHttpClient"/>.</param>
        /// <param name="method">The <see cref="HttpMethod"/>.</param>
        /// <param name="requestUri">The string that represents the request <see cref="Uri"/>.</param>
        internal MockHttpClientRequest(MockHttpClient client, HttpMethod method, string requestUri)
        {
            _client = client;
            _method = method ?? throw new ArgumentNullException(nameof(method));
            _requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            Respond = new MockHttpClientResponse(this);
        }

        /// <summary>
        /// Provides the mocked response.
        /// </summary>
        /// <param name="content">The optional <see cref="HttpContent"/>.</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        internal MockHttpClient MockResponse(HttpContent? content = null, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null)
        {
            _client.MessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(x => x.Method == _method && x.RequestUri.ToString().EndsWith(_requestUri, StringComparison.InvariantCultureIgnoreCase) && ValidateRequestMessage(x)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    var resp = new HttpResponseMessage(statusCode ?? HttpStatusCode.OK);
                    if (content != null)
                        resp.Content = content;

                    response?.Invoke(resp);
                    return resp;
                });

            return _client;
        }

        /// <summary>
        /// Validate the request message.
        /// </summary>
        private bool ValidateRequestMessage(HttpRequestMessage request)
        {
            string? json = null;
            if (request.Content != null)
                json = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var l = Logger.Create<MockHttpClientFactory>();
            l.LogInformation($"HTTP Agent Request: {request.Method} {request.RequestUri} {json}");

            _requestAssert?.Assert(request);
            return true;
        }

        /// <summary>
        /// Asserts the <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="assert">The optional runtime action to verify the <see cref="HttpRequestMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClientRequestAssert"/>.</returns>
        public MockHttpClientRequestAssert Assert(Action<HttpRequestMessage>? assert)
        {
            var ra = new MockHttpClientRequestAssert(this, assert);
            _requestAssert = ra;
            return ra;
        }

        /// <summary>
        /// Asserts the <see cref="HttpRequestMessage"/> and its body.
        /// </summary>
        /// <typeparam name="TBody">The request body <see cref="Type"/>.</typeparam>
        /// <param name="assert">The optional runtime action to verify the <see cref="HttpRequestMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClientRequestAssert"/>.</returns>
        public MockHttpClientRequestAssert<TBody> Assert<TBody>(Action<HttpRequestMessage>? assert = null)
        {
            var ra = new MockHttpClientRequestAssert<TBody>(this, assert);
            _requestAssert = ra;
            return ra;
        }

        /// <summary>
        /// Gets the <see cref="MockHttpClientResponse"/>.
        /// </summary>
        public MockHttpClientResponse Respond { get; }
    }

    /// <summary>
    /// Enables the <see cref="HttpRequestMessage"/> assertion.
    /// </summary>
    internal interface IMockHttpClientRequestAssert
    {
        /// <summary>
        /// Asserts the <see cref="HttpRequestMessage"/> body.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        void Assert(HttpRequestMessage request);
    }

    /// <summary>
    /// Provides the request assertion.
    /// </summary>
    public class MockHttpClientRequestAssert : IMockHttpClientRequestAssert
    {
        private readonly Action<HttpRequestMessage>? _assert;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpClientRequestAssert"/> class.
        /// </summary>
        /// <param name="request">The <see cref="MockHttpClientRequest"/>.</param>
        /// <param name="assert">The optional runtime action to verify the <see cref="HttpRequestMessage"/>.</param>
        internal protected MockHttpClientRequestAssert(MockHttpClientRequest request, Action<HttpRequestMessage>? assert)
        {
            Request = request;
            _assert = assert;
        }

        /// <summary>
        /// Gets the <see cref="MockHttpClientRequest"/>.
        /// </summary>
        internal protected MockHttpClientRequest Request { get; }

        /// <summary>
        /// Gets the <see cref="MockHttpClientResponse"/>.
        /// </summary>
        public MockHttpClientResponse Respond => Request.Respond;

        /// <inheritdoc/>
        void IMockHttpClientRequestAssert.Assert(HttpRequestMessage request)
        {
            OnAssert(request);
            _assert?.Invoke(request);
        }

        /// <summary>
        /// Executed to perform the assertion.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        protected virtual void OnAssert(HttpRequestMessage request) { }
    }

    /// <summary>
    /// Provides the request with body of type <typeparamref name="TBody"/> assertion.
    /// </summary>
    /// <typeparam name="TBody">The request body <see cref="Type"/>.</typeparam>
    public class MockHttpClientRequestAssert<TBody> : MockHttpClientRequestAssert
    {
        private readonly ComparisonConfig _comparisonConfig = TestSetUp.GetDefaultComparisonConfig();
        private bool _isExpectNullValue;
        private Func<TBody>? _expectValueFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpClientRequestAssert{TBody}"/> class.
        /// </summary>
        /// <param name="request">The <see cref="MockHttpClientRequest"/>.</param>
        /// <param name="assert">The optional runtime action to verify the <see cref="HttpRequestMessage"/>.</param>
        internal MockHttpClientRequestAssert(MockHttpClientRequest request, Action<HttpRequestMessage>? assert) : base(request, assert) { }

        /// <summary>
        /// Expect <c>null</c> request body value.
        /// </summary>
        /// <returns>The <see cref="MockHttpClientRequestAssert{TBody}"/> instance to support fluent/chaining usage.</returns>
        public MockHttpClientRequestAssert<TBody> ExpectNullValue()
        {
            _isExpectNullValue = true;
            return this;
        }

        /// <summary>
        /// Expect a request body comparing the specified <paramref name="valueFunc"/> (and optionally any additional <paramref name="membersToIgnore"/> from the comparison).
        /// </summary>
        /// <param name="valueFunc">The function to generate the request body to compare.</param>
        /// <param name="membersToIgnore"></param>
        /// <returns>The <see cref="MockHttpClientRequestAssert{TBody}"/> instance to support fluent/chaining usage.</returns>
        public MockHttpClientRequestAssert<TBody> ExpectValue(Func<TBody> valueFunc, params string[] membersToIgnore)
        {
            _expectValueFunc = Check.NotNull(valueFunc, nameof(valueFunc));
            _comparisonConfig.MembersToIgnore.AddRange(membersToIgnore);
            return this;
        }

        /// <summary>
        /// Expect a request body comparing the deserialized JSON value within the named embedded resource (and optionally any additional <paramref name="membersToIgnore"/> from the comparison).
        /// </summary>
        /// <param name="resourceName">The embedded resource name (matches to the end of the fully qualifed resource name) within the <see cref="Assembly.GetCallingAssembly()"/>.</param>
        /// <param name="membersToIgnore">The members to ignore from the comparison.</param>
        /// <returns>The <see cref="MockHttpClientRequestAssert{TBody}"/> instance to support fluent/chaining usage.</returns>
        public MockHttpClientRequestAssert<TBody> ExpectJsonResourceValue(string resourceName, params string[] membersToIgnore)
        {
            var ass = Assembly.GetCallingAssembly();
            return ExpectValue(() => TestSetUp.GetValueFromJsonResource<TBody>(resourceName, ass), membersToIgnore);
        }

        /// <inheritdoc/>
        protected override void OnAssert(HttpRequestMessage request)
        {
            string? json = null;
            if (request.Content != null)
                json = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (_isExpectNullValue && json != null)
                Assert.Fail($"Expected null HTTP Request body; the following content was returned: '{json}'.");

            if (_expectValueFunc != null)
            {
                var exp = _expectValueFunc();
                if (exp == null)
                    throw new InvalidOperationException("ExpectValue function must not return null.");

                // Get the request value.
                TBody body = default;
                if (json != null)
                {
                    // Parse out the content.
                    try
                    {
                        body = JsonConvert.DeserializeObject<TBody>(json);
                    }
                    catch (Exception) { }
                }

                // Further configure the comparison configuration.
                _comparisonConfig.AttributesToIgnore.AddRange(new Type[] { typeof(ReferenceDataInterfaceAttribute) });
                TestSetUp.InferAdditionalMembersToIgnore(_comparisonConfig, typeof(TBody));

                // Perform the actual comparison.
                var cl = new CompareLogic(_comparisonConfig);
                var cr = cl.Compare(exp, body);
                if (!cr.AreEqual)
                    Assert.Fail($"Expected vs Actual HTTP Request body mismatch: {cr.DifferencesString}");
            }
        }
    }

    /// <summary>
    /// Provides the <see cref="HttpResponseMessage"/> configuration for mocking.
    /// </summary>
    public class MockHttpClientResponse
    {
        private readonly MockHttpClientRequest _clientRequest;

        internal MockHttpClientResponse(MockHttpClientRequest clientRequest) => _clientRequest = clientRequest;

        /// <summary>
        /// Provides the mocked response.
        /// </summary>
        /// <param name="content">The optional <see cref="HttpContent"/>.</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient With(HttpContent? content = null, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null) => _clientRequest.MockResponse(content, statusCode, response);

        /// <summary>
        /// Provides the mocked response for the specified <paramref name="statusCode"/> (no content).
        /// </summary>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient With(HttpStatusCode statusCode, Action<HttpResponseMessage>? response = null) => With((HttpContent?)null, statusCode, response);

        /// <summary>
        /// Provides the mocked response using the <see cref="string"/> content.
        /// </summary>
        /// <param name="content">The text content.</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient With(string content, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null) => With(new StringContent(content ?? throw new ArgumentNullException(nameof(content))), statusCode, response);

        /// <summary>
        /// Provides the mocked response using the <paramref name="value"/> which will be automatically converted to JSON content.
        /// </summary>
        /// <param name="value">The value to convert to <see cref="MediaTypeNames.Application.Json"/> content.</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient With(object? value, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null) => WithJson(JsonConvert.SerializeObject(value), statusCode, response);

        /// <summary>
        /// Provides the mocked response using the <paramref name="json"/> formatted string as the content.
        /// </summary>
        /// <param name="json">The <see cref="MediaTypeNames.Application.Json"/> content.</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient WithJson(string json, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null)
        {
            var content = new StringContent(json ?? throw new ArgumentNullException(nameof(json)));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
            return With(content, statusCode, response);
        }

        /// <summary>
        /// Provides the mocked response using the JSON formatted embedded reosource string as the content.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> used to infer <see cref="Assembly"/> that contains the embedded resource.</typeparam>
        /// <param name="resourceName">The embedded resource name (matches to the end of the fully qualifed resource name).</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient WithJsonResource<T>(string resourceName, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null)
            => WithJsonResource(resourceName, statusCode, response, typeof(T).Assembly);

        /// <summary>
        /// Provides the mocked response using the JSON formatted embedded reosource string as the content.
        /// </summary>
        /// <param name="resourceName">The embedded resource name (matches to the end of the fully qualifed resource name).</param>
        /// <param name="statusCode">The optional <see cref="HttpStatusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).</param>
        /// <param name="response">The optional action to enable additional configuration of the <see cref="HttpResponseMessage"/>.</param>
        /// <param name="assembly">The <see cref="Assembly"/> that contains the embedded resource; defaults to <see cref="Assembly.GetCallingAssembly"/>.</param>
        /// <returns>The <see cref="MockHttpClient"/> to enable a further request to be specified for the client.</returns>
        public MockHttpClient WithJsonResource(string resourceName, HttpStatusCode? statusCode = null, Action<HttpResponseMessage>? response = null, Assembly? assembly = null)
        {
            using var sr = TestSetUp.GetResourceStream(resourceName, assembly ?? Assembly.GetCallingAssembly());
            return WithJson(sr.ReadToEnd(), statusCode, response);
        }
    }
}