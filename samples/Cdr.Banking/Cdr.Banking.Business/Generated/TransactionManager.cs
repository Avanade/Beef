/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Entities;
using Beef.Validation;
using Cdr.Banking.Common.Entities;
using Cdr.Banking.Business.DataSvc;
using Cdr.Banking.Business.Validation;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business
{
    /// <summary>
    /// Provides the <see cref="Transaction"/> business functionality.
    /// </summary>
    public partial class TransactionManager : ITransactionManager
    {
        private readonly ITransactionDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="ITransactionDataSvc"/>.</param>
        public TransactionManager(ITransactionDataSvc dataService)
            { _dataService = Check.NotNull(dataService, nameof(dataService)); TransactionManagerCtor(); }

        partial void TransactionManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get transaction for account.
        /// </summary>
        /// <param name="accountId">The Account Id.</param>
        /// <param name="args">The Args (see <see cref="Entities.TransactionArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="TransactionCollectionResult"/>.</returns>
        public async Task<TransactionCollectionResult> GetTransactionsAsync(string? accountId, TransactionArgs? args, PagingArgs? paging)
        {
            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(accountId, args);
                (await MultiValidator.Create()
                    .Add(accountId.Validate(nameof(accountId)).Mandatory().Common(Validators.AccountId))
                    .Add(args.Validate(nameof(args)).Entity().With<IValidator<TransactionArgs>>())
                    .RunAsync().ConfigureAwait(false)).ThrowOnError();

                return Cleaner.Clean(await _dataService.GetTransactionsAsync(accountId, args, paging).ConfigureAwait(false));
            }, BusinessInvokerArgs.Read).ConfigureAwait(false);
        }
    }
}

#pragma warning restore
#nullable restore