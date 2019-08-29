# Data (Data access)

The _Data_ layer is primarily responsible for performing the persistence CRUD (`Create`, `Read`, `Update` or `Delete`) and `Query` requests, handling the underlying wire transport, protocols and payload to enable. 

This is the area in which a developer using _Beef_ is likely to spend most of their time. The basic CRUD capabilities are enabled mostly out-of-the-box; however, the data access logic complexity will likely exceed this as the complexity of the requirements increase.

There are _Beef_ capabilities to encapsulate the data access in a largely consistent manner; in that they largely support the following pattern (where `Xxx` is the data access name):

Class | Description
-|-
`XxxBase` | Provides the base data access logic that is inherited to enable. This encapsulates access to the standard `Get`, `Create`, `Update`, `Delete` and `Query` operations.
`XxxArgs` | Provides a consistent means to pass data source specific arguments to the underlying operations.
`XxxQuery` | Encapsulates the query logic supporting `SelectFirst`, `SelectFirstOrDefault`, `SelectSingle`, `SelectSingleOrDefault` and `SelectQuery` (selects multiple items and either creates, or updates an existing, collection; and where the corresponding [`Paging`](../src/Beef.Core/Entities/PagingResult.cs) is provided the configured paging, and optional get count, will be enacted.)
`XxxMapper` | Provides mapping of a .NET entity to/from the data source representation (where applicable). This allows for property name changes, type conversion, and sub-entity mapping.

<br/>

## Supported

The following **data access** capabilities are currently supported by a corresponding _Beef_ framework; as well as being integrated into the code-generation:

Assembly | Description
-|-
[`Beef.Data.Database`](../src/Beef.Data.Database) | ADO.NET database framework. 
[`Beef.Data.EntityFrameworkCore`](../src/Beef.Data.EntityFrameworkCore) | Entity Framework (EF) Core framework. 
[`Beef.Data.Cosmos`](../src/Beef.Data.Cosmos) | Cosmos DB execution framework. 
[`Beef.Data.OData`](../src/Beef.Data.OData) | OData execution framework. 

This obviously does not prohibit access to other data sources; just that these will need to be implemented in a [custom](#Custom) manner.

<br>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, and has extension opportunities to inject additional logic into the processing pipeline.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}Data`.

There is also a corresonding interface named `I{Entity}Data` generated so the likes of test mocking etc. can be employed.

<br/>

### Code-generated

An end-to-end code-generated processing pipeline generally consists of:

Step | Description
-|-
`DataInvoker` | The logic is wrapped by a [`DataInvoker`](../src/Beef.Core/Business/DataInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`DataContextScopeOption`](../src/Beef.Core/DataContextScopeOption.cs), [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`XxxArgs` | The creation of the data source specific arguments (for example, a stored procedure name for a relational database).
`OnBefore` | The `OnBefore` extension opportunity; where set this will be invoked. This enables logic to be invoked _before_ the primary `Operation` is performed.
`Operation` | The actual operation execution (`Get`, `Create`, `Update`, `Delete` and `Query`). This will include an `OnQuery` extension opportunity where performing a `Query`.
`OnAfter` | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.

The following demonstrates the usage (a snippet from the sample [`PersonData`](../samples/Demo/Beef.Demo.Business/Data/Generated/PersonData.cs)):

``` csharp
/// <summary>
/// Gets the <see cref="Person"/> collection object that matches the selection criteria.
/// </summary>
/// <param name="args">The Args (see <see cref="PersonArgs"/>).</param>
/// <param name="paging">The <see cref="PagingArgs"/>.</param>
/// <returns>A <see cref="PersonCollectionResult"/>.</returns>
public Task<PersonCollectionResult> GetByArgsAsync(PersonArgs args, PagingArgs paging)
{
    return DataInvoker.Default.InvokeAsync(this, async () =>
    {
        PersonCollectionResult __result = new PersonCollectionResult(paging);
        var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonGetByArgs]", __result.Paging);
        if (_getByArgsOnBeforeAsync != null) await _getByArgsOnBeforeAsync(args, __dataArgs);
        __result.Result = Database.Default.Query(__dataArgs, p => _getByArgsOnQuery?.Invoke(p, args, __dataArgs)).SelectQuery<PersonCollection>();
        if (_getByArgsOnAfterAsync != null) await _getByArgsOnAfterAsync(__result, args);
        return __result;
    }, new BusinessInvokerArgs { ExceptionHandler = _getByArgsOnException });
}
```

<br/>

### Custom

A custom (`OnImplementation`) processing pipeline generally consists of:

Step | Description
-|-
`DataInvoker` | The logic is wrapped by a [`DataInvoker`](../src/Beef.Core/Business/DataInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`DataContextScopeOption`](../src/Beef.Core/DataContextScopeOption.cs), [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`OnImplementation` | Invocation of a named `XxxxxOnImplementaionAsync` method that must be implemented in a non-generated partial class.

The following demonstrates the usage (a snippet from the sample [`PersonData`](../samples/Demo/Beef.Demo.Business/Data/Generated/PersonData.cs)):

``` csharp
/// <summary>
/// Gets the <see cref="PersonDetail"/> collection object that matches the selection criteria.
/// </summary>
/// <param name="args">The Args (see <see cref="PersonArgs"/>).</param>
/// <param name="paging">The <see cref="PagingArgs"/>.</param>
/// <returns>A <see cref="PersonDetailCollectionResult"/>.</returns>
public Task<PersonDetailCollectionResult> GetDetailByArgsAsync(PersonArgs args, PagingArgs paging)
{
    return DataInvoker.Default.InvokeAsync(this, () => GetDetailByArgsOnImplementationAsync(args, paging),
        new BusinessInvokerArgs { ExceptionHandler = _getDetailByArgsOnException });
}
```