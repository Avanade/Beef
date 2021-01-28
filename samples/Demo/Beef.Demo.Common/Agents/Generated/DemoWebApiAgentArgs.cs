/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using Beef.WebApi;
using System;
using System.Net.Http;

namespace Beef.Demo.Common.Agents
{
    /// <summary>
    /// Defines an application-based (domain) <see cref="IWebApiAgentArgs"/>.
    /// </summary>
    public interface IDemoWebApiAgentArgs : IWebApiAgentArgs { }

    /// <summary>
    /// Provides an application-based (domain) <see cref="IDemoWebApiAgentArgs"/> (see <see cref="IWebApiAgentArgs"/>).
    /// </summary>
    public class DemoWebApiAgentArgs : WebApiAgentArgs, IDemoWebApiAgentArgs 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoWebApiAgentArgs"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The optional <see cref="WebApiAgentArgs.BeforeRequest"/> action.</param>
        public DemoWebApiAgentArgs(HttpClient httpClient, Action<HttpRequestMessage>? beforeRequest = null) : base(httpClient, beforeRequest) { }
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore