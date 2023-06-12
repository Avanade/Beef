/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business.DataSvc
{
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

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public Task<Result<Employee?>> GetAsync(Guid id) => Result.Go().CacheGetOrAddAsync(_cache, id, () => _data.GetAsync(id));

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> CreateAsync(Employee value)
        {
            return Result.GoAsync(_data.CreateAsync(value ?? throw new ArgumentNullException(nameof(value))))
                         .Then(r => _cache.SetValue(r));
        }

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> UpdateAsync(Employee value)
        {
            return Result.GoAsync(_data.UpdateAsync(value ?? throw new ArgumentNullException(nameof(value))))
                         .Then(r => _cache.SetValue(r));
        }

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task<Result> DeleteAsync(Guid id)
        {
            return Result.Go(_cache.Remove<Employee>(id))
                         .ThenAsAsync(_ => _data.DeleteAsync(id));
        }

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.EmployeeArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EmployeeBaseCollectionResult"/>.</returns>
        public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging) => _data.GetByArgsAsync(args, paging);

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id)
        {
            return Result.GoAsync(_data.TerminateAsync(value ?? throw new ArgumentNullException(nameof(value)), id))
                         .Then(r => _cache.SetValue(r));
        }
    }
}

#pragma warning restore
#nullable restore