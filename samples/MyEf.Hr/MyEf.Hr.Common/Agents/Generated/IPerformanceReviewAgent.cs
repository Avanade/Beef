/*
 * This file is automatically generated; any changes will be lost.
 */

namespace MyEf.Hr.Common.Agents;

/// <summary>
/// Defines the <see cref="PerformanceReview"/> HTTP agent.
/// </summary>
public partial interface IPerformanceReviewAgent
{
    /// <summary>
    /// Gets the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="PerformanceReview"/> identifier.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult<PerformanceReview?>> GetAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="value">The <see cref="PerformanceReview"/>.</param>
    /// <param name="id">The <see cref="PerformanceReview"/> identifier.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an existing <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="patchOption">The <see cref="HttpPatchOption"/>.</param>
    /// <param name="value">The <see cref="string"/> that contains the patch content for the <see cref="PerformanceReview"/>.</param>
    /// <param name="id">The <see cref="PerformanceReview"/> identifier.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult<PerformanceReview>> PatchAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="PerformanceReview"/> identifier.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult> DeleteAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
    /// </summary>
    /// <param name="employeeId">The <see cref="Employee"/> identifier.</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="value">The <see cref="PerformanceReview"/>.</param>
    /// <param name="employeeId">The <see cref="Employee"/> identifier.</param>
    /// <param name="requestOptions">The optional <see cref="HttpRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="HttpResult"/>.</returns>
    Task<HttpResult<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}