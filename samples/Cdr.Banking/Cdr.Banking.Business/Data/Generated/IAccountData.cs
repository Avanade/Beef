/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Entities;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Defines the <see cref="Account"/> data access.
    /// </summary>
    public partial interface IAccountData
    {
        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.AccountArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
        Task<AccountCollectionResult> GetAccountsAsync(AccountArgs? args, PagingArgs? paging);

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
        Task<AccountDetail?> GetDetailAsync(string? accountId);

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="Balance"/> where found.</returns>
        Task<Balance?> GetBalanceAsync(string? accountId);
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore