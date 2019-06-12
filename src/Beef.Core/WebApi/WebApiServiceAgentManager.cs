// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides centralised management capabilities for the <see cref="WebApiServiceAgentBase"/> classes.
    /// </summary>
    public static class WebApiServiceAgentManager
    {
        private static readonly Dictionary<string, RegisteredData> _dict = new Dictionary<string, RegisteredData>();
        private static readonly object _lock = new object();
        private static Func<RegisteredData, HttpClient> _httpClientCreate;

        /// <summary>
        /// Register a <paramref name="httpClientCreate"/> function to create the <see cref="HttpClient"/> where not previously set.
        /// </summary>
        /// <param name="httpClientCreate">The function to create the <see cref="HttpClient"/>.</param>
        public static void RegisterHttpClientCreate(Func<RegisteredData, HttpClient> httpClientCreate)
        {
            _httpClientCreate = httpClientCreate;
        }

        /// <summary>
        /// Registers the default <paramref name="client"/> and <paramref name="beforeRequest"/> for the specified .NET <paramref name="nameSpace"/>.
        /// </summary>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public static void Register(string nameSpace, HttpClient client, Action<HttpRequestMessage> beforeRequest = null)
        {
            if (string.IsNullOrEmpty(nameSpace))
                throw new ArgumentNullException(nameof(nameSpace));

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            lock (_lock)
            {
                _dict[nameSpace] = new RegisteredData { Client = client, BeforeRequest = beforeRequest };
            }
        }

        /// <summary>
        /// Registers the default <paramref name="baseAddress"/> (will automatically create the <see cref="HttpClient"/> on first use) and <paramref name="beforeRequest"/> for the specified .NET <paramref name="nameSpace"/>.
        /// </summary>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="baseAddress">Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public static void Register(string nameSpace, Uri baseAddress, Action<HttpRequestMessage> beforeRequest = null)
        {
            if (string.IsNullOrEmpty(nameSpace))
                throw new ArgumentNullException(nameof(nameSpace));

            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));

            lock (_lock)
            {
                _dict[nameSpace] = new RegisteredData { BaseAddress = baseAddress, BeforeRequest = beforeRequest };
            }
        }

        /// <summary>
        /// Gets the <see cref="RegisteredData"/> using the namespace for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to derive the namespace for.</typeparam>
        /// <returns>The <see cref="RegisteredData"/> where found; otherwise, <c>null</c>.</returns>
        public static RegisteredData Get<T>()
        {
            return Get(typeof(T).Namespace);
        }

        /// <summary>
        /// Gets the <see cref="RegisteredData"/> for the specified .NET <paramref name="nameSpace"/>.
        /// </summary>
        /// <param name="nameSpace">The namespace.</param>
        /// <returns>The <see cref="RegisteredData"/> where found; otherwise, <c>null</c>.</returns>
        public static RegisteredData Get(string nameSpace)
        {
            if (_dict.TryGetValue(nameSpace, out RegisteredData rd))
                return rd;

            return null;
        }

        /// <summary>
        /// Represents the registered data.
        /// </summary>
        public class RegisteredData
        {
            private static Dictionary<Uri, HttpClient> _clientCache = new Dictionary<Uri, HttpClient>();

            private HttpClient _client;
            private Uri _baseAddress;
            private readonly object _lock = new object();

            /// <summary>
            /// Gets the <see cref="HttpClient"/>.
            /// </summary>
            public HttpClient Client
            {
                get
                {
                    if (_client == null && _httpClientCreate != null)
                        return _httpClientCreate(this);

                    if (_client != null)
                        return _client;

                    lock (_lock)
                    {
                        if (_client == null)
                        {
                            if (!_clientCache.TryGetValue(BaseAddress, out _client))
                            {
                                _client = new HttpClient() { BaseAddress = BaseAddress };
                                _clientCache.Add(BaseAddress, _client);
                            }
                        }

                        return _client;
                    }
                }

                internal set { _client = value ?? throw new ArgumentNullException(nameof(Client)); }
            }

            /// <summary>
            /// Gets the <see cref="BaseAddress"/>.
            /// </summary>
            public Uri BaseAddress
            {
                get { return _baseAddress ?? _client?.BaseAddress; }
                internal set { _baseAddress = value ?? throw new ArgumentNullException(nameof(BaseAddress)); }
            }

            /// <summary>
            /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).
            /// </summary>
            public Action<HttpRequestMessage> BeforeRequest { get; internal set; }
        }
    }
}
