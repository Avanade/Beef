/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Business
{
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
            { _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService)); PerformanceReviewManagerCtor(); }

        partial void PerformanceReviewManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="PerformanceReview"/> where found.</returns>
        public Task<PerformanceReview?> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            await id.Validate(nameof(id)).Mandatory().ValidateAsync(true).ConfigureAwait(false);
            return await _dataService.GetAsync(id).ConfigureAwait(false);
        }, InvokerArgs.Read);

        /// <summary>
        /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PerformanceReviewCollectionResult"/>.</returns>
        public Task<PerformanceReviewCollectionResult> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            return await _dataService.GetByEmployeeIdAsync(employeeId, paging).ConfigureAwait(false);
        }, InvokerArgs.Read);

        /// <summary>
        /// Creates a new <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <returns>The created <see cref="PerformanceReview"/>.</returns>
        public Task<PerformanceReview> CreateAsync(PerformanceReview value, Guid employeeId) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            value.Required().EmployeeId = employeeId;
            await value.Validate().Entity().With<PerformanceReviewValidator>().ValidateAsync(true).ConfigureAwait(false);
            return await _dataService.CreateAsync(value).ConfigureAwait(false);
        }, InvokerArgs.Create);

        /// <summary>
        /// Updates an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="PerformanceReview"/>.</returns>
        public Task<PerformanceReview> UpdateAsync(PerformanceReview value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            value.Required().Id = id;
            await value.Validate().Entity().With<PerformanceReviewValidator>().ValidateAsync(true).ConfigureAwait(false);
            return await _dataService.UpdateAsync(value).ConfigureAwait(false);
        }, InvokerArgs.Update);

        /// <summary>
        /// Deletes the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        public Task DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            await id.Validate(nameof(id)).Mandatory().ValidateAsync(true).ConfigureAwait(false);
            await _dataService.DeleteAsync(id).ConfigureAwait(false);
        }, InvokerArgs.Delete);
    }
}

#pragma warning restore
#nullable restore