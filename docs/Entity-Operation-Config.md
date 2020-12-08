# 'CodeGeneration' object (entity-driven) - YAML/JSON

The code generation for an `Operation` is primarily driven by the `OperationType` attribute. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as required.


The valid `OperationType` values are as follows:

- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](./Http-Patch.md)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customised operation where arguments and return value will be explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer.

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
operations: [
  { name: Get, type: Get, uniqueKey: true, webApiRoute: '{id}', autoImplement: None },
  { name: Create, type: Create, webApiRoute: , autoImplement: None },
  { name: Update, type: Update, uniqueKey: true, webApiRoute: '{id}', autoImplement: None },
  { name: Patch, type: Patch, uniqueKey: true, webApiRoute: '{id}' },
  { name: Delete, type: Delete, webApiRoute: '{id}',
    parameters: [
      { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
    ]
  }
]
```

<br/>

## Property categories
The `Operation` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`WebApi`](#WebApi) | Provides the data _Web API_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`DataSvc`](#DataSvc) | Provides the _Data Services-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Exclude`](#Exclude) | Provides the _Exclude_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique operation name.
**`type`** | The operation type. Valid options are: `Get`, `GetColl`, `Create`, `Update`, `Patch`, `Delete`, `Custom`.
`text` | The text for use in comments. The `Text` will be defaulted for all the `Operation.Type` options with the exception of `Custom`. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
**`validator`** | The name of the .NET Type that will perform the validation. Defaults to the `Entity.Validator` where not specified explicitly. Only used for `Operation.Type` options `Create` or `Update`.
**`uniqueKey`** | Indicates whether the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters. This simplifies the specification of these properties versus having to declare each specifically.
**`paging`** | Indicates whether a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.
`valueType` | The .NET value parameter `Type` for the operation. Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Create` or `Update`.
`returnType` | The .NET return `Type` for the operation. Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.
`returnTypeNullable` | Indicates whether the `ReturnType` is nullable for the operation. This is only applicable for an `Operation.Type` of `Custom`. Will be inferred where the `ReturnType` is denoted as nullable; i.e. suffixed by a `?`.
`returnText` | The text for use in comments to describe the `ReturnType`. A default will be created where not specified. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
`privateName` | The overriding private name. Overrides the `Name` to be used for private usage. By default reformatted from `Name`; e.g. `GetByArgs` as `_getByArgs`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
`authPermission` | The permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.
`authRole` | The permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
**`webApiRoute`** | The Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.
`webApiAuthorize` | The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`. Where not specified no attribute output will occur; it will then inherit as supported by .NET.
**`webApiMethod`** | The HTTP Method for the operation. Valid options are: `HttpGet`, `HttpPost`, `HttpPut`, `HttpDelete`. The value defaults as follows: `HttpGet` for `Operation.Type` value `Get` or `GetColl`, `HttpPost` for `Operation.Type` value `Create` or `Custom`, `HttpPut` for `Operation.Type` value `Update`, and `HttpDelete` for `Operation.Type` value `Delete`. An `Operation.Type` value `Patch` can not be specified and will always default to `HttpPatch`.
`webApiStatus` | The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`. The value defaults as follows: `OK` for `Operation.Type` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `Operation.Type` value `Create`.
`webApiAlternateStatus` | The primary HTTP Status Code that will be returned for the operation where there is a `null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`, `ThrowException`. The value defaults as follows: `NotFound` for `Operation.Type` value `Get`, `NoContent` for `Operation.Type` value `GetColl`, `Create`, `Update` or `Patch`; otherwise, `ThrowException` which will result in an `InvalidOperationException`.
`webApiOperationType` | The `ExecutionContext.OperationType` (CRUD denotation) where the `Operation.Type` is `Custom` (i.e. can not be inferred). Valid options are: `Create`, `Read`, `Update`, `Delete`, `Unspecified`. The default will be inferred where possible; otherwise, set to `Unspecified`.
`patchGetOperation` | The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`. Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.
`patchUpdateOperation` | The corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`. Defaults to `Update`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
**`managerCustom`** | Indicates whether the `Manager` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`managerTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.

<br/>

## DataSvc
Provides the _Data Services-layer_ configuration.

Property | Description
-|-
**`dataSvcCustom`** | Indicates whether the `DataSvc` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`dataSvcTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.
`eventPublish` | Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a `Create`, `Update` or `Delete` operation. Defaults to the `CodeGeneration.EventPublish` or `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.
`eventSubject` | The event subject template and corresponding event action pair (separated by a colon). The event subject template defaults to `{AppName}.{Entity.Name}` plus each of the unique key placeholders comma separated; e.g. `Domain.Entity.{id1},{id2}`. The event action defaults to `WebApiOperationType` or `Operation.Type` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. E.g. `Demo.Person.{id}:Create;Demo.Other.{id}:Update`.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`autoImplement`** | The operation override for the `Entity.AutoImplement`. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `None`. Defaults to `Entity.AutoImplement`. The corresponding `Entity.AutoImplement` must be defined for this to be enacted. Auto-implementation is applicable for all `Operation.Type` options with the exception of `Custom`.
`dataEntityMapper` | The override for the data entity `Mapper`. Used where the default generated `Mapper` is not applicable.
`databaseStoredProc` | The database stored procedure name used where `Operation.AutoImplement` is `Database`. Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.
`cosmosContainerId` | The Cosmos `ContainerId` override used where `Operation.AutoImplement` is `Cosmos`. Overrides the `Entity.CosmosContainerId`.
`cosmosPartitionKey` | The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Operation.AutoImplement` is `Cosmos`. Overrides the `Entity.CosmosPartitionKey`.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`grpc`** | Indicates whether gRPC support (more specifically service-side) is required for the Operation. gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.

<br/>

## Exclude
Provides the _Exclude_ configuration.

Property | Description
-|-
**`excludeAll`** | The option to exclude the generation of all `Operation` related output. Valid options are: `No`, `Yes`. Is a shorthand means for setting all of the other `Exclude*` properties to `true`.
`excludeIData` | The option to exclude the generation of the operation within the `Data` interface (`IXxxData.cs`) output. Valid options are: `No`, `Yes`.
`excludeData` | The option to exclude the generation of the operation within the `Data` class (`XxxData.cs`) output. Valid options are: `No`, `Yes`.
`excludeIDataSvc` | The option to exclude the generation of the operation within the `DataSvc` interface (`IXxxDataSvc.cs`) output. Valid options are: `No`, `Yes`.
`excludeDataSvc` | The option to exclude the generation of the operation within the `DataSvc` class (`XxxDataSvc.cs`) output. Valid options are: `No`, `Yes`.
`excludeIManager` | The option to exclude the generation of the operation within the `Manager` interface (`IXxxManager.cs`) output. Valid options are: `No`, `Yes`.
`excludeManager` | The option to exclude the generation of the operation within the `Manager` class (`XxxManager.cs`) output. Valid options are: `No`, `Yes`.
`excludeWebApi` | The option to exclude the generation of the operation within the WebAPI `Controller` class (`XxxController.cs`) output. Valid options are: `No`, `Yes`.
`excludeWebApiAgent` | The option to exclude the generation of the operation within the WebAPI consuming `Agent` class (`XxxAgent.cs`) output. Valid options are: `No`, `Yes`.
`excludeGrpcAgent` | The option to exclude the generation of the operation within the gRPC consuming `Agent` class (`XxxAgent.cs`) output. Valid options are: `No`, `Yes`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`parameters` | The corresponding [`Parameter`](Entity-Parameter-Config.md) collection.

<br/>

