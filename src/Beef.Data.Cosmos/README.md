# Beef.Data.Cosmos

[![NuGet version](https://badge.fury.io/nu/Beef.Data.Cosmos.svg)](https://badge.fury.io/nu/Beef.Data.Cosmos)

Adds additional capabilities extending	[`Microsoft.Azure.Cosmos`](https://github.com/Azure/azure-cosmos-dotnet-v3) that standardise and simply usage of the Cosmos [`Database`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.database) for _Beef_.

<br/>

## Database

To encapsulate the Cosmos database access the [CosmosDb](./CosmosDbBase.cs) or [CosmosDbBase](./CosmosDbBase.cs) is inherited to enable.

The following demonstrates the usage:

``` csharp
public class MyCosmosDb : CosmosDb<MyCosmosDb>
{
    public MyCosmosDb() : base(new Microsoft.Azure.Cosmos("https://localhost:8081", "C2=="), "Beef.UnitTest", true)
    { }
}
```

</br>

## Mapping

Mapping between the .NET entity to/from a cosmos-oriented .NET model (these can be the same) is managed using [AutoMapper](https://automapper.org/).

</br>

## Operation arguments

The [`CosmosDbArgs`](./CosmosDbArgs.cs) provides the required [`Container`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container) operation arguments.

Property | Description
-|-
`Mapper` | The _AutoMapper_ `IMapper` instance to be used to perform the source to/from destination model mapping.
`ContainerId` | The Cosmos `Container` identifier.
`PartitionKey` | The [PartitionKey](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.partitionkey) (defaults to `PartitionKey.None`).
`Paging` | The [paging](../Beef.Abstractions/Entities/PagingResult.cs) configuration (used by `Query` operation only).
`ItemRequestOptions` | The [`ItemRequestOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.itemrequestoptions) used for `Get`, `Create`, `Update` and `Delete`.
`QueryRequestOptions` | The [`QueryRequestOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.queryrequestoptions) used for `Query` only.
`NullOnNotFoundResponse` | Indicates that a `null` is to be returned where the *response* has an `HttpStatusCode.NotFound` on a `Get`.
`SetAuthorizedFilter` | Sets the filter (`IQueryable`) for all operations to ensure consistent authorisation is applied. Applies automatically to all queries, in that the filter is applied each time a `CosmosDbQuery` or `CosmosDbValueQuery` is executed. Additionally, the filter is applied to the standard `Get`, `Create`, `Update` and `Delete` (CRUD) operations to ensure only authorised data is accessed and modified.

The following demonstrates the usage:

``` csharp
var args1 = CosmosDbArgs.Create(_mapping, "Persons");
var args2 = CosmosDbArgs.Create(_mapping, "Persons", paging);
```

<br/>

## Container-based

A [`CosmosDbContainer`](./CosmosDbContainer.cs) (and [`CosmosDbValueContainer`](./CosmosDbValueContainer.cs) for [`CosmosDbValue`](./CosmosDbValue.cs)) enables all of the _CRUD_ and _Query_ access for a configured Cosmos [`Container`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container); versus, having to specify the container identifier per operation.

Examples as follows:

``` csharp
public class CosmosDb : CosmosDbBase
{
    public CosmosDb() : base(new Microsoft.Azure.Cosmos("https://localhost:8081", "C2=="), "Beef.UnitTest", true)
    {
        Persons = new CosmosDbContainer<Person, Person>(this, CosmosMapper.Default.CreateArgs("Persons"));
    }

    public CosmosDbContainer<Person, Person> Persons { get; private set; }
}

...

var db = new CosmosDb();
var v = await db.Persons.GetAsync(Guid.NewGuid());
```

<br/>

## CRUD

The primary data persistence activities are CRUD (Create, Read, Update and Delete) related; [`CosmosDbContainer`](./CosmosDbContainer.cs) (and [`CosmosDbValueContainer`](./CosmosDbValueContainer.cs) for [`CosmosDbValue`](./CosmosDbValue.cs)) enable:

Operation | Description
-|-
`GetAsync` | Gets the entity for the specified key where found; otherwise, `null` (default) or [`NotFoundException`](../Beef.Abstractions/NotFoundException.cs) depending on the corresponding [`CosmosDbArgs.NullOnNotFoundResponse`](./CosmosDbArgs.cs).
`CreateAsync` | Creates the entity. Automatically updates the `Created*` fields of [`IChangeLog`](../Beef.Abstractions/Entities/IChangeLog.cs) where implemented. Where the  the corresponding [`CosmosDbArgs.SetIdentifierOnCreate`](./CosmosDbArgs.cs) is `true` (default), then the Cosmos `Id` will be set to `Guid.NewGuid` (overriding any prior value).
`UpdateAsync` | Updates the entity. Automatically updates the `Updated*` fields of [`IChangeLog`](../Beef.Abstractions/Entities/IChangeLog.cs) where implemented; also ensuring that the existing `Created*` fields are not changed.
`DeleteAsync` | Deletes the entity. Given a delete is idempotent it will be successful even where the entity does not exist.

Additional information:
- Where the entity implements [`IETag`](../Beef.Abstractions/Entities/IEtag.cs) then the `UpdateAsync` will be performed with an `If-Match` header; and a corresponding [`ConcurrencyException`](../Beef.Abstractions/ConcurrencyException.cs) will be thrown where it does not match. **Note**: for the `ETag` to function correctly the JSON name on the model must be `_etag`.
- Where uniqueness has been defined for the `Container` and a create or update results in a duplicate a [`DuplicateException`](../Beef.Abstractions/DuplicateException.cs) will be thrown.

<br/>

## Query

More advanced query operations are enabled via by the [`CosmosDbQuery`](./CosmosDbQuery.cs) (and [`CosmosDbValueQuery`](./CosmosDbValueQuery.cs) for [`CosmosDbValue`](./CosmosDbValue.cs)) which further extends on the LINQ capabilities provided by the `Container`. This supports an overload where a query `Func` can be added to simplify the likes of filtering, etc. where needed:

Operation | Description
-|-
`AsQueryable` | Gets a prepared `IQueryable` (with any `CosmosDbValue.Type` filtering as applicable). <br/> **Note**: for this reason this is the recommended approach for all ad-hoc queries. <br/> **Note**: [`CosmosDbArgs.Paging`](./CosmosDbArgs.cs) is not supported and must be applied using the provided `IQueryable.Paging`.
`SelectFirst` | Selects the first item.**<sup>*</sup>** 
`SelectFirstOrDefault` | Selects the first item or default.**<sup>*</sup>**
`SelectSingle` | Selects a single item.**<sup>*</sup>**
`SelectSingleOrDefault` | Selects a single item or default.**<sup>*</sup>**
`SelectQuery` | Select multiple items and either creates, or updates an existing, collection. Where the corresponding [`CosmosDbArgs.Paging`](./CosmosDbArgs.cs) is provided the configured paging, and optional get count, will be enacted.

**<sup>*</sup>** These are provided for use versus than the default `IQueryable` equivalents (which are currently not supported) as they will only internally page one or two items accordingly to minimise query and data costs. Paging cannot be applied more than once as it will result in a invalid sub-query.

<br/>

## Cosmos specific model

Where the _Entity_ does not naturally map to a Cosmos _Model_ a couple of options are provided:
- Inherit _Model_ from [`CosmosDbModelBase`](./CosmosDbModelBase.cs); this provides the basic `Id`, `_etag` and [`ttl`](https://docs.microsoft.com/en-us/azure/cosmos-db/time-to-live).
- Use the `CosmosDbValueContainer` that in turn leverages the [`CosmosDbValue`](./CosmosDbValue.cs) for persisting the _Model_ `Value`. This inherits from `CosmosDbModelBase`, and extends by adding a `Type` (enables values with multiple types to be persisted in a single containger; for example, reference data), and the `Value` itself.

<br/>

## Row-level Authorisation

Additional row-level like authorisation can be applied to all CRUD and Query operations. The [`CosmosDbArgs.SetAuthorizedFilter`](./CosmosDbArgs.cs) (described [here](#Operation-Arguments)) defines the authorization filter. This should be set before any operation is performed. The _beef_ code-generation provides a `_onDataArgsCreate` method that can be set to perform; this will be invoked each time a `CosmosDbArgs` is instantiated.

Example as follows. This demonstrates filtering a `Content` entity by allowable `ContentType` that has been added to the `ExecutionContext` set when the user is configured at startup:

``` csharp
_onDataArgsCreate = OnDataArgsCreate;

...

private void OnDataArgsCreate(ICosmosDbArgs dbArgs)
{
    dbArgs.SetAuthorizedFilter((q) => ((IQueryable<CosmosDbValue<Content>>)q).Where(c => ExecutionContext.Current.ContentType.Contains(c.Value.ContentType)));
}
```

<br/>

## Local emulator

Given there are costs associated with using Cosmos DB, consider using the local emulator for development and testing purposes: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator