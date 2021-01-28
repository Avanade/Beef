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
using Beef.Business;
using Beef.Entities;
using Cdr.Banking.Business.Data;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="Transaction"/> data repository services.
    /// </summary>
    public partial class TransactionDataSvc : ITransactionDataSvc
    {
        private readonly ITransactionData _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="ITransactionData"/>.</param>
        public TransactionDataSvc(ITransactionData data)
            { _data = Check.NotNull(data, nameof(data)); TransactionDataSvcCtor(); }

        partial void TransactionDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get transaction for account.
        /// </summary>
        /// <param name="accountId">The Account Id.</param>
        /// <param name="args">The Args (see <see cref="Common.Entities.TransactionArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="TransactionCollectionResult"/>.</returns>
        public Task<TransactionCollectionResult> GetTransactionsAsync(string? accountId, TransactionArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.GetTransactionsAsync(accountId, args, paging).ConfigureAwait(false);
                return __result;
            });
        }
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore