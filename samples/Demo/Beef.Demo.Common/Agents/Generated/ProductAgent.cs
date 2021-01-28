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
    /// Defines the <see cref="Product"/> Web API agent.
    /// </summary>
    public partial interface IProductAgent
    {
        /// <summary>
        /// Gets the specified <see cref="Product"/>.
        /// </summary>
        /// <param name="id">The <see cref="Product"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Product?>> GetAsync(int id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="ProductCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.ProductArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<ProductCollectionResult>> GetByArgsAsync(ProductArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the <see cref="Product"/> Web API agent.
    /// </summary>
    public partial class ProductAgent : WebApiAgentBase, IProductAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IDemoWebApiAgentArgs"/>.</param>
        public ProductAgent(IDemoWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Gets the specified <see cref="Product"/>.
        /// </summary>
        /// <param name="id">The <see cref="Product"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Product?>> GetAsync(int id, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Product?>("api/v1/products/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<int>("id", id) });

        /// <summary>
        /// Gets the <see cref="ProductCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.ProductArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<ProductCollectionResult>> GetByArgsAsync(ProductArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<ProductCollectionResult, ProductCollection, Product>("api/v1/products", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<ProductArgs?>("args", args, WebApiArgType.FromUriUseProperties), new WebApiPagingArgsArg("paging", paging) });
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore