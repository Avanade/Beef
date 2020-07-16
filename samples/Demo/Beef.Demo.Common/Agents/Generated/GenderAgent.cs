/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

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
    /// Defines the Gender Web API agent.
    /// </summary>
    public partial interface IGenderAgent
    {
        /// <summary>
        /// Gets the <see cref="Gender"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Gender>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Creates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Gender>> CreateAsync(Gender value, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Gender>> UpdateAsync(Gender value, Guid id, WebApiRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the Gender Web API agent.
    /// </summary>
    public partial class GenderAgent : WebApiAgentBase, IGenderAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenderAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IWebApiAgentArgs"/>.</param>
        public GenderAgent(IWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Gets the <see cref="Gender"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Gender>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null)
        {
            return GetAsync<Gender>("api/v1/demo/ref/genders/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });
        }

        /// <summary>
        /// Creates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Gender>> CreateAsync(Gender value, WebApiRequestOptions? requestOptions = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PostAsync<Gender>("api/v1/demo/ref/genders", value, requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());
        }

        /// <summary>
        /// Updates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Gender>> UpdateAsync(Gender value, Guid id, WebApiRequestOptions? requestOptions = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PutAsync<Gender>("api/v1/demo/ref/genders/{id}", value, requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });
        }
    }
}

#pragma warning restore IDE0005
#nullable restore