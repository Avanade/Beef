# Change log

Represents the **NuGet** versions.

## v4.2.2
- *Enhancement:* Add support for using [AutoMapper](https://docs.automapper.org/en/stable/index.html) for the entity-to-entity based mapping (remove existing `EntityMapper`-based functionality) - may result in _breaking changes_:
  - `CosmosDbArgs<T, TModel>` renamed to `CosmosDbArgs` and updated to support the new AutoMapper requirements.
  - `ICosmosDbArgs` removed to simplify.
  - `CosmosDbBase` and `CosmosDbQuery` updated to support AutoMapper mappings.
  - `CosmosMapper` was deleted.
  - _Note:_ All code-generated artefacts must be re-generated.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.5
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.4
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.3
- *Removed:* The `ICosmosDbArgs.SetIdentifierOnCreate` has been deprecated (removed). The new `IIdentifierGenerator` capability should be leveraged as a more flexible alternative.

## v4.1.2
- *Fixed:* A `NotFoundException` will be thrown on a delete if it does not exist; otherwise, the application will assume it deleted successfully and the likes of a related event could be raised incorrectly.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.x
- *Enhancement:* `CosmosDbMapper.CreateArgs` method extended to support an optional `action` to further update the `CosmosDbArgs` (helps simplify the generated code logic).

## v3.1.4
- *Enhancement:* All references to `DateTime.Now` have been updated to `Cleaner.Clean(DateTime.Now)`.

## v3.1.3
- *Fixed:* A new nullable compile error fixed that appeared with Visual Studio 2019 v16.5.4.

## v3.1.2
- *Enhancement:* Add option to set identifier automatically when performing a `ImportBatchAsync` or `ImportValueBatchAsync`.
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Core 3.1.
- *Added:* Nullable rollout phase: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/

## v2.1.10
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.9
- *Fixed:* Introduced FxCop Analysis to `Beef.Data.Cosmos`; this version represents the remediation based on the results.

## v2.1.8
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.

## v2.1.7
- *Added:* The `CosmosDbArgs` supports a new `SetAuthorizedFilter` that allows a filter to be specified to remove any records that should not be accessed. For a query operation it will automatically add to the `IQueryable`. For the other operations it will be used before enabling access and result in an `AuthorizationException`.

## v2.1.6
- *Added:* The `ImportValueRefDataBatchAsync` has been updated to use the new `IReferenceDataProvider`.

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
- *New:* Initial publish to GitHub. New capability to support CRUD-style activities to a *Cosmos* DB / DocumentDB repository. Built using similar pattern as provided for *Database*, *EntityFramework* and *OData* - this allows for similar code generation output/approach and runtime exectuion.