/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business;

/// <summary>
/// Provides the <see cref="PerformanceReview"/> business functionality.
/// </summary>
public partial class PerformanceReviewManager : IPerformanceReviewManager
{
    private readonly IPerformanceReviewDataSvc _dataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewManager"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IPerformanceReviewDataSvc"/>.</param>
    public PerformanceReviewManager(IPerformanceReviewDataSvc dataService)
        { _dataService = dataService.ThrowIfNull(); PerformanceReviewManagerCtor(); }

    partial void PerformanceReviewManagerCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<PerformanceReview?>> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go().Requires(id)
                     .ThenAsAsync(() => _dataService.GetAsync(id));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go()
                     .ThenAsAsync(() => _dataService.GetByEmployeeIdAsync(employeeId, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<PerformanceReview>> CreateAsync(PerformanceReview value, Guid employeeId) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go(value).Required().Then(v => v.EmployeeId = employeeId)
                     .ValidateAsync(v => v.Entity().With<PerformanceReviewValidator>())
                     .ThenAsAsync(v => _dataService.CreateAsync(value));
    }, InvokerArgs.Create);

    /// <inheritdoc/>
    public Task<Result<PerformanceReview>> UpdateAsync(PerformanceReview value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go(value).Required().Requires(id).Then(v => v.Id = id)
                     .ValidateAsync(v => v.Entity().With<PerformanceReviewValidator>())
                     .ThenAsAsync(v => _dataService.UpdateAsync(value));
    }, InvokerArgs.Update);

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go().Requires(id)
                     .ThenAsync(() => _dataService.DeleteAsync(id));
    }, InvokerArgs.Delete);
}

#pragma warning restore
#nullable restore