# 'Entity' object (entity-driven)

The `Entity` is used as the primary configuration for driving the entity-driven code generation.

<br/>

## Example

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) for a _standard_ entity is as follows:
``` yaml
entities:
- { name: Employee, inherits: EmployeeBase, validator: EmployeeValidator, webApiRoutePrefix: api/v1/employees, autoImplement: Database, databaseSchema: Hr, databaseMapperInheritsFrom: EmployeeBaseData.DbMapper, entityFrameworkModel: EfModel.Employee, entityFrameworkCustomMapper: true,
```

<br/>

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/refdata.beef.yaml) for a _reference data_ entity is as follows:
``` yaml
entities:
- { name: Gender, refDataType: Guid, collection: true, webApiRoutePrefix: api/v1/demo/ref/genders, autoImplement: Database, databaseSchema: Ref }
```

<br/>

## Property categories
The `Entity` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`RefData`](#RefData) | Provides the _Reference Data_ configuration.
[`Entity`](#Entity) | Provides the _Entity class_ configuration.
[`Collection`](#Collection) | Provides the _Entity collection class_ configuration.
[`Operation`](#Operation) | Provides the _Operation_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Events`](#Events) | Provides the _Events_ configuration.
[`WebApi`](#WebApi) | Provides the data _Web API_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`DataSvc`](#DataSvc) | Provides the _Data Services-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`Database`](#Database) | Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.
[`EntityFramework`](#EntityFramework) | Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.
[`Cosmos`](#Cosmos) | Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.
[`OData`](#OData) | Provides the specific _OData_ configuration where `AutoImplement` is `OData`.
[`HttpAgent`](#HttpAgent) | Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.
[`Model`](#Model) | Provides the data _Model_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Exclude`](#Exclude) | Provides the _Exclude_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique entity name. [Mandatory]
`text` | The overriding text for use in comments.<br/>&dagger; Overrides the Name (as sentence text) for the summary comments. It will be formatted as: `Represents the {Text} entity.`. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}). To have the text used as-is prefix with a `+` plus-sign character.
`fileName` | The overriding file name.<br/>&dagger; Overrides the Name as the code-generated file name.
`privateName` | The overriding private name.<br/>&dagger; Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.
`argumentName` | The overriding argument name.<br/>&dagger; Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.
`constType` | The Const .NET Type option. Valid options are: `int`, `long`, `Guid`, `string`.<br/>&dagger; The .NET Type to be used for the `const` values. Defaults to `string`.
`isInitialOverride` | Indicates whether to override the `IInitial.IsInitial` property.<br/>&dagger; Set to either `true` or `false` to override as specified; otherwise, `null` to check each property. Defaults to `null`.
`withResult` | Indicates whether to use `CoreEx.Results` (aka Railway-oriented programming).<br/>&dagger; Defaults to `CodeGeneration.WithResult`. This can be overridden within the Operation`(s).

<br/>

## RefData
Provides the _Reference Data_ configuration.

Property | Description
-|-
**`refDataType`** | The Reference Data identifier Type option. Valid options are: `int`, `long`, `Guid`, `string`.<br/>&dagger; Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier.
`refDataText` | Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` overriding the `CodeGeneration.RefDataText` selection.<br/>&dagger; This is used where serializing within the Web API`Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`). Defaults from `CodeGeneration.RefDataText`.
`refDataSortOrder` | The Reference Data sort order option. Valid options are: `SortOrder`, `Id`, `Code`, `Text`.<br/>&dagger; Specifies the default sort order for the underlying Reference Data collection. Defaults to `SortOrder`.
`refDataIdDataName` | The Reference Data `Id` data name.<br/>&dagger; Defaults to `Name` + `Id` (literal).
`refDataCodeDataName` | The Reference Data `Code` data name.<br/>&dagger; Defaults to `Code` (literal).
`refDataTextDataName` | The Reference Data `Text` data name.<br/>&dagger; Defaults to `Text` (literal).
`refDataIsActiveDataName` | The Reference Data `IsActive` data name.<br/>&dagger; Defaults to `IsActive` (literal).
`refDataSortOrderDataName` | The Reference Data `SortOrder` data name.<br/>&dagger; Defaults to `SortOrder` (literal).
`refDataETagDataName` | The Reference Data `ETag` data name.<br/>&dagger; Defaults to `RowVersion` (literal).
`refDataStoredProcedureName` | The Reference Data database stored procedure name.<br/>&dagger; Defaults to `sp` (literal) + `Name` + `GetAll` (literal).

<br/>

## Entity
Provides the _Entity class_ configuration.

Property | Description
-|-
`inherits` | The base class that the entity inherits from.<br/>&dagger; Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseEx<xxx>` depending on the corresponding `RefDataType` value. See `OmitEntityBase` if the desired outcome is to not inherit from any of the aforementioned base classes.
`implements` | The list of comma separated interfaces that are to be declared for the entity class.
`implementsAutoInfer` | Indicates whether to automatically infer the interface implements for the entity from the properties declared.<br/>&dagger; Will attempt to infer the following: `IIdentifier<Guid>`, `IIdentifier<int>`, `IIdentifier<long>`, `IIdentifier<string>`, `IETag` and `IChangeLog`. Defaults to `true`.
`abstract` | Indicates whether the class should be defined as abstract.
`genericWithT` | Indicates whether the class should be defined as a generic with a single parameter `T`.
`namespace` | The entity namespace to be appended.<br/>&dagger; Appended to the end of the standard structure as follows: `{Company}.{AppName}.Business.Entities.{Namespace}`.
`omitEntityBase` | Indicates that the entity should not inherit from `EntityBase`.<br/>&dagger; As such any of the `EntityBase` related capabilites are not supported (are omitted from generation). The intention for this is more for the generation of simple internal entities.
`jsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `SystemText`, `Newtonsoft`.<br/>&dagger; Defaults to the `CodeGeneration.JsonSerializer` configuration property where specified; otherwise, `SystemText`.
`internalOnly` | Indicates whether the entity is for internal use only; declared in Business entities only.

<br/>

## Collection
Provides the _Entity collection class_ configuration.

Property | Description
-|-
**`collection`** | Indicates whether a corresponding entity collection class should be created.
**`collectionResult`** | Indicates whether a corresponding entity collection result class should be created<br/>&dagger; Enables the likes of additional paging state to be stored with the underlying collection.
`collectionType` | The entity collection type used where `CollectionInherits` is not specified. Valid options are: `Standard`, `Keyed`, `Dictionary`.
`collectionInherits` | The base class that a `Collection` inherits from.<br/>&dagger; Defaults to `EntityBaseCollection` or `EntityBaseKeyedCollection` depending on `CollectionKeyed`. For Reference Data it will default to `ReferenceDataCollectionBase`.
`collectionResultInherits` | The base class that a `CollectionResult` inherits from.<br/>&dagger; Defaults to `EntityCollectionResult`.

<br/>

## Operation
Provides the _Operation_ configuration. These primarily provide a shorthand to create the standard `Get`, `Create`, `Update` and `Delete` operations (versus having to specify directly).

Property | Description
-|-
`behavior` | Defines the key CRUD-style behavior (operation types), being 'C'reate, 'G'et (or 'R'ead), 'U'pdate, 'P'atch and 'D'elete). Additionally, GetByArgs ('B') and GetAll ('A') operations that will be automatically generated where not otherwise explicitly specified.<br/>&dagger; Value may only specifiy one or more of the `CGRUDBA` characters (in any order) to define the automatically generated behavior (operations); for example: `CRUPD` or `CRUP` or `rba` (case insensitive). This is shorthand for setting one or more of the following properties: `Get`, `GetByArgs`, `GetAll`, 'Create', `Update`, `Patch` and `Delete`. Where one of these properties is set to either `true` or `false` this will take precedence over the value set for `Behavior`.
`get` | Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.
`getByArgs` | Indicates that a `GetByArgs` operation will be automatically generated where not otherwise explicitly specified.
`getAll` | Indicates that a `GetAll` operation will be automatically generated where not otherwise explicitly specified.
`create` | Indicates that a `Create` operation will be automatically generated where not otherwise explicitly specified.
`update` | Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.
`patch` | Indicates that a `Patch` operation will be automatically generated where not otherwise explicitly specified.
`delete` | Indicates that a `Delete` operation will be automatically generated where not otherwise explicitly specified.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`authRole`** | The role (permission) used by the `ExecutionContext.IsInRole(role)` for each `Operation`.<br/>&dagger; Used where not overridden specifically for an `Operation`; i.e. acts as the default.

<br/>

## Events
Provides the _Events_ configuration.

Property | Description
-|-
**`eventPublish`** | The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation. Valid options are: `None`, `DataSvc`, `Data`.<br/>&dagger; Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc. This can be overridden within the `Operation`(s).
`eventSource` | The Event Source.<br/>&dagger; Defaults to `Name` (as lowercase) appended with the `/{$key}` placeholder. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. This can be overridden for the `Operation`.
**`eventTransaction`** | Indicates whether a `System.TransactionScope` should be created and orchestrated whereever generating event publishing logic.<br/>&dagger; Usage will force a rollback of any underlying data transaction (where the provider supports TransactionScope) on failure, such as an `EventPublish` error. This is by no means implying a Distributed Transaction (DTC) should be invoked; this is only intended for a single data source that supports a TransactionScope to guarantee reliable event publishing. Defaults to `CodeGeneration.EventTransaction`. This essentially defaults the `Operation.DataSvcTransaction` where not otherwise specified. This should only be used where a transactionally-aware data source is being used.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
**`webApiRoutePrefix`** | The `RoutePrefixAtttribute` for the corresponding entity Web API controller.<br/>&dagger; This is the base (prefix) `URI` for the entity and can be further extended when defining the underlying `Operation`(s). The `CodeGeneration.WebApiRoutePrefix` will be prepended where specified. Where not specified will automatically default to the pluralized `Name` (as lowercase).
**`webApiAuthorize`** | The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.<br/>&dagger; Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also.
`webApiCtor` | The access modifier for the generated Web API `Controller` constructor. Valid options are: `Public`, `Private`, `Protected`.<br/>&dagger; Defaults to `Public`.
**`webApiCtorParams`** | The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored.
`webApiAutoLocation` | Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.<br/>&dagger; This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This is defaulted from the `CodeGen.WebApiAutoLocation`.
`webApiConcurrency` | Indicates whether the Web API is responsible for managing (simulating) concurrency via auto-generated ETag.<br/>&dagger; This provides an alternative where the underlying data source does not natively support optimistic concurrency (native support should always be leveraged as a priority). Where the `Operation.Type` is `Update` or `Patch`, the request ETag will be matched against the response for a corresponding `Get` operation to verify no changes have been made prior to updating. For this to function correctly the .NET response Type for the `Get` must be the same as that returned from the corresponding `Create`, `Update` and `Patch` (where applicable) as the generated ETag is a SHA256 hash of the resulting JSON. This defaults the `Operation.WebApiConcurrency`.
`webApiGetOperation` | The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Update` and `SimulateConcurrency` is `true`.<br/>&dagger; Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.
`webApiTags` | The list of tags to add for the generated `WebApi` controller.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
`managerCtor` | The access modifier for the generated `Manager` constructor. Valid options are: `Public`, `Private`, `Protected`.<br/>&dagger; Defaults to `Public`.
**`managerCtorParams`** | The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored.
`managerExtensions` | Indicates whether the `Manager` extensions logic should be generated.<br/>&dagger; This can be overridden using `Operation.ManagerExtensions`.
**`validator`** | The name of the .NET implementing `Type` or interface `Type` that will perform the validation.<br/>&dagger; Only used for defaulting the `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.
`identifierGenerator` | Indicates whether the `IIdentifierGenerator` should be used to generate the `Id` property where the operation types (`Operation.Type`) is `Create`.
`managerCleanUp` | Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.<br/>&dagger; This can be overridden within the `CodeGeneration` and `Operation`(s).
`validationFramework` | The `Validation` framework to use for the entity-based validation. Valid options are: `CoreEx`, `FluentValidation`.<br/>&dagger; Defaults to `CodeGeneration.ValidationFramework`. This can be overridden within the `Operation`(s) and `Parameter`(s).

<br/>

## DataSvc
Provides the _Data Services-layer_ configuration.

Property | Description
-|-
`dataSvcCaching` | Indicates whether request-based `IRequestCache` caching is to be performed at the `DataSvc` layer to improve performance (i.e. reduce chattiness).<br/>&dagger; Defaults to `true`.
`dataSvcCtor` | The access modifier for the generated `DataSvc` constructor. Valid options are: `Public`, `Private`, `Protected`.<br/>&dagger; Defaults to `Public`.
`dataSvcCtorParams` | The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored.
`dataSvcExtensions` | Indicates whether the `DataSvc` extensions logic should be generated.<br/>&dagger; This can be overridden using `Operation.DataSvcExtensions`.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`autoImplement`** | The data source auto-implementation option. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `HttpAgent`, `None`.<br/>&dagger; Defaults to `CodeGeneration.AutoImplement` (where `RefDataType` or `EntityFrameworkModel` or `CosmosModel` or `HttpAgent` is not null; otherwise, `None`. Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicitly overridden). When selected some of the related attributes will also be required (as documented). Additionally, the `AutoImplement` can be further specified/overridden per `Operation`.
`dataCtor` | The access modifier for the generated `Data` constructor. Valid options are: `Public`, `Private`, `Protected`.<br/>&dagger; Defaults to `Public`.
`dataCtorParams` | The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored.
`dataExtensions` | Indicates whether the `Data` extensions logic should be generated.<br/>&dagger; This can be overridden using `Operation.DataExtensions`.

<br/>

## Database
Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.

Property | Description
-|-
**`databaseName`** | The .NET database interface name (used where `AutoImplement` is `Database`).<br/>&dagger; Defaults to the `CodeGeneration.DatabaseName` configuration property (its default value is `IDatabase`).
**`databaseSchema`** | The database schema name (used where `AutoImplement` is `Database`).<br/>&dagger; Defaults to `dbo`.
`databaseMapperInheritsFrom` | The name of the `Mapper` that the generated Database `Mapper` inherits from.
`databaseCustomMapper` | Indicates that a custom Database `Mapper` will be used; i.e. not generated.<br/>&dagger; Otherwise, by default, a `Mapper` will be generated.
`databaseMapperEx` | Indicates that a `DatabaseMapperEx` (extended/explicit) will be used; versus, `DatabaseMapper` (which uses Reflection internally).<br/>&dagger; Defaults to `CodeGeneration.DatabaseMapperEx` (its default value is `true`). The `DatabaseMapperEx` essentially replaces the `DatabaseMapper` as it is more performant; this option can be used where leagcy/existing behavior is required.

<br/>

## EntityFramework
Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.

Property | Description
-|-
**`entityFrameworkName`** | The .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`.<br/>&dagger; Defaults to `CodeGeneration.EntityFrameworkName`.
**`entityFrameworkModel`** | The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).
`entityFrameworkCustomMapper` | Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.<br/>&dagger; Otherwise, by default, a `Mapper` will be generated.
`entityFrameworkMapperBase` | The EntityFramework data-layer name that should be used for base mappings.

<br/>

## Cosmos
Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.

Property | Description
-|-
**`cosmosName`** | The .NET Cosmos interface name used where `AutoImplement` is `Cosmos`.<br/>&dagger; Defaults to the `CodeGeneration.CosmosName` configuration property (its default value is `ICosmosDb`).
**`cosmosModel`** | The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).
**`cosmosContainerId`** | The Cosmos `ContainerId` required where `AutoImplement` is `Cosmos`.
`cosmosPartitionKey` | The C# code to be used for setting the optional Cosmos `PartitionKey` where `AutoImplement` is `Cosmos`.<br/>&dagger; The value `PartitionKey.None` can be specified. Literals will need to be quoted.
`cosmosValueContainer` | Indicates whether the `CosmosDbValueContainer` is to be used; otherwise, `CosmosDbContainer`.
`cosmosCustomMapper` | Indicates that a custom Cosmos `Mapper` will be used; i.e. not generated.<br/>&dagger; Otherwise, by default, a `Mapper` will be generated.
`cosmosMapperBase` | The Cosmos data-layer name that should be used for base mappings.
`httpAgentMapperBase` | The HTTP Agent data-layer name that should be used for base mappings.

<br/>

## OData
Provides the specific _OData_ configuration where `AutoImplement` is `OData`.

Property | Description
-|-
**`odataName`** | The .NET OData interface name used where `AutoImplement` is `OData`.<br/>&dagger; Defaults to the `CodeGeneration.ODataName` configuration property (its default value is `IOData`).
**`odataModel`** | The corresponding OData model name (required where `AutoImplement` is `OData`).
**`odataCollectionName`** | The name of the underlying OData collection where `AutoImplement` is `OData`.<br/>&dagger; The underlying `Simple.OData.Client` will attempt to infer.
`odataCustomMapper` | Indicates that a custom OData `Mapper` will be used; i.e. not generated.<br/>&dagger; Otherwise, by default, a `Mapper` will be generated.
`httpAgentCustomMapper` | Indicates that a custom HTTP Agent `Mapper` will be used; i.e. not generated.<br/>&dagger; Otherwise, by default, a `Mapper` will be generated.

<br/>

## HttpAgent
Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.

Property | Description
-|-
**`httpAgentName`** | The .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; Defaults to `CodeGeneration.HttpAgentName` configuration property (its default value is `IHttpAgent`).
`httpAgentRoutePrefix` | The base HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; This is the base (prefix) `URI` for the HTTP Agent endpoint and can be further extended when defining the underlying `Operation`(s).
**`httpAgentModel`** | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).<br/>&dagger; This can be overridden within the `Operation`(s).
`httpAgentReturnModel` | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).<br/>&dagger; This can be overridden within the `Operation`(s).
`httpAgentCode` | The fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; Prepended to `Operation.HttpAgentCode` where specified to enable standardized functionality.

<br/>

## Model
Provides the data _Model_ configuration.

Property | Description
-|-
`dataModel` | Indicates whether a data `model` version of the entity should also be generated (output to `.\Business\Data\Model`).<br/>&dagger; The model will be generated with `OmitEntityBase = true`. Any reference data properties will be defined using their `RefDataType` intrinsic `Type` versus their corresponding (actual) reference data `Type`.
`dataModelInherits` | Overrides the default data `model` inherits value.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`grpc`** | Indicates whether gRPC support (more specifically service-side) is required for the Entity.<br/>&dagger; gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Property` and `Operation` will also need to be opted-in specifically.

<br/>

## Exclude
Provides the _Exclude_ configuration.

Property | Description
-|-
**`excludeEntity`** | Indicates whether to exclude the generation of the `Entity` class (`Xxx.cs`).
**`excludeAll`** | Indicates whether to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class.<br/>&dagger; Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to exclude.
`excludeIData` | Indicates whether to exclude the generation of the `Data` interface (`IXxxData.cs`).
`excludeData` | The option to exclude the generation of the `Data` class (`XxxData.cs`). Valid options are: `Include`, `Exclude`, `RequiresMapper`.<br/>&dagger; Defaults to `Include` indicating _not_ to exlude. A value of `Exclude` indicates to exclude all output; alternatively, `RequiresMapper` indicates to at least output the corresponding `Mapper` class.
`excludeIDataSvc` | Indicates whether to exclude the generation of the `DataSvc` interface (`IXxxDataSvc.cs`).
`excludeDataSvc` | Indicates whether to exclude the generation of the `DataSvc` class (`XxxDataSvc.cs`).
`excludeIManager` | Indicates whether to exclude the generation of the `Manager` interface (`IXxxManager.cs`).
`excludeManager` | Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`).
`excludeWebApi` | The option to exclude the generation of the WebAPI `Controller` class (`XxxController.cs`).
`excludeWebApiAgent` | Indicates whether to exclude the generation of the WebAPI consuming `Agent` class (`XxxAgent.cs`).
`excludeGrpcAgent` | Indicates whether to exclude the generation of the gRPC consuming `Agent` class (`XxxAgent.cs`).

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`properties` | The corresponding [`Property`](Entity-Property-Config.md) collection.
`operations` | The corresponding [`Operation`](Entity-Operation-Config.md) collection.
`consts` | The corresponding [`Const`](Entity-Const-Config.md) collection.

