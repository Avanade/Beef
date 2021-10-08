// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the <b>HTTP Agent</b> arguments.
    /// </summary>
    public interface IHttpAgentArgs : IWebApiAgentArgs 
    {
        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked.
        /// </summary>
        Action<IHttpAgentResult>? AfterResponse { get; }

        /// <summary>
        /// Gets the <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is invoked (asynchronously).
        /// </summary>
        Func<IHttpAgentResult, Task>? AfterResponseAsync { get; }
    }
}