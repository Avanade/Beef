/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.DataSvc;

/// <summary>
/// Provides the <see cref="Robot"/> data repository services.
/// </summary>
public partial class RobotDataSvc : IRobotDataSvc
{
    private readonly IRobotData _data;
    private readonly IEventPublisher _events;
    private readonly IRequestCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotDataSvc"/> class.
    /// </summary>
    /// <param name="data">The <see cref="IRobotData"/>.</param>
    /// <param name="events">The <see cref="IEventPublisher"/>.</param>
    /// <param name="cache">The <see cref="IRequestCache"/>.</param>
    public RobotDataSvc(IRobotData data, IEventPublisher events, IRequestCache cache)
        { _data = data.ThrowIfNull(); _events = events.ThrowIfNull(); _cache = cache.ThrowIfNull(); RobotDataSvcCtor(); }

    partial void RobotDataSvcCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Robot?>> GetAsync(Guid id) => Result.Go().CacheGetOrAddAsync(_cache, id, () => _data.GetAsync(id));

    /// <inheritdoc/>
    public Task<Result<Robot>> CreateAsync(Robot value) => DataSvcInvoker.Current.InvokeAsync(this, (_, __) =>
    {
        return Result.GoAsync(_data.CreateAsync(value))
                     .Then(r => _events.PublishValueEvent(r, new Uri($"/robots/{r.Id}", UriKind.Relative), $"Demo.Robot", "Create"))
                     .CacheSet(_cache);
    }, new InvokerArgs { EventPublisher = _events });

    /// <inheritdoc/>
    public Task<Result<Robot>> UpdateAsync(Robot value) => DataSvcInvoker.Current.InvokeAsync(this, (_, __) =>
    {
        return Result.GoAsync(_data.UpdateAsync(value))
                     .Then(r => _events.PublishValueEvent(r, new Uri($"/robots/{r.Id}", UriKind.Relative), $"Demo.Robot", "Update"))
                     .CacheSet(_cache);
    }, new InvokerArgs { EventPublisher = _events });

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id) => DataSvcInvoker.Current.InvokeAsync(this, (_, __) =>
    {
        return Result.Go().CacheRemove<Robot>(_cache, id)
                     .ThenAsync(() => _data.DeleteAsync(id))
                     .Then(() => _events.PublishValueEvent(new Robot { Id = id }, new Uri($"/robots/{id}", UriKind.Relative), $"Demo.Robot", "Delete"));
    }, new InvokerArgs { EventPublisher = _events });

    /// <inheritdoc/>
    public Task<Result<RobotCollectionResult>> GetByArgsAsync(RobotArgs? args, PagingArgs? paging) => _data.GetByArgsAsync(args, paging);
}

#pragma warning restore
#nullable restore