/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

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
    /// Provides the <see cref="Account"/> data access.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Will not always appear static depending on code-gen options")]
    public partial class AccountData : IAccountData
    {
        private readonly ICosmosDb _cosmos;

        #region Extensions
        #pragma warning disable CS0649, IDE0044 // Defaults to null by design; can be overridden in constructor.

        private Action<ICosmosDbArgs>? _onDataArgsCreate;
        private Func<IQueryable<Model.Account>, AccountArgs?, ICosmosDbArgs, IQueryable<Model.Account>>? _getAccountsOnQuery;

        #pragma warning restore CS0649, IDE0044
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountData"/> class.
        /// </summary>
        /// <param name="cosmos">The <see cref="ICosmosDb"/>.</param>
        public AccountData(ICosmosDb cosmos)
            { _cosmos = Check.NotNull(cosmos, nameof(cosmos)); AccountDataCtor(); }

        partial void AccountDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.AccountArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
        public Task<AccountCollectionResult> GetAccountsAsync(AccountArgs? args, PagingArgs? paging)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                AccountCollectionResult __result = new AccountCollectionResult(paging);
                var __dataArgs = CosmosMapper.Default.CreateArgs("Account", __result.Paging!, PartitionKey.None, onCreate: _onDataArgsCreate);
                __result.Result = _cosmos.Container(__dataArgs).Query(q => _getAccountsOnQuery?.Invoke(q, args, __dataArgs) ?? q).SelectQuery<AccountCollection>();
                return await Task.FromResult(__result).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
        public Task<AccountDetail?> GetDetailAsync(string? accountId)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = AccountDetailData.CosmosMapper.Default.CreateArgs("Account", PartitionKey.None, onCreate: _onDataArgsCreate);
                return await _cosmos.Container(__dataArgs).GetAsync(accountId).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="Balance"/> where found.</returns>
        public Task<Balance?> GetBalanceAsync(string? accountId)
            => DataInvoker.Current.InvokeAsync(this, () => GetBalanceOnImplementationAsync(accountId));

        /// <summary>
        /// Provides the <see cref="Account"/> and Cosmos <see cref="Model.Account"/> property mapping.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "By design; as there is a direct relationship")]
        public partial class CosmosMapper : CosmosDbMapper<Account, Model.Account, CosmosMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosMapper"/> class.
            /// </summary>
            public CosmosMapper()
            {
                Property(s => s.Id, d => d.Id).SetUniqueKey(false);
                Property(s => s.CreationDate, d => d.CreationDate);
                Property(s => s.DisplayName, d => d.DisplayName);
                Property(s => s.Nickname, d => d.Nickname);
                Property(s => s.OpenStatusSid, d => d.OpenStatus);
                Property(s => s.IsOwned, d => d.IsOwned);
                Property(s => s.MaskedNumber, d => d.MaskedNumber);
                Property(s => s.ProductCategorySid, d => d.ProductCategory);
                Property(s => s.ProductName, d => d.ProductName);
                AddStandardProperties();
                CosmosMapperCtor();
            }
            
            partial void CosmosMapperCtor(); // Enables the CosmosMapper constructor to be extended.
        }
    }
}

#pragma warning restore IDE0005
#nullable restore