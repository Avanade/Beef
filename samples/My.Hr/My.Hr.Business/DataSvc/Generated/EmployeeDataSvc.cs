/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace My.Hr.Business.DataSvc;

/// <summary>
/// Provides the <see cref="Employee"/> data repository services.
/// </summary>
public partial class EmployeeDataSvc : IEmployeeDataSvc
{
    private readonly IEmployeeData _data;
    private readonly IRequestCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeDataSvc"/> class.
    /// </summary>
    /// <param name="data">The <see cref="IEmployeeData"/>.</param>
    /// <param name="cache">The <see cref="IRequestCache"/>.</param>
    public EmployeeDataSvc(IEmployeeData data, IRequestCache cache)
        { _data = data.ThrowIfNull(); _cache = cache.ThrowIfNull(); EmployeeDataSvcCtor(); }

    partial void EmployeeDataSvcCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Employee?>> GetAsync(Guid id) => Result.Go().CacheGetOrAddAsync(_cache, id, () => _data.GetAsync(id));

    /// <inheritdoc/>
    public Task<Result<Employee>> CreateAsync(Employee value)
    {
        return Result.GoAsync(_data.CreateAsync(value))
                     .CacheSet(_cache);
    }

    /// <inheritdoc/>
    public Task<Result<Employee>> UpdateAsync(Employee value)
    {
        return Result.GoAsync(_data.UpdateAsync(value))
                     .CacheSet(_cache);
    }

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id)
    {
        return Result.Go().CacheRemove<Employee>(_cache, id)
                     .ThenAsync(() => _data.DeleteAsync(id));
    }

    /// <inheritdoc/>
    public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging) => _data.GetByArgsAsync(args, paging);

    /// <inheritdoc/>
    public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id)
    {
        return Result.GoAsync(_data.TerminateAsync(value, id))
                     .CacheSet(_cache);
    }
}