# 'CodeGeneration' object (entity-driven)

The `CodeGeneration` object defines global properties that are used to drive the underlying entity-driven code generation.

<br/>

## Example

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
refDataNamespace: My.Hr.Common.Entities
refDataText: true
eventSubjectRoot: My
eventActionFormat: PastTense
entities:
```

<br/>

## Property categories
The `CodeGeneration` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`DotNet`](#DotNet) | Provides the _.NET_ configuration.
[`RefData`](#RefData) | Provides the _Reference Data_ configuration.
[`Entity`](#Entity) | Provides the _Entity class_ configuration.
[`Events`](#Events) | Provides the _Events_ configuration.
[`WebApi`](#WebApi) | Provides the _Web API (Controller)_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`Database`](#Database) | Provides the _Database Data-layer_ configuration.
[`EntityFramework`](#EntityFramework) | Provides the _Entity Framewotrk (EF) Data-layer_ configuration.
[`Cosmos`](#Cosmos) | Provides the _CosmosDB Data-layer_ configuration.
[`OData`](#OData) | Provides the _OData Data-layer_ configuration.
[`HttpAgent`](#HttpAgent) | Provides the _HTTP Agent Data-layer_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Path`](#Path) | Provides the _Path (Directory)_ configuration for the generated artefacts.
[`Namespace`](#Namespace) | Provides the _.NET Namespace_ configuration for the generated artefacts.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## DotNet
Provides the _.NET_ configuration.

Property | Description
-|-
`withResult` | Indicates whether to use `CoreEx.Results` (aka Railway-oriented programming).<br/>&dagger; Defaults to `true`. This can be overridden within the `Entity`(s) and/or `Operation`(s).
`preprocessorDirectives` | Indicates whether to use preprocessor directives in the generated output.

<br/>

## RefData
Provides the _Reference Data_ configuration.

Property | Description
-|-
**`refDataNamespace`** | The namespace for the Reference Data entities (adds as a c# `using` statement).<br/>&dagger; Defaults to `Company` + `.` (literal) + AppName + `.Business.Entities` (literal).
**`refDataCommonNamespace`** | The namespace for the Reference Data common entities (adds as a c# `using` statement).<br/>&dagger; Defaults to `Company` + `.` (literal) + AppName + `.Common.Entities` (literal).
**`refDataText`** | Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` for an `Entity`.<br/>&dagger; This is used where serializing within the Web API `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`). This can be further configured on the `Entity` and for each `Property`.
**`refDataType`** | The Reference Data identifier Type option. Valid options are: `int`, `long`, `Guid`, `string`.<br/>&dagger; Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier. Results in all underlying entities becoming Reference Data.
**`refDataWebApiRoute`** | The `RouteAtttribute` for the Reference Data Web API controller required for named pre-fetching. The `WebApiRoutePrefix` will be prepended where specified.
`refDataCodeDataName` | The Reference Data `Code` data name.<br/>&dagger; Defaults to `Code` (literal).
`refDataTextDataName` | The Reference Data `Text` data name.<br/>&dagger; Defaults to `Text` (literal).
`refDataIsActiveDataName` | The Reference Data `IsActive` data name.<br/>&dagger; Defaults to `IsActive` (literal).
`refDataSortOrderDataName` | The Reference Data `SortOrder` data name.<br/>&dagger; Defaults to `SortOrder` (literal).
`refDataETagDataName` | The Reference Data `ETag` data name.<br/>&dagger; Defaults to `RowVersion` (literal).

<br/>

## Entity
Provides the _Entity class_ configuration.

Property | Description
-|-
`jsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `SystemText`, `Newtonsoft`.<br/>&dagger; Defaults to `SystemText`. This can be overridden within the `Entity`(s).
`etagJsonName` | The default JSON name for the `ETag` property. Valid options are: `etag`, `eTag`, `_etag`, `_eTag`, `ETag`, `ETAG`.<br/>&dagger; Defaults to `etag`. Note that the `JsonName` can be set individually per property where required.
`usingNamespace1` | The additional Namespace using statement to be added to the generated `Entity` code.<br/>&dagger; Typically used where referening a `Type` from a Namespace that is not generated by default.
`usingNamespace2` | The additional Namespace using statement to be added to the generated `Entity` code.<br/>&dagger; Typically used where referening a `Type` from a Namespace that is not generated by default.
`usingNamespace3` | The additional Namespace using statement to be added to the generated `Entity` code.<br/>&dagger; Typically used where referening a `Type` from a Namespace that is not generated by default.

<br/>

## Events
Provides the _Events_ configuration.

Property | Description
-|-
**`eventPublish`** | The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation. Valid options are: `None`, `DataSvc`, `Data`.<br/>&dagger; Defaults to `DataSvc`. Used to enable the sending of messages to the likes of EventHub, ServiceBus, SignalR, etc. This can be overridden within the `Entity`(s).
`eventSourceRoot` | The URI root for the event source by prepending to all event source URIs.<br/>&dagger; The event source is only updated where an `EventSourceKind` is not `None`. This can be extended within the `Entity`(s).
`eventSourceKind` | The URI kind for the event source URIs. Valid options are: `None`, `Absolute`, `Relative`, `RelativeOrAbsolute`.<br/>&dagger; Defaults to `None` (being the event source is not updated).
**`eventSubjectRoot`** | The root for the event Subject name by prepending to all event subject names.<br/>&dagger; Used to enable the sending of messages to the likes of EventHub, ServiceBus, SignalR, etc. This can be overridden within the `Entity`(s).

<br/>

## WebApi
Provides the _Web API (Controller)_ configuration.

Property | Description
-|-
`webApiAuthorize` | The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.<br/>&dagger; This can be overridden within the `Entity`(s) and/or their corresponding `Operation`(s).
`webApiAutoLocation` | Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.<br/>&dagger; This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This can be overridden within the `Entity`(s).
**`webApiRoutePrefix`** | The base (prefix) `URI` prepended to all `Operation.WebApiRoute` values.
`webApiTags` | The list of tags to add for the generated `WebApi`.<br/>&dagger; This can be overridden within the `Entity`(s) and/or their corresponding `Operation`(s).

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
`managerCleanUp` | Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.<br/>&dagger; This can be overridden within the `Entity`(s) and `Operation`(s).
`validationFramework` | The `Validation` framework to use for the entity-based validation. Valid options are: `CoreEx`, `FluentValidation`.<br/>&dagger; Defaults to `CoreEx` (literal). This can be overridden within the `Entity`(s), `Operation`(s) and `Parameter`(s).

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
`refDataDataCtorParams` | The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `ReferenceDataData` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored.
**`autoImplement`** | The data source auto-implementation option. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `HttpAgent`, `None`.<br/>&dagger; Defaults to `None`. Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicitly overridden). When selected some of the related attributes will also be required (as documented). Additionally, the `AutoImplement` can be further specified/overridden per `Operation`.
**`etagDefaultMapperConverter`** | The default ETag to/from RowVersion column Mapping Converter used where `Operation.AutoImplement` is `Database` or `EntityFramework`.<br/>&dagger; Defaults to `StringToBase64Converter`.
`refDataDefaultMapperConverter` | The default Reference Data property `Converter` used by the generated `Mapper`(s) where not specifically defined. Valid options are: `ReferenceDataCodeConverter`, `ReferenceDataCodeConverter{T}`, `ReferenceDataCodeConverter<T>`, `ReferenceDataIdConverter{T, int}`, `ReferenceDataIdConverter<T, int>`, `ReferenceDataIdConverter{T, int?}`, `ReferenceDataIdConverter<T, int?>`, `ReferenceDataIdConverter{T, long}`, `ReferenceDataIdConverter<T, long>`, `ReferenceDataIdConverter{T, long?}`, `ReferenceDataIdConverter<T, long?>`, `ReferenceDataIdConverter{T, Guid}`, `ReferenceDataIdConverter<T, Guid>`, `ReferenceDataIdConverter{T, Guid?}`, `ReferenceDataIdConverter<T, Guid?>`, `ReferenceDataInt32IdConverter`, `ReferenceDataInt32IdConverter{T}`, `ReferenceDataInt32IdConverter<T>`, `ReferenceDataNullableInt32IdConverter`, `ReferenceDataNullableInt32IdConverter{T}`, `ReferenceDataNullableInt32IdConverter<T>`, `ReferenceDataInt64IdConverter`, `ReferenceDataInt64IdConverter{T}`, `ReferenceDataInt64IdConverter<T>`, `ReferenceDataNullableInt64IdConverter`, `ReferenceDataNullableInt64IdConverter{T}`, `ReferenceDataNullableInt64IdConverter<T>`, `ReferenceDataGuidIdConverter`, `ReferenceDataGuidIdConverter{T}`, `ReferenceDataGuidIdConverter<T>`, `ReferenceDataNullableGuidIdConverter`, `ReferenceDataNullableGuidIdConverter{T}`, `ReferenceDataNullableGuidIdConverter<T>`.<br/>&dagger; Defaults to `ReferenceDataCodeConverter<T>`. Where this value is suffixed by `<T>` or `{T}` this will automatically be set to the `Type`.

<br/>

## Database
Provides the _Database Data-layer_ configuration.

Property | Description
-|-
**`databaseType`** | The .NET database type and optional name (used where `Operation.AutoImplement` is `Database`).<br/>&dagger; Defaults to `IDatabase`. Should be formatted as `Type` + `^` + `Name`; e.g. `IDatabase^Db`. Where the `Name` portion is not specified it will be inferred. This can be overridden within the `Entity`(s).
**`databaseSchema`** | The default database schema name.<br/>&dagger; Defaults to `dbo`.
**`databaseProvider`** | The default database schema name. Valid options are: `SqlServer`, `MySQL`, `Postgres`.<br/>&dagger; Defaults to `SqlServer`. Enables specific database provider functionality/formatting/etc. where applicable.
`databaseMapperEx` | Indicates that a `DatabaseMapperEx` will be used; versus, `DatabaseMapper` (which uses Reflection internally).<br/>&dagger; Defaults to `true`. The `DatabaseMapperEx` essentially replaces the `DatabaseMapper` as it is more performant (extended/explicit); this option can be used where leagcy/existing behavior is required.

<br/>

## EntityFramework
Provides the _Entity Framewotrk (EF) Data-layer_ configuration.

Property | Description
-|-
`entityFrameworkType` | The .NET Entity Framework type and optional name (used where `Operation.AutoImplement` is `EntityFramework`).<br/>&dagger; Defaults to `IEfDb`. Should be formatted as `Type` + `^` + `Name`; e.g. `IEfDb^Ef`. Where the `Name` portion is not specified it will be inferred. This can be overridden within the `Entity`(s).

<br/>

## Cosmos
Provides the _CosmosDB Data-layer_ configuration.

Property | Description
-|-
**`cosmosType`** | The .NET Cosmos DB type and name (used where `Operation.AutoImplement` is `Cosmos`).<br/>&dagger; Defaults to `ICosmosDb`. Should be formatted as `Type` + `^` + `Name`; e.g. `ICosmosDb^Cosmos`. Where the `Name` portion is not specified it will be inferred. This can be overridden within the `Entity`(s).

<br/>

## OData
Provides the _OData Data-layer_ configuration.

Property | Description
-|-
**`odataType`** | The .NET OData interface name used where `Operation.AutoImplement` is `OData`.<br/>&dagger; Defaults to `IOData`. Should be formatted as `Type` + `^` + `Name`; e.g. `IOData^OData`. Where the `Name` portion is not specified it will be inferred. This can be overridden within the `Entity`(s).

<br/>

## HttpAgent
Provides the _HTTP Agent Data-layer_ configuration.

Property | Description
-|-
**`httpAgentType`** | The default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; Defaults to `IHttpAgent`. Should be formatted as `Type` + `^` + `Name`; e.g. `IHttpAgent^HttpAgent`. Where the `Name` portion is not specified it will be inferred. This can be overridden within the `Entity`(s).

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`grpc`** | Indicates whether gRPC support (more specifically service-side) is required.<br/>&dagger; gRPC support is an explicit opt-in model. Must be set to `true` for any of the subordinate gRPC capabilities to be code-generated. Will require each `Entity`, and corresponding `Property` and `Operation` to be opted-in specifically.

<br/>

## Path
Provides the _Path (Directory)_ configuration for the generated artefacts.

Property | Description
-|-
`pathBase` | The base path (directory) prefix for the artefacts; other `Path*` properties append to this value when they are not specifically overridden.<br/>&dagger; Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`pathCommon` | The path (directory) for the Database-related artefacts.<br/>&dagger; Defaults to `PathBase` + `.Common` (literal). For example `Beef.Demo.Common`.
`pathBusiness` | The path (directory) for the Business-related (.NET) artefacts.<br/>&dagger; Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`pathApi` | The path (directory) for the API-related (.NET) artefacts.<br/>&dagger; Defaults to `PathBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`namespaceBase` | The base Namespace (root) for the .NET artefacts.<br/>&dagger; Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`namespaceCommon` | The Namespace (root) for the Common-related .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.
`namespaceBusiness` | The Namespace (root) for the Business-related .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`namespaceApi` | The Namespace (root) for the Api-related .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`entities`** | The corresponding [`Entity`](Entity-Entity-Config.md) collection.<br/><br/>An `Entity` object provides the primary configuration for an entity, its properties and operations.

