/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreEx.Configuration;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using My.Hr.Common.Entities;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Common.Agents
{
    /// <summary>
    /// Defines the <see cref="PerformanceReview"/> HTTP agent.
    /// </summary>
    public partial interface IPerformanceReviewAgent
    {
        /// <summary>
        /// Gets the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<PerformanceReview?>> GetAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Patches an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="HttpPatchOption"/>.</param>
        /// <param name="value">The <see cref="string"/> that contains the patch content for the <see cref="PerformanceReview"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult<PerformanceReview>> PatchAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        Task<HttpResult> DeleteAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides the <see cref="PerformanceReview"/> HTTP agent.
    /// </summary>
    public partial class PerformanceReviewAgent : TypedHttpClientBase<PerformanceReviewAgent>, IPerformanceReviewAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewAgent"/> class.
        /// </summary>
        /// <param name="client">The underlying <see cref="HttpClient"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="executionContext">The <see cref="CoreEx.ExecutionContext"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public PerformanceReviewAgent(HttpClient client, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext, SettingsBase settings, ILogger<PerformanceReviewAgent> logger) 
            : base(client, jsonSerializer, executionContext, settings, logger) { }

        /// <summary>
        /// Gets the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult<PerformanceReview?>> GetAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PerformanceReview?>("reviews/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <summary>
        /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PerformanceReviewCollectionResult>("employees/{employeeId}/reviews", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<Guid>("employeeId", employeeId)), cancellationToken: cancellationToken);

        /// <summary>
        /// Creates a new <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<PerformanceReview, PerformanceReview>("employees/{employeeId}/reviews", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("employeeId", employeeId)), cancellationToken: cancellationToken);

        /// <summary>
        /// Updates an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<PerformanceReview, PerformanceReview>("reviews/{id}", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <summary>
        /// Patches an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="HttpPatchOption"/>.</param>
        /// <param name="value">The <see cref="string"/> that contains the patch content for the <see cref="PerformanceReview"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult<PerformanceReview>> PatchAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PatchAsync<PerformanceReview>("reviews/{id}", patchOption, value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <summary>
        /// Deletes the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="HttpResult"/>.</returns>
        public Task<HttpResult> DeleteAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => DeleteAsync("reviews/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);
    }
}

#pragma warning restore
#nullable restore