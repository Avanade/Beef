/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace MyEf.Hr.Business;

/// <summary>
/// Provides the <see cref="Employee"/> business functionality.
/// </summary>
public partial class EmployeeManager : IEmployeeManager
{
    private readonly IEmployeeDataSvc _dataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeManager"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IEmployeeDataSvc"/>.</param>
    public EmployeeManager(IEmployeeDataSvc dataService)
        { _dataService = dataService.ThrowIfNull(); EmployeeManagerCtor(); }

    partial void EmployeeManagerCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Employee?>> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go().Requires(id)
                     .ThenAsAsync(() => _dataService.GetAsync(id));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Employee>> CreateAsync(Employee value) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go(value).Required()
                     .ValidateAsync(vc => vc.Entity().With<EmployeeValidator>(), cancellationToken: ct)
                     .ThenAsAsync(v => _dataService.CreateAsync(v));
    }, InvokerArgs.Create);

    /// <inheritdoc/>
    public Task<Result<Employee>> UpdateAsync(Employee value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go(value).Required().Requires(id).Adjusts(v => v.Id = id)
                     .ValidateAsync(vc => vc.Entity().With<EmployeeValidator>(), cancellationToken: ct)
                     .ThenAsAsync(v => _dataService.UpdateAsync(v));
    }, InvokerArgs.Update);

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go().Requires(id)
                     .ValidatesAsync(id, vc => vc.Common(EmployeeValidator.CanDelete), cancellationToken: ct)
                     .ThenAsync(() => _dataService.DeleteAsync(id));
    }, InvokerArgs.Delete);

    /// <inheritdoc/>
    public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go()
                     .ValidatesAsync(args, vc => vc.Entity().With<EmployeeArgsValidator>(), cancellationToken: ct)
                     .ThenAsAsync(() => _dataService.GetByArgsAsync(args, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<EmployeeBaseCollectionResult>> GetByQueryAsync(QueryArgs? query, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go()
                     .ThenAsAsync(() => _dataService.GetByQueryAsync(query, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go(value).Required().Requires(id)
                     .ValidateAsync(vc => vc.Entity().With<TerminationDetailValidator>(), cancellationToken: ct)
                     .ThenAsAsync(v => _dataService.TerminateAsync(v, id));
    }, InvokerArgs.Update);
}