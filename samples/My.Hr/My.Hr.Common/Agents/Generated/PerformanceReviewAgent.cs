/*
 * This file is automatically generated; any changes will be lost.
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using My.Hr.Common.Entities;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Common.Agents
{
    /// <summary>
    /// Provides the <see cref="PerformanceReview"/> HTTP agent.
    /// </summary>
    public partial class PerformanceReviewAgent : TypedHttpClientBase<PerformanceReviewAgent>, IPerformanceReviewAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewAgent"/> class.
        /// </summary>
        /// <param name="client">The underlying <see cref="HttpClient"/>.</param>
        /// <param name="jsonSerializer">The optional <see cref="IJsonSerializer"/>.</param>
        /// <param name="executionContext">The optional <see cref="CoreEx.ExecutionContext"/>.</param>
        public PerformanceReviewAgent(HttpClient client, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null) : base(client, jsonSerializer, executionContext) { }

        /// <inheritdoc/>
        public Task<HttpResult<PerformanceReview?>> GetAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PerformanceReview?>("reviews/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PerformanceReviewCollectionResult>("employees/{employeeId}/reviews", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<Guid>("employeeId", employeeId)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<PerformanceReview, PerformanceReview>("employees/{employeeId}/reviews", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("employeeId", employeeId)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<PerformanceReview, PerformanceReview>("reviews/{id}", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PerformanceReview>> PatchAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PatchAsync<PerformanceReview>("reviews/{id}", patchOption, value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> DeleteAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => DeleteAsync("reviews/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);
    }
}