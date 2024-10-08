/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business.Data;

/// <summary>
/// Defines the <see cref="Account"/> data access.
/// </summary>
public partial interface IAccountData
{
    /// <summary>
    /// Get all accounts.
    /// </summary>
    /// <param name="args">The Args (see <see cref="Entities.AccountArgs"/>).</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
    Task<Result<AccountCollectionResult>> GetAccountsAsync(AccountArgs? args, PagingArgs? paging);

    /// <summary>
    /// Get all accounts.
    /// </summary>
    /// <param name="query">The <see cref="QueryArgs"/>.</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
    Task<Result<AccountCollectionResult>> GetAccountsQueryAsync(QueryArgs? query, PagingArgs? paging);

    /// <summary>
    /// Get <see cref="AccountDetail"/>.
    /// </summary>
    /// <param name="accountId">The <see cref="Account"/> identifier.</param>
    /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
    Task<Result<AccountDetail?>> GetDetailAsync(string? accountId);

    /// <summary>
    /// Get <see cref="Account"/> <see cref="Balance"/>.
    /// </summary>
    /// <param name="accountId">The <see cref="Account"/> identifier.</param>
    /// <returns>The selected <see cref="Balance"/> where found.</returns>
    Task<Result<Balance?>> GetBalanceAsync(string? accountId);

    /// <summary>
    /// Get <see cref="Account"/> statement (file).
    /// </summary>
    /// <param name="accountId">The <see cref="Account"/> identifier.</param>
    /// <returns>A resultant <see cref="FileContentResult"/>.</returns>
    Task<Result<FileContentResult?>> GetStatementAsync(string? accountId);
}