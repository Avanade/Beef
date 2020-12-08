# 'Entity' object (entity-driven) - YAML/JSON

The `Entity` is used as the primary configuration for driving the entity-driven code generation.

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
The `Entity` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`RefData`](#RefData) | Provides the _Reference Data_ configuration.
[`Entity`](#Entity) | Provides the _Entity class_ configuration.
[`Collection`](#Collection) | Provides the _Entity collection class_ configuration.
[`Operation`](#Operation) | Provides the _Operation_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`WebApi`](#WebApi) | Provides the data _Web API_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`DataSvc`](#DataSvc) | Provides the _Data Services-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`Database`](#Database) | Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.
[`EntityFramework`](#EntityFramework) | Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.
[`Cosmos`](#Cosmos) | Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.
[`OData`](#OData) | Provides the specific _OData_ configuration where `AutoImplement` is `OData`.
[`Model`](#Model) | Provides the data _Model_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Exclude`](#Exclude) | Provides the _Exclude_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique entity name.
`text` | The overriding text for use in comments. Overrides the Name (as sentence text) for the summary comments. It will be formatted as: `Represents the {Text} entity.`. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
`fileName` | The overriding file name. Overrides the Name as the code-generated file name.
`entityScope` | The entity scope option. Valid options are: `Common`, `Business`. Determines whether the entity is considered `Common` (default) or should be scoped to the `Business` namespace/assembly only (i.e. not externally visible).
`privateName` | The overriding private name. Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.
`argumentName` | The overriding argument name. Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.
`constType` | The Const .NET Type option. Valid options are: `int`, `Guid`, `string`. The .NET Type to be used for the `const` values. Defaults to `string`.
`isInitialOverride` | Indicates whether to override the `ICleanup.IsInitial` property. Set to either `true` or `false` to override as specified; otherwise, `null` to check each property. Defaults to `null`.

<br/>

## RefData
Provides the _Reference Data_ configuration.

Property | Description
-|-
**`refDataType`** | The Reference Data identifier Type option. Valid options are: `int`, `Guid`. Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier.
`refDataText` | Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` overridding the `CodeGeneration.RefDataText` selection. This is used where serializing within the Web API`Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`).
`refDataSortOrder` | The Reference Data sort order option. Valid options are: `SortOrder`, `Id`, `Code`, `Text`. Specifies the default sort order for the underlying Reference Data collection. Defaults to `SortOrder`.
`refDataStringFormat` | The Reference Data `ToString` composite format. The string format supports the standard composite formatting; where the following indexes are used: `{0}` for `Id`, `{1}` for `Code` and `{2}` for `Text`. Defaults to `{2}`.

<br/>

## Entity
Provides the _Entity class_ configuration.

Property | Description
-|-
`inherits` | The base class that the entity inherits from. Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseInt` or `ReferenceDataBaseGuid` depending on the corresponding `RefDataType` value. See `OmitEntityBase` if the desired outcome is to not inherit from any of the aforementioned base classes.
`implements` | The list of comma separated interfaces that are to be declared for the entity class.
`autoInferImplements` | Indicates whether to automatically infer the interface implements for the entity from the properties declared. Will attempt to infer the following: `IGuidIdentifier`, `IIntIdentifier`, `IStringIdentifier`, `IETag` and `IChangeLog`. Defaults to `true`.
`abstract` | Indicates whether the class should be defined as abstract.
`genericWithT` | Indicates whether the class should be defined as a generic with a single parameter `T`.
`namespace` | The entity namespace to be appended. Appended to the end of the standard structure as follows: `{Company}.{AppName}.Common.Entities.{Namespace}`.
`omitEntityBase` | Indicates that the entity should not inherit from `EntityBase`. As such any of the `EntityBase` related capabilites are not supported (are omitted from generation). The intention for this is more for the generation of simple internal entities.
`jsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `None`, `Newtonsoft`. Defaults to the `CodeGeneration.JsonSerializer` configuration property where specified; otherwise, `Newtonsoft`.

<br/>

## Collection
Provides the _Entity collection class_ configuration.

Property | Description
-|-
**`collection`** | Indicates whether a corresponding entity collection class should be created.
**`collectionResult`** | Indicates whether a corresponding entity collection result class should be created Enables the likes of additional paging state to be stored with the underlying collection.
`collectionKeyed` | Indicates whether the entity collection is keyed using the properties defined as forming part of the unique key.
`collectionInherits` | The base class that a `Collection` inherits from. Defaults to `EntityBaseCollection` or `EntityBaseKeyedCollection` depending on `CollectionKeyed`. For Reference Data it will default to `ReferenceDataCollectionBase`.
`collectionResultInherits` | The base class that a `CollectionResult` inherits from. Defaults to `EntityCollectionResult`.

<br/>

## Operation
Provides the _Operation_ configuration. These primarily provide a shorthand to create the standard `Get`, `Create`, `Update` and `Delete` operations (versus having to specify directly).

Property | Description
-|-
**`validator`** | The name of the .NET `Type` that will perform the validation. Only used for `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.
`get` | Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.
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
**`authRole`** | The role (permission) used by the `ExecutionContext.IsInRole(role)` for each `Operation`. Used where not overridden specifically for an `Operation`; i.e. acts as the default.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
**`webApiRoutePrefix`** | The `RoutePrefixAtttribute` for the corresponding entity Web API controller. This is the base (prefix) `URI` for the entity and can be further extended when defining the underlying `Operation`(s).
**`webApiAuthorize`** | The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`. Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also.
`webApiConstructor` | The access modifier for the generated Web API `Controller` constructor. Valid options are: `Public`, `Private`, `Protected`. Defaults to `Public`.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
`managerConstructor` | The access modifier for the generated `Manager` constructor. Valid options are: `Public`, `Private`, `Protected`. Defaults to `Public`.
`managerExtensions` | Indicates whether the `Manager` extensions logic should be generated.

<br/>

## DataSvc
Provides the _Data Services-layer_ configuration.

Property | Description
-|-
`dataSvcCaching` | Indicates whether request-based `IRequestCache` caching is to be performed at the `DataSvc` layer to improve performance (i.e. reduce chattiness). Defaults to `true`.
`eventPublish` | Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a `Create`, `Update` or `Delete` operation. Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.
`dataSvcConstructor` | The access modifier for the generated `DataSvc` constructor. Valid options are: `Public`, `Private`, `Protected`. Defaults to `Public`.
`dataSvcExtensions` | Indicates whether the `DataSvc` extensions logic should be generated.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`autoImplement`** | The data source auto-implementation option. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `None`. Defaults to `None`. Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicity overridden). When selected some of the related attributes will also be required (as documented). Additionally, the `AutoImplement` indicator must be selected for each underlying `Operation` that is to be auto-implemented.
`mapperAddStandardProperties` | Indicates that the `AddStandardProperties` method call is to be included for the generated (corresponding) `Mapper`. Defaults to `true`.
`dataConstructor` | The access modifier for the generated `Data` constructor. Valid options are: `Public`, `Private`, `Protected`. Defaults to `Public`.
`dataExtensions` | Indicates whether the `Data` extensions logic should be generated.

<br/>

## Database
Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.

Property | Description
-|-
**`databaseName`** | The .NET database interface name (used where `AutoImplement` is `Database`). Defaults to the `CodeGeneration.DatabaseName` configuration property (its default value is `IDatabase`).
**`databaseSchema`** | The database schema name (used where `AutoImplement` is `Database`). Defaults to `dbo`.
`databaseMapperInheritsFrom` | The name of the `Mapper` that the generated Database `Mapper` inherits from.
`databaseCustomerMapper` | Indicates that a custom Database `Mapper` will be used; i.e. not generated. Otherwise, by default, a `Mapper` will be generated.

<br/>

## EntityFramework
Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.

Property | Description
-|-
**`entityFrameworkName`** | The .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`. Defaults to the `CodeGeneration.EntityFrameworkName` configuration property (its default value is `IEfDb`).
**`entityFrameworkModel`** | The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).
`entityFrameworkMapperInheritsFrom` | The name of the `Mapper  that the generated Entity Framework `Mapper` inherits from. Defaults to `Model.{Name}`; i.e. an entity with the same name in the `Model` namespace.
`entityFrameworkCustomMapper` | Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated. Otherwise, by default, a `Mapper` will be generated.

<br/>

## Cosmos
Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.

Property | Description
-|-
**`cosmosName`** | The .NET Cosmos interface name used where `AutoImplement` is `Cosmos`. Defaults to the `CodeGeneration.CosmosName` configuration property (its default value is `ICosmosDb`).
**`cosmosModel`** | The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).
**`cosmosContainerId`** | The Cosmos `ContainerId` required where `AutoImplement` is `Cosmos`.
`cosmosPartitionKey` | The C# code to be used for setting the optional Cosmos `PartitionKey` where `AutoImplement` is `Cosmos`. Defaults to `PartitionKey.None`.
`cosmosValueContainer` | Indicates whether the `CosmosDbValueContainer` is to be used; otherwise, `CosmosDbContainer`.
`cosmosMapperInheritsFrom` | The name of the `Mapper` that the generated Cosmos `Mapper` inherits from.
`cosmosCustomMapper` | Indicates that a custom Cosmos `Mapper` will be used; i.e. not generated. Otherwise, by default, a `Mapper` will be generated.

<br/>

## OData
Provides the specific _OData_ configuration where `AutoImplement` is `OData`.

Property | Description
-|-
**`odataName`** | The .NET OData interface name used where `AutoImplement` is `OData`. Defaults to the `CodeGeneration.ODataName` configuration property (its default value is `IOData`).
**`odataModel`** | The corresponding OData model name (required where `AutoImplement` is `OData`).
**`odataCollectionName`** | The name of the underlying OData collection where `AutoImplement` is `OData`. The underlying `Simple.OData.Client` will attempt to infer.
`odataMapperInheritsFrom` | The name of the `Mapper` that the generated OData `Mapper` inherits from.
`odataCustomMapper` | Indicates that a custom OData `Mapper` will be used; i.e. not generated. Otherwise, by default, a `Mapper` will be generated.

<br/>

## Model
Provides the data _Model_ configuration.

Property | Description
-|-
`dataModel` | Indicates whether a data `model` version of the entity should also be generated (output to `.\Business\Data\Model`). The model will be generated with `OmitEntityBase = true`. Any reference data properties will be defined using their `RefDataType` intrinsic `Type` versus their corresponding (actual) reference data `Type`.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`grpc`** | Indicates whether gRPC support (more specifically service-side) is required for the Entity. gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Property` and `Operation` will also need to be opted-in specifically.

<br/>

## Exclude
Provides the _Exclude_ configuration.

Property | Description
-|-
**`excludeEntity`** | The option to exclude the generation of the `Entity` class (`Xxx.cs`). Valid options are: `No`, `Yes`.
**`excludeAll`** | The option to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class. Valid options are: `No`, `Yes`. Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `Yes`.
`excludeIData` | The option to exclude the generation of the `Data` interface (`IXxxData.cs`). Valid options are: `No`, `Yes`.
`excludeData` | The option to exclude the generation of the `Data` class (`XxxData.cs`). Valid options are: `No`, `Yes`, `RequiresMapper`. Defaults to `No` indicating _not_ to exlude. A value of `Yes` indicates to exclude all output; alternatively, `RequiresMapper` indicates to at least output the corresponding `Mapper` class.
`excludeIDataSvc` | The option to exclude the generation of the `DataSvc` interface (`IXxxDataSvc.cs`). Valid options are: `No`, `Yes`.
`excludeDataSvc` | The option to exclude the generation of the `DataSvc` class (`XxxDataSvc.cs`). Valid options are: `No`, `Yes`.
`excludeIManager` | The option to exclude the generation of the `Manager` interface (`IXxxManager.cs`). Valid options are: `No`, `Yes`.
`excludeManager` | The option to exclude the generation of the `Manager` class (`XxxManager.cs`). Valid options are: `No`, `Yes`.
`excludeWebApi` | The option to exclude the generation of the WebAPI `Controller` class (`XxxController.cs`). Valid options are: `No`, `Yes`.
`excludeWebApiAgent` | The option to exclude the generation of the WebAPI consuming `Agent` class (`XxxAgent.cs`). Valid options are: `No`, `Yes`.
`excludeGrpcAgent` | The option to exclude the generation of the gRPC consuming `Agent` class (`XxxAgent.cs`). Valid options are: `No`, `Yes`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`properties` | The corresponding [`Property`](Entity-Property-Config.md) collection.
`operations` | The corresponding [`Operation`](Entity-Operation-Config.md) collection.
`consts` | The corresponding [`Const`](Entity-Const-Config.md) collection.

<br/>

