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
    /// Defines the <see cref="Contact"/> Web API agent.
    /// </summary>
    public partial interface IContactAgent
    {
        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<ContactCollectionResult>> GetAllAsync(WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Contact?>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Contact>> CreateAsync(Contact value, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Contact>> UpdateAsync(Contact value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> DeleteAsync(Guid id, WebApiRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the <see cref="Contact"/> Web API agent.
    /// </summary>
    public partial class ContactAgent : WebApiAgentBase, IContactAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IDemoWebApiAgentArgs"/>.</param>
        public ContactAgent(IDemoWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<ContactCollectionResult>> GetAllAsync(WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<ContactCollectionResult, ContactCollection, Contact>("api/v1/contacts", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Contact?>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Contact?>("api/v1/contacts/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Contact>> CreateAsync(Contact value, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<Contact>("api/v1/contacts", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Contact>> UpdateAsync(Contact value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PutAsync<Contact>("api/v1/contacts/{id}", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> DeleteAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            DeleteAsync("api/v1/contacts/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });
    }
}

#pragma warning restore IDE0005
#nullable restore