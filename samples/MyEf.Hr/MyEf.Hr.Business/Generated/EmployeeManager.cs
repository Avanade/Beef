/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

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
    public Task<Result<Employee?>> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go().Requires(id)
                     .ThenAsAsync(() => _dataService.GetAsync(id));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Employee>> CreateAsync(Employee value) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go(value).Required()
                     .ValidateAsync(v => v.Entity().With<EmployeeValidator>())
                     .ThenAsAsync(v => _dataService.CreateAsync(value));
    }, InvokerArgs.Create);

    /// <inheritdoc/>
    public Task<Result<Employee>> UpdateAsync(Employee value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go(value).Required().Requires(id).Then(v => v.Id = id)
                     .ValidateAsync(v => v.Entity().With<EmployeeValidator>())
                     .ThenAsAsync(v => _dataService.UpdateAsync(value));
    }, InvokerArgs.Update);

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go().Requires(id)
                     .ValidatesAsync(id, v => v.Common(EmployeeValidator.CanDelete))
                     .ThenAsync(() => _dataService.DeleteAsync(id));
    }, InvokerArgs.Delete);

    /// <inheritdoc/>
    public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go()
                     .ValidatesAsync(args, v => v.Entity().With<EmployeeArgsValidator>())
                     .ThenAsAsync(() => _dataService.GetByArgsAsync(args, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
    {
        return Result.Go(value).Required()
                     .ValidateAsync(v => v.Entity().With<TerminationDetailValidator>())
                     .ThenAsAsync(v => _dataService.TerminateAsync(value, id));
    }, InvokerArgs.Update);
}

#pragma warning restore
#nullable restore