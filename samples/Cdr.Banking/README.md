# Banking APIs

The purpose of this sample is to demonstrate the usage of _Beef_ in a real-world scenario. The scenario that has been chosen is a partial implementation of the Open Banking APIs as defined by the Consumer Data Standards in Australia.

<br/>

## Consumer Data Standards

As described on the web [https://consumerdatastandards.org.au/](https://consumerdatastandards.org.au/):

> The Australian government has introduced a Consumer Data Right giving consumers greater control over their data. Part of this right requires the creation of common technical standards making it easier and safer for consumers to access data held about them by businesses, and – if they choose to – share this data via application programming interfaces (APIs) with trusted, accredited third parties."

The Banking APIs and corresponding schema are documented [here](https://consumerdatastandardsaustralia.github.io/standards/#consumer-data-standards-banking-apis).

</br>

## Scope

The full scope of the APIs has not been implemented; only the following endpoints have been created to demonstrate:
- Get Accounts - [GET /banking/accounts](https://consumerdatastandardsaustralia.github.io/standards/#get-accounts)
- Get Account Balance - [GET /banking/accounts/\{accountId\}/balance](https://consumerdatastandardsaustralia.github.io/standards/#get-account-balance)
- Get Account Detail - [GET /banking/accounts/\{accountId\}](https://consumerdatastandardsaustralia.github.io/standards/#get-account-detail)
- Get Transactions for Account [GET /banking/accounts/\{accountId\}/transactions](https://consumerdatastandardsaustralia.github.io/standards/#get-transactions-for-account)

</br>

## Assumptions

Assumptions are as follows:
- **Authentication** - no formal authentication (such as [OAuth](https://oauth.net/2/)) has been implemented; a faux authentication is implemented by passing a HTTP header (`cdr-user`) containing the user name (in plain-text) compared to a hard-coded list of values. This is obviously not how you would implement proper, but for the sake of brevity enables the basic capability.
- **Authorization** - again for brevity, a list of accounts are allocated to a user to limit (filter) the accounts a user should be able to see and interact with. Where a user attempts to interact with an account they do not have permission for an HTTP status of Forbidden (403) will be returned.
- Any additional request HTTP headers specified by the `x-` prefix, and the corresponding response enveloping `data`, `links` and `meta` would be managed by an API Gateway, for example [Azure API Management](https://azure.microsoft.com/en-us/services/api-management/).

</br>

## Cosmos DB usage

For the purposes of this sample, [Azure Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/) has been chosen as the data store.

As the operations are read-only, the data store would be (should be) optimised for these required read activitiies. In this instance the assumption is that the Cosmos DB store would be a near real-time replica from the _system of record_. There would be an on-going process to synchronise the Cosmos DB data with the latest infomation, using the likes of [event steaming](../../src/Beef.Events/README.md) for example.

The following [Containers](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container) will be used:
- **`RefData`** - All Reference Data types and their corresponding items. No partitioning will be used, as there is limited benefit in partitioning per reference data type given the limited volume within, and the limited access given largely cached in memory.
- **`Account`** - All Accounts, for all users. No partioning will be used. Accounts will need to be explicitly filtered per user request to ensure correct access.
- **`Transaction`** - All Transactions, for all accounts. Partitioning will be used, a partition per Account; i.e. all transactions for a given Account will reside exclusively within its Account partition. Access to a given partition will need to be explicitly allowed per user request to ensure correct access.

The requirements specify when filtering Transactions the following:
> As the date and time for a transaction can alter depending on status and transaction type two separate date/times are included in the payload. There are still some scenarios where neither of these time stamps is available. For the purpose of filtering and ordering it is expected that the data holder will use the “effective” date/time which will be defined as: - Posted date/time if available, then - Execution date/time if available, then - A reasonable date/time nominated by the data holder using internal data structures.

To simplify any querying for date and time will occur against a new property defined in the underlying data model for `Transaction`. This property `TransactionDateTime` should be updated with the correct datetime when loaded into the Cosmos DB Container; this will avoid the need for an overly complex query against multiple data and time properties.

<br/>

## Solution skeleton

This solution was originally created using the solution [template](../../templates/Beef.Template.Solution/README.md) capability, following the getting started [guide](../../docs/Sample-Cosmos-GettingStarted.md).

The following command was issued to create the solution structure.

```
dotnet new beef --company Cdr --appname Banking --datasource Cosmos
```

```
└── Cdr.Banking
  └── Cdr.Banking.Api         # API end-point and operations
  └── Cdr.Banking.Business    # Core business logic components
  └── Cdr.Banking.CodeGen     # Entity and Reference Data code generation console
  └── Cdr.Banking.Common      # Common / shared components
  └── Cdr.Banking.Test        # Unit and intra-integration tests
  └── Cdr.Banking.sln         # Solution file that references all above projects
```

_Note:_ Code generation was **not** performed before updating the corresponding YAML files described in the next section. Otherwise, extraneous files would have been generated that would then need to be manually removed.

Also, any files that started with `Person` (being the demonstration entity) were removed (deleted) from their respective projects. This then represented the base-line to build up the solution from.

</br>

## Code generation

The following code-generation files require configuration:
- [`entity.beef.yaml`](./Cdr.Banking.CodeGen/entity.beef.yaml) - this describes the entities, their properties and operations, to fulfil the aforementioned CDR Banking API endpoint and schema requirements.
- [`refdata.beef.yaml`](./Cdr.Banking.CodeGen/refdata.beef.yaml) - this describes the reference data entities, their properties, and corresponding get all (read) operation. These are defined seperately as different code-generation templates are used.
- [`datamodel.beef.yaml`](./Cdr.Banking.CodeGen/datamodel.beef.yaml) - this describes the Cosmos DB data models and their properties. These are logically seperated as only a _basic model_ class is generated. 

Each of the files have comments added within to aid the reader as to purpose of the configuration. Otherwise, see the related entity-driven code-generation [documentation](../../tools/Beef.CodeGen.Core/README.md) for more information.

The following command was issued to perform the code-generation.

```
dotnet new all
```

</br>

## Authentication/Authorisation

To demonstrate the authentication and authorisation the following is required:
- HTTP Header (`cdr-user`) containing the username.
- Customised `ExecutionContext` with a list of Accounts the user has permission to view.
- Data access filtering to ensure only the correct Accounts and Transactions are accessed for a given user.

<br/>

### Execution Context

A custom [`ExecutionContext`](./Cdr.Banking.Business/ExecutionContext.cs) that inherits from the _Beef_ [`ExecutionContext`](../../src/Beef.Abstractions/ExecutionContext.cs) is required. An `Accounts` property is added to contain the list of permissable Accounts.

``` csharp
/// <summary>
/// Extended <see cref="Beef.ExecutionContext"/> that stores the list of <see cref="Accounts"/> that a user has access to.
/// </summary>
public class ExecutionContext : Beef.ExecutionContext
{
    /// <summary>
    /// Gets the current <see cref="ExecutionContext"/> instance.
    /// </summary>
    public static new ExecutionContext Current => (ExecutionContext)Beef.ExecutionContext.Current;

    /// <summary>
    /// Gets the list of account (identifiers) that the user has access/permission to.
    /// </summary>
    public List<string> Accounts { get; } = new List<string>();
}
```

<br/>

### Set up Execution Context

The `ExecutionContext` is required to be configured for each and every request, this is performed within the API [`Startup.cs`](./Cdr.Banking.Api/Startup.cs).

Firstly, the custom `ExecutionContext` instantiation must be registered within the `ConfigureServices` method.

``` csharp
// Add the core beef services (including the customized ExecutionContext).
services.AddBeefExecutionContext(() => new Business.ExecutionContext())
```

Within the `Configure` method the User to Accounts mapping is performed. As stated previously in the earlier assumptions, the likes of an OAuth token would have previously been validated, then would either contain the list of Accounts, or these would be loaded from an appropriate data store into the `ExecutionContext`.

``` csharp
// Add execution context set up to the pipeline.
app.UseExecutionContext((hc, ec) =>
{
    if (!hc.Request.Headers.TryGetValue("cdr-user", out var username) || username.Count != 1)
        throw new Beef.AuthorizationException();

    var mec = (ExecutionContext)ec;

    switch (username[0])
    {
        case "jessica":
            mec.Accounts.AddRange(new string[] { "12345678", "34567890", "45678901" });
            break;

        case "jenny":
            mec.Accounts.Add("23456789");
            break;

        case "jason":
            break;

        default:
            throw new Beef.AuthorizationException();
    }
});
```

<br/>

### Account-wide filtering

To ensure consistent filtering for all CRUD (Query, Get, Create, Update and Delete) operations an authorization filter can be applied within the [`CosmosDb.cs`](./Cdr.Banking.Business/Data/CosmosDb.cs). This class inherits from the [`CosmosDbBase`](../../src/Beef.Data.Cosmos/CosmosDbBase.cs). The `SetAuthorizeFilter` can be used to define a filter to be applied to all operations that occur against the specified `Model` and `Container`.

By defining holistically, it allows the capability to be added or maintained independent of its individual usage. Whereby minimising on-going change and potential errors where not implemented consistently through-out the code base.

In this case, the filter will ensure that only Accounts that have been defined for the User can be accessed. 

``` csharp
public class CosmosDb : CosmosDbBase
{
    public CosmosDb(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = null) : base(client, databaseId, createDatabaseIfNotExists, throughput)
    {
        // Apply an authorization filter to all operations to ensure only the valid data is available based on the users context - only allow access to Accounts within list defined on ExecutionContext.
        SetAuthorizeFilter<Model.Account>("Account", (q) => ((IQueryable<Model.Account>)q).Where(am => ExcutionContext.Current.Accounts.Contains(am.Id!)));
    }
}
```

<br/>

### Transaction-wide filtering

For the Transaction entity, we have chosen the strategy to leverage the [`PartitionKey`](https://docs.microsoft.com/en-us/azure/cosmos-db/partitioning-overview) as a means to divide (and isolate) the Transactions to an owning Account.

In this instance, the onus is on the developer to set the `PartitionKey` appropriately before performing the underlying Cosmos DB operation.

In this case, we are setting this using the code-generation for the operation. The `DataCosmosPartitionKey` attribute for the `Operation` element enables. The `accountId` parameter value will be used for partitioning.

``` yaml
# Operation to get all Transactions for a specified Account.
# Operation and Route requires accountId; e.g. api/v1/banking/accounts/{accountId}/transactions
# Supports filtering using defined properies from TransactionArgs (the args will be validated TransactionArgsValidator) to ensure valid values are passed).
# Supports paging.
# Data access will be auto-implemented for Cosmos as defined for the entity.
# Cosmos PartitionKey will be set to the accountId parameter value for data access.
# 
{ name: GetTransactions, text: Get transaction for account, type: GetColl, webApiRoute: '{accountId}/ransactions', paging: true, cosmosPartitionKey: accountId,
  parameters: [
    # Note usage of ValidatorFluent which will inject the code as-is into the validation logic; beinga common validator 'Validators.Account' that will perform the authorization check.
    { name: AccountId, type: string, validatorCode: Common(Validators.AccountId), webApiFrom: romRoute, isMandatory: true },
    { name: Args, type: TransactionArgs, validator: TransactionArgsValidator }
  ]
}
```

As stated in the above YAML comments, a common [validator](../../docs/Beef-Validation.md) will be used to perform the authorization logic. The static [`Validators.AccountId`](./Cdr.Banking.Business/Validation/Validators.cs) is used to perform the validation.

``` csharp
public static CommonValidator<string?> AccountId => CommonValidator.Create<string?>(v => v.Custom(ctx =>
{
    if (ctx.Value == null || !ExecutionContext.Current.Accounts.Contains(ctx.Value))
        throw new AuthorizationException();
}));
```

<br/>

## Data access

Where possible the _Beef_ "out-of-the-box" data access is leveraged via the code-generation configuration. However, to meet the CDR requirements around data filtering this logic must be customized. This is achieved using the plug-in opportunities offered by the code generation.

<br/>

### Get Accounts

The `Account` filtering as required by `/banking/accounts` uses the [`AccountArgs`](./Cdr.Banking.Common/Entities/Generated/AccountArgs.cs) entity to provide the possible selection criteria. The filtering then uses the standard `LINQ` filtering capabilities offered by the Cosmos DB SDK. The `GetAccountsOnQuery` method within the non-generated [`AccountData.cs`](./Cdr.Banking.Business/Data/AccountData.cs) partial class contains the filtering logic. 

``` csharp
/// <summary>
/// Perform the query filering for the GetAccounts.
/// </summary>
private IQueryable<Model.Account> GetAccountsOnQuery(IQueryable<Model.Account> query, AccountArgs? args, CosmosDbArgs dbArgs)
{
    if (args == null || args.IsInitial)
        return query;

    // Where an argument value has been specified then add as a filter - the WhereWhen and WhereWith are enabled by Beef.
    var q = query.WhereWhen(!(args.OpenStatus == null) && args.OpenStatus != OpenStatus.All, x => x.OpenStatus == args!.OpenStatus!.Code);
    q = q.WhereWith(args?.ProductCategory, x => x.ProductCategory == args.ProductCategory!.Code);

    // With checking IsOwned a simple false check cannot be performed with Cosmos; assume "not IsDefined" is equivalent to false also. 
    if (args!.IsOwned == null)
        return q;

    if (args.IsOwned == true)
        return q.Where(x => x.IsOwned == true);
    else
        return q.Where(x => !x.IsOwned.IsDefined() || !x.IsOwned);
}
```

</br>

### Get Account Balance

The `Account` balance as required is implemented in a fully customised manner. The code-generated output will invoke the `GetBalanceOnImplementationAsync` method within the non-generated [`AccountData.cs`](./Cdr.Banking.Business/Data/AccountData.cs) partial class.

The underlying logic queries the `Account` container for the specified `accountId` and returns the corresponding `Balance`.

``` csharp
/// <summary>
/// Gets the balance for the specified account.
/// </summary>
private Task<Balance?> GetBalanceOnImplementationAsync(string? accountId)
{
    // Create an IQueryable for the 'Account' container, then select for the specified id just the balance property.
    var args = _accountMapper.CreateArgs("Account");
    var val = (from a in CosmosDb.Default.Container(args).AsQueryable()
                where a.Id == accountId
                select new { a.Id, a.Balance }).SelectSingleOrDefault();

    if (val == null)
        return Task.FromResult<Balance?>(null);

    // Map the Model.Balance to Balance and return.
    var bal = _balanceMapper.MapToSrce(val.Balance)!;
    bal.Id = val.Id;
    return Task.FromResult<Balance?>(bal);
}
```

<br/>

### Get Transactions for Account

The `Transaction` filtering as required by `/banking/accounts/\{accountId\}/transactions` uses the [`TransactionArgs`](./Cdr.Banking.Common/Entities/Generated/TransactionArgs.cs) entity to provide the possible selection criteria. The filtering then uses the standard `LINQ` filtering capabilities offered by the Cosmos DB [SDK](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos?view=azure-dotnet). The `GetTransactionsOnQuery` method within the non-generated [`TransactionData.cs`](./Cdr.Banking.Business/Data/TransactionData.cs) partial class contains the filtering logic. 

``` csharp
/// <summary>
/// Perform the query filering for the GetTransactions.
/// </summary>
private IQueryable<Model.Transaction> GetTransactionsOnQuery(IQueryable<Model.Transaction> query, string? _, TransactionArgs? args, CosmosDbArgs? __)
{
    if (args == null || args.IsInitial)
        return query.OrderByDescending(x => x.TransactionDateTime);

    var q = query.WhereWith(args.FromDate, x => x.TransactionDateTime >= args.FromDate);
    q = q.WhereWith(args.ToDate, x => x.TransactionDateTime <= args.ToDate);
    q = q.WhereWith(args.MinAmount, x => x.Amount >= args.MinAmount);
    q = q.WhereWith(args.MaxAmount, x => x.Amount <= args.MaxAmount);

    // The text filtering will perform a case-insensitive (based on uppercase) comparison on Description and Reference properties. 
    q = q.WhereWith(args.Text, x => x.Description!.ToUpper().Contains(args.Text!.ToUpper()) || x.Reference!.ToUpper().Contains(args.Text!.ToUpper()));

    // Order by TransactionDateTime in descending order.
    return q.OrderByDescending(x => x.TransactionDateTime);
}
```

<br/>

## Validation

To minimise the bad request data, and meet the CDR functional requirements for the operation arguments, validation has been included.

<br/>

### Get Accounts

The validation for the [`AccountArgs`](./Cdr.Banking.Common/Entities/Generated/AccountArgs.cs) is relatively straightforward in that the `OpenStatus` and `ProductCategory` will be checked to ensure the passed reference data values are considered valid. The [`AccountArgsValidator.cs`](./Cdr.Banking.Business/Validation/AccountArgsValidator.cs) provides the implementation.

``` csharp
/// <summary>
/// Represents a <see cref="AccountArgs"/> validator.
/// </summary>
public class AccountArgsValidator : Validator<AccountArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountArgsValidator"/>.
    /// </summary>
    public AccountArgsValidator()
    {
        Property(x => x.OpenStatus).IsValid();
        Property(x => x.ProductCategory).IsValid();
    }
}
```

<br/>

### Get Transactions for Account

The validation for the [`TransactionArgs`](./Cdr.Banking.Common/Entities/Generated/TransactionArgs.cs) is a little more nuanced. The following needs to be performed as per the CDR requirements:
- Default `FromDate` where not provided, as 90 days less than ToDate; where no ToDate then assume today (now).
- Make sure `FromDate` is not greater than `ToDate`.
- Make sure `MinAmount` is not greater than `MaxAmount`.
- Additionally, make sure `Text` does not include the '*' wildcard character (do not want to give appearance is support).

The [`TransactionArgsValidator.cs`](./Cdr.Banking.Business/Validation/TransactionArgsValidator.cs) provides the implementation.

``` csharp
/// <summary>
/// Represents a <see cref="TransactionArgs"/> validator.
/// </summary>
public class TransactionArgsValidator : Validator<TransactionArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionArgsValidator"/>.
    /// </summary>
    public TransactionArgsValidator()
    {
        // Default FromDate where not provided, as 90 days less than ToDate; where no ToDate then assume today (now). Make sure FromDate is not greater than ToDate.
        Property(x => x.FromDate)
            .Default(a => (a.ToDate!.HasValue ? a.ToDate.Value : DateTime.Now).AddDays(-90))
            .CompareProperty(CompareOperator.LessThanEqual, y => y.ToDate).DependsOn(y => y.ToDate);

        // Make sure MinAmount is not greater than MaxAmount.
        Property(x => x.MinAmount).CompareProperty(CompareOperator.LessThanEqual, y => y.MaxAmount).DependsOn(y => y.MaxAmount);

        // Make sure the Text does not include the '*' wildcard character.
        Property(x => x.Text).Wildcard(Beef.Wildcard.None);
    }
}
```

<br/>

## Paging

_Beef_ enables, and supports, paging out-of-the-box, with built-in support to minimize the requirement for a developer to implement beyond declaring intent through the code-generation configuration. This is how the paging capability has been implemented within the solution.

However, the CDR specification specifies that the paging is to be supported using the following query string parameters:
- `page` - representing the page number
- `page-size` - represents the page size (defaults to 25).

These are not supported out-of-the-box and support must be added. This is performed within the API [`Startup.cs`](./Cdr.Banking.Api/Startup.cs). The following is added to the `Startup` method.

``` csharp
// Add "page" and "page-size" to the supported paging query string parameters as defined by the CDR specification; and default the page size to 25 from config.
WebApiQueryString.PagingArgsPageQueryStringNames.Add("page");
WebApiQueryString.PagingArgsTakeQueryStringNames.Add("page-size");
PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
```

</br>

## Testing

A reasonably thorough set of [intra-domain integration tests](../../tools/Beef.Test.NUnit/README.md) have been added to demonstrate usage, as well as validate that the selected CDR Banking operations function as described. For the most part the tests should be self-explanatory.

- [`AccountTest.cs`](./Cdr.Banking.Test/AccountTest.cs)
- [`TransactionTest.cs`](./Cdr.Banking.Test/TransactionTest.cs)

Of note, within the [`FixtureSetup.cs`](./Cdr.Banking.Test/FixtureSetup.cs) the authorization header and paging configuration is set up.

``` csharp
// TODO: Passing the username as an http header for all requests; this would be replaced with OAuth integration, etc.
AgentTester.RegisterBeforeRequest(r => r.Headers.Add("cdr-user", Beef.ExecutionContext.Current.Username));

// Set "page" and "page-size" as the supported paging query string parameters as defined by the CDR specification.
WebApiPagingArgsArg.PagingArgsPageQueryStringName = "page";
WebApiPagingArgsArg.PagingArgsSizeQueryStringName = "page-size";
```
