# Change log

Represents the **NuGet** versions.

## v2.1.5
  - *Fixed:* The `CosmosDbMapper` not correctly instantiates the `ChangeLog` mappings within `AddStandardProperties` where the underlying types are different.

## v2.1.4
- *Enhancement:* A number of refactorings, including the introduction of `CosmosDbMapper`, and `CosmosDbValueContainer` option for a `CosmosDbValue`. 

## v2.1.3
- *Enhancement:* A number of refactorings, including the introduction of `CosmosDbContainer` to formalize the Entity _to_ Container relationship. 

## v2.1.2
- *Added:* New `CosmosDbTypeValue` added so that the underlying `Type` is also persisted to Cosmos; for example, all reference data entities can exist within the same container and can be queried by `Type`.
- *Added:* New `Container` extensions `ImportBatchAsync` and `ImportRefDataBatchAsync` to support the initial loading of data. Note: these result in single `CreateItemAsync` operation per item, and are non-transactional.

## v2.1.1
- *New:* Initial publish to GitHub. New capability to support CRUD-style activities to a *Cosmos* DB / DocumentDB repository. Built using similar pattern as provided for *Database*, *EntityFramework* and *OData* - this allows for similar code generation output/approach and run-time exectuion.