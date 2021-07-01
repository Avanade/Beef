// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the <b>Web API</b> agent arguments.
    /// </summary>
    public interface IWebApiAgentArgs
    {
        /// <summary>
        /// Gets the <see cref="HttpClient"/>.
        /// </summary>
        HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked.
        /// </summary>
        Action<HttpRequestMessage>? BeforeRequest { get; }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked (asynchronously).
        /// </summary>
        Func<HttpRequestMessage, Task>? BeforeRequestAsync { get; }
    }
}