# 'CodeGeneration' object (entity-driven)

The code generation for an `Operation` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as required.

The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/Merge/JsonMergePatch.cs)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customized operation where parameters and return value are explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer. This is the default where not specified.

<br/>

## Example

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
operations: [
  { name: Get, type: Get, primaryKey: true, webApiRoute: '{id}', autoImplement: None },
  { name: Create, type: Create, webApiRoute: , autoImplement: None },
  { name: Update, type: Update, primaryKey: true, webApiRoute: '{id}', autoImplement: None },
  { name: Patch, type: Patch, primaryKey: true, webApiRoute: '{id}' },
  { name: Delete, type: Delete, webApiRoute: '{id}',
    parameters: [
      { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
    ]
  }
]
```

<br/>

## Property categories
The `Operation` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

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
[`EntityFramework`](#EntityFramework) | Provides the specific _EntityFramework_ configuration where `AutoImplement` is `EntityFramework`.
[`Cosmos`](#Cosmos) | Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.
[`OData`](#OData) | Provides the specific _OData_ configuration where `AutoImplement` is `OData`.
[`HttpAgent`](#HttpAgent) | Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.
[`Exclude`](#Exclude) | Provides the _Exclude_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique operation name. [Mandatory]
**`type`** | The type of operation that is to be code-generated. Valid options are: `Get`, `GetColl`, `Create`, `Update`, `Patch`, `Delete`, `Custom`.<br/>&dagger; Defaults to `Custom`.
`text` | The text for use in comments.<br/>&dagger; The `Text` will be defaulted for all the `Operation.Type` options with the exception of `Custom`. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
**`primaryKey`** | Indicates whether the properties marked as a primary key (`Property.PrimaryKey`) are to be used as the parameters.<br/>&dagger; This simplifies the specification of these properties as parameters versus having to declare each specifically. Each of the parameters will also be set to be mandatory.
**`paging`** | Indicates whether a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.
`valueType` | The .NET value parameter `Type` for the operation.<br/>&dagger; Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Create` or `Update`.
`returnType` | The .NET return `Type` for the operation.<br/>&dagger; Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.
`returnTypeNullable` | Indicates whether the `ReturnType` is nullable for the operation.<br/>&dagger; This is only applicable for an `Operation.Type` of `Custom`. Will be inferred where the `ReturnType` is denoted as nullable; i.e. suffixed by a `?`.
`returnText` | The text for use in comments to describe the `ReturnType`.<br/>&dagger; A default will be created where not specified. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
`privateName` | The overriding private name.<br/>&dagger; Overrides the `Name` to be used for private usage. By default reformatted from `Name`; e.g. `GetByArgs` as `_getByArgs`.
`withResult` | Indicates whether to use `CoreEx.Results` (aka Railway-oriented programming).<br/>&dagger; Defaults to `Entity.WilhResult`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
`authPermission` | The permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.
`authRole` | The permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.

<br/>

## Events
Provides the _Events_ configuration.

Property | Description
-|-
**`eventPublish`** | The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation. Valid options are: `None`, `DataSvc`, `Data`.<br/>&dagger; Defaults to the `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.
`eventSource` | The Event Source.<br/>&dagger; Defaults to `Entity.EventSource`. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. Otherwise, specify the C# string interpolation expression; for example: `person/{r.Id}`.
`eventSubject` | The event subject template and corresponding event action pair (separated by a colon).<br/>&dagger; The event subject template defaults to `{AppName}.{Entity.Name}`, plus each of the unique key placeholders comma separated; e.g. `Domain.Entity.{id1},{id2}` (depending on whether `Entity.EventSubjectFormat` is `NameAndKey` or `NameOnly`). The event action defaults to `WebApiOperationType` or `Operation.Type` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. E.g. `Demo.Person.{id}:Create;Demo.Other.{id}:Update`.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
**`webApiRoute`** | The Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.<br/>&dagger; Where the value is specified with a leading `!` character this indicates that the `Entity.WebApiRoutePrefix` should not be used, and the value should be used as-is (with the `!` removed).
`webApiAuthorize` | The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.<br/>&dagger; Where not specified no attribute output will occur; it will then inherit as supported by .NET.
**`webApiMethod`** | The HTTP Method for the operation. Valid options are: `HttpGet`, `HttpPost`, `HttpPut`, `HttpDelete`.<br/>&dagger; The value defaults as follows: `HttpGet` for `Operation.Type` value `Get` or `GetColl`, `HttpPost` for `Operation.Type` value `Create` or `Custom`, `HttpPut` for `Operation.Type` value `Update`, and `HttpDelete` for `Operation.Type` value `Delete`. An `Operation.Type` value `Patch` can not be specified and will always default to `HttpPatch`.
`webApiStatus` | The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`.<br/>&dagger; The value defaults as follows: `OK` for `Operation.Type` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `Operation.Type` value `Create`.
`webApiAlternateStatus` | The primary HTTP Status Code that will be returned for the operation where there is a `null` return value. Valid options are: `OK`, `Accepted`, `Created`, `NoContent`, `NotFound`.<br/>&dagger; The value defaults as follows: `NotFound` for `Operation.Type` value `Get` and `NoContent` for `Operation.Type` value `GetColl`; otherwise, `null`.
`webApiLocation` | The HTTP Response Location Header route.<br/>&dagger; This uses similar formatting to the `WebApiRoute`. The response value is accessed using `r.` notation to access underlying properties; for example `{r.Id}` or `person/{r.Id}`. The `Entity.WebApiRoutePrefix` will be prepended automatically; however, to disable set the first character to `!`, e.g. `!person/{r.Id}`. The URI can be inferred from another `Operation` by using a lookup `^`; for example `^Get` indicates to infer from the named `Get` operation (where only `^` is specified this is shorthand for `^Get` as this is the most common value). The Location URI will ensure the first character is a `/` so it acts a 'relative URL absolute path'.
`webApiConcurrency` | Indicates whether the Web API is responsible for managing (simulating) concurrency via auto-generated ETag.<br/>&dagger; This provides an alternative where the underlying data source does not natively support optimistic concurrency (native support should always be leveraged as a priority). Where the `Operation.Type` is `Update` or `Patch`, the request ETag will be matched against the response for a corresponding `Get` operation to verify no changes have been made prior to updating. For this to function correctly the .NET response Type for the `Get` must be the same as that returned from the corresponding `Create`, `Update` and `Patch` (where applicable) as the generated ETag is a SHA256 hash of the resulting JSON. Defaults to `Entity.WebApiConcurrency`.
`webApiGetOperation` | The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Update` and `SimulateConcurrency` is `true`.<br/>&dagger; Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.
`webApiUpdateOperation` | The corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.<br/>&dagger; Defaults to `Update`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.
`webApiProduces` | The value(s) for the optional `[Produces()]` attribute for the operation within the Web Api Controller for the Swagger/OpenAPI documentation.
`webApiProducesResponseType` | The `[ProducesResponseType()]` attribute `typeof` for the operation within the Web Api Controller for the Swagger/OpenAPI documentation.<br/>&dagger; Defaults to the _Common_ type. A value of `None`, `none` or `` will ensure no type is emitted.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
**`managerCustom`** | Indicates whether the `Manager` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`managerTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.
**`validator`** | The name of the .NET implementing `Type` or interface `Type` that will perform the validation.<br/>&dagger; Defaults to the `Entity.Validator` where not specified explicitly (where `Operation.Type` options `Create` or `Update`).
`validationFramework` | The `Validation` framework to use for the entity-based validation. Valid options are: `CoreEx`, `FluentValidation`.<br/>&dagger; Defaults to `Entity.ValidationFramework`. This can be overridden within the `Parameter`(s).
`managerOperationType` | The `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer. Valid options are: `Create`, `Read`, `Update`, `Delete`, `Unspecified`.<br/>&dagger; The default will be inferred from the `Operation.Type`; however, where the `Operation.Type` is `Custom` it will default to `Unspecified`.
`managerCleanUp` | Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.<br/>&dagger; This can be overridden within the `CodeGeneration` and `Entity`.

<br/>

## DataSvc
Provides the _Data Services-layer_ configuration.

Property | Description
-|-
**`dataSvcCustom`** | Indicates whether the `DataSvc` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
`dataSvcTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.
`dataSvcInvoker` | Indicates whether a `DataSvcInvoker` should orchestrate the `DataSvc`-layer.<br/>&dagger; Where `DataSvcTransaction` or `EventPublish` is `DataSvc` then orchestration will default to `true`.
`dataSvcExtensions` | Indicates whether the `DataSvc` extensions logic should be generated.<br/>&dagger; Defaults to `Entity.ManagerExtensions`.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`autoImplement`** | The operation override for the `Entity.AutoImplement`. Valid options are: `Database`, `EntityFramework`, `Cosmos`, `OData`, `HttpAgent`, `None`.<br/>&dagger; Defaults to `Entity.AutoImplement`. The corresponding `Entity.AutoImplement` must be defined for this to be enacted. Auto-implementation is applicable for all `Operation.Type` options with the exception of `Custom`.
`dataEntityMapper` | The override for the data entity `Mapper`.<br/>&dagger; Used where the default generated `Mapper` is not applicable.
`dataExtensions` | Indicates whether the `Data` extensions logic should be generated.<br/>&dagger; Defaults to `Entity.DataExtensions`.
`dataInvoker` | Indicates whether a `DataInvoker` should orchestrate the `Data`-layer.<br/>&dagger; Where `Dataransaction` or `EventPublish` is `Data` then orchestration will default to `true`.
`dataTransaction` | Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Data`-layer.<br/>&dagger; Where using an `EventOutbox` this is ignored as it is implied through its usage.
`managerExtensions` | Indicates whether the `Manager` extensions logic should be generated.<br/>&dagger; Defaults to `Entity.ManagerExtensions`.

<br/>

## Database
Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.

Property | Description
-|-
`databaseStoredProc` | The database stored procedure name used where `Operation.AutoImplement` is `Database`.<br/>&dagger; Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.

<br/>

## EntityFramework
Provides the specific _EntityFramework_ configuration where `AutoImplement` is `EntityFramework`.

Property | Description
-|-
**`entityFrameworkModel`** | The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).<br/>&dagger; Overrides the `Entity.EntityFrameworkModel`.

<br/>

## Cosmos
Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.

Property | Description
-|-
**`cosmosModel`** | The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).<br/>&dagger; Overrides the `Entity.CosmosModel`.
`cosmosContainerId` | The Cosmos `ContainerId` override used where `Operation.AutoImplement` is `Cosmos`.<br/>&dagger; Overrides the `Entity.CosmosContainerId`.
`cosmosPartitionKey` | The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Operation.AutoImplement` is `Cosmos`.<br/>&dagger; Overrides the `Entity.CosmosPartitionKey`.

<br/>

## OData
Provides the specific _OData_ configuration where `AutoImplement` is `OData`.

Property | Description
-|-
**`odataCollectionName`** | The override name of the underlying OData collection where `Operation.AutoImplement` is `OData`.<br/>&dagger; Overriddes the `Entity.ODataCollectionName`; otherwise, the underlying `Simple.OData.Client` will attempt to infer.

<br/>

## HttpAgent
Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.

Property | Description
-|-
`httpAgentRoute` | The HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; This is appended to the `Entity.HttpAgentRoutePrefix`.
**`httpAgentMethod`** | The HTTP Agent Method for the operation. Valid options are: `HttpGet`, `HttpPost`, `HttpPut`, `HttpDelete`, `HttpPatch`.<br/>&dagger; Defaults to `Operation.WebApiMethod`.
**`httpAgentModel`** | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).<br/>&dagger; This can be overridden within the `Operation`(s).
`httpAgentReturnModel` | The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).<br/>&dagger; Defaults to `Operation.HttpAgentModel` where the `Operation.ReturnType` is equal to `Entity.Name` (same type). This can be overridden within the `Operation`(s).
`httpAgentCode` | The fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.<br/>&dagger; Appended to `Entity.HttpAgentCode` where specified to extend.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`grpc`** | Indicates whether gRPC support (more specifically service-side) is required for the Operation.<br/>&dagger; gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.

<br/>

## Exclude
Provides the _Exclude_ configuration.

Property | Description
-|-
**`excludeAll`** | Indicates whether to exclude the generation of all `Operation` related output.<br/>&dagger; Is a shorthand means for setting all of the other `Exclude*` properties to `true`.
`excludeIData` | Indicates whether to exclude the generation of the operation within the `Data` interface (`IXxxData.cs`) output.
`excludeData` | Indicates whether to exclude the generation of the operation within the `Data` class (`XxxData.cs`) output.
`excludeIDataSvc` | Indicates whether to exclude the generation of the operation within the `DataSvc` interface (`IXxxDataSvc.cs`) output.
`excludeDataSvc` | Indicates whether to exclude the generation of the operation within the `DataSvc` class (`XxxDataSvc.cs`) output.
`excludeIManager` | Indicates whether to exclude the generation of the operation within the `Manager` interface (`IXxxManager.cs`) output.
`excludeManager` | Indicates whether to exclude the generation of the operation within the `Manager` class (`XxxManager.cs`) output.
`excludeWebApi` | Indicates whether to exclude the generation of the operation within the WebAPI `Controller` class (`XxxController.cs`) output.
`excludeWebApiAgent` | Indicates whether to exclude the generation of the operation within the WebAPI consuming `Agent` class (`XxxAgent.cs`) output.
`excludeGrpcAgent` | Indicates whether to exclude the generation of the operation within the gRPC consuming `Agent` class (`XxxAgent.cs`) output.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`parameters` | The corresponding [`Parameter`](Entity-Parameter-Config.md) collection.

