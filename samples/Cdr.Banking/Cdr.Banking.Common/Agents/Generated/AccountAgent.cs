/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreEx.Configuration;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Agents
{
    /// <summary>
    /// Provides the <see cref="Account"/> HTTP agent.
    /// </summary>
    public partial class AccountAgent : TypedHttpClientBase<AccountAgent>, IAccountAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountAgent"/> class.
        /// </summary>
        /// <param name="client">The underlying <see cref="HttpClient"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="executionContext">The <see cref="CoreEx.ExecutionContext"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AccountAgent(HttpClient client, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext, SettingsBase settings, ILogger<AccountAgent> logger) 
            : base(client, jsonSerializer, executionContext, settings, logger) { }

        /// <inheritdoc/>
        public Task<HttpResult<AccountCollectionResult>> GetAccountsAsync(AccountArgs? args, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<AccountCollectionResult>("api/v1/banking/accounts", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<AccountArgs?>("args", args, HttpArgType.FromUriUseProperties)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<AccountDetail?>> GetDetailAsync(string? accountId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<AccountDetail?>("api/v1/banking/accounts/{accountId}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<string?>("accountId", accountId)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Balance?>> GetBalanceAsync(string? accountId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Balance?>("api/v1/banking/accounts/{accountId}/balance", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<string?>("accountId", accountId)), cancellationToken: cancellationToken);
    }
}

#pragma warning restore
#nullable restore