/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Beef.Entities;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Agents
{
    /// <summary>
    /// Defines the <b>Config</b> Web API agent.
    /// </summary>
    public partial interface IConfigAgent
    {
        /// <summary>
        /// Get Env Vars.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<System.Collections.IDictionary>> GetEnvVarsAsync(WebApiRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the <b>Config</b> Web API agent.
    /// </summary>
    public partial class ConfigAgent : WebApiAgentBase, IConfigAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IDemoWebApiAgentArgs"/>.</param>
        public ConfigAgent(IDemoWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Get Env Vars.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<System.Collections.IDictionary>> GetEnvVarsAsync(WebApiRequestOptions? requestOptions = null) =>
            PostAsync<System.Collections.IDictionary>("api/v1/envvars", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore