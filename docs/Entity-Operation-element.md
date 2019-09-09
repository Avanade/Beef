# 'Operation' element (entity-driven)

The code generation for an **Operation** is primarily driven by the **`OperationType`** attribute. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as requited. Options are as follows:
- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](./Http-Patch.md)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customised operation where arguments and return value will be explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer.

An example is as follows:

```xml
<Operation Name="Get" OperationType="Get" UniqueKey="true" WebApiRoute="{id}" AutoImplement="true" />
<Operation Name="GetByArgs" OperationType="GetColl" PagingArgs="true" WebApiRoute="" AutoImplement="true" >
  <Parameter Name="Args" Type="PersonArgs" Validator="PersonArgsValidator" WebApiFrom="FromUriJsonBinder" />
</Operation>
<Operation Name="Create" OperationType="Create" Validator="PersonValidator" WebApiRoute="" AutoImplement="true" />
<Operation Name="Update" OperationType="Update" Validator="PersonValidator" UniqueKey="true" WebApiRoute="{id}" AutoImplement="true" />
<Operation Name="Delete" OperationType="Delete" UniqueKey="true" WebApiRoute="{id}" AutoImplement="true" />
<Operation Name="UpdateBatch" Text="Upserts a {{CustomerGroupCollection}} as a batch" OperationType="Custom" WebApiRoute="{company}" WebApiMethod="HttpPut">
```

<br>

## Attributes

The **`Operation`** element supports a number of attributes that control the generated code output. These attributes have been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** operation attributes: 

Attribute | Description
---|---
**`Name`** | Unique property name. This is mandatory.
**`Text`** | Text to be used in comments. Defaults the text for all the `OperationType` options except for `Custom`. To create a `<see cref="XXX"/>` use `{{XXX}}` shorthand.
**`OperationType`** | Specifies the operation type. Options are: `Get`, `GetColl`, `Create`, `Update`, `Patch`, `Delete` or `Custom`. 
**`Validator`** | Specifies the name of the .NET Type that will perform the value validation. This is only used for `OperationType` options: `Create` and `Update`. The validator is used within the `Manager` layer code.
**`UniqueKey`** | Indicates that the properties marked as a unique key are to be used as the parameters. This simplifies the specification of these properties versus having to declare each specifically.
**`PagingArgs`** | Indicates that a `PagingArgs` argument is to be added to the operation to enable paging related logic.
`ValueType` | Specifies the .NET value Type for the operation. Defaults to the parent `Entity` name. This is only used for `OperationType` options: `Create` and `Update`.
`ReturnType` | Specifies the .NET return Type for the operation. Defaults to the parent `Entity` name for `OperationType` options: `Get`, `Create` and `Update`; otherwise, defaults to `void`.
`ReturnText` | Text to be used in comments. This is only used for `OperationType` option `Custom`. To create a `<see cref="XXX"/>` use `{{XXX}}` shorthand.

<br>

### Data attributes

The following represents the **Data** attributes:

Attribute | Description
---|---
`AutoImplement` | Indicates whether the data logic for the operation is to be auto-implemented. The corresponding `Entity.AutoImplement` must be defined for this to enacted. Auto-implementation is applicable for all `OperationType` values with the exception of `Custom`.
`DatabaseStoredProc` | Specifies the database stored procedure. Used where the	`Entity.AutoImplement` is `Database` and `Operation.AutoImplement` is `true`. Defaults to `sp + Entity.Name + Operation.Name`; e.g. `spPersonCreate`.
`DataEntityMapper` | Overrides the data mapper class name.

<br>

### Event attributes

The following represents the **Event** attributes: 

Attribute | Description
---|---
`EventPublish` | Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a Create, Update or Delete. Uses `Config` value / `Entity` value (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.
`EventSubject` | Specifies the event subject template and corresponding event action pair separated by a colon. The event subject template defaults to `Config.AppName`.`Entity.Name` plus each of the unique key placeholders comma separated; e.g. `Domain.Entity.{id1},{id2}`. The event action defaults to `WebApiOperationType` or `OperationType` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. E.g. `Demo.Person.{id}:Create;Demo.Other.{id}:Update`.

<br>

### Web API attributes

The following represents the **Web API** attributes: 

Attribute | Description
---|---
`WebApiRoute` | Specifies the Web API `RouteAtttribute` to be appended to the route prefix defined for the parent `Entity`.
`WebApiMethod` | Specifies the HTTP Method for the operation. Options are `HttpGet` (default for `OperationType` values `Get` and `GetColl`), `HttpPost` (default for `OperationType` values`Create` and `Custom`), `HttpPut` (default for `OperationType` value `Update`), or `HttpDelete` (default for `OperationType` value `Delete`).
`WebApiStatus` | Specifies the primary HTTP Status Code that will be returned for the operation where there is a non-null return value. Options are: `OK`, `Accepted`, `Created`, `NoContent` and `NotFound`. Default inferred from `Operation` configuration where not specified.
`WebApiAlternateStatus` | Specifies the alternate HTTP Status Code that will be returned for the operation where there is a null return value. Options are: `OK`, `Accepted`, `Created`, `NoContent` and `NotFound`. Default inferred from `Operation` configuration where not specified. Additional option is `ThrowException` which will result in an `InvalidOperationException` being thrown when there is no return value.
`WebApiOperationType` | Specifies (overrides) the `ExecutionContext.OperationType` (CRUD denotation) where it cannot be inferred from the `OperationType`. Options are `Create`, `Read`, `Update`, `Delete` and `Unspecified`.
`PagingArgsParams` | Specifies the `PagingArgs` parameters to specifically use for the WebApi Controller. Options are: `PageNumberSize` (default), `PageBookmarkSize`, or `PageNumberBookmarkSize`.

<br>

### Authorization attributes

The following represents the attributes that control the **authorization** code generation within the **Manager** layer:

Attribute | Description
---|---
`AuthPermission` | Specifies the permission of the `ExecutionContext.IsAuthorized(permission)`.
`AuthEntity` | Specifies the entity (permission) of the `ExecutionContext.IsAuthorized(entity, action)`. Defaults to the entity name where not specified.
`AuthAction` | Specifies the action (permission) of the `ExecutionContext.IsAuthorized(entity, action)`.

<br>

### Layer option attributes

The following represents the **layer option** attributes:

Attribute | Description
---|---
`ManagerCustom` | Indicates that the `Manager` logic is a custom implementation; i.e. no `DataSvc` invocation logic is to be generated.
`ManagerTransaction` | Indicates that a `System.TransactionScope` should be created around the `Manager` logic.
`DataSvcCustom` | Indicates that the `DataSvc` logic is a custom implementation; i.e. no `Data` invocation logic is to be generated.
`DataSvcTransaction` | Indicates that a `System.TransactionScope` should be created around the `DataSvc` logic.

<br>

### Exclusion attributes

The following represents the attributes for controlling (selecting) which specific code **artefacts** are to be generated; where none are specified all artefacts are implied:

Attribute | Description
---|---
**`ExcludeAll`** | Indicates whether to exclude the creation of all Operations versus specifying each layer (below).
`ExcludeIData` | Indicates whether to exclude the creation of the Data interface (`IXxxData.cs`).
`ExcludeData` | Indicates whether to exclude the creation of the Data class (`XxxData.cs`).
`ExcludeDataSvc` | Indicates whether to exclude the creation of the DataSvc class (`XxxDataSvc.cs`).
`ExcludeIManager` | Indicates whether to exclude the creation of the Manager interface (`IXxxManager.cs`)
`ExcludeManager` | Indicates whether to exclude the creation of the Manager class (`XxxManager.cs`)
`ExcludeWebApi` | Indicates whether to exclude the creation of the Web API Controller class (`XxxController.cs`)
`ExcludeWebApiAgent` | Indicates whether to exclude the creation of the Web API Agent class (`XxxAgent.cs`)
