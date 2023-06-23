/*
 * This file is automatically generated; any changes will be lost. 
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreEx.Configuration;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using CoreEx.RefData;
using Microsoft.Extensions.Logging;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Agents
{
    /// <summary>
    /// Defines the <b>ReferenceData</b> HTTP agent.
    /// </summary>
    public partial interface IReferenceDataAgent
    {
        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.OpenStatus"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.OpenStatusCollection>> OpenStatusGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.ProductCategory"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.ProductCategoryCollection>> ProductCategoryGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.AccountUType"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.AccountUTypeCollection>> AccountUTypeGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.MaturityInstructions"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.MaturityInstructionsCollection>> MaturityInstructionsGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionType"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.TransactionTypeCollection>> TransactionTypeGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all of the <see cref="RefDataNamespace.TransactionStatus"/> items that match the filter arguments.
        /// </summary>
        /// <param name="args">The optional <see cref="ReferenceDataFilter"/> arguments.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<RefDataNamespace.TransactionStatusCollection>> TransactionStatusGetAllAsync(ReferenceDataFilter? args = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the reference data entries for the specified entities and codes from the query string; e.g: api/v1/ref?entity=codeX,codeY&amp;entity2=codeZ&amp;entity3
        /// </summary>
        /// <param name="names">The optional list of reference data names.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        /// <remarks>The reference data objects will need to be manually extracted from the corresponding response content.</remarks>
        Task<HttpResult> GetNamedAsync(string[] names, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
    }
}