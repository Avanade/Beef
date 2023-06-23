/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business;

/// <summary>
/// Provides the <see cref="Account"/> business functionality.
/// </summary>
public partial class AccountManager : IAccountManager
{
    private readonly IAccountDataSvc _dataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountManager"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IAccountDataSvc"/>.</param>
    public AccountManager(IAccountDataSvc dataService)
        { _dataService = dataService.ThrowIfNull(); AccountManagerCtor(); }

    partial void AccountManagerCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<AccountCollectionResult>> GetAccountsAsync(AccountArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, ct =>
    {
        return Result.Go()
                     .ValidatesAsync(args, v => v.Entity().With<AccountArgsValidator>(), cancellationToken: ct)
                     .ThenAsAsync(() => _dataService.GetAccountsAsync(args, paging));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<AccountDetail?>> GetDetailAsync(string? accountId) => ManagerInvoker.Current.InvokeAsync(this, ct =>
    {
        return Result.Go().Requires(accountId)
                     .ThenAsAsync(() => _dataService.GetDetailAsync(accountId));
    }, InvokerArgs.Read);

    /// <inheritdoc/>
    public Task<Result<Balance?>> GetBalanceAsync(string? accountId) => ManagerInvoker.Current.InvokeAsync(this, ct =>
    {
        return Result.Go().Requires(accountId)
                     .ThenAsAsync(() => _dataService.GetBalanceAsync(accountId));
    }, InvokerArgs.Read);
}