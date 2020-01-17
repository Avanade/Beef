# Change log

Represents the **NuGet** versions.

## v2.1.27
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/. Templates also updated to ensure code-generated output complies. 

## v2.1.26
- *Fixed:* `ISNULL` for `IsDeleted` in OrgUnit permission check for Get and Update stored procedures.

## v2.1.25
- *Fixed:* Reference Data Controller code-gen now uses `StringComparison.InvariantCultureIgnoreCase` for the string comparison.
- *Fixed:* Entity Framework model code-gen uses property expressions versus property names as strings. 
- *Fixed:* Introduced FxCop Analysis to `Beef.CodeGen.Core`; this version represents the remediation based on the results.

## v2.1.24
- *Fixed:* Manager code-gen output fixed where `OperationType="GetColl"` and `ManagerCustom="true"`; a comma is now placed between the parameters correctly.

## v2.1.23
- *Fixed:* Entity code-gen updated to override AcceptChanges and TrackChanges to support change tracking through the entity object graph. There are required changes within `Beef.Core` to enable.

## v2.1.22
- *Added:* Code-gen of the data access for `Cosmos` adds a new method `_onDataArgsCreate` that is invoked each time a `CosmosDbArgs` is created.

## v2.1.21
- *Added:* Code-gen attribute `RefDataText=true|false` has been added to `Config`, `Entity` and `Property` elements. Where set to true for a reference data value a corresponding property `xxxText` will be created. This will only be populated during serialization when `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true`.
- *Fixed:* Optimised the reference data `Controller` code-gen including corresponding `ETag` value.

## v2.1.20
- *Fixed:* Incorrect end-point generated for Reference Data Service Agent.

## v2.1.19
- *Fixed:* Consolidated the `/ref` and `/ref/codes` endpoints into `/ref`. Supports list of names as per previous, as well as the new specified entity+code, within the query string.

## v2.1.18
- *Added:* Reference data updated to support multiple run-time providers, versus the previous single only. A new `IReferenceDataProvider` enables a provider to be created. The underlying code-gen templates have been updated to enable.
- *Fixed:* Code-gen for the database where `IsDeleted` is used will perform `ISNULL(IsDeleted, 0) = 0` to check for null or zero as not-deleted.

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