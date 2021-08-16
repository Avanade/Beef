# Beef.Data.OData

[![NuGet version](https://badge.fury.io/nu/Beef.Data.OData.svg)](https://badge.fury.io/nu/Beef.Data.OData)

Adds additional capabilities extending	[Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client/) that standardises and simplifies usage of OData endpoints for _Beef_.

</br>

## Client

[Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client/) is being used as the OData client as it is lightweight and completely decoupled from the underlying OData endpoint. It has a simple, albeit rich, capability that fits nicely into the approach and philosophy of _Beef_'s data source adapters.

To encapsulate the OData access the [OData](./ODataBase.cs) or [ODataBase](./ODataBase.cs) is inherited to enable. Additional capabilities are added by _Beef_ to simplify usage, and provide a similar experience to the other data source adapters.

The following demonstrates the usage:

``` charp
public class TestOData : OData<TestOData>
{
    public TestOData(Uri baseUri) : base(baseUri) { }
}
```

<br/>

### Considered, but rejected

Microsoft's [OData Connected Service](https://docs.microsoft.com/en-us/odata/client/getting-started) was considered as this certainly adds a level of acceleration by providing an early-bound, strongly-typed, client that is code-generated from the OData metadata endpoint. The two issues that were cause for **exclusion** related to:

a) It is all or nothing, in that all entities, properties, operations, etc. are generated from the `$metadata` even where only a subset is required. This is an issue where there any many 100s/1000s as is the case with the likes of Microsoft Dynamics 365. There is a PBI in their backlog that states they are considering the addition of entity selection in the future.

b) Where only interested in a subset of the properties for an entity there is no simple way to only reference those, more specifically for create and update (patch) where there is a requirement for all properties to be provided.

This may be added, as an alternative in the future, where there is demand.

<br/>

## Mapping

Mapping between the .NET entity to/from a cosmos-oriented .NET model (these can be the same) is managed using [AutoMapper](https://automapper.org/).

<br/>

## Operation arguments

The [`ODataArgs`](./ODataArgs.cs) provides the required `Collection` operation arguments.

Property | Description
-|-
`Mapper` | The _AutoMapper_ `IMapper` instance to be used to perform the source to/from destination model mapping.
`CollectionName` | The OData `Collection` name; where `null` will infer from the model `Type` name.
`Paging` | The [paging](../Beef.Abstractions/Entities/PagingResult.cs) configuration (used by `Query` operation only).
`NullOnNotFoundResponse` | Indicates that a `null` is to be returned where the *response* has an `HttpStatusCode.NotFound` on a `Get`.

The following demonstrates the usage:

``` csharp
var args1 = ODataArgs.Create(_mapper, "Persons");
var args2 = ODataArgs.Create(_mapper, paging, "Persons");
```

<br/>

## CRUD

The primary data persistence activities are CRUD (Create, Read, Update and Delete) related; [`ODataBase`](./ODataBase.cs) enables:

Operation | Description
-|-
`GetAsync` | Gets the entity for the specified key where found; otherwise, `null` (default) or [`NotFoundException`](../Beef.Abstractions/NotFoundException.cs) depending on the corresponding [`ODataArgs.NullOnNotFoundResponse`](./ODataArgs.cs).
`CreateAsync` | Creates the entity.
`UpdateAsync` | Updates the entity.
`DeleteAsync` | Deletes the entity. Given a delete is idempotent it will be successful even where the entity does not exist.

<br/>

## Query

More advanced query operations are enabled via by the [`ODataQuery`](./ODataQuery.cs) which further extends on the LINQ-like capabilities provided by the [Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client/). This supports an overload where a query `Func` can be added to simplify the likes of filtering, etc. where needed:

Operation | Description
-|-
`SelectFirst` | Selects the first item.**<sup>*</sup>** 
`SelectFirstOrDefault` | Selects the first item or default.**<sup>*</sup>**
`SelectSingle` | Selects a single item.**<sup>*</sup>**
`SelectSingleOrDefault` | Selects a single item or default.**<sup>*</sup>**
`SelectQuery` | Select multiple items and either creates, or updates an existing, collection. Where the corresponding [`ODataArgs.Paging`](./ODataArgs.cs) is provided the configured paging, and optional get count, will be enacted.

**<sup>*</sup>** These are provided for use versus than the default `IQueryable` equivalents as they will only internally page one or two items accordingly to minimise query and data costs.

</br> 

### Filtering

The [`IBoundClient`](https://github.com/simple-odata-client/Simple.OData.Client/blob/master/src/Simple.OData.Client.Core/Fluent/IBoundClient.cs) provides LINQ-like filtering (although `Filter` is used instead of `Where`); _Beef_ extends the capabilities as follows:

Operation | Description
-|-
`FilterWhen` | Filters a sequence of values based only where a _predicate_ results in `true`.
`FilterWildcard` | Filters a sequence of values using the specified `property` and `text` containing the supported wildcards.