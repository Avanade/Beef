/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business;

/// <summary>
/// Defines the <see cref="PerformanceReview"/> business functionality.
/// </summary>
public partial interface IPerformanceReviewManager
{
    /// <summary>
    /// Gets the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    /// <returns>The selected <see cref="PerformanceReview"/> where found.</returns>
    Task<Result<PerformanceReview?>> GetAsync(Guid id);

    /// <summary>
    /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
    /// </summary>
    /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    /// <returns>The <see cref="PerformanceReviewCollectionResult"/>.</returns>
    Task<Result<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging);

    /// <summary>
    /// Creates a new <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="value">The <see cref="PerformanceReview"/>.</param>
    /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
    /// <returns>The created <see cref="PerformanceReview"/>.</returns>
    Task<Result<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId);

    /// <summary>
    /// Updates an existing <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="value">The <see cref="PerformanceReview"/>.</param>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    /// <returns>The updated <see cref="PerformanceReview"/>.</returns>
    Task<Result<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id);

    /// <summary>
    /// Deletes the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    Task<Result> DeleteAsync(Guid id);
}

#pragma warning restore
#nullable restore