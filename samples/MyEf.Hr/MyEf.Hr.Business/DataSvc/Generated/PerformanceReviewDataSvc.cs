/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="PerformanceReview"/> data repository services.
    /// </summary>
    public partial class PerformanceReviewDataSvc : IPerformanceReviewDataSvc
    {
        private readonly IPerformanceReviewData _data;
        private readonly IEventPublisher _events;
        private readonly IRequestCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IPerformanceReviewData"/>.</param>
        /// <param name="events">The <see cref="IEventPublisher"/>.</param>
        /// <param name="cache">The <see cref="IRequestCache"/>.</param>
        public PerformanceReviewDataSvc(IPerformanceReviewData data, IEventPublisher events, IRequestCache cache)
            { _data = data.ThrowIfNull(); _events = events.ThrowIfNull(); _cache = cache.ThrowIfNull(); PerformanceReviewDataSvcCtor(); }

        partial void PerformanceReviewDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="PerformanceReview"/> where found.</returns>
        public Task<Result<PerformanceReview?>> GetAsync(Guid id) => Result.Go().CacheGetOrAddAsync(_cache, id, () => _data.GetAsync(id));

        /// <summary>
        /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PerformanceReviewCollectionResult"/>.</returns>
        public Task<Result<PerformanceReviewCollectionResult>> GetByEmployeeIdAsync(Guid employeeId, PagingArgs? paging) => _data.GetByEmployeeIdAsync(employeeId, paging);

        /// <summary>
        /// Creates a new <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <returns>The created <see cref="PerformanceReview"/>.</returns>
        public Task<Result<PerformanceReview>> CreateAsync(PerformanceReview value) => DataSvcInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.GoAsync(_data.CreateAsync(value))
                         .Then(r => _events.PublishValueEvent(r, new Uri($"myef/hr/performancereview/{r.Id}", UriKind.Relative), $"MyEf.Hr.PerformanceReview", "Created"))
                         .Then(r => _cache.SetValue(r));
        }, new InvokerArgs { IncludeTransactionScope = true, EventPublisher = _events });

        /// <summary>
        /// Updates an existing <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="value">The <see cref="PerformanceReview"/>.</param>
        /// <returns>The updated <see cref="PerformanceReview"/>.</returns>
        public Task<Result<PerformanceReview>> UpdateAsync(PerformanceReview value) => DataSvcInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.GoAsync(_data.UpdateAsync(value))
                         .Then(r => _events.PublishValueEvent(r, new Uri($"myef/hr/performancereview/{r.Id}", UriKind.Relative), $"MyEf.Hr.PerformanceReview", "Updated"))
                         .Then(r => _cache.SetValue(r));
        }, new InvokerArgs { IncludeTransactionScope = true, EventPublisher = _events });

        /// <summary>
        /// Deletes the specified <see cref="PerformanceReview"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        public Task<Result> DeleteAsync(Guid id) => DataSvcInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go(_cache.Remove<PerformanceReview>(id))
                         .ThenAsAsync(_ => _data.DeleteAsync(id))
                         .Then(() => _events.PublishValueEvent(new PerformanceReview { Id = id }, new Uri($"myef/hr/performancereview/{id}", UriKind.Relative), $"MyEf.Hr.PerformanceReview", "Deleted"));
        }, new InvokerArgs { IncludeTransactionScope = true, EventPublisher = _events });
    }
}

#pragma warning restore
#nullable restore