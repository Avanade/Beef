# Change log

Represents the **NuGet** versions.

## v2.1.17
- *Fixed:* Optimisations made to the entity code generation for reference data so that internal operations use the property serialization identifier (SID); otherwise, was resulting in unecessary reference data loads.

## v2.1.16
- *Fixed:* Database merge statements updated to include `AND EXISTS (...)` for a `WHEN MATCHED` to avoid updates where column data has not changed.

## v2.1.15
- *Fixed:* Code-gen `Entity.DataCosmosMapperInheritsFrom` not generating correctly.

## v2.1.14
- *Fixed:* Code-gen `Entity.DataCosmosValueContainer` not generating correctly. Added support for `Operation.DataCosmosValueContainer` to override.
- *Fixed:* Code-gen for the private `Data` variables are now `readonly` as they are only intended for update within the constructor. Will remove corresponding compiler warnings.

## v2.1.13
- *Fixed:* Code-gen of `Entity.cs` outputs incorrect `using` statement when `EntityScope="Business"` is used.

## v2.1.12
- *Added:* New `DataModel` code generation support has been added to enable the specification and generation of back-end only data model entities.

## v2.1.11
- *Fixed:* The `Entity.DatabaseName`, `Entity.EntityFrameworkName`, `Entity.CosmosName` are now being honoured when generating for reference data.
- *Enhancement:* A new `Entity.OmitEntityBase` attribute is now supported in the code-generation to omit the output of the `EntityBase` inherited capabilities.

## v2.1.10
- *Enhancment:* Cosmos code-generation enhancements to support changes to `CosmosDb` implementation.

## v2.1.9
- *Enhancment:* Additional code-generation enhancements to support the auto-implements of Cosmos DB data access.

## v2.1.8
- *Enhancement:* An invocation with an `If-Match` will override the value where it implements `IEtag` as this should take precedence over the value inside of the value itself via `WebApiActionBase.Value`. Code-gen has updated to take advantage of this; next gen will introduce usage within `XxxApiController` classes.

## v2.1.7
- *Enhancement:* `ReferenceDataData_cs.xml` template updated to support `AutoImplement="EntityFramework"` to simplify the loading of reference data items where using Entity Framework. 

## v2.1.6
- *Enhancement:* Added `WithHints` to stored procedure configuration to output `WITH(value)` table [hint](https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table).
- *Enhancement:* Added support to generate data logic access using *Cosmos* DB. This follows the same pattern as *Database*, *OData* and *EntityFramework*. 
- *Fixed:* `IEntityData` code-gen did not correctly output the value type.
- *Enhancement:* Added code-gen support for snake_case and kebab-case.

## v2.1.5
- *Fixed:* `InvokerBase` was non functioning as a generic class; reimplemented. Invoker invocation code generation updated accordingly.

## v2.1.4
- *Fixed:* FromBody not applied to `ServiceAgent` code generation.

## v2.1.3
- *Fixed:* `CodeGenConsoleWrapper` was supporting database generation by default.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.