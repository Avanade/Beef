# 'CodeGeneration' object (entity-driven) - XML

The code generation for an `Operation` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as required.

The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](./Http-Patch.md)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customised operation where arguments and return value will be explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer.

<br/>

## Property categories
The `Operation` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Events`](#Events) | Provides the _Events_ configuration.
[`WebApi`](#WebApi) | Provides the data _Web API_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`DataSvc`](#DataSvc) | Provides the _Data Services-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`Database`](#Database) | Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.
[`Cosmos`](#Cosmos) | Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.
[`OData`](#OData) | Provides the specific _OData_ configuration where `AutoImplement` is `OData`.
[`HttpAgent`](#HttpAgent) | Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Exclude`](#Exclude) | Provides the _Exclude_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The unique operation name.
**`OperationType`** | The type of operation that is to be code-generated. Valid options are: `Get`, `GetColl`, `Create`, `Update`, `Patch`, `Delete`, `Custom`.
`Text` | The text for use in comments. The `Text` will be defaulted for all the `Operation.Type` options with the exception of `Custom`. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
**`UniqueKey`** | Indicates whether the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters. This simplifies the specification of these properties versus having to declare each specifically.
**`PagingArgs`** | Indicates whether a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.
`ValueType` | The .NET value parameter `Type` for the operation. Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Create` or `Update`.
`ReturnType` | The .NET return `Type` for the operation. Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.
`ReturnTypeNullable` | Indicates whether the `ReturnType` is nullable for the operation. This is only applicable for an `Operation.Type` of `Custom`. Will be inferred where the `ReturnType` is denoted as nullable; i.e. suffixed by a `?`.
`ReturnText` | The text for use in comments to describe the `ReturnType`. A default will be created where not specified. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
`PrivateName` | The overriding private name. Overrides the `Name` to be used for private usage. By default reformatted from `Name`; e.g. `GetByArgs` as `_getByArgs`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
`AuthPermission` | The permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.
`AuthRole` | The permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.

<br/>

## Events
Provides the _Events_ configuration.

Property | Description
-|-
`EventPublish` | The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation. Valid options are: `None`, `false`, `DataSvc`, `true`, `Data`. Defaults to the `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s).
**`EventOutbox`** | The the data-tier event outbox persistence technology (where the events will be transactionally persisted in an outbox as part of the data-tier processing). Valid options are: `None`, `Database`. Defaults to `Entity.EventOutbox` configuration property (inherits) where not specified and `EventPublish` is `Data`; otherwise, `None`. A value of `Database` will result in the `DatabaseEventOutboxInvoker` being used to orchestrate.
`EventSource` | The Event Source. Defaults to `Entity.EventSource`. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`.
`EventSubject` | The event subject template and corresponding event action pair (separated by a colon). The event subject template defaults to `{AppName}.{Entity.Name}`, plus each of the unique key placeholders comma separated; e.g. `Domain.Entity.{id1},{id2}` (depending on whether `Entity.EventSubjectFormat` is `NameAndKey` or `NameOnly`). The event action defaults to `WebApiOperationType` or `Operation.Type` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. E.g. `Demo.Person.{id}:Create;Demo.Other.{id}:Update`.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
**`WebApiRoute`** | The Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.
`WebApiAuthorize` | The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`). Defaults to the `Entity.WebApiAuthorize` configuration property (inherits) where not specified.
**`WebApiMethod`** | The HTTP Method for the operation. Valid options are: `HttpGet`, `HttpPost`, `HttpPut`, `HttpDelete`. The value defaults as follows: `HttpGet` for `Operation.Type` value `Get` or `GetColl`, `HttpPost` for `Operation.Type` value `Create` or `Custom`, `HttpPut` for `Operation.Type` value `Update`, and `HttpDelete` for `Operation.Type` value `Delete`. An `Operation.Type` value `Patch` can not be specified and will always default to `HttpPatch`.
`WebApiStatus` | The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`. The value defaults as follows: `OK` for `Operation.Type` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `Operation.Type` value `Create`.
`WebApiAlternateStatus` | The primary HTTP Status Code that will be returned for the operation where there is a `null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`, `ThrowException`. The value defaults as follows: `NotFound` for `Operation.Type` value `Get`, `NoContent` for `Operation.Type` value `GetColl`, `Create`, `Update` or `Patch`; otherwise, `ThrowException` which will result in an `InvalidOperationException`.
`WebApiLocation` | The HTTP Response Location Header route. This uses similar formatting to the `WebApiRoute`. The response value is accessed using `r.` notation to access underlying properties; for example `{r.Id}` or `person/{r.Id}`. The `Entity.WebApiRoutePrefix` will be prepended automatically; however, to disable set the first character to `!`, e.g. `!person/{r.Id}`. The URI can be inferred from another `Operation` by using a lookup `^`; for example `^Get` indicates to infer from the named `Get` operation (where only `^` is specified this is shorthand for `^Get` as this is the most common value). The Location URI will ensure the first character is a `/` so it acts a 'relative URL absolute path'.
`PatchGetOperation` | The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`. Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.
`PatchUpdateOperation` | The corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`. Defaults to `Update`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
**`ManagerCustom`** | Indicates whether the `Manager` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`ManagerTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.
**`Validator`** | The name of the .NET Type that will perform the validation. Defaults to the `Entity.Validator` where not specified explicitly. Only used for `Operation.Type` options `Create` or `Update`.
`IValidator` | The name of the .NET Interface that the `Validator` implements/inherits. Defaults to the `Entity.IValidator` where specified; otherwise, defaults to `IValidator<{Type}>` where the `{Type}` is `ValueType`. Only used `Operation.Type` options `Create` or `Update`.
`ManagerOperationType` | The `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer. Valid options are: `Create`, `Read`, `Update`, `Delete`, `Unspecified`. The default will be inferred from the `Operation.Type`; however, where the `Operation.Type` is `Custom` it will default to `Unspecified`.

<br/>

## DataSvc
Provides the _Data Services-layer_ configuration.

Property | Description
-|-
**`DataSvcCustom`** | Indicates whether the `DataSvc` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`DataSvcTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.
`DataSvcExtensions` | Indicates whether the `DataSvc` extensions logic should be generated. Defaults to `Entity.ManagerExtensions`.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`AutoImplement`** | The operation override for the `Entity.AutoImplement`. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `HttpAgent`, `None`. Defaults to `Entity.AutoImplement`. The corresponding `Entity.AutoImplement` must be defined for this to be enacted. Auto-implementation is applicable for all `Operation.Type` options with the exception of `Custom`.
`DataEntityMapper` | The override for the data entity `Mapper`. Used where the default generated `Mapper` is not applicable.
`DataExtensions` | Indicates whether the `Data` extensions logic should be generated. Defaults to `Entity.DataExtensions`.
`DataTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Data`-layer. Where using an `EventOutbox` this is ignored as it is implied through its usage.
`ManagerExtensions` | Indicates whether the `Manager` extensions logic should be generated. Defaults to `Entity.ManagerExtensions`.

<br/>

## Database
Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.

Property | Description
-|-
`DatabaseStoredProc` | The database stored procedure name used where `Operation.AutoImplement` is `Database`. Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.

<br/>

## Cosmos
Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.

Property | Description
-|-
`DataCosmosContainerId` | The Cosmos `ContainerId` override used where `Operation.AutoImplement` is `Cosmos`. Overrides the `Entity.CosmosContainerId`.
`DataCosmosPartitionKey` | The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Operation.AutoImplement` is `Cosmos`. Overrides the `Entity.CosmosPartitionKey`.

<br/>

## OData
Provides the specific _OData_ configuration where `AutoImplement` is `OData`.

Property | Description
-|-
**`ODataCollectionName`** | The override name of the underlying OData collection where `Operation.AutoImplement` is `OData`. Overriddes the `Entity.ODataCollectionName`; otherwise, the underlying `Simple.OData.Client` will attempt to infer.

<br/>

## HttpAgent
Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.

Property | Description
-|-
`HttpAgentRoute` | The HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`. This is appended to the `Entity.HttpAgentRoutePrefix`.
**`HttpAgentMethod`** | The HTTP Agent Method for the operation. Valid options are: `HttpGet`, `HttpPost`, `HttpPut`, `HttpDelete`, `HttpPatch`. Defaults to `Operation.WebApiMethod`.
**`HttpAgentModel`** | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`). This can be overridden within the `Operation`(s).
`HttpAgentReturnModel` | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`). Defaults to `Operation.HttpAgentModel` where the `Operation.ReturnType` is equal to `Entity.Name` (same type). This can be overridden within the `Operation`(s).

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`Grpc`** | Indicates whether gRPC support (more specifically service-side) is required for the Operation. gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.

<br/>

## Exclude
Provides the _Exclude_ configuration.

Property | Description
-|-
`ExcludeIData` | Indicates whether to exclude the generation of the `IData` interface (`IXxxData.cs`) output.
`ExcludeData` | Indicates whether to exclude the generation of the `Data` class (`XxxData.cs`) output.
`ExcludeIDataSvc` | Indicates whether to exclude the generation of the `IDataSvc` interface (`IXxxDataSvc.cs`) output.
`ExcludeDataSvc` | Indicates whether to exclude the generation of the `DataSvc` class (`IXxxDataSvc.cs`) output.
`ExcludeIManager` | Indicates whether to exclude the generation of the `IManager` interface (`IXxxManager.cs`) output.
`ExcludeManager` | Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`) output.
`ExcludeWebApi` | Indicates whether to exclude the generation of the `XxxController` class (`IXxxController.cs`) output.
`ExcludeWebApiAgent` | Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output.
`ExcludeGrpcAgent` | Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`Parameters` | The corresponding [`Parameter`](Entity-Parameter-Config-Xml.md) collection.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
