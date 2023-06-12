/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Defines the <see cref="Transaction"/> data access.
    /// </summary>
    public partial interface ITransactionData
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
}

#pragma warning restore
#nullable restore