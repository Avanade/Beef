# Data (Data access)

The _Data_ layer is primarily responsible for performing the persistence CRUD (`Create`, `Read`, `Update` or `Delete`) and `Query` requests, handling the underlying wire transport, protocols and payload to enable. 

This is the area in which a developer using _Beef_ is likely to spend most of their time. The basic CRUD capabilities are enabled mostly out-of-the-box; however, the data access logic complexity will likely exceed this as the complexity of the requirements increase.

<br/>

## Supported

There are _[CoreEx](https://github.com/Avanade/CoreEx)_ capabilities to encapsulate the data access in a largely consistent manner; in that they largely support the following similar patterns for CRUD and Query (including [paging](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/PagingArgs.cs)).

The following **data access** capabilities are currently supported; as well as being integrated into the _Beef_ code-generation:

Assembly | Description
-|-
[`CoreEx.Database`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database) | ADO.NET using the likes of Stored Procedures and inline SQL. 
[`CoreEx.EntityFrameworkCore`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.EntityFrameworkCore) | Entity Framework (EF) Core framework. 
[`CoreEx.Cosmos`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Cosmos) | Cosmos DB execution framework. 
[`CoreEx.Http`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/Extended/TypedMappedHttpClientBase.cs) | HTTP endpoint invocation. 

This obviously does not prohibit access to other data sources; just that these will need to be implemented in a fully [custom](#Custom) manner.

<br>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, and has extension opportunities to inject additional logic into the processing pipeline.

The [`Operation`](./Entity-Operation-Config.md) element within the `entity.beef-5.yaml` configuration primarily drives the output

There is a generated class per [`Entity`](./Entity-Entity-Config.md) named `{Entity}Data`. There is also a corresonding interface named `I{Entity}Data` generated so the likes of test mocking etc. can be employed. For example, if the entity is named `Person`, there will be corresponding `PersonData` and `IPersonData` classes.

<br/>

## Railway-oriented programming

_CoreEx_ version `3.0.0` introduced [monadic](https://en.wikipedia.org/wiki/Monad_(functional_programming)) error-handling, often referred to as [Railway-oriented programming](https://swlaschin.gitbooks.io/fsharpforfunandprofit/content/posts/recipe-part2.html). This is enabled via the key types of `Result` and `Result<T>`; please review the corresponding [documentation](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Results/README.md) for more detail on purpose and usage. 

The [`Result`]() and [`Result<T>`]() have been integrated into the code-generated output and is leveraged within the underlying validation. This is intended to simplify success and failure tracking, avoiding the need, and performance cost, in throwing resulting exceptions. 

This is implemented by default; however, can be disabled by setting the `useResult` attribute to `false` within the code-generation configuration.

<br/>

### Code-generated

An end-to-end code-generated processing pipeline generally consists of:

Step | Description
-|-
`DataInvoker` | The logic is wrapped by a [`DataInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/DataInvoker.cs). This enables the [`InvokerArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerArgs.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`OnBefore`&dagger; | The `OnBefore` extension opportunity; where set this will be invoked. This enables logic to be invoked _before_ the primary `Operation` is performed.
`Operation` | The actual operation execution (`Get`, `Create`, `Update`, `Delete` and `Query`). This will include an `OnQuery` extension opportunity where performing a `Query`.
`OnAfter`&dagger; | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.
`OnException`&dagger; | The `OnException` extension opportunity; where set this will be invoked to handle any unhandled exceptions.

_&dagger; Note:_ To minimize the generated code the extension opportunities are only generated where selected. This is performed by setting the `dataExtensions` attribute to `true` within the [`Entity`](./Entity-Entity-Config.md) code-generation configuration.

The following demonstrates the generated code (a snippet from the sample [`PersonData`](../samples/Demo/Beef.Demo.Business/Data/Generated/ContactData.cs)) that invokes a stored procedure that does not include `DataExtensions`:

``` csharp
public Task<Result<Person?> GetAsync(Guid id)
{
    return _db.StoredProcedure("[Demo].[spPersonGet]").GetAsync(DbMapper.Default, id);
}
```

The following demonstrates the generated code (a snippet from the sample [`EmployeeData`](../samples/My.Hr/My.Hr.Business/Data/Generated/EmployeeData.cs)) that leverages Entity Framework that does not include `DataExtensions`:

``` csharp
public Task<EmployeeBaseCollectionResult> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging)
{
    return _ef.Query<EmployeeBase, EfModel.Employee>(q => _getByArgsOnQuery?.Invoke(q, args) ?? q).WithPaging(paging).SelectResultAsync<EmployeeBaseCollectionResult, EmployeeBaseCollection>();
}
``` 

The following demonstrates the generated code (a snippet from the sample [`PersonData`](../samples/Demo/Beef.Demo.Business/Data/Generated/PersonData.cs)) that includes `DataExtensions`:

``` csharp
public Task<Person?> GetWithEfAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async _ => 
{
    await Invoker.InvokeAsync(_getWithEfOnBeforeAsync?.Invoke(id)).ConfigureAwait(false);
    var __result = await _ef.GetAsync<Person, EfModel.Person>(id).ConfigureAwait(false);
    await Invoker.InvokeAsync(_getWithEfOnAfterAsync?.Invoke(__result, id)).ConfigureAwait(false);
    return __result;
}, new InvokerArgs { ExceptionHandler = _getWithEfOnException });
```

<br/>

### Custom

A custom (`OnImplementation`) processing pipeline generally consists of:

Step | Description
-|-
`DataInvoker` | The logic is wrapped by a [`DataInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/DataInvoker.cs). This enables the [`InvokerArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerArgs.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`OnImplementation` | Invocation of a named `XxxOnImplementaionAsync` method that must be implemented in a non-generated partial class.

The following demonstrates the usage (a snippet from the sample [`PersonData`](../samples/Demo/Beef.Demo.Business/Data/Generated/PersonData.cs)):

``` csharp
public Task<PersonDetailCollectionResult> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging) => GetDetailByArgsOnImplementationAsync(args, paging);
```