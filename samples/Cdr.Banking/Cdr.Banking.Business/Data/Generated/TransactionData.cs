/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Beef;
using Beef.Business;
using Beef.Data.Cosmos;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Transaction"/> data access.
    /// </summary>
    public partial class TransactionData : ITransactionData
    {
        private readonly ICosmosDb _cosmos;

        private Action<ICosmosDbArgs>? _onDataArgsCreate;
        private Func<IQueryable<Model.Transaction>, string?, TransactionArgs?, ICosmosDbArgs, IQueryable<Model.Transaction>>? _getTransactionsOnQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionData"/> class.
        /// </summary>
        /// <param name="cosmos">The <see cref="ICosmosDb"/>.</param>
        public TransactionData(ICosmosDb cosmos)
            { _cosmos = Check.NotNull(cosmos, nameof(cosmos)); TransactionDataCtor(); }

        partial void TransactionDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get transaction for account.
        /// </summary>
        /// <param name="accountId">The Account Id.</param>
        /// <param name="args">The Args (see <see cref="Entities.TransactionArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="TransactionCollectionResult"/>.</returns>
        public Task<TransactionCollectionResult> GetTransactionsAsync(string? accountId, TransactionArgs? args, PagingArgs? paging) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            TransactionCollectionResult __result = new TransactionCollectionResult(paging);
            var __dataArgs = CosmosMapper.Default.CreateArgs("Transaction", __result.Paging!, new PartitionKey(accountId), onCreate: _onDataArgsCreate);
            __result.Result = _cosmos.Container(__dataArgs).Query(q => _getTransactionsOnQuery?.Invoke(q, accountId, args, __dataArgs) ?? q).SelectQuery<TransactionCollection>();
            return await Task.FromResult(__result).ConfigureAwait(false);
        });

        /// <summary>
        /// Provides the <see cref="Transaction"/> and Cosmos  property mapping.
        /// </summary>
        public partial class CosmosMapper : CosmosDbMapper<Transaction, Model.Transaction, CosmosMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosMapper"/> class.
            /// </summary>
            public CosmosMapper()
            {
                Property(s => s.Id, d => d.Id).SetUniqueKey(false);
                Property(s => s.AccountId, d => d.AccountId);
                Property(s => s.IsDetailAvailable, d => d.IsDetailAvailable);
                Property(s => s.TypeSid, d => d.Type);
                Property(s => s.StatusSid, d => d.Status);
                Property(s => s.Description, d => d.Description);
                Property(s => s.PostingDateTime, d => d.PostingDateTime);
                Property(s => s.ExecutionDateTime, d => d.ExecutionDateTime);
                Property(s => s.Amount, d => d.Amount);
                Property(s => s.Currency, d => d.Currency);
                Property(s => s.Reference, d => d.Reference);
                Property(s => s.MerchantName, d => d.MerchantName);
                Property(s => s.MerchantCategoryCode, d => d.MerchantCategoryCode);
                Property(s => s.BillerCode, d => d.BillerCode);
                Property(s => s.BillerName, d => d.BillerName);
                Property(s => s.ApcaNumber, d => d.ApcaNumber);
                AddStandardProperties();
                CosmosMapperCtor();
            }
            
            partial void CosmosMapperCtor(); // Enables the CosmosMapper constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore