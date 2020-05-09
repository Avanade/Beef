/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Beef;
using Beef.Entities;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using Cdr.Banking.Common.Entities;
using Cdr.Banking.Common.Agents.ServiceAgents;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Agents
{
    /// <summary>
    /// Provides the Account Web API agent.
    /// </summary>
    public partial class AccountAgent : WebApiAgentBase, IAccountServiceAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountAgent"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> (where overridding the default value).</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public AccountAgent(HttpClient? httpClient = null, Action<HttpRequestMessage>? beforeRequest = null)
        {
            AccountServiceAgent = Beef.Factory.Create<IAccountServiceAgent>(httpClient, beforeRequest);
        }
        
        /// <summary>
        /// Gets the underlyng <see cref="IAccountServiceAgent"/> instance.
        /// </summary>
        public IAccountServiceAgent AccountServiceAgent { get; private set; }

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="args">The Args (see <see cref="AccountArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<AccountCollectionResult>> GetAccountsAsync(AccountArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null)
            => AccountServiceAgent.GetAccountsAsync(args, paging, requestOptions);

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<AccountDetail>> GetDetailAsync(string? accountId, WebApiRequestOptions? requestOptions = null)
            => AccountServiceAgent.GetDetailAsync(accountId, requestOptions);

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Balance>> GetBalanceAsync(string? accountId, WebApiRequestOptions? requestOptions = null)
            => AccountServiceAgent.GetBalanceAsync(accountId, requestOptions);
    }
}

#nullable restore