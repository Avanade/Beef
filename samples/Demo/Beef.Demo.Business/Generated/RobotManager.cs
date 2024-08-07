/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business;

/// <summary>
/// Provides the <see cref="Robot"/> business functionality.
/// </summary>
public partial class RobotManager : IRobotManager
{
    private readonly IRobotDataSvc _dataService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IIdentifierGenerator _identifierGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotManager"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IRobotDataSvc"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="identifierGenerator">The <see cref="IIdentifierGenerator"/>.</param>
    public RobotManager(IRobotDataSvc dataService, IEventPublisher eventPublisher, IIdentifierGenerator identifierGenerator)
        { _dataService = dataService.ThrowIfNull(); _eventPublisher = eventPublisher.ThrowIfNull(); _identifierGenerator = identifierGenerator.ThrowIfNull(); RobotManagerCtor(); }

    partial void RobotManagerCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Robot?>> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go().Requires(id)
                     .Then(() => Cleaner.CleanUp(id))
                     .ThenAsAsync(() => _dataService.GetAsync(id));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Robot>> CreateAsync(Robot value) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go(value).Required()
                     .AdjustsAsync(async v => v.Id = await _identifierGenerator.GenerateIdentifierAsync<Guid, Robot>().ConfigureAwait(false))
                     .Then(v => Cleaner.CleanUp(v))
                     .ValidateAsync(vc => vc.Interop(() => FluentValidator.Create<RobotValidator>().Wrap()), cancellationToken: ct)
                     .ThenAsAsync(v => _dataService.CreateAsync(v));
    }, InvokerArgs.Create);

    /// <inheritdoc/>
    public Task<Result<Robot>> UpdateAsync(Robot value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go(value).Required().Requires(id).Adjusts(v => v.Id = id)
                     .Then(v => Cleaner.CleanUp(v))
                     .ValidateAsync(vc => vc.Interop(() => FluentValidator.Create<RobotValidator>().Wrap()), cancellationToken: ct)
                     .ThenAsAsync(v => _dataService.UpdateAsync(v));
    }, InvokerArgs.Update);

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go().Requires(id)
                     .Then(() => Cleaner.CleanUp(id))
                     .ThenAsync(() => _dataService.DeleteAsync(id));
    }, InvokerArgs.Delete);

    /// <inheritdoc/>
    public Task<Result<RobotCollectionResult>> GetByArgsAsync(RobotArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go()
                     .Then(() => Cleaner.CleanUp(args))
                     .ValidatesAsync(args, vc => vc.Entity().With<RobotArgsValidator>(), cancellationToken: ct)
                     .ThenAsAsync(() => _dataService.GetByArgsAsync(args, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result> RaisePowerSourceChangeAsync(Guid id, RefDataNamespace.PowerSource? powerSource) => ManagerInvoker.Current.InvokeAsync(this, (_, ct) =>
    {
        return Result.Go().Requires(id)
                     .ThenAsync(() => RaisePowerSourceChangeOnImplementationAsync(id, powerSource));
    }, InvokerArgs.Unspecified);
}

#pragma warning restore
#nullable restore