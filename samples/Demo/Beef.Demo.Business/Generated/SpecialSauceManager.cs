/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business;

/// <summary>
/// Provides the <see cref="SpecialSauce"/> business functionality.
/// </summary>
public partial class SpecialSauceManager : ISpecialSauceManager
{
    private readonly ISpecialSauceDataSvc _dataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialSauceManager"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="ISpecialSauceDataSvc"/>.</param>
    public SpecialSauceManager(ISpecialSauceDataSvc dataService)
        { _dataService = dataService.ThrowIfNull(); SpecialSauceManagerCtor(); }

    partial void SpecialSauceManagerCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task PourAsync() => ManagerInvoker.Current.InvokeAsync(this, async (_, ct) =>
    {
        await _dataService.PourAsync().ConfigureAwait(false);
    }, InvokerArgs.Unspecified);
}

#pragma warning restore
#nullable restore