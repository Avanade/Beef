/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Beef;
using Beef.Entities;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using Beef.Demo.Common.Entities;
using Beef.Demo.Common.Agents.ServiceAgents;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Agents
{
    /// <summary>
    /// Provides the Product Web API agent.
    /// </summary>
    public partial class ProductAgent : WebApiAgentBase, IProductServiceAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAgent"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> (where overridding the default value).</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public ProductAgent(HttpClient? httpClient = null, Action<HttpRequestMessage>? beforeRequest = null)
        {
            ProductServiceAgent = Beef.Factory.Create<IProductServiceAgent>(httpClient, beforeRequest);
        }
        
        /// <summary>
        /// Gets the underlyng <see cref="IProductServiceAgent"/> instance.
        /// </summary>
        public IProductServiceAgent ProductServiceAgent { get; private set; }

        /// <summary>
        /// Gets the <see cref="Product"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Product"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Product>> GetAsync(int id, WebApiRequestOptions? requestOptions = null)
            => ProductServiceAgent.GetAsync(id, requestOptions);

        /// <summary>
        /// Gets the <see cref="Product"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="ProductArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<ProductCollectionResult>> GetByArgsAsync(ProductArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null)
            => ProductServiceAgent.GetByArgsAsync(args, paging, requestOptions);
    }
}

#pragma warning restore IDE0005
#nullable restore