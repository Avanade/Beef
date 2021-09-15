// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents the <b>HTTP</b> agent arguments.
    /// </summary>
    public class HttpAgentArgs : WebApiAgentArgs, IHttpAgentArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentArgs"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The optional <see cref="WebApiAgentArgs.BeforeRequest"/> action.</param>
        /// <param name="beforeRequestAsync">The optional <see cref="WebApiAgentArgs.BeforeRequestAsync"/> asynchronous function.</param>
        /// <param name="afterResponse">The optional <see cref="AfterResponse"/> action.</param>
        /// <param name="afterResponseAsync">The optional <see cref="AfterResponseAsync"/> asynchronous function.</param>
        public HttpAgentArgs(HttpClient httpClient, Action<HttpRequestMessage>? beforeRequest = null, Func<HttpRequestMessage, Task>? beforeRequestAsync = null, Action<IHttpAgentResult>? afterResponse = null, Func<IHttpAgentResult, Task>? afterResponseAsync = null)
            : base(httpClient, beforeRequest, beforeRequestAsync)
        {
            AfterResponse = afterResponse;
            AfterResponseAsync = afterResponseAsync;
        }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked.
        /// </summary>
        public Action<IHttpAgentResult>? AfterResponse { get; }

        /// <summary>
        /// Gets the <see cref="Func{HttpRequestMessage, Task}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked (asynchronously).
        /// </summary>
        public Func<IHttpAgentResult, Task>? AfterResponseAsync { get; }
    }
}