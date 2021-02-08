# Change log

Represents the **NuGet** versions.

## v4.1.15
- *Fixed:* Issue [100](https://github.com/Avanade/Beef/issues/100) fixed. Corrected CDC codegen to no longer output errant code where there is a single left outer join specified.

## v4.1.14
- *Fixed:* The XML schema was being generated with the incorrect `targetNamespace` which has been corrected. Schema order has been corrected to allow any. Missing `List<string>` properties are now included.
- *Fixed:* The `CdcTracking` table `Id` column changed from `INT` to `UNIQUEIDENTIFIER` as the preferred identifier data type.

## v4.1.13
- *Enhancement:* The last stage of the custom code-gen capability retirement; now completely replaced by [`Handlebars.Net`](https://github.com/rexm/Handlebars.Net) as the code-generation engine. The _database_ related code-generation has been ported. The existing engine has been deprecated (removed).
- *Enhancement:* The _Entity_ and _Database_ related XML schemas are now generated from the internal configuration model; these have been re-gen'd.
- *Enhancement:* New [`database.beef.json`](./Schema/database.beef.json) and [`entity.beef.json`](./Schema/entity.beef.json) JSON schemas are now also generated from the internal configuration model to support alternate JSON and YAML configurations - to be introduced in a later release. These will be added to the [JSON Schema Store](https://www.schemastore.org/json/) to enable editor support (validation and intellisense) prior to introduction proper.
- *Enhancement:* To simplify testing of _Agents_ with the recent introduction of dependency injection (DI) a new code-gen global setting `AppBasedAgentArgs="true"` has been added. This will result in a new `{{Company}}.{{AppName}}.Common.Agents.I{{AppName}}WebApiAgentArgs` and `{{Company}}.{{AppName}}.Common.Agents.{{AppName}}WebApiAgentArgs` classes created; all _Agents_ will then use this for the constructor versus the default `IWebApiAgentArgs`. These new generated artefacts can then be addressed directly to simplify DI for both general usage and testing.
- *Fixed:* A return type of `string` will always be treated as nullable. This is because the `Cleaner.Clean` (based on settings) could override the return value with `null`.
- *Enhancement:* The code-gen `Scripts` XML files support a new `Inherits` attribute to enable references to one or more other `Scripts` files. This allows referencing to remove the need to duplicate setting across multiple files.
- *Enhancement:* The code-gen has been updated to enable [personalization of code-generation](./README.md#personalization-andor-overriding); to either override an existing template or execute entirely new templates; including the ability to access additive configuration.
- *Enhancement:* Added new configuration and templates to support Microsoft SQL Server [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server) (CDC) capability. Additionally, [`Beef.Data.Database.Cdc`](./../../src/Beef.Data.Database.Cdc/README.md) has been added to support; this documentation also describes its usage.
- *Enhancment:* To simplify the addition of additional constructor parameters for DI the following `Entity` configuration can be specified `DataCtorParams`, `DataSvcCtorParams`, `ManagerCtorParams` and `WebApiCtorParams`. This is a comma seperated list of additional (non-inferred) Dependency Injection (DI) parameters for the generated constructor. Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. "here the `Type` matches an already inferred value it will be ignored.
- *Enhancement*: Added new `Property.IdentifierGenerator` to specify the `Type` (via dependency injection) to generate the identifier value on `Create` (within `XxxManager`).
- *Enhancement*: Added new `CodeGeneration.EventTransaction` and `Entity.EventTransaction` as a shorthand to set the `Operation.DataSvcTransaction` whereever an event is being published; this will rollback any updates where the event publish fails (only functional where the data source supports `System.TransactionScope`). 
- *Enhancement*: Code-gen for all c# code will simply use `#pragma warning disable` with no list (disables all) versus specifiying list; minimizes need to keep updated as more warnings are potentially added.

## v4.1.12
- *Fixed:* Issue [93](https://github.com/Avanade/Beef/issues/93) fixed. The `XxxServiceCollectionExtensions.cs` classes were errantly being generated where there are no corresponding operations that would require.

## v4.1.11
- *Fixed:* Issue [91](https://github.com/Avanade/Beef/issues/91) fixed. `WebApiAuthorize` attribute code-gen output reverted back to pre-_Handlebars_ behaviour. Controller-level will only output where specified; will no longer default to `AllowAnonymous`. Method-level will only output where specified; will no longer default to parent (entity) value.

## v4.1.10
- *Fixed:* Issue [89](https://github.com/Avanade/Beef/issues/89) fixed. Event publish code was being incorrectly generated where the subject was being overridden; was missing the corresponding inferred `Action`.

## v4.1.9
- *Fixed:* Issue [87](https://github.com/Avanade/Beef/issues/87) fixed. Event publish code was being incorrectly generated where `EventPublish="false"`.

## v4.1.8
- *Fixed:* Issue [85](https://github.com/Avanade/Beef/issues/85) fixed. Additional challenge identified with HTML encoding for generated code output. The `TextEncoding` for _Handlebars_ is now set to `null` so no encoding should now occur. This will fix this particular issue and others that have not been explicitly formatted using `{{{xxx}}}`.

## v4.1.7
- *Fixed:* Issue [80](https://github.com/Avanade/Beef/issues/80) fixed. Was generating incomplete `GetHashCode()` where no properties specified.
- *Fixed:* Issue [81](https://github.com/Avanade/Beef/issues/81) fixed. Was generating incorrect (non-compiling) code where `DataSvcCaching="false"`. 

## v4.1.6
- *Fixed:* Issue [79](https://github.com/Avanade/Beef/issues/79) fixed. Integrated suggested Pull Request [78](https://github.com/Avanade/Beef/pull/78) with minor rollback of default logic. Removed the HTML escaping for the generated C# generic types.

## v4.1.5
- *Fixed:* Issue 74 also resolved for Reference Data code-generation.

## v4.1.4
- *Fixed*: Issue [74](https://github.com/Avanade/Beef/issues/74) fixed. There was an additional issue where the value was set to `false` it in turn resulted in `Authorize` versus `AllowAnonymous` which has been corrected.

## v4.1.3
- *Fixed*: Issue [74](https://github.com/Avanade/Beef/issues/74) fixed. `WebApiAuthorize` attribute was honouring previous override sideeffect. Changed attribute configuration from a `bool` to a `string`. Code-gen will output the supplied value as-is. XML boolean values are automatically converted. _Note:_ the XML Schema and corresponding documentation have not been updated; this will occur during a future release.

## v4.1.2
- *Enhancement:* The first stage of the custom code-gen capability retirement; to be replaced by [`Handlebars.Net`](https://github.com/rexm/Handlebars.Net) as the code-generation engine. All the _entity_ and _reference date_ related code-generation templates (and related) have been ported. The _database_ related code-generation is still using the legacy custom.
- *Enhancement:* The `CodeGen` namespace from `Beef.Core` has been moved relocated. A new `StringConversion` now provides access to the existing string conversion functions (e.g. `ToSentenceCase`).
- *Fixed:* Issue [71](https://github.com/Avanade/Beef/issues/71) has been resolved. A runtime error will now correctly result in a return code of `-1`.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.20
- *Fixed*: Issue [66](https://github.com/Avanade/Beef/issues/66) fixed. Changed the path separator to be `/` so that is compatible on Windows and Linux. By using `/` this matches the `Path.AltDirectorySeparatorChar`(https://docs.microsoft.com/en-us/dotnet/api/system.io.path.altdirectoryseparatorchar) for Windows and `Path.DirectorySeparatorChar`(https://docs.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar) for Linux, making it universally compatible.

## v3.1.19
- *Enhancment:* Request [62](https://github.com/Avanade/Beef/issues/62) applied. Generate a non-zero exit code when detecting updated files if none are expected. E.g. if executing the code generation as part of build pipeline.

## v3.1.18
- *Fixed:* Issue [63](https://github.com/Avanade/Beef/issues/63) fixed. The `OperationType="Get"` with no arguments will no longer result in errant code (non-compiling) being generated.

## v3.1.17
- *Fixed:* Issue [57](https://github.com/Avanade/Beef/issues/57) fixed. The redundant `?? null` for the property set on an entity with a reference data collection has been removed.
- *Fixed:* Tidied up the entity code-gen for the `Equals` method.

## v3.1.16
- *Enhancement:* Generate a new `ModelBuilderExtensions.AddGeneratedModels` to simplify the adding of the generated models to the `ModelBuilder` (Entity Framework).

## v3.1.15
- *Fixed*: Code-gen templates updated to correct additional warnings identified by FxCop. Where no direct fix, or by intention, these have been explicitly ignored.

## v3.1.14
- *Fixed*: Code-gen templates updated to correct warnings identified by FxCop. Where no direct fix, or by intention, these have been explicitly ignored.

## v3.1.13
- *Enhancement*: `PropertyConfigLoader` updated to default the `DateTimeTransform`, `StringTransform` and `StringTrim` enum selections to the new `UseDefault` value.

## v3.1.12
- *Enhancement:* Added `Config.EventSubjectRoot` attribute used by the `DataSvc` code generation that provides the root for the event name by prepending to all event subject names.

## v3.1.11
- *Enhancement:* The `DataSvc` code generation updated to use the new `Event` methods as existing have been obsoleted.
- *Enhancement:* Added `Property.WebApiQueryStringConverter` attribute to enable `Type` to `string` conversion for writing to and parsing from the query string.
- *Enhancement:* Shortcut added where the `Type` attribute for the `Property` element starts with `RefDataNamespace.` (e.g. `Type="RefDataNamespace.Gender"`) and the corresponding `RefDataType` attribute is not specified it will default to `string`.
- *Enhancement:* Shortcut added where the `DataConverter` attribute for the `Property` element ends with `{T}` (e.g. `DataConverter="ReferenceDataNullableGuidIdConverter{T}"`) and the corresponding `IsDataConverterGeneric` attribute is not specified the `{T}` will be removed and the `IsDataConverterGeneric` will default to `true`.
- *Enhancement:* Added `Operation.AuthRole` and `Entity.AuthRole` attributes to enable `ExecutionContext.IsInRole(role)` checking.

## v3.1.10
- *Fixed:* Fix to include the `Beef` namespace for the `ReferenceDataProvider.PrefetchAsync` capability.

## v3.1.9
- *Fixed:* Fix for `ReferenceDataProvider.PrefetchAsync` to leverage the new `ExecutionContext.FlowSuppression`.

## v3.1.8
- *Fixed:* Fix for `EntityData` code generation; `DataName` was not always being output where using `Database`.
- *Enhancement:* Added `Config.EventActionFormat` to control the formatting of the event action text.

## v3.1.7
- *Fixed:* Fix for `EntityDataSvc` code generation; internal caching was being accidently skipped for custom operation types.

## v3.1.6
- *Enhancement:* Added code-generation templates and configuration for gRPC support.

## v3.1.5
- *Fixed:* A null reference would occur where using a custom operation type and the resulting value is `null`. Code generation has been amended to support nullable return types (e.g. `Person?`) to allow.

## v3.1.4
- *Enhanced:* Added `IEquatable<T>` implementation to the entity code generation. Enables support for full property, sub entity and collection equality `Equals` checking and `GetHashCode` calculation.

## v3.1.3
- *Enhancement:* Code generation enhanced to support new approach to OData.
- *Enhancement:* New `Entity.JsonSerializer` attribute added to control the entity/property serializer used. Currently supports `None` or `Newtonsoft`.

## v3.1.2
- *Fixed:* `ReferenceDataData` code generation for Cosmos DB was generating invalid code where the entity had addtional properties which has been corrected.
- *Fixed:* `ReferenceDataServiceAgent` and `ReferenceDataAgent` code generation where a `RefDataNamespace` is defined.
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).
- *Enhancement:* Tool updated to execute asynchoronously. Both `CodeGenConsole` and `CodeGenConsoleWrapper` have breaking change; `Run` has been removed, replaced with `RunAsync`.
- *Enhancement:* The templates where database access is performed have been updated to leverage the new asynchronous methods. All previous synchronous access has been removed.
- *Enhancement:* All C# templates (e.g. `Entity_cs.xml`) have been updated to support nullable reference types (https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/).
- *Enhancement:* The entity code-generation (`Entity_cs.xml`) will output all reference types as nullable unless overridden explicitly for a `Property` element using `Nullable="true|false"`.

## v2.1.29
- *Fixed:* Code-gen of corresponding reference data text (`xxxText`) was being incorrectly output where the property supported multiple values (`RefDataList="true"`).

## v2.1.28
- *Fixed:* Code-gen of the data access for `Cosmos` will default the `CosmosEntity` attribute where not specified.
- *Fixed:* Code-gen for a custom `DataSvc` was incorrectly outputting an `OnAfterAsync` method invocation; see https://github.com/Avanade/Beef/issues/15.

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
- *Added:* Reference data updated to support multiple runtime providers, versus the previous single only. A new `IReferenceDataProvider` enables a provider to be created. The underlying code-gen templates have been updated to enable.
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