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

## Operation arguments

The [`CosmosDbArgs`](./CosmosDbArgs.cs) provides the required [`Container`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container) operation arguments:

Property | Description
-|-
`ContainerId` | The Cosmos `Container` identifier.
`PartitionKey` | The [PartitionKey](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.partitionkey) (defaults to `PartitionKey.None`).
`Paging` | The [paging](../Beef.Core/Entities/PagingResult.cs) configuration (used by **query** operation only).
`ItemRequestOptions` | The [`ItemRequestOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.itemrequestoptions) used for `Get`, `Create`, `Update` and `Delete`.
`QueryRequestOptions` | The [`QueryRequestOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.queryrequestoptions) used for `Query` only.
`NullOnNotFoundResponse` | Indicates that a `null` is to be returned where the *response* has an `HttpStatusCode.NotFound` on a `Get`.
`SetIdentifierOnCreate` | Indicates whether to the set (override) the identifier on `Create` where the entity implements [`IIdentifer`](../Beef.Core/Entities/IIdentifier.cs).

The following demonstrates the usage:

``` csharp
var args1 = CosmosDbArgs<Person>.Create("Persons");
var args2 = CosmosDbArgs<Person>.Create("Persons", paging);
```

<br/>

## CRUD

The primary data persistence activities are CRUD (Create, Read, Update and Delete) related; the [`CosmosDbBase`](./CosmosDbBase.cs) enables the following capabilities for a specified Cosmos [`Container`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container):

Operation | Description
-|-
`GetAsync` | Gets the entity for the specified key where found; otherwise, `null` (default) or [`NotFoundException`](../Beef.Core/NotFoundException.cs) depending on the corresponding [`CosmosDbArgs.NullOnNotFoundResponse`](./CosmosDbArgs.cs).
`CreateAsync` | Creates the entity. Automatically updates the `Created*` fields of [`IChangeLog`](../Beef.Core/Entities/IChangeLog.cs) where implemented. Where the entity implements either [`IGuidIdentifier`](../Beef.Core/Entities/IIdentifier.cs) or [`IStringIdentifier`](../Beef.Core/Entities/IIdentifier.cs) and the corresponding [`CosmosDbArgs.SetIdentifierOnCreate`](./CosmosDbArgs.cs) is `true` (default), then the value will be set to `Guid.NewGuid` (overridding any prior value).
`UpdateAsync` | Updates the entity. Automatically updates the `Updated*` fields of [`IChangeLog`](../Beef.Core/Entities/IChangeLog.cs) where implemented; also ensuring that the existing `Created*` fields are not changed.
`DeleteAsync` | Deletes the entity. Given a delete is idempotent it will be successful even where the entity does not exist.

Additional information:
- Each of the operations must be provided a [`CosmosDbArgs`](./CosmosDbArgs.cs) that provides additional context for the operation. At a minimum the `ContainerId` must be provided.
- Each of the operations support the entity being of type [`CosmosDbTypeValue`](./CosmosDbTypeValue.cs); this will persist both the .NET [`Type.Name`](https://docs.microsoft.com/en-us/dotnet/api/system.type.name) and underlying `Value`. This enables values with multiple types to be persisted in a single containger; for example, reference data.
- Where the entity implements [`IETag`](../Beef.Core/Entities/IEtag.cs) then the `UpdateAsync` will be performed with an `If-Match` header; and a corresponding [`ConcurrencyException`](../Beef.Core/ConcurrencyException.cs) will be thrown where it does not match. **Note**: for the `ETag` to function correctly the JSON name on the entity must be `_etag`.
- Where uniqueness has been defined for the `Container` and a create or update results in a duplicate a [`DuplicateException`](../Beef.Core/DuplicateException.cs) will be thrown.
<br/>

## Query

More advanced query operations are enabled via by the `CosmosDbBase.Query` which will return a [`CosmosDbQuery`](./CosmosDbQuery.cs) which further extends on the LINQ capabilities provided by the `Container`. This supports an overload where a query `Func` can be added to simplify the likes of filtering, etc. where needed:

Operation | Description
-|-
`AsQueryable` | Gets a prepared `IQueryable` with any `CosmosDbTypeValue` filtering as applicable. <br/> **Note**: for this reason this is the recommended approach for all ad-hoc queries. <br/> **Note**: [`CosmosDbArgs.Paging`](./CosmosDbArgs.cs) is not supported and must be applied using the provided `IQueryable.Paging`.
`SelectFirst` | Selects the first item.**<sup>*</sup>** 
`SelectFirstOrDefault` | Selects the first item or default.**<sup>*</sup>**
`SelectSingle` | Selects a single item.**<sup>*</sup>**
`SelectSingleOrDefault` | Selects a single item or default.**<sup>*</sup>**
`SelectQuery` | Select multiple items and either creates, or updates an existing, collection. Where the corresponding [`CosmosDbArgs.Paging`](./CosmosDbArgs.cs) is provided the configured paging, and optional get count, will be enacted.
`SelectValueQuery` | Select multiple items as per the `SelectQuery`; however, as the data was persisted using [`CosmosDbTypeValue`](./CosmosDbTypeValue.cs) this will ensure only the `Value` will be selected into the resulting collection.

**<sup>*</sup>** These are provided for use versus than the default `IQueryable` equivalents (which are currently not supported) as they will only internally page one or two items accordingly to minimise query and data costs. Paging cannot be applied more than once as it will result in a invalid sub-query.

<br/>

## Container-based

A [`CosmosDbContainer`](./CosmosDbContainer.cs) enables all of the previously decribed _CRUD_ and _Query_ access for a configured container; versus, having to specify the container identifier per operation.

Examples as follows:

``` csharp
public class CosmosDb : CosmosDbBase
{
    public CosmosDb() : base(new Microsoft.Azure.Cosmos("https://localhost:8081", "C2=="), "Beef.UnitTest", true)
    {
        Persons = new CosmosDbContainer<Person>(this, CosmosDbArgs<Person>.Create("Persons"));
    }

    public CosmosDbContainer<Person> Persons { get; private set; }
}

...

var db = new CosmosDb();
var v = await db.Persons.GetAsync(Guid.NewGuid());
```

<br/>

## Local emulator

Given there are costs associated with using Cosmos DB, consider using the local emulator for development and testing purposes: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator