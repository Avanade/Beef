/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Business
{
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

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public Task<Result<Employee?>> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go().Requires(id)
                         .ThenAsAsync(() => _dataService.GetAsync(id));
        }, InvokerArgs.Read);

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> CreateAsync(Employee value) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go(value).Required()
                         .ValidateAsync(v => v.Entity().With<EmployeeValidator>())
                         .ThenAsAsync(v => _dataService.CreateAsync(value));
        }, InvokerArgs.Create);

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> UpdateAsync(Employee value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go(value).Required().Requires(id).Then(v => v.Id = id)
                         .ValidateAsync(v => v.Entity().With<EmployeeValidator>())
                         .ThenAsAsync(v => _dataService.UpdateAsync(value));
        }, InvokerArgs.Update);

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task<Result> DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go().Requires(id)
                         .ValidatesAsync(id, v => v.Common(EmployeeValidator.CanDelete))
                         .ThenAsync(() => _dataService.DeleteAsync(id));
        }, InvokerArgs.Delete);

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.EmployeeArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EmployeeBaseCollectionResult"/>.</returns>
        public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go()
                         .ValidatesAsync(args, v => v.Entity().With<EmployeeArgsValidator>())
                         .ThenAsAsync(() => _dataService.GetByArgsAsync(args, paging));
        }, InvokerArgs.Read);

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go(value).Required()
                         .ValidateAsync(v => v.Entity().With<TerminationDetailValidator>())
                         .ThenAsAsync(v => _dataService.TerminateAsync(value, id));
        }, InvokerArgs.Update);
    }
}

#pragma warning restore
#nullable restore