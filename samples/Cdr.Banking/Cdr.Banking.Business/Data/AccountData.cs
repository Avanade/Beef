﻿using Microsoft.Azure.Cosmos.Linq;
using System.Text;

namespace Cdr.Banking.Business.Data;

public partial class AccountData
{
    private static QueryArgsConfig _config = QueryArgsConfig.Create()
        .WithFilter(filter => filter
            .AddReferenceDataField<OpenStatus>(nameof(Model.Account.OpenStatus), c => c.WithValue(os => os == OpenStatus.All ? throw new FormatException("Value not valid for filtering.") : os))
            .AddReferenceDataField<ProductCategory>(nameof(Model.Account.ProductCategory))
            .AddField<bool>(nameof(Model.Account.IsOwned)));

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class setting the required internal configurations.
    /// </summary>
    partial void AccountDataCtor()
    {
        _getAccountsOnQuery = GetAccountsOnQuery;   // Wire up the plug-in to enable filtering. 
        _getAccountsQueryOnQuery = (q, args) => q.Where(_config, args).OrderBy(x => x.Id);  // Wire up the OData-like query syntax.
    }

    /// <summary>
    /// Perform the query filering for the GetAccounts.
    /// </summary>
    private IQueryable<Model.Account> GetAccountsOnQuery(IQueryable<Model.Account> query, AccountArgs? args)
    {
        var q = query.OrderBy(x => x.Id).AsQueryable();
        if (args == null || args.IsInitial)
            return q;

        // Where an argument value has been specified then add as a filter - the WhereWhen and WhereWith are enabled by CoreEx.
        q = q.WhereWhen(!(args.OpenStatus == null) && args.OpenStatus != OpenStatus.All, x => x.OpenStatus == args.OpenStatus!.Code);
        q = q.WhereWith(args?.ProductCategory, x => x.ProductCategory == args!.ProductCategory!.Code);

        // With checking IsOwned a simple false check cannot be performed with Cosmos; assume "not IsDefined" is equivalent to false also. 
        if (args!.IsOwned == null)
            return q;

        if (args.IsOwned == true)
            return q.Where(x => x.IsOwned == true);
        else
            return q.Where(x => !x.IsOwned.IsDefined() || !x.IsOwned);
    }

    /// <summary>
    /// Gets the balance for the specified account.
    /// </summary>
    private Task<Result<Balance?>> GetBalanceOnImplementationAsync(string? accountId)
    {
        // Use the 'Account' model and select for the specified id to access the balance property.
        return Result.GoAsync(_cosmos.Accounts.ModelContainer.Query(q => q.Where(x => x.Id == accountId)).SelectFirstOrDefaultWithResultAsync())
            .WhenAs(a => a is not null, a =>
            {
                var bal = _cosmos.Mapper.Map<Model.Balance, Balance>(a!.Balance);
                return bal.Adjust(b => b.Id = a.Id);
            });
    }

    /// <summary>
    /// Gets the statement (file) for the specified account.
    /// </summary>
    private Task<Result<FileContentResult?>> GetStatementOnImplementationAsync(string? accountId)
        => Result.GoAsync(GetDetailAsync(accountId))
            .WhenAs(d => d is not null, d => new FileContentResult(Encoding.UTF8.GetBytes($"Statement for Account '{d.AccountNumber}'."), "text/plain") { FileDownloadName = $"{accountId}.statement.txt" });
}