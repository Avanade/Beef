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
using Beef.RefData;
using Beef.WebApi;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Agents.ServiceAgents
{
    /// <summary>
    /// Defines the <b>ReferenceData</b> service agent.
    /// </summary>
    public partial interface IReferenceDataServiceAgent
    {
        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.OpenStatus"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.OpenStatusCollection>> OpenStatusGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.ProductCategory"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.ProductCategoryCollection>> ProductCategoryGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.AccountUType"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.AccountUTypeCollection>> AccountUTypeGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.MaturityInstructions"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.MaturityInstructionsCollection>> MaturityInstructionsGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionType"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.TransactionTypeCollection>> TransactionTypeGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionStatus"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<RefDataNamespace.TransactionStatusCollection>> TransactionStatusGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the reference data entries for the specified entities and codes from the query string; e.g: api/v1/ref?entity=codeX,codeY&amp;entity2=codeZ&amp;entity3
        /// </summary>
        /// <param name="names">The optional list of reference data names.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        /// <remarks>The reference data objects will need to be manually extracted from the corresponding response content.</remarks>
        Task<WebApiAgentResult> GetNamedAsync(string[] names, WebApiRequestOptions? requestOptions);
    }

    /// <summary>
    /// Provides the <b>ReferenceData</b> Web API service agent.
    /// </summary>
    public partial class ReferenceDataServiceAgent : WebApiAgentBase<ReferenceDataServiceAgent>, IReferenceDataServiceAgent
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static ReferenceDataServiceAgent()
        {
            Register(() =>
            {
                var rd = WebApiServiceAgentManager.Get<ReferenceDataServiceAgent>();
                return rd == null ? null : new ReferenceDataServiceAgent(rd.Client, rd.BeforeRequest);
            }, false);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataServiceAgent"/> class with a <paramref name="httpClient"/>.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public ReferenceDataServiceAgent(HttpClient? httpClient = null, Action<HttpRequestMessage>? beforeRequest = null) : base(httpClient, beforeRequest) { }

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.OpenStatus"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.OpenStatusCollection>> OpenStatusGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.OpenStatusCollection>("api/v1/ref/openStatuses", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.ProductCategory"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.ProductCategoryCollection>> ProductCategoryGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.ProductCategoryCollection>("api/v1/ref/productCategories", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.AccountUType"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.AccountUTypeCollection>> AccountUTypeGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.AccountUTypeCollection>("api/v1/ref/accountUTypes", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.MaturityInstructions"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.MaturityInstructionsCollection>> MaturityInstructionsGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.MaturityInstructionsCollection>("api/v1/ref/maturityInstructions", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionType"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.TransactionTypeCollection>> TransactionTypeGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.TransactionTypeCollection>("api/v1/ref/transactionTypes", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionStatus"/> objects that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<RefDataNamespace.TransactionStatusCollection>> TransactionStatusGetAllAsync(ReferenceDataFilter? args = null, WebApiRequestOptions? requestOptions = null) =>
            base.GetAsync<RefDataNamespace.TransactionStatusCollection>("api/v1/ref/transactionStatuses", requestOptions: requestOptions, args: new WebApiArg[] { new WebApiArg<ReferenceDataFilter>("args", args!, WebApiArgType.FromUriUseProperties) });      

        /// <summary>
        /// Gets the reference data entries for the specified entities and codes from the query string; e.g: api/v1/ref?entity=codeX,codeY&amp;entity2=codeZ&amp;entity3
        /// </summary>
        /// <param name="names">The list of reference data names.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        /// <remarks>The reference data objects will need to be manually extracted from the corresponding response content.</remarks>
        public Task<WebApiAgentResult> GetNamedAsync(string[] names, WebApiRequestOptions? requestOptions = null)
        {
            var ro = requestOptions ?? new WebApiRequestOptions();
            if (names != null)
                ro.UrlQueryString += string.Join("&", names);
                
            return base.GetAsync("api/v1/ref", requestOptions: ro);
        }
    }
}

#pragma warning restore IDE0005
#nullable restore