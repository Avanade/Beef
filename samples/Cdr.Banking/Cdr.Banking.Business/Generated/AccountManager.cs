/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Cdr.Banking.Business
{
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

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.AccountArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
        public Task<Result<AccountCollectionResult>> GetAccountsAsync(AccountArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go()
                         .ValidatesAsync(args, v => v.Entity().With<AccountArgsValidator>())
                         .ThenAsAsync(() => _dataService.GetAccountsAsync(args, paging));
        }, InvokerArgs.Read);

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
        public Task<Result<AccountDetail?>> GetDetailAsync(string? accountId) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go().Requires(accountId)
                         .ThenAsAsync(() => _dataService.GetDetailAsync(accountId));
        }, InvokerArgs.Read);

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="Balance"/> where found.</returns>
        public Task<Result<Balance?>> GetBalanceAsync(string? accountId) => ManagerInvoker.Current.InvokeAsync(this, _ =>
        {
            return Result.Go().Requires(accountId)
                         .ThenAsAsync(() => _dataService.GetBalanceAsync(accountId));
        }, InvokerArgs.Read);
    }
}

#pragma warning restore
#nullable restore