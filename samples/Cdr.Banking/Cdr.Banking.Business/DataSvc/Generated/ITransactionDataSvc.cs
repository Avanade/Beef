/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business.DataSvc;

/// <summary>
/// Defines the <see cref="Transaction"/> data repository services.
/// </summary>
public partial interface ITransactionDataSvc
{
    /// <summary>
    /// Get transaction for account.
    /// </summary>
    /// <param name="accountId">The Account Id.</param>
    /// <param name="args">The Args (see <see cref="Entities.TransactionArgs"/>).</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    /// <returns>The <see cref="TransactionCollectionResult"/>.</returns>
    Task<Result<TransactionCollectionResult>> GetTransactionsAsync(string? accountId, TransactionArgs? args, PagingArgs? paging);
}