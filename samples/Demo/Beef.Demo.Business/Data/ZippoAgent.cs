using Beef.WebApi;
using System;
using System.Net.Http;
using System.Threading.Tasks;

#nullable enable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Defines the <b>zippopotam</b> <see cref="IHttpAgentArgs"/>.
    /// </summary>
    public interface IZippoAgentArgs : IHttpAgentArgs { }

    /// <summary>
    /// Provides the <b>zippopotam</b> <see cref="IHttpAgentArgs"/>.
    /// </summary>
    public class ZippoAgentArgs : HttpAgentArgs, IZippoAgentArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZippoAgentArgs"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The optional <see cref="WebApiAgentArgs.BeforeRequest"/> action.</param>
        /// <param name="beforeRequestAsync">The optional <see cref="WebApiAgentArgs.BeforeRequestAsync"/> asynchronous function.</param>
        /// <param name="afterResponse">The optional <see cref="AfterResponse"/> action.</param>
        /// <param name="afterResponseAsync">The optional <see cref="AfterResponseAsync"/> asynchronous function.</param>
        public ZippoAgentArgs(HttpClient httpClient, Action<HttpRequestMessage>? beforeRequest = null, Func<HttpRequestMessage, Task>? beforeRequestAsync = null, Action<IHttpAgentResult>? afterResponse = null, Func<IHttpAgentResult, Task>? afterResponseAsync = null)
            : base(httpClient, beforeRequest, beforeRequestAsync, afterResponse, afterResponseAsync) { }
    }

    /// <summary>
    /// Defines the <b>zippopotam</b> <see cref="IHttpAgentArgs"/>.
    /// </summary>
    public interface IZippoAgent : IHttpAgent { }

    /// <summary>
    /// Provides the <b>zippopotam</b> <see cref="HttpAgentBase"/>.
    /// </summary>
    public class ZippoAgent : HttpAgentBase, IZippoAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZippoAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IZippoAgentArgs"/>.</param>
        public ZippoAgent(IZippoAgentArgs args) : base(args) { }
    }
}

#nullable restore