// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents the <b>Web API</b> agent arguments.
    /// </summary>
    public class WebApiAgentArgs : IWebApiAgentArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentArgs"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The optional <see cref="BeforeRequest"/> action.</param>
        /// <param name="beforeRequestAsync">The optional <see cref="BeforeRequestAsync"/> asynchronous function.</param>
        public WebApiAgentArgs(HttpClient httpClient, Action<HttpRequestMessage>? beforeRequest = null, Func<HttpRequestMessage, Task>? beforeRequestAsync = null)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            BeforeRequest = beforeRequest;
            BeforeRequestAsync = beforeRequestAsync;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/>.
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked.
        /// </summary>
        public Action<HttpRequestMessage>? BeforeRequest { get; }

        /// <summary>
        /// Gets the <see cref="Func{HttpRequestMessage, Task}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked (asynchronously).
        /// </summary>
        public Func<HttpRequestMessage, Task>? BeforeRequestAsync { get; }
    }
}