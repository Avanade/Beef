/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005, IDE0044 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Caching;
using Beef.Entities;
using Cdr.Banking.Business.Data;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="Account"/> data repository services.
    /// </summary>
    public partial class AccountDataSvc : IAccountDataSvc
    {
        private readonly IAccountData _data;
        private readonly IRequestCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IAccountData"/>.</param>
        /// <param name="cache">The <see cref="IRequestCache"/>.</param>
        public AccountDataSvc(IAccountData data, IRequestCache cache)
            { _data = Check.NotNull(data, nameof(data)); _cache = Check.NotNull(cache, nameof(cache)); AccountDataSvcCtor(); }

        partial void AccountDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.AccountArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="AccountCollectionResult"/>.</returns>
        public Task<AccountCollectionResult> GetAccountsAsync(AccountArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.GetAccountsAsync(args, paging).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
        public Task<AccountDetail?> GetDetailAsync(string? accountId)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __key = new UniqueKey(accountId);
                if (_cache.TryGetValue(__key, out AccountDetail? __val))
                    return __val;

                var __result = await _data.GetDetailAsync(accountId).ConfigureAwait(false);
                _cache.SetValue(__key, __result);
                return __result;
            });
        }

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="Balance"/> where found.</returns>
        public Task<Balance?> GetBalanceAsync(string? accountId)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __key = new UniqueKey(accountId);
                if (_cache.TryGetValue(__key, out Balance? __val))
                    return __val;

                var __result = await _data.GetBalanceAsync(accountId).ConfigureAwait(false);
                _cache.SetValue(__key, __result);
                return __result;
            });
        }
    }
}

#pragma warning restore IDE0005, IDE0044
#nullable restore