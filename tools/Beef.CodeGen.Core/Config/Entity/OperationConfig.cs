// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Operation</b> code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Operation", Title = "'CodeGeneration' object (entity-driven)",
        Description = "The code generation for an `Operation` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as required.", 
        Markdown = @"The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](./Http-Patch.md)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customised operation where arguments and return value will be explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer.",
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
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
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CategorySchema("Events", Title = "Provides the _Events_ configuration.")]
    [CategorySchema("WebApi", Title = "Provides the data _Web API_ configuration.")]
    [CategorySchema("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CategorySchema("DataSvc", Title = "Provides the _Data Services-layer_ configuration.")]
    [CategorySchema("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CategorySchema("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CategorySchema("Exclude", Title = "Provides the _Exclude_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class OperationConfig : ConfigBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Operation", Name);

        #region Key

        /// <summary>
        /// Gets or sets the unique property name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique operation name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the type of operation that is to be code-generated.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The type of operation that is to be code-generated.", IsMandatory = true, IsImportant = true,
            Options = new string[] { "Get", "GetColl", "Create", "Update", "Patch", "Delete", "Custom" })]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The text for use in comments.",
            Description = "The `Text` will be defaulted for all the `Operation.Type` options with the exception of `Custom`. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? Text { get; set; }

        /// <summary>
        /// Indicates whether the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters. 
        /// </summary>
        [JsonProperty("uniqueKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters.", IsImportant = true,
            Description = "This simplifies the specification of these properties versus having to declare each specifically.")]
        public bool? UniqueKey { get; set; }

        /// <summary>
        /// Indicates whether a PagingArgs argument is to be added to the operation to enable paging related logic.
        /// </summary>
        [JsonProperty("paging", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.", IsImportant = true)]
        public bool? Paging { get; set; }

        /// <summary>
        /// Gets or sets the .NET value parameter <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("valueType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The .NET value parameter `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Create` or `Update`.")]
        public string? ValueType { get; set; }

        /// <summary>
        /// Gets or sets the .NET return <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("returnType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The .NET return `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.")]
        public string? ReturnType { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="ReturnType"/> is nullable for the operation.
        /// </summary>
        [JsonProperty("returnTypeNullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the `ReturnType` is nullable for the operation.",
            Description = "This is only applicable for an `Operation.Type` of `Custom`. Will be inferred where the `ReturnType` is denoted as nullable; i.e. suffixed by a `?`.")]
        public bool? ReturnTypeNullable { get; set; }

        /// <summary>
        /// Gets or sets the text for use in comments to describe the <see cref="ReturnType"/>.
        /// </summary>
        [JsonProperty("returnText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The text for use in comments to describe the `ReturnType`.",
            Description = "A default will be created where not specified. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? ReturnText { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonProperty("privateName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private usage. By default reformatted from `Name`; e.g. `GetByArgs` as `_getByArgs`.")]
        public string? PrivateName { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the operation override for the `Entity.AutoImplement`.
        /// </summary>
        [JsonProperty("autoImplement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The operation override for the `Entity.AutoImplement`.", IsImportant = true, Options = new string[] { "Database", "EntityFramework", "Cosmos", "OData", "None" },
            Description = "Defaults to `Entity.AutoImplement`. The corresponding `Entity.AutoImplement` must be defined for this to be enacted. Auto-implementation is applicable for all `Operation.Type` options with the exception of `Custom`.")]
        public string? AutoImplement { get; set; }

        /// <summary>
        /// Gets or sets the override for the data entity <c>Mapper</c>.
        /// </summary>
        [JsonProperty("dataEntityMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The override for the data entity `Mapper`.",
            Description = "Used where the default generated `Mapper` is not applicable.")]
        public string? DataEntityMapper { get; set; }

        /// <summary>
        /// Indicates whether the `Data` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the `Data` extensions logic should be generated.",
            Description = "Defaults to `Entity.DataExtensions`.")]
        public bool? DataExtensions { get; set; }

        /// <summary>
        /// Gets or sets the database stored procedure name.
        /// </summary>
        [JsonProperty("databaseStoredProc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The database stored procedure name used where `Operation.AutoImplement` is `Database`.",
            Description = "Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.")]
        public string? DatabaseStoredProc { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos <c>ContainerId</c> override.
        /// </summary>
        [JsonProperty("cosmosContainerId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The Cosmos `ContainerId` override used where `Operation.AutoImplement` is `Cosmos`.",
            Description = "Overrides the `Entity.CosmosContainerId`.")]
        public string? CosmosContainerId { get; set; }

        /// <summary>
        /// Gets or sets the C# code override to be used for setting the optional Cosmos <c>PartitionKey</c>.
        /// </summary>
        [JsonProperty("cosmosPartitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Operation.AutoImplement` is `Cosmos`.",
            Description = "Overrides the `Entity.CosmosPartitionKey`.")]
        public string? CosmosPartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the override name of the underlying OData collection name where <see cref="OperationConfig.AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataCollectionName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The override name of the underlying OData collection where `Operation.AutoImplement` is `OData`.", IsImportant = true,
            Description = "Overriddes the `Entity.ODataCollectionName`; otherwise, the underlying `Simple.OData.Client` will attempt to infer.")]
        public string? ODataCollectionName { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Indicates whether the `Manager`-layer is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
        /// </summary>
        [JsonProperty("managerCustom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "Indicates whether the `Manager` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.", IsImportant = true)]
        public bool? ManagerCustom { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.
        /// </summary>
        [JsonProperty("managerTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.")]
        public bool? ManagerTransaction { get; set; }

        /// <summary>
        /// Indicates whether the `Manager` extensions logic should be generated.
        /// </summary>
        [JsonProperty("managerExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the `Manager` extensions logic should be generated.",
            Description = "Defaults to `Entity.ManagerExtensions`.")]
        public bool? ManagerExtensions { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET Type that will perform the validation.", IsImportant = true,
            Description = "Defaults to the `Entity.Validator` where not specified explicitly. Only used for `Operation.Type` options `Create` or `Update`.")]
        public string? Validator { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Interface that the `Validator` implements/inherits.
        /// </summary>
        [JsonProperty("iValidator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET Interface that the `Validator` implements/inherits.",
            Description = "Defaults to the `Entity.IValidator` where specified; otherwise, defaults to `IValidator<{Type}>` where the `{Type}` is `ValueType`. Only used `Operation.Type` options `Create` or `Update`.")]
        public string? IValidator { get; set; }

        /// <summary>
        /// Gets or sets the `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer.
        /// </summary>
        [JsonProperty("managerOperationType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer.", Options = new string[] { "Create", "Read", "Update", "Delete", "Unspecified" },
            Description = "The default will be inferred from the `Operation.Type`; however, where the `Operation.Type` is `Custom` it will default to `Unspecified`.")]
        public string? ManagerOperationType { get; set; }

        #endregion

        #region DataSvc

        /// <summary>
        /// Indicates whether the `DataSvc`-layer is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
        /// </summary>
        [JsonProperty("dataSvcCustom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether the `DataSvc` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.", IsImportant = true)]
        public bool? DataSvcCustom { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.
        /// </summary>
        [JsonProperty("dataSvcTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.")]
        public bool? DataSvcTransaction { get; set; }

        /// <summary>
        /// Indicates whether the `DataSvc` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataSvcExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether the `DataSvc` extensions logic should be generated.",
            Description = "Defaults to `Entity.ManagerExtensions`.")]
        public bool? DataSvcExtensions { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = new string[] { "None", "DataSvc", "Data" },
            Description = "Defaults to the `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonProperty("eventSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The Event Source.",
            Description = "Defaults to `Entity.EventSource`. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. " +
            "To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. This can be overridden for the `Entity`.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Gets or sets the event subject template and corresponding event action pair (separated by a colon).
        /// </summary>
        [JsonProperty("eventSubject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The event subject template and corresponding event action pair (separated by a colon).",
            Description = "The event subject template defaults to `{AppName}.{Entity.Name}`, plus each of the unique key placeholders comma separated; e.g. `Domain.Entity.{id1},{id2}` (depending on whether `Entity.EventSubjectFormat` is `NameAndKey` or `NameOnly`). " +
            "The event action defaults to `WebApiOperationType` or `Operation.Type` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. " +
            "E.g. `Demo.Person.{id}:Create;Demo.Other.{id}:Update`.")]
        public string? EventSubject { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.
        /// </summary>
        [JsonProperty("webApiRoute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.", IsImportant = true)]
        public string? WebApiRoute { get; set; }

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.",
            Description = "Where not specified no attribute output will occur; it will then inherit as supported by .NET.")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Method for the operation.
        /// </summary>
        [JsonProperty("webApiMethod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The HTTP Method for the operation.", IsImportant = true, Options = new string[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete" },
            Description = "The value defaults as follows: `HttpGet` for `Operation.Type` value `Get` or `GetColl`, `HttpPost` for `Operation.Type` value `Create` or `Custom`, " +
            "`HttpPut` for `Operation.Type` value `Update`, and `HttpDelete` for `Operation.Type` value `Delete`. An `Operation.Type` value `Patch` can not be specified and will always default to `HttpPatch`.")]
        public string? WebApiMethod { get; set; }

        /// <summary>
        /// Gets or sets the primary HTTP Status Code that will be returned for the operation where there is a non-null return value. 
        /// </summary>
        [JsonProperty("webApiStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value.", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound" },
            Description = "The value defaults as follows: `OK` for `Operation.Type` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `Operation.Type` value `Create`.")]
        public string? WebApiStatus { get; set; }

        /// <summary>
        /// Gets or sets the alternate HTTP Status Code that will be returned for the operation where there is a null return value. 
        /// </summary>
        [JsonProperty("webApiAlternateStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a `null` return value.", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound", "ThrowException" },
            Description = "The value defaults as follows: `NotFound` for `Operation.Type` value `Get`, `NoContent` for `Operation.Type` value `GetColl`, `Create`, `Update` or `Patch`; otherwise, `ThrowException` which will result in an `InvalidOperationException`.")]
        public string? WebApiAlternateStatus { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Response Location Header route. 
        /// </summary>
        [JsonProperty("webApiLocation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The HTTP Response Location Header route.",
            Description = "This uses similar formatting to the `WebApiRoute`. The response value is accessed using `r.` notation to access underlying properties; for example `{r.Id}` or `person/{r.Id}`. The `Entity.WebApiRoutePrefix` will be prepended automatically; however, to disable set the first character to `!`, e.g. `!person/{r.Id}`. " +
            "The URI can be inferred from another `Operation` by using a lookup `^`; for example `^Get` indicates to infer from the named `Get` operation (where only `^` is specified this is shorthand for `^Get` as this is the most common value). The Location URI will ensure the first character is a `/` so it acts a 'relative URL absolute path'.")]
        public string? WebApiLocation { get; set; }

        /// <summary>
        /// Gets or sets the override for the corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.
        /// </summary>
        [JsonProperty("patchGetOperation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.",
            Description = "Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.")]
        public string? PatchGetOperation { get; set; }

        /// <summary>
        /// Gets or sets the override for the corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.
        /// </summary>
        [JsonProperty("patchUpdateOperation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.",
            Description = "Defaults to `Update`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.")]
        public string? PatchUpdateOperation { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.
        /// </summary>
        [JsonProperty("authPermission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.")]
        public string? AuthPermission { get; set; }

        /// <summary>
        /// Gets or sets the permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.
        /// </summary>
        [JsonProperty("authRole", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.")]
        public string? AuthRole { get; set; }

        #endregion

        #region Exclude

        /// <summary>
        /// The option to exclude the generation of <b>all</b> <c>Operation</c> related output.
        /// </summary>
        [JsonProperty("excludeAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of all `Operation` related output.", IsImportant = true, Options = new string[] { NoOption, YesOption },
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties to `true`.")]
        public string? ExcludeAll { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeIData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `Data` interface (`IXxxData.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeIData { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `Data` class (`XxxData.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeData { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeIDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `DataSvc` interface (`IXxxDataSvc.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `DataSvc` class (`XxxDataSvc.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeDataSvc { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeIManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `Manager` interface (`IXxxManager.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeIManager { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the `Manager` class (`XxxManager.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeManager { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the WebAPI `Controller` class (`XxxController.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeWebApi { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApiAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the WebAPI consuming `Agent` class (`XxxAgent.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// The option to exclude the generation of the operation within the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeGrpcAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the operation within the gRPC consuming `Agent` class (`XxxAgent.cs`) output.", Options = new string[] { NoOption, YesOption })]
        public string? ExcludeGrpcAgent { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required for the Operation.
        /// </summary>
        [JsonProperty("grpc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Operation.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="ParameterConfig"/> collection.
        /// </summary>
        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Parameter` collection.")]
        public List<ParameterConfig>? Parameters { get; set; }

        #endregion 

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection filtered for data access.
        /// </summary>
        public List<ParameterConfig> DataParameters => Parameters!.Where(x => !x.LayerPassing!.StartsWith("ToManager", StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection filtered for data access without value.
        /// </summary>
        public List<ParameterConfig> ValueLessDataParameters => DataParameters!.Where(x => CompareNullOrValue(x.IsValueArg, false)).ToList();

        /// <summary>
        /// Gets the <see cref="DataParameters"/> without the paging parameter.
        /// </summary>
        public List<ParameterConfig> PagingLessDataParameters => DataParameters!.Where(x => CompareNullOrValue(x.IsPagingArgs, false)).ToList();

        /// <summary>
        /// Gets the <see cref="DataParameters"/> without the value and paging parameters.
        /// </summary>
        public List<ParameterConfig> CoreDataParameters => ValueLessDataParameters!.Where(x => CompareNullOrValue(x.IsPagingArgs, false)).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection filtered for validation.
        /// </summary>
        public List<ParameterConfig> ValidateParameters => Parameters!.Where(x => CompareValue(x.IsMandatory, true) || x.Validator != null || x.ValidatorCode != null).OrderBy(x => x.IsValueArg).ToList();

        /// <summary>
        /// Indicates whether there is only a single parameter to be validated.
        /// </summary>
        public bool SingleValidateParameters => CompareNullOrValue(Parent!.ManagerExtensions, false) && ValidateParameters.Count <= 1; 

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection without the value parameter.
        /// </summary>
        public List<ParameterConfig>? ValueLessParameters => Parameters!.Where(x => !x.IsValueArg).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection without the paging parameter.
        /// </summary>
        public List<ParameterConfig>? PagingLessParameters => Parameters!.Where(x => !x.IsPagingArgs).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection without the value and paging parameter.
        /// </summary>
        public List<ParameterConfig>? CoreParameters => ValueLessParameters!.Where(x => !x.IsPagingArgs).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection without parameters that do not need cleaning.
        /// </summary>
        public List<ParameterConfig>? CleanerParameters => Parameters!.Where(x => !x.LayerPassing!.StartsWith("ToManager", StringComparison.OrdinalIgnoreCase) && !x.IsPagingArgs).ToList();

        /// <summary>
        /// The operation event properties.
        /// </summary>
        public class OperationEvent
        {
            /// <summary>
            /// Gets or sets the event subject.
            /// </summary>
            public string? Subject { get; set; }

            /// <summary>
            /// Gets or sets the event action.
            /// </summary>
            public string? Action { get; set; }

            /// <summary>
            /// Gets or sets the event source.
            /// </summary>
            public string? Source { get; set; }

            /// <summary>
            /// Gets or sets the event value (if any).
            /// </summary>
            public string? Value { get; set; }
        }

        /// <summary>
        /// Gets the list of events derived from the <see cref="EventSubject"/>.
        /// </summary>
        public List<OperationEvent> Events { get; } = new List<OperationEvent>();

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => Text + ".";

        /// <summary>
        /// Gets the return type including nullability (where specified).
        /// </summary>
        public string OperationReturnType => CompareValue(ReturnTypeNullable, true) ? ReturnType + "?" : ReturnType!;

        /// <summary>
        /// Gets the <see cref="Task"/> <see cref="ReturnType"/>.
        /// </summary>
        public string OperationTaskReturnType => HasReturnValue ? $"Task<{OperationReturnType}>" : "Task";

        /// <summary>
        /// Gets the gRPC return type.
        /// </summary>
        public string GrpcReturnType => PropertyConfig.InferGrpcType(BaseReturnType!) + (Type == "GetColl" ? "CollectionResult" : "");

        /// <summary>
        /// Gets the <see cref="Task"/> <see cref="ReturnType"/> for an agent.
        /// </summary>
        public string AgentOperationTaskReturnType => HasReturnValue ? $"Task<WebApiAgentResult<{OperationReturnType}>>" : "Task<WebApiAgentResult>";

        /// <summary>
        /// Gets the <see cref="Task"/> <see cref="ReturnType"/> for a gRPC agent.
        /// </summary>
        public string GrpcAgentOperationTaskReturnType => HasReturnValue ? $"Task<GrpcAgentResult<{GrpcReturnType}>>" : "Task<GrpcAgentResult>";

        /// <summary>
        /// Gets or sets the base return type.
        /// </summary>
        public string? BaseReturnType { get; set; }

        /// <summary>
        /// Gets or sets the WebAPI return text.
        /// </summary>
        public string? WebApiReturnText { get; set; }

        /// <summary>
        /// Indicates whether the operation is returning a vale.
        /// </summary>
        public bool HasReturnValue => ReturnType != "void";

        /// <summary>
        /// Indicates whether there is a value operation.
        /// </summary>
        public bool HasValue => Parameters.Any(x => x.IsValueArg);

        /// <summary>
        /// Indicates whether the operation supports caching.
        /// </summary>
        public bool SupportsCaching => CompareValue(Parent!.DataSvcCaching, true) && new string[] { "Get", "Create", "Update", "Delete" }.Contains(Type);

        /// <summary>
        /// Gets or sets the data arguments.
        /// </summary>
        public ParameterConfig? DataArgs { get; set; }

        /// <summary>
        /// Indicates whether the data entity mapper is to be created for the entity.
        /// </summary>
        public bool DataEntityMapperCreate { get; set; }

        /// <summary>
        /// Gets the Cosmos PartitionKey as C# code.
        /// </summary>
        public string CosmosPartitionKeyCode => CosmosPartitionKey!.StartsWith("PartitionKey.", StringComparison.InvariantCulture) ? CosmosPartitionKey : $"new PartitionKey({CosmosPartitionKey})";

        /// <summary>
        /// Indicates whether the operation is a 'Patch'.
        /// </summary>
        public bool IsPatch => Type == "Patch";

        /// <summary>
        /// Gets or sets the PATCH Get variable.
        /// </summary>
        public string? PatchGetVariable { get; set; }

        /// <summary>
        /// Gets or sets the PATCH Update variable.
        /// </summary>
        public string? PatchUpdateVariable { get; set; }

        /// <summary>
        /// Gets or sets the gRPC converter for the return value.
        /// </summary>
        public string? GrpcReturnConverter { get; set; }

        /// <summary>
        /// Gets or sets the gRPC mapper for the return value.
        /// </summary>
        public string? GrpcReturnMapper { get; set; }

        /// <summary>
        /// Gets the event source URI.
        /// </summary>
        public string EventSourceUri => Root!.EventSourceRoot + (EventSource!.StartsWith('/') || (Root!.EventSourceRoot != null && Root!.EventSourceRoot.EndsWith('/')) ? EventSource : ("/" + EventSource));

        /// <summary>
        /// Gets the event format key code.
        /// </summary>
        public string? EventFormatKey { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            BaseReturnType = DefaultWhereNull(ReturnType, () => Type switch
            {
                "Get" => Parent!.EntityName,
                "GetColl" => Parent!.EntityName,
                "Create" => Parent!.EntityName,
                "Update" => Parent!.EntityName,
                "Patch" => Parent!.EntityName,
                "Delete" => "void",
                _ => "void"
            });

            if (BaseReturnType!.EndsWith("?", StringComparison.InvariantCulture))
            {
                ReturnType = BaseReturnType = BaseReturnType[0..^1];
                ReturnTypeNullable = true;
            }

            ReturnTypeNullable = DefaultWhereNull(ReturnTypeNullable, () => false);
            if (ReturnType == "void")
                ReturnTypeNullable = false;
            else if (Type == "Get")
                ReturnTypeNullable = true;
            else if (Type != "Custom")
                ReturnTypeNullable = false;

            if (ReturnType == "string")
                ReturnTypeNullable = true;

            if (ReturnType != null && Type == "GetColl")
                ReturnType += "CollectionResult";

            ReturnType = DefaultWhereNull(ReturnType, () => Type switch
            {
                "Get" => Parent!.EntityName,
                "GetColl" => Parent!.EntityCollectionResultName,
                "Create" => Parent!.EntityName,
                "Update" => Parent!.EntityName,
                "Patch" => Parent!.EntityName,
                "Delete" => "void",
                _ => "void"
            });

            ValueType = DefaultWhereNull(ValueType, () => Type switch
            {
                "Create" => Parent!.EntityName,
                "Update" => Parent!.EntityName,
                "Patch" => Parent!.EntityName,
                _ => null
            });

            Text = ToComments(DefaultWhereNull(Text, () => Type switch
                {
                    "Get" => $"Gets the specified {{{{{ReturnType}}}}}",
                    "GetColl" => $"Gets the {{{{{ReturnType}}}}} that contains the items that match the selection criteria",
                    "Create" => $"Creates a new {{{{{ValueType}}}}}",
                    "Update" => $"Updates an existing {{{{{ValueType}}}}}",
                    "Patch" => $"Patches an existing {{{{{ValueType}}}}}",
                    "Delete" => $"Deletes the specified {{{{{Parent!.EntityName}}}}}",
                    _ => StringConversion.ToSentenceCase(Name)
                }));

            ReturnText = ToComments(DefaultWhereNull(ReturnText, () => Type switch
            {
                "Get" => $"The selected {{{{{ReturnType}}}}} where found",
                "GetColl" => $"The {{{{{ReturnType}}}}}",
                "Create" => $"The created {{{{{ReturnType}}}}}",
                "Update" => $"The updated {{{{{ReturnType}}}}}",
                "Patch" => $"The patched {{{{{ReturnType}}}}}",
                "Delete" => null,
                _ => HasReturnValue ? $"A resultant {{{{{ReturnType}}}}}" : null
            })) + ".";

            WebApiReturnText = Type == "GetColl" ? ToComments($"The {{{{{BaseReturnType}Collection}}}}") : ReturnText;

            PrivateName = DefaultWhereNull(PrivateName, () => StringConversion.ToPrivateCase(Name));
            Validator = DefaultWhereNull(Validator, () => Parent!.Validator);
            IValidator = DefaultWhereNull(IValidator, () => Validator != null ? Parent!.IValidator ?? $"IValidator<{ValueType}>" : null);
            AutoImplement = DefaultWhereNull(AutoImplement, () => Parent!.AutoImplement);
            if (Type == "Custom")
                AutoImplement = "None";

            ManagerExtensions = DefaultWhereNull(ManagerExtensions, () => Parent!.ManagerExtensions);
            DataExtensions = DefaultWhereNull(DataExtensions, () => Parent!.DataExtensions);
            DataEntityMapperCreate = string.IsNullOrEmpty(DataEntityMapper);
            DataEntityMapper = DefaultWhereNull(DataEntityMapper, () => AutoImplement switch
            {
                "Database" => "DbMapper",
                "EntityFramework" => "EfMapper",
                "Cosmos" => "CosmosMapper",
                "OData" => "ODataMapper",
                _ => null
            });

            DatabaseStoredProc = DefaultWhereNull(DatabaseStoredProc, () => $"sp{Parent!.Name}{Name}");
            CosmosContainerId = DefaultWhereNull(CosmosContainerId, () => Parent!.CosmosContainerId);
            CosmosPartitionKey = DefaultWhereNull(CosmosPartitionKey, () => Parent!.CosmosPartitionKey);
            ODataCollectionName = DefaultWhereNull(ODataCollectionName, () => Parent!.ODataCollectionName);

            WebApiStatus = DefaultWhereNull(WebApiStatus, () => Type! == "Create" ? "Created" : (HasReturnValue ? "OK" : "NoContent"));
            WebApiMethod = Type == "Patch" ? "HttpPatch" : DefaultWhereNull(WebApiMethod, () => Type switch
            {
                "Create" => "HttpPost",
                "Custom" => "HttpPost",
                "Update" => "HttpPut",
                "Delete" => "HttpDelete",
                _ => "HttpGet"
            });

            WebApiAlternateStatus = DefaultWhereNull(WebApiAlternateStatus, () => Type switch
            {
                "Get" => "NotFound",
                "GetColl" => "NoContent",
                "Create" => "ThrowException",
                "Update" => "ThrowException",
                "Patch" => "ThrowException",
                "Delete" => "ThrowException",
                _ => HasReturnValue ? "NoContent" : "ThrowException"
            });

            ManagerOperationType = DefaultWhereNull(ManagerOperationType, () => Type switch
            {
                "Get" => "Read",
                "GetColl" => "Read",
                "Create" => "Create",
                "Update" => "Update",
                "Patch" => "Update",
                "Delete" => "Delete",
                _ => "Unspecified"
            });

            EventSource = DefaultWhereNull(EventSource, () => Parent!.EventSource);
            EventPublish = DefaultWhereNull(EventPublish, () => new string[] { "Create", "Update", "Delete" }.Contains(Type) ? Parent!.EventPublish : "None");

            EventFormatKey = Type switch
            {
                "Create" => "{_evtPub.FormatKey(__result)}",
                "Update" => "{_evtPub.FormatKey(__result)}",
                "Delete" => $"{{_evtPub.FormatKey({string.Join(", ", Parent!.Properties.Where(p => p.UniqueKey.HasValue && p.UniqueKey.Value).Select(x => $"{x.ArgumentName}"))})}}",
                _ => null
            };

            EventSubject = DefaultWhereNull(EventSubject, () => Type switch
            {
                "Create" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}{(EventFormatKey == null ? "" : $"{Root!.EventSubjectSeparator}" + EventFormatKey)}:{ConvertEventAction(ManagerOperationType!)}",
                "Update" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}{(EventFormatKey == null ? "" : $"{Root!.EventSubjectSeparator}" + EventFormatKey)}:{ConvertEventAction(ManagerOperationType!)}",
                "Delete" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}{(EventFormatKey == null ? "" : $"{Root!.EventSubjectSeparator}" + EventFormatKey)}:{ConvertEventAction(ManagerOperationType!)}",
                _ => null
            });

            DataSvcTransaction = DefaultWhereNull(DataSvcTransaction, () => CompareValue(EventPublish, "DataSvc") && CompareValue(Parent!.EventTransaction, true));
            DataSvcExtensions = DefaultWhereNull(DataSvcExtensions, () => Parent!.DataSvcExtensions);
            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, YesOption) ? YesOption : NoOption);

            if (Type == "Patch")
                ExcludeIData = ExcludeData = ExcludeIDataSvc = ExcludeDataSvc = ExcludeIManager = ExcludeManager = YesOption;

            PrepareParameters();
            PrepareEvents();

            WebApiRoute = DefaultWhereNull(WebApiRoute, () => Type switch
            {
                "GetColl" => "",
                "Custom" => "",
                _ => string.Join(",", Parameters.Where(x => !x.IsValueArg && !x.IsPagingArgs).Select(x => $"{{{x.ArgumentName}}}"))
            });

            if (Type == "Patch")
            {
                PatchGetOperation = DefaultWhereNull(PatchGetOperation, () => "Get");
                var parts = string.IsNullOrEmpty(PatchGetOperation) ? Array.Empty<string>() : PatchGetOperation.Split(".", StringSplitOptions.RemoveEmptyEntries);
                PatchGetVariable = parts.Length <= 1 ? "_manager" : StringConversion.ToPrivateCase(parts[0][1..]);
                if (parts.Length > 1)
                {
                    PatchGetOperation = parts[1];
                    if (!Parent!.WebApiCtorParameters.Any(x => x.Type == parts[0]))
                        Parent!.WebApiCtorParameters.Add(new ParameterConfig { Name = parts[0][1..], Type = parts[0], Text = $"{{{{{parts[0]}}}}}" });
                }

                PatchUpdateOperation = DefaultWhereNull(PatchUpdateOperation, () => "Update");
                parts = string.IsNullOrEmpty(PatchUpdateOperation) ? Array.Empty<string>() : PatchUpdateOperation.Split(".", StringSplitOptions.RemoveEmptyEntries);
                PatchUpdateVariable = parts.Length <= 1 ? "_manager" : StringConversion.ToPrivateCase(parts[0][1..]);
                if (parts.Length > 1)
                {
                    PatchUpdateOperation = parts[1];
                    if (!Parent!.WebApiCtorParameters.Any(x => x.Type == parts[0]))
                        Parent!.WebApiCtorParameters.Add(new ParameterConfig { Name = parts[0][1..], Type = parts[0], Text = $"{{{{{parts[0]}}}}}" });
                }
            }

            PrepareData();

            GrpcReturnMapper = SystemTypes.Contains(BaseReturnType) ? null : GrpcReturnType;
            GrpcReturnConverter = BaseReturnType switch
            {
                "DateTime" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}DateTimeToTimestamp",
                "Guid" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}GuidToStringConverter",
                "decimal" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}DecimalToDecimalConverter",
                _ => null
            };
        }

        /// <summary>
        /// Converts the event action.
        /// </summary>
        private string ConvertEventAction(string action) => Root!.EventActionFormat switch
        {
            "PastTense" => StringConversion.ToPastTense(action)!,
            _ => action
        };

        /// <summary>
        /// Prepares the parameters.
        /// </summary>
        private void PrepareParameters()
        {
            if (Parameters == null)
                Parameters = new List<ParameterConfig>();

            var i = 0;
            var isCreateUpdate = new string[] { "Create", "Update", "Patch" }.Contains(Type);
            if (isCreateUpdate)
                Parameters.Insert(i++, new ParameterConfig { Name = "Value", Type = ValueType, Text = $"{{{{{ValueType}}}}}", Nullable = false, IsMandatory = false, Validator = Validator, IValidator = IValidator, IsValueArg = true, WebApiFrom = "FromBody" });

            if (UniqueKey.HasValue && UniqueKey.Value)
            {
                foreach (var pc in Parent!.Properties.Where(p => p.UniqueKey.HasValue && p.UniqueKey.Value))
                {
                    Parameters.Insert(i++, new ParameterConfig { Name = pc.Name, Text = pc.Text, IsMandatory = new string[] { "Get", "Delete" }.Contains(Type), LayerPassing = isCreateUpdate ? "ToManagerSet" : "All", Property = pc.Name });
                }
            }

            if (Type == "GetColl" && CompareValue(Paging, true))
                Parameters.Add(new ParameterConfig { Name = "Paging", Type = "PagingArgs", Text = "{{PagingArgs}}", IsPagingArgs = true });

            foreach (var parameter in Parameters)
            {
                parameter.Prepare(Root!, this);
            }
        }

        /// <summary>
        /// Prepares the <see cref="Events"/>.
        /// </summary>
        private void PrepareEvents()
        {
            if (string.IsNullOrEmpty(EventSubject) || !CompareNullOrValue(Parent!.EventPublish, "None"))
                return;

            foreach (var @event in EventSubject!.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var ed = new OperationEvent();
                var parts = @event.Split(":");
                if (parts.Length > 0)
                    ed.Subject = parts[0];

                if (parts.Length > 1)
                    ed.Action = parts[1];
                else
                    ed.Action = ConvertEventAction(ManagerOperationType!);

                if (Root!.EventSubjectRoot != null)
                    ed.Subject = Root!.EventSubjectRoot + "." + ed.Subject;

                if (Root!.EventSourceKind != "None")
                    ed.Source = EventSourceUri.Replace("{$key}", EventFormatKey);

                if (HasReturnValue)
                    ed.Value = "__result";
                else if (Type == "Delete" && UniqueKey == true)
                {
                    var sb = new StringBuilder();
                    foreach (var dp in DataParameters)
                    {
                        if (sb.Length == 0)
                            sb.Append($"new {Parent!.Name} {{ ");
                        else
                            sb.Append(", ");

                        sb.Append($"{dp.Name} = {dp.ArgumentName}");
                    }

                    sb.Append(" }");
                    ed.Value = sb.ToString();
                }

                Events.Add(ed);
            }
        }

        /// <summary>
        /// Prepares for the data access.
        /// </summary>
        private void PrepareData()
        {
            DataArgs = new ParameterConfig { Name = "<internal>", PrivateName = "__dataArgs" };
            switch (AutoImplement)
            {
                case "Database":
                    DataArgs.Name = "_db";
                    DataArgs.Type = "IDatabaseArgs";
                    break;

                case "EntityFramework":
                    DataArgs.Name = "_ef";
                    DataArgs.Type = "IEfDbArgs";
                    break;

                case "Cosmos":
                    DataArgs.Name = "_cosmos";
                    DataArgs.Type = "ICosmosDbArgs";
                    break;

                case "OData":
                    DataArgs.Name = "_odata";
                    DataArgs.Type = "IODataArgs";
                    break;
            }

            DataArgs.Prepare(Root!, this);
        }

        /// <summary>
        /// Prepare after previous Prepare round.
        /// </summary>
        public void PrepareAfter()
        {
            RefactorApiLocation();
        }

        /// <summary>
        /// Refactor the API location URI.
        /// </summary>
        private void RefactorApiLocation()
        {
            if (WebApiLocation == null)
                return;

            if (WebApiLocation.StartsWith('!'))
                WebApiLocation = WebApiLocation[1..];
            else
            {
                if (WebApiLocation.StartsWith('^'))
                {
                    var name = WebApiLocation.Length == 1 ? "Get" : WebApiLocation[1..];
                    var op = Parent!.Operations.FirstOrDefault(x => x.Name == name);
                    if (op == null)
                        throw new CodeGenException(this, nameof(WebApiLocation), $"Attempt to lookup Operation '{name}' which does not exist.");

                    WebApiLocation = op.WebApiRoute;
                    var s = 0;
                    if (WebApiLocation != null)
                    {
                        while (true)
                        {
                            var i = WebApiLocation.IndexOf('{', s);
                            if (i < 0)
                                break;

                            var j = WebApiLocation.IndexOf('}', s);
                            if (j < 0 || j < i)
                                throw new CodeGenException(this, nameof(WebApiLocation), $"Operation '{name}' WebApiRoute '{op.WebApiRoute}' tokens are invalid.");

                            var arg = WebApiLocation.Substring(i, j - i + 1);
                            var p = op.Parameters.FirstOrDefault(x => x.ArgumentName == arg[1..^1]);
                            if (p == null)
                                throw new CodeGenException(this, nameof(WebApiLocation), $"Operation '{name}' WebApiRoute '{op.WebApiRoute}' references Parameter token '{arg}' that does not exist.");

                            WebApiLocation = WebApiLocation.Replace(arg, "{r." + p.Name + "}");
                            s = j + 1;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Parent!.WebApiRoutePrefix))
                    WebApiLocation = Parent!.WebApiRoutePrefix + "/" + WebApiLocation;
            }

            if (WebApiLocation.FirstOrDefault() != '/')
                WebApiLocation = "/" + WebApiLocation;
        }
    }
}