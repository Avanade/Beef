// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
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
    [CodeGenClass("Operation", Title = "'CodeGeneration' object (entity-driven)",
        Description = "The code generation for an `Operation` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting fully customised operations as required.",
        Markdown = @"The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single entity value.
- **`GetColl`** - indicates a get (read) returning an entity collection.
- **`Create`** - indicates the creation of an entity.
- **`Update`** - indicates the updating of an entity.
- **[`Patch`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/Merge/JsonMergePatch.cs)** - indicates the patching (update) of an entity (leverages `Get` and `Update` to perform).
- **`Delete`** - indicates the deleting of an entity.
- **`Custom`** - indicates a customized operation where parameters and return value are explicitly defined. As this is a customised operation there is no `AutoImplement` and as such the underlying data implementation will need to be performed by the developer. This is the default where not specified.",
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
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
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Events", Title = "Provides the _Events_ configuration.")]
    [CodeGenCategory("WebApi", Title = "Provides the data _Web API_ configuration.")]
    [CodeGenCategory("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CodeGenCategory("DataSvc", Title = "Provides the _Data Services-layer_ configuration.")]
    [CodeGenCategory("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CodeGenCategory("Database", Title = "Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.")]
    [CodeGenCategory("EntityFramework", Title = "Provides the specific _EntityFramework_ configuration where `AutoImplement` is `EntityFramework`.")]
    [CodeGenCategory("Cosmos", Title = "Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.")]
    [CodeGenCategory("OData", Title = "Provides the specific _OData_ configuration where `AutoImplement` is `OData`.")]
    [CodeGenCategory("HttpAgent", Title = "Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.")]
    [CodeGenCategory("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CodeGenCategory("Exclude", Title = "Provides the _Exclude_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class OperationConfig : ConfigBase<CodeGenConfig, EntityConfig>
    {
        private int _ensureValueCount;

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
        [CodeGenProperty("Key", Title = "The unique operation name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the type of operation that is to be code-generated.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The type of operation that is to be code-generated.", IsImportant = true,
            Description = "Defaults to `Custom`.", Options = new string[] { "Get", "GetColl", "Create", "Update", "Patch", "Delete", "Custom" })]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The text for use in comments.",
            Description = "The `Text` will be defaulted for all the `Operation.Type` options with the exception of `Custom`. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? Text { get; set; }

        /// <summary>
        /// Indicates whether the properties marked as a primary key (`Property.PrimaryKey`) are to be used as the parameters. 
        /// </summary>
        [JsonProperty("primaryKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether the properties marked as a primary key (`Property.PrimaryKey`) are to be used as the parameters.", IsImportant = true,
            Description = "This simplifies the specification of these properties versus having to declare each specifically.")]
        public bool? PrimaryKey { get; set; }

        /// <summary>
        /// Indicates whether a PagingArgs argument is to be added to the operation to enable paging related logic.
        /// </summary>
        [JsonProperty("paging", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.", IsImportant = true)]
        public bool? Paging { get; set; }

        /// <summary>
        /// Gets or sets the .NET value parameter <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("valueType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The .NET value parameter `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Create` or `Update`.")]
        public string? ValueType { get; set; }

        /// <summary>
        /// Gets or sets the .NET return <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("returnType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The .NET return `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `Operation.Type` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.")]
        public string? ReturnType { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="ReturnType"/> is nullable for the operation.
        /// </summary>
        [JsonProperty("returnTypeNullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether the `ReturnType` is nullable for the operation.",
            Description = "This is only applicable for an `Operation.Type` of `Custom`. Will be inferred where the `ReturnType` is denoted as nullable; i.e. suffixed by a `?`.")]
        public bool? ReturnTypeNullable { get; set; }

        /// <summary>
        /// Gets or sets the text for use in comments to describe the <see cref="ReturnType"/>.
        /// </summary>
        [JsonProperty("returnText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The text for use in comments to describe the `ReturnType`.",
            Description = "A default will be created where not specified. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? ReturnText { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonProperty("privateName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private usage. By default reformatted from `Name`; e.g. `GetByArgs` as `_getByArgs`.")]
        public string? PrivateName { get; set; }

        /// <summary>
        /// Indicates whether to use Results.
        /// </summary>
        [JsonProperty("withResult", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether to use `CoreEx.Results` (aka Railway-oriented programming).",
            Description = "Defaults to `Entity.WilhResult`.")]
        public bool? WithResult { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the operation override for the `Entity.AutoImplement`.
        /// </summary>
        [JsonProperty("autoImplement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "The operation override for the `Entity.AutoImplement`.", IsImportant = true, Options = new string[] { "Database", "EntityFramework", "Cosmos", "OData", "HttpAgent", "None" },
            Description = "Defaults to `Entity.AutoImplement`. The corresponding `Entity.AutoImplement` must be defined for this to be enacted. Auto-implementation is applicable for all `Operation.Type` options with the exception of `Custom`.")]
        public string? AutoImplement { get; set; }

        /// <summary>
        /// Gets or sets the override for the data entity <c>Mapper</c>.
        /// </summary>
        [JsonProperty("dataEntityMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "The override for the data entity `Mapper`.",
            Description = "Used where the default generated `Mapper` is not applicable.")]
        public string? DataEntityMapper { get; set; }

        /// <summary>
        /// Indicates whether the `Data` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "Indicates whether the `Data` extensions logic should be generated.",
            Description = "Defaults to `Entity.DataExtensions`.")]
        public bool? DataExtensions { get; set; }

        /// <summary>
        /// Indicates whether a `DataInvoker` should orchestrate the `Data`-layer.
        /// </summary>
        [JsonProperty("dataInvoker", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "Indicates whether a `DataInvoker` should orchestrate the `Data`-layer.",
            Description = "Where `Dataransaction` or `EventPublish` is `Data` then orchestration will default to `true`.")]
        public bool? DataInvoker { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Data`-layer.
        /// </summary>
        [JsonProperty("dataTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Data`-layer.",
            Description = "Where using an `EventOutbox` this is ignored as it is implied through its usage.")]
        public bool? DataTransaction { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the database stored procedure name.
        /// </summary>
        [JsonProperty("databaseStoredProc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The database stored procedure name used where `Operation.AutoImplement` is `Database`.",
            Description = "Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.")]
        public string? DatabaseStoredProc { get; set; }

        #endregion

        #region EntityFramework

        /// <summary>
        /// Gets or sets the corresponding Entity Framework model name required where <see cref="AutoImplement"/> is <c>EntityFramework</c>.
        /// </summary>
        [JsonProperty("entityFrameworkModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("EntityFramework", Title = "The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).", IsImportant = true,
            Description = "Overrides the `Entity.EntityFrameworkModel`.")]

        public string? EntityFrameworkModel { get; set; }

        #endregion

        #region Cosmos

        /// <summary>
        /// Gets or sets the corresponding Cosmos model name required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Cosmos", Title = "The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).", IsImportant = true,
            Description = "Overrides the `Entity.CosmosModel`.")]
        public string? CosmosModel { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos <c>ContainerId</c> override.
        /// </summary>
        [JsonProperty("cosmosContainerId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Cosmos", Title = "The Cosmos `ContainerId` override used where `Operation.AutoImplement` is `Cosmos`.",
            Description = "Overrides the `Entity.CosmosContainerId`.")]
        public string? CosmosContainerId { get; set; }

        /// <summary>
        /// Gets or sets the C# code override to be used for setting the optional Cosmos <c>PartitionKey</c>.
        /// </summary>
        [JsonProperty("cosmosPartitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Cosmos", Title = "The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Operation.AutoImplement` is `Cosmos`.",
            Description = "Overrides the `Entity.CosmosPartitionKey`.")]
        public string? CosmosPartitionKey { get; set; }

        #endregion

        #region OData

        /// <summary>
        /// Gets or sets the override name of the underlying OData collection name where <see cref="OperationConfig.AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataCollectionName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("OData", Title = "The override name of the underlying OData collection where `Operation.AutoImplement` is `OData`.", IsImportant = true,
            Description = "Overriddes the `Entity.ODataCollectionName`; otherwise, the underlying `Simple.OData.Client` will attempt to infer.")]
        public string? ODataCollectionName { get; set; }

        #endregion

        #region HttpAgent

        /// <summary>
        /// Gets or sets the HTTP Agent API route prefix where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentRoute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`.",
            Description = "This is appended to the `Entity.HttpAgentRoutePrefix`.")]
        public string? HttpAgentRoute { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Agent API Method for the operation.
        /// </summary>
        [JsonProperty("httpAgentMethod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The HTTP Agent Method for the operation.", IsImportant = true, Options = new string[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch" },
            Description = "Defaults to `Operation.WebApiMethod`.")]
        public string? HttpAgentMethod { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).", IsImportant = true,
            Description = "This can be overridden within the `Operation`(s).")]
        public string? HttpAgentModel { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentReturnModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).",
            Description = "Defaults to `Operation.HttpAgentModel` where the `Operation.ReturnType` is equal to `Entity.Name` (same type). This can be overridden within the `Operation`(s).")]
        public string? HttpAgentReturnModel { get; set; }

        /// <summary>
        /// Gets or sets the fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.",
            Description = "Appended to `Entity.HttpAgentCode` where specified to extend.")]
        public string? HttpAgentCode { get; set; }

        #endregion 

        #region Manager

        /// <summary>
        /// Indicates whether the `Manager`-layer is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
        /// </summary>
        [JsonProperty("managerCustom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "Indicates whether the `Manager` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.", IsImportant = true)]
        public bool? ManagerCustom { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.
        /// </summary>
        [JsonProperty("managerTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `Manager`-layer.")]
        public bool? ManagerTransaction { get; set; }

        /// <summary>
        /// Indicates whether the `Manager` extensions logic should be generated.
        /// </summary>
        [JsonProperty("managerExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "Indicates whether the `Manager` extensions logic should be generated.",
            Description = "Defaults to `Entity.ManagerExtensions`.")]
        public bool? ManagerExtensions { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "The name of the .NET implementing `Type` or interface `Type` that will perform the validation.", IsImportant = true,
            Description = "Defaults to the `Entity.Validator` where not specified explicitly (where `Operation.Type` options `Create` or `Update`).")]
        public string? Validator { get; set; }

        /// <summary>
        /// Gets or sets the `Validation` framework. 
        /// </summary>
        [JsonProperty("validationFramework", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "The `Validation` framework to use for the entity-based validation.", Options = new string[] { "CoreEx", "FluentValidation" },
            Description = "Defaults to `Entity.ValidationFramework`. This can be overridden within the `Parameter`(s).")]
        public string? ValidationFramework { get; set; }

        /// <summary>
        /// Gets or sets the `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer.
        /// </summary>
        [JsonProperty("managerOperationType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "The `ExecutionContext.OperationType` (CRUD denotation) defined at the `Manager`-layer.", Options = new string[] { "Create", "Read", "Update", "Delete", "Unspecified" },
            Description = "The default will be inferred from the `Operation.Type`; however, where the `Operation.Type` is `Custom` it will default to `Unspecified`.")]
        public string? ManagerOperationType { get; set; }

        /// <summary>
        /// Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.
        /// </summary>
        [JsonProperty("managerCleanUp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.",
            Description = "This can be overridden within the `CodeGeneration` and `Entity`.")]
        public bool? ManagerCleanUp { get; set; }

        #endregion

        #region DataSvc

        /// <summary>
        /// Indicates whether the `DataSvc`-layer is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.
        /// </summary>
        [JsonProperty("dataSvcCustom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DataSvc", Title = "Indicates whether the `DataSvc` logic is a custom implementation; i.e. no auto-`DataSvc` invocation logic is to be generated.", IsImportant = true)]
        public bool? DataSvcCustom { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.
        /// </summary>
        [JsonProperty("dataSvcTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DataSvc", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer.")]
        public bool? DataSvcTransaction { get; set; }

        /// <summary>
        /// Indicates whether a `DataSvcInvoker` should orchestrate the `DataSvc`-layer.
        /// </summary>
        [JsonProperty("dataSvcInvoker", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DataSvc", Title = "Indicates whether a `DataSvcInvoker` should orchestrate the `DataSvc`-layer.",
            Description = "Where `DataSvcTransaction` or `EventPublish` is `DataSvc` then orchestration will default to `true`.")]
        public bool? DataSvcInvoker { get; set; }

        /// <summary>
        /// Indicates whether the `DataSvc` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataSvcExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DataSvc", Title = "Indicates whether the `DataSvc` extensions logic should be generated.",
            Description = "Defaults to `Entity.ManagerExtensions`.")]
        public bool? DataSvcExtensions { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = new string[] { "None", "DataSvc", "Data" },
            Description = "Defaults to the `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonProperty("eventSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The Event Source.",
            Description = "Defaults to `Entity.EventSource`. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. " +
            "To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. Otherwise, specify the C# string interpolation expression; for example: `person/{r.Id}`.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Gets or sets the event subject template and corresponding event action pair (separated by a colon).
        /// </summary>
        [JsonProperty("eventSubject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The event subject template and corresponding event action pair (separated by a colon).",
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
        [CodeGenProperty("WebApi", Title = "The Web API `RouteAtttribute` to be appended to the `Entity.WebApiRoutePrefix`.", IsImportant = true)]
        public string? WebApiRoute { get; set; }

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.",
            Description = "Where not specified no attribute output will occur; it will then inherit as supported by .NET.")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Method for the operation.
        /// </summary>
        [JsonProperty("webApiMethod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The HTTP Method for the operation.", IsImportant = true, Options = new string[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete" },
            Description = "The value defaults as follows: `HttpGet` for `Operation.Type` value `Get` or `GetColl`, `HttpPost` for `Operation.Type` value `Create` or `Custom`, " +
            "`HttpPut` for `Operation.Type` value `Update`, and `HttpDelete` for `Operation.Type` value `Delete`. An `Operation.Type` value `Patch` can not be specified and will always default to `HttpPatch`.")]
        public string? WebApiMethod { get; set; }

        /// <summary>
        /// Gets or sets the primary HTTP Status Code that will be returned for the operation where there is a non-null return value. 
        /// </summary>
        [JsonProperty("webApiStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value.", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound" },
            Description = "The value defaults as follows: `OK` for `Operation.Type` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `Operation.Type` value `Create`.")]
        public string? WebApiStatus { get; set; }

        /// <summary>
        /// Gets or sets the alternate HTTP Status Code that will be returned for the operation where there is a null return value. 
        /// </summary>
        [JsonProperty("webApiAlternateStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a `null` return value.", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound" },
            Description = "The value defaults as follows: `NotFound` for `Operation.Type` value `Get` and `NoContent` for `Operation.Type` value `GetColl`; otherwise, `null`.")]
        public string? WebApiAlternateStatus { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Response Location Header route. 
        /// </summary>
        [JsonProperty("webApiLocation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The HTTP Response Location Header route.",
            Description = "This uses similar formatting to the `WebApiRoute`. The response value is accessed using `r.` notation to access underlying properties; for example `{r.Id}` or `person/{r.Id}`. The `Entity.WebApiRoutePrefix` will be prepended automatically; however, to disable set the first character to `!`, e.g. `!person/{r.Id}`. " +
            "The URI can be inferred from another `Operation` by using a lookup `^`; for example `^Get` indicates to infer from the named `Get` operation (where only `^` is specified this is shorthand for `^Get` as this is the most common value). The Location URI will ensure the first character is a `/` so it acts a 'relative URL absolute path'.")]
        public string? WebApiLocation { get; set; }

        /// <summary>
        /// Indicates whether the Web API is responsible for managing concurrency via auto-generated ETag.
        /// </summary>
        [CodeGenProperty("WebApi", Title = "Indicates whether the Web API is responsible for managing (simulating) concurrency via auto-generated ETag.",
            Description = "This provides an alternative where the underlying data source does not natively support optimistic concurrency (native support should always be leveraged as a priority). Where the `Operation.Type` is `Update` or `Patch`, the request ETag will " +
            "be matched against the response for a corresponding `Get` operation to verify no changes have been made prior to updating. For this to function correctly the .NET response Type for the `Get` must be the same as that returned from " +
            "the corresponding `Create`, `Update` and `Patch` (where applicable) as the generated ETag is a SHA256 hash of the resulting JSON. Defaults to `Entity.WebApiConcurrency`.")]
        [JsonProperty("webApiConcurrency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? WebApiConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the override for the corresponding `Get` method name (in the `XxxManager`) either where, the `Operation.Type` is `Update` and `WebApiConcurrency` is `true`, or the `Operation.Type` is `Patch`.
        /// </summary>
        [JsonProperty("webApiGetOperation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Update` and `SimulateConcurrency` is `true`.",
            Description = "Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.")]
        public string? WebApiGetOperation { get; set; }

        /// <summary>
        /// Gets or sets the override for the corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.
        /// </summary>
        [JsonProperty("webApiUpdateOperation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The corresponding `Update` method name (in the `XxxManager`) where the `Operation.Type` is `Patch`.",
            Description = "Defaults to `Update`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.")]
        public string? WebApiUpdateOperation { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.
        /// </summary>
        [JsonProperty("authPermission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "The permission used by the `ExecutionContext.IsAuthorized(AuthPermission)` to determine whether the user is authorized.")]
        public string? AuthPermission { get; set; }

        /// <summary>
        /// Gets or sets the permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.
        /// </summary>
        [JsonProperty("authRole", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "The permission used by the `ExecutionContext.IsInRole(AuthRole)` to determine whether the user is authorized.")]
        public string? AuthRole { get; set; }

        #endregion

        #region Exclude

        /// <summary>
        /// Indicates whether to exclude the generation of <b>all</b> <c>Operation</c> related output.
        /// </summary>
        [JsonProperty("excludeAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of all `Operation` related output.", IsImportant = true,
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties to `true`.")]
        public bool? ExcludeAll { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeIData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `Data` interface (`IXxxData.cs`) output.")]
        public bool? ExcludeIData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `Data` class (`XxxData.cs`) output.")]
        public bool? ExcludeData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeIDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `DataSvc` interface (`IXxxDataSvc.cs`) output.")]
        public bool? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `DataSvc` class (`XxxDataSvc.cs`) output.")]
        public bool? ExcludeDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeIManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `Manager` interface (`IXxxManager.cs`) output.")]
        public bool? ExcludeIManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the `Manager` class (`XxxManager.cs`) output.")]
        public bool? ExcludeManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the WebAPI `Controller` class (`XxxController.cs`) output.")]
        public bool? ExcludeWebApi { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApiAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the WebAPI consuming `Agent` class (`XxxAgent.cs`) output.")]
        public bool? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the operation within the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeGrpcAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the operation within the gRPC consuming `Agent` class (`XxxAgent.cs`) output.")]
        public bool? ExcludeGrpcAgent { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required for the Operation.
        /// </summary>
        [JsonProperty("grpc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Operation.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="ParameterConfig"/> collection.
        /// </summary>
        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Parameter` collection.")]
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
        /// Gets the <see cref="ParameterConfig"/> collection filtered for validation that have either a Validator or ValidatorCode specified (exclude IsMandatory only).
        /// </summary>
        public List<ParameterConfig> ValidatorParameters => Parameters!.Where(x => x.Validator != null || x.ValidatorCode != null).OrderBy(x => x.IsValueArg).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection filtered where mandatory and without value.
        /// </summary>
        public List<ParameterConfig> ValueLessMandatoryParameters => ValueLessParameters!.Where(x => CompareValue(x.IsMandatory, true)).ToList();

        /// <summary>
        /// Indicates whether there is only a single parameter to be validated.
        /// </summary>
        public bool SingleValidateParameters => CompareNullOrValue(Parent!.ManagerExtensions, false) && ValidateParameters.Count <= 1;

        /// <summary>
        /// Indicates whether there is only a single parameter to be validated.
        /// </summary>
        public bool SingleValidatorParameters => CompareNullOrValue(Parent!.ManagerExtensions, false) && ValidatorParameters.Count <= 1;

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
        public List<ParameterConfig>? CleanerParameters => Parameters!.Where(x => !x.LayerPassing!.StartsWith("ToManager", StringComparison.OrdinalIgnoreCase) && !x.IsPagingArgs && IsTrue(x.Parent!.ManagerCleanUp)).ToList();

        /// <summary>
        /// Gets the <see cref="ParameterConfig"/> collection for those parameters marked as ToManager*.
        /// </summary>
        public List<ParameterConfig>? ManagerPassingParameters => Parameters!.Where(x => x.LayerPassing!.StartsWith("ToManager", StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Gets the parameter that is <see cref="ParameterConfig.IsPagingArgs"/>.
        /// </summary>
        public ParameterConfig? PagingParameter => Parameters!.Where(x => x.IsPagingArgs).FirstOrDefault();

        /// <summary>
        /// Indicates whether the <see cref="CoreParameters"/> are the same as the entity primary key.
        /// </summary>
        public bool IsCoreParametersSameAsPrimaryKey
        {
            get
            {
                if (CoreParameters!.Count != Parent!.PrimaryKeyProperties.Count)
                    return false;

                for (int i = 0; i < CoreParameters.Count; i++)
                {
                    if (CoreParameters[i].Name != Parent!.PrimaryKeyProperties[i].Name || CoreParameters[i].Type != Parent!.PrimaryKeyProperties[i].Type)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Indicates whether any parameters exist with WebApiFrom contain "FromEntityProperties".
        /// </summary>
        public bool HasFromEntityPropertiesParameters => Parameters!.Any(x => x.WebApiFrom == "FromEntityProperties");

        /// <summary>
        /// Indicates whether the <see cref="ReturnType"/> is an entity.
        /// </summary>
        public bool IsReturnTypeEntity => !DotNet.SystemTypes.Contains(ReturnType!) && (!ReturnType!.StartsWith("System.") && !ReturnType!.StartsWith("Microsoft."));

        /// <summary>
        /// Gets the fully qualified database stored procedure name.
        /// </summary>
        public string FullyQualifiedStoredProcedureName => Root!.DatabaseProvider switch
        {
            "MySql" => $"`{DatabaseStoredProc}`",
            _ => $"[{Parent!.DatabaseSchema}].[{DatabaseStoredProc}]"
        };

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

            /// <summary>
            /// Gets or sets the parent configuration.
            /// </summary>
            public OperationConfig? Parent { get; set; }
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
        public string OperationTaskReturnType => HasReturnValue ? $"Task<{(IsTrue(WithResult) ? "Result<" : "")}{OperationReturnType}{(IsTrue(WithResult) ? ">" : "")}>" : $"Task{(IsTrue(WithResult) ? "<Result>" : "")}";

        /// <summary>
        /// Gets the <see cref="Task"/> <see cref="ReturnType"/> for an agent.
        /// </summary>
        public string AgentOperationTaskReturnType => HasReturnValue ? $"Task<HttpResult<{OperationReturnType}>>" : "Task<HttpResult>";

        /// <summary>
        /// Gets the controller operation method call/invoke statement.
        /// </summary>
        public string ControllerOperationWebApiMethod => CreateControllerOperationWebApiMethod();

        /// <summary>
        /// Gets the agent operation HTTP method call/invoke statement.
        /// </summary>
        public string AgentOperationHttpMethod => CreateAgentOperationHttpMethod();

        /// <summary>
        /// Gets tthe agent web api route.
        /// </summary>
        public string AgentWebApiRoute => (string.IsNullOrEmpty(Parent!.WebApiRoutePrefix) ? WebApiRoute : $"{Parent!.WebApiRoutePrefix}{(string.IsNullOrEmpty(WebApiRoute) || WebApiRoute.StartsWith("/") ? "" : "/")}{WebApiRoute}") ?? "";

        /// <summary>
        /// Gets the gRPC return type.
        /// </summary>
        public string GrpcReturnType => PropertyConfig.InferGrpcType(BaseReturnType!) + (Type == "GetColl" ? "CollectionResult" : "");

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
        public bool HasValue => Parameters!.Any(x => x.IsValueArg);

        /// <summary>
        /// Gets the value <see cref="ParameterConfig"/>.
        /// </summary>
        public ParameterConfig? ValueParameter => Parameters!.FirstOrDefault(x => x.IsValueArg);

        /// <summary>
        /// Indicates whether the operation supports caching.
        /// </summary>
        public bool SupportsCaching => CompareValue(Parent!.DataSvcCaching, true) && new string[] { "Get", "Create", "Update", "Delete" }.Contains(Type) && Parent.PrimaryKeyPropertiesIncludeInherited.Count > 0 && CompareNullOrValue(DataSvcCustom, false);

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
        public string? CosmosPartitionKeyCode => CosmosPartitionKey == null ? null : (CosmosPartitionKey!.StartsWith("PartitionKey.", StringComparison.InvariantCulture) ? $"Mac.{CosmosPartitionKey}" : $"new Mac.PartitionKey({CosmosPartitionKey})");

        /// <summary>
        /// Indicates whether the operation is a 'Patch'.
        /// </summary>
        public bool IsPatch => Type == "Patch";

        /// <summary>
        /// Gets or sets the UPDATE/PATCH Get variable.
        /// </summary>
        public string? WebApiGetVariable { get; set; }

        /// <summary>
        /// Gets or sets the PATCH Update variable.
        /// </summary>
        public string? WebApiUpdateVariable { get; set; }

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
        public string EventSourceUri => Root!.EventSourceRoot?.ToLowerInvariant() + (EventSource!.StartsWith('/') || (Root!.EventSourceRoot != null && Root!.EventSourceRoot.EndsWith('/')) ? EventSource : ("/" + EventSource));

        /// <summary>
        /// Gets the event format key code.
        /// </summary>
        public string? EventFormatKey { get; private set; }

        /// <summary>
        /// Indicates whether the manual (OnImplementation) can be implemened as shorthand - minimal amount of code.
        /// </summary>
        public bool IsManualShorthand => AutoImplement == "None" && !HasDataEvents;

        /// <summary>
        /// Indicates whether any of the operations will raise an event within the Data-layer.
        /// </summary>
        public bool HasDataEvents => EventPublish == "Data";

        /// <summary>
        /// Indicates whether the HTTP Agent operation requires a mapper.
        /// </summary>
        public bool HttpAgentRequiresMapper { get; set; } = true;

        /// <summary>
        /// Gets or sets the HTTP agent sent statement - composed internally given complexity to construct.
        /// </summary>
        public string? HttpAgentSendStatement { get; set; }

        /// <summary>
        /// Provides an ensure value counter (automatically updates by 1 on each access).
        /// </summary>
        public int EnsureValueCount => _ensureValueCount++;

        /// <summary>
        /// Indicates whether the operation is a 'Get' with caching.
        /// </summary>
        public bool IsGetWithCache => Type == "Get" && SupportsCaching;

        /// <summary>
        /// Indicates whether the DataSvc operation invocation can occur on a single line.
        /// </summary>
        public bool DataSvcSingleLine => (Type == "GetColl" && CompareNullOrValue(DataSvcExtensions, false) && CompareNullOrValue(DataSvcTransaction, false) && !DataSvcWillEventPublish)
            || (Type == "Get" && CompareNullOrValue(DataSvcExtensions, false) && CompareNullOrValue(DataSvcTransaction, false) && !DataSvcWillEventPublish)
            || (Type == "Custom" && CompareNullOrValue(DataSvcExtensions, false) && CompareNullOrValue(DataSvcTransaction, false) && !DataSvcWillEventPublish)
            || (new string[] { "Create", "Update", "Delete" }.Contains(Type) && !SupportsCaching! && CompareNullOrValue(DataSvcExtensions, false) && CompareNullOrValue(DataSvcTransaction, false) && !DataSvcWillEventPublish);

        /// <summary>
        /// Indicates whether the DataSvc operation invocation will result in an event publish.
        /// </summary>
        public bool DataSvcWillEventPublish => CompareValue(EventPublish, "DataSvc") && Events.Count > 0;

        /// <summary>
        /// Indicates whether the Data operation invocation can occur on a single line.
        /// </summary>
        public bool DataSingleLine => CompareNullOrValue(DataExtensions, false) && CompareNullOrValue(DataTransaction, false) && !CompareValue(EventPublish, "Data");

        /// <summary>
        /// Indicates whether the Data operation where auto implement is none then invocation can occur on a single line.
        /// </summary>
        public bool DataNoneSingleLine => AutoImplement == "None" && CompareNullOrValue(DataExtensions, false) && !CompareValue(EventPublish, "Data");

        /// <summary>
        /// Used exclusively by the code templates to track whether "Result" (railway-oriented programming) code has been output, etc.
        /// </summary>
        public bool HasResultCode { get; set; }

        /// <summary>
        /// Indicates whether the result will need to change the type as a result of the operation.
        /// </summary>
        public bool HasResultTypeChange => (HasValue && HasReturnValue && ValueType != ReturnType) || (HasValue && !HasReturnValue) || (!HasValue && HasReturnValue);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            Type = DefaultWhereNull(Type, () => "Custom");
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

            Text = StringConverter.ToComments(DefaultWhereNull(Text, () => Type switch
                {
                    "Get" => $"Gets the specified {{{{{ReturnType}}}}}",
                    "GetColl" => $"Gets the {{{{{ReturnType}}}}} that contains the items that match the selection criteria",
                    "Create" => $"Creates a new {{{{{ValueType}}}}}",
                    "Update" => $"Updates an existing {{{{{ValueType}}}}}",
                    "Patch" => $"Patches an existing {{{{{ValueType}}}}}",
                    "Delete" => $"Deletes the specified {{{{{Parent!.EntityName}}}}}",
                    _ => StringConverter.ToSentenceCase(Name)
                }));

            ReturnText = StringConverter.ToComments(DefaultWhereNull(ReturnText, () => Type switch
            {
                "Get" => $"The selected {{{{{ReturnType}}}}} where found",
                "GetColl" => $"The {{{{{ReturnType}}}}}",
                "Create" => $"The created {{{{{ReturnType}}}}}",
                "Update" => $"The updated {{{{{ReturnType}}}}}",
                "Patch" => $"The patched {{{{{ReturnType}}}}}",
                "Delete" => null,
                _ => HasReturnValue ? $"A resultant {{{{{ReturnType}}}}}" : null
            })) + ".";

            WebApiReturnText = Type == "GetColl" ? StringConverter.ToComments($"The {{{{{BaseReturnType}Collection}}}}") : ReturnText;

            PrivateName = DefaultWhereNull(PrivateName, () => StringConverter.ToPrivateCase(Name));
            AuthRole = DefaultWhereNull(AuthRole, () => Parent!.AuthRole);
            Validator = DefaultWhereNull(Validator, () => Parent!.Validator);
            AutoImplement = DefaultWhereNull(AutoImplement, () => Parent!.AutoImplement);
            if (Type == "Custom")
                AutoImplement = "None";

            WithResult = DefaultWhereNull(WithResult, () => Parent!.WithResult);
            ManagerExtensions = DefaultWhereNull(ManagerExtensions, () => Parent!.ManagerExtensions);
            DataExtensions = DefaultWhereNull(DataExtensions, () => Parent!.DataExtensions);
            if (AutoImplement == "None")
                DataExtensions = false;

            DataEntityMapperCreate = string.IsNullOrEmpty(DataEntityMapper);
            DataEntityMapper = DefaultWhereNull(DataEntityMapper, () => AutoImplement switch
            {
                "Database" => "DbMapper",
                _ => null
            });

            DatabaseStoredProc = DefaultWhereNull(DatabaseStoredProc, () => $"sp{Parent!.Name}{Name}");
            EntityFrameworkModel = DefaultWhereNull(EntityFrameworkModel, () => Parent!.EntityFrameworkModel);
            CosmosModel = DefaultWhereNull(CosmosModel, () => Parent!.CosmosModel);
            CosmosContainerId = DefaultWhereNull(CosmosContainerId, () => Parent!.CosmosContainerId);
            CosmosPartitionKey = DefaultWhereNull(CosmosPartitionKey, () => Parent!.CosmosPartitionKey);
            ODataCollectionName = DefaultWhereNull(ODataCollectionName, () => Parent!.ODataCollectionName);

            WebApiConcurrency = DefaultWhereNull(WebApiConcurrency, () => Parent!.WebApiConcurrency);
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
                "Create" => "null",
                "Update" => "null",
                "Patch" => "null",
                "Delete" => "null",
                _ => HasReturnValue ? "NoContent" : "null"
            });

            ValidationFramework = DefaultWhereNull(ValidationFramework, () => Parent!.ValidationFramework);
            ManagerCleanUp = DefaultWhereNull(ManagerCleanUp, () => Parent!.ManagerCleanUp);
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
            EventPublish = DefaultWhereNull(EventPublish, () => new string[] { "Create", "Update", "Delete" }.Contains(Type) || !string.IsNullOrEmpty(EventSubject) ? Parent!.EventPublish : "None");

            EventFormatKey = Type switch
            {
                "Create" => $"{string.Join(", ", Parent!.Properties!.Where(p => p.PrimaryKey.HasValue && p.PrimaryKey.Value).Select(x => $"{{r.{x.Name}}}"))}",
                "Update" => $"{string.Join(", ", Parent!.Properties!.Where(p => p.PrimaryKey.HasValue && p.PrimaryKey.Value).Select(x => $"{{r.{x.Name}}}"))}",
                "Delete" => $"{string.Join(", ", Parent!.Properties!.Where(p => p.PrimaryKey.HasValue && p.PrimaryKey.Value).Select(x => $"{{{x.ArgumentName}}}"))}",
                _ => null
            };

            EventSubject = DefaultWhereNull(EventSubject, () => Type switch
            {
                "Create" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}:{(Root!.EventActionFormat == "PastTense" ? "Created" : ManagerOperationType!)}",
                "Update" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}:{(Root!.EventActionFormat == "PastTense" ? "Updated" : ManagerOperationType!)}",
                "Delete" => $"{Root!.AppName}{Root!.EventSubjectSeparator}{Parent!.Name}:{(Root!.EventActionFormat == "PastTense" ? "Deleted" : ManagerOperationType!)}",
                _ => null
            });

            DataTransaction = DefaultWhereNull(DataTransaction, () => CompareValue(EventPublish, "Data") && CompareValue(Parent!.EventTransaction, true));
            DataInvoker = DefaultWhereNull(DataInvoker, () => false);
            if (DataTransaction!.Value || CompareValue(EventPublish, "Data") || CompareValue(DataExtensions, true))
                DataInvoker = true;

            DataSvcTransaction = DefaultWhereNull(DataSvcTransaction, () => CompareValue(EventPublish, "DataSvc") && CompareValue(Parent!.EventTransaction, true));
            DataSvcInvoker = DefaultWhereNull(DataSvcInvoker, () => false);
            if (DataSvcTransaction!.Value || CompareValue(EventPublish, "DataSvc"))
                DataSvcInvoker = true;

            DataSvcExtensions = DefaultWhereNull(DataSvcExtensions, () => Parent!.DataSvcExtensions);
            ExcludeAll = DefaultWhereNull(ExcludeAll, () => false);
            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, true));
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, true));
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, true));
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, true));
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, true));
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, true));
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, true));

            if (Type == "Patch")
                ExcludeIData = ExcludeData = ExcludeIDataSvc = ExcludeDataSvc = ExcludeIManager = ExcludeManager = true;

            await PrepareParametersAsync().ConfigureAwait(false);
            PrepareEvents();

            WebApiRoute = DefaultWhereNull(WebApiRoute, () => Type switch
            {
                "GetColl" => "",
                "Custom" => "",
                _ => string.Join(",", Parameters!.Where(x => !x.IsValueArg && !x.IsPagingArgs).Select(x => $"{{{x.ArgumentName}}}"))
            });

            if (Type == "Patch" || Type == "Update")
            {
                WebApiGetOperation = DefaultWhereNull(WebApiGetOperation, () => Parent!.WebApiGetOperation ?? Parent!.Operations!.Where(x => x.Type == "Get").FirstOrDefault()?.Name ?? "Get");
                var parts = string.IsNullOrEmpty(WebApiGetOperation) ? Array.Empty<string>() : WebApiGetOperation.Split(".", StringSplitOptions.RemoveEmptyEntries);
                WebApiGetVariable = parts.Length <= 1 ? "_manager" : StringConverter.ToPrivateCase(parts[0][1..]);
                if (parts.Length > 1)
                {
                    WebApiGetOperation = parts[1];
                    if (!Parent!.WebApiCtorParameters.Any(x => x.Type == parts[0]))
                        Parent!.WebApiCtorParameters.Add(new ParameterConfig { Name = parts[0][1..], Type = parts[0], Text = $"{{{{{parts[0]}}}}}" });
                }

                WebApiUpdateOperation = DefaultWhereNull(WebApiUpdateOperation, () => Parent!.Operations!.Where(x => x.Type == "Update").FirstOrDefault()?.Name ?? "Update");
                parts = string.IsNullOrEmpty(WebApiUpdateOperation) ? Array.Empty<string>() : WebApiUpdateOperation.Split(".", StringSplitOptions.RemoveEmptyEntries);
                WebApiUpdateVariable = parts.Length <= 1 ? "_manager" : StringConverter.ToPrivateCase(parts[0][1..]);
                if (parts.Length > 1)
                {
                    WebApiUpdateOperation = parts[1];
                    if (!Parent!.WebApiCtorParameters.Any(x => x.Type == parts[0]))
                        Parent!.WebApiCtorParameters.Add(new ParameterConfig { Name = parts[0][1..], Type = parts[0], Text = $"{{{{{parts[0]}}}}}" });
                }
            }

            await PrepareDataAsync().ConfigureAwait(false);
            PrepareHttpAgent();

            GrpcReturnMapper = DotNet.SystemTypes.Contains(BaseReturnType) ? null : GrpcReturnType;
            GrpcReturnConverter = BaseReturnType switch
            {
                "DateTime" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}DateTimeToTimestamp",
                "Guid" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}GuidToStringConverter",
                "decimal" => $"{(CompareValue(ReturnTypeNullable, true) ? "Nullable" : "")}DecimalToDecimalConverter",
                _ => null
            };

            CheckDeprecatedProperties();
        }

        /// <summary>
        /// Prepares the parameters.
        /// </summary>
        private async Task PrepareParametersAsync()
        {
            Parameters ??= new List<ParameterConfig>();

            var i = 0;
            var isCreateUpdate = new string[] { "Create", "Update", "Patch" }.Contains(Type);
            if (isCreateUpdate)
                Parameters.Insert(i++, new ParameterConfig { Name = "Value", Type = ValueType, Text = $"{{{{{ValueType}}}}}", Nullable = false, IsMandatory = true, Validator = Validator, IsValueArg = true, WebApiFrom = "FromBody" });

            if (PrimaryKey.HasValue && PrimaryKey.Value)
            {
                foreach (var pc in Parent!.Properties!.Where(p => p.PrimaryKey.HasValue && p.PrimaryKey.Value))
                {
                    // Do not add where same parameter (name) has been configured manually, assume overridden on purpose.
                    if (Parameters.Any(x => x.Name == pc.Name))
                        continue;

                    Parameters.Insert(i++, new ParameterConfig { Name = pc.Name, Text = pc.Text, IsMandatory = new string[] { "Get", "Delete" }.Contains(Type) || (Type == "Update" && CompareValue(WithResult, true)), LayerPassing = isCreateUpdate ? "ToManagerSet" : "All", Property = pc.Name });
                }
            }

            if (Type == "GetColl" && CompareValue(Paging, true))
                Parameters.Add(new ParameterConfig { Name = "Paging", Type = "PagingArgs", Text = "{{PagingArgs}}", IsPagingArgs = true });

            foreach (var parameter in Parameters)
            {
                await parameter.PrepareAsync(Root!, this).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Prepares the <see cref="Events"/>.
        /// </summary>
        private void PrepareEvents()
        {
            if (string.IsNullOrEmpty(EventSubject) || CompareNullOrValue(EventPublish, "None"))
                return;

            foreach (var @event in EventSubject!.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var ed = new OperationEvent { Parent = this };
                var parts = @event.Split(":");
                if (parts.Length > 0)
                    ed.Subject = parts[0];

                if (parts.Length > 1)
                    ed.Action = parts[1];
                else
                    ed.Action = ManagerOperationType!;

                if (Root!.EventSubjectRoot != null)
                    ed.Subject = Root!.EventSubjectRoot + Root!.EventSubjectSeparator + ed.Subject;

                if (ed.Subject != null)
                    ed.Subject = ed.Subject.Replace("{$key}", EventFormatKey);

                if (Root!.EventSourceKind != "None")
                    ed.Source = EventSourceUri.Replace("{$key}", EventFormatKey);

                if (HasReturnValue)
                    ed.Value = "r";
                else if (Type == "Delete" && (PrimaryKey == true || IsCoreParametersSameAsPrimaryKey))
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
                else if (CoreParameters!.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var dp in CoreParameters)
                    {
                        if (sb.Length == 0)
                            sb.Append($"new {{ ");
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
        private async Task PrepareDataAsync()
        {
            DataArgs = new ParameterConfig { Name = "<internal>", PrivateName = "__dataArgs" };
            switch (AutoImplement != "None" ? AutoImplement : Parent!.AutoImplement)
            {
                case "Database":
                    DataArgs.Name = "_db";
                    DataArgs.Type = "IDatabaseArgs";
                    break;

                case "EntityFramework":
                    DataArgs.Name = "_ef";
                    DataArgs.Type = "EfDbArgs";
                    break;

                case "Cosmos":
                    DataArgs.Name = "_cosmos";
                    DataArgs.Type = "CosmosDbArgs";
                    break;

                case "OData":
                    DataArgs.Name = "_odata";
                    DataArgs.Type = "ODataArgs";
                    break;

                case "HttpAgent":
                    DataArgs.Name = "_httpAgent";
                    DataArgs.Type = null;
                    break;

                default:
                    break;
            }

            await DataArgs.PrepareAsync(Root!, this).ConfigureAwait(false);
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
                    var op = Parent!.Operations!.FirstOrDefault(x => x.Name == name) ?? throw new CodeGenException(this, nameof(WebApiLocation), $"Attempt to lookup Operation '{name}' which does not exist.");
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
                            var p = op.Parameters!.FirstOrDefault(x => x.ArgumentName == arg[1..^1]) ?? throw new CodeGenException(this, nameof(WebApiLocation), $"Operation '{name}' WebApiRoute '{op.WebApiRoute}' references Parameter token '{arg}' that does not exist.");
                            WebApiLocation = WebApiLocation.Replace(arg, "{r." + p.Name + "}");
                            s = j + 1;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Parent!.WebApiRoutePrefix))
                    WebApiLocation = Parent!.WebApiRoutePrefix + "/" + WebApiLocation;
            }

            if (WebApiLocation?.FirstOrDefault() != '/')
                WebApiLocation = "/" + WebApiLocation;
        }

        /// <summary>
        /// Prepares the HTTP Agent properties.
        /// </summary>
        private void PrepareHttpAgent()
        {
            if (AutoImplement != "HttpAgent")
                return;

            if (HttpAgentRoute == null && !string.IsNullOrEmpty(WebApiRoute))
            {
                if (Type == "Update")
                    HttpAgentRoute = string.Join(",", Parameters!.Where(x => !x.IsValueArg && !x.IsPagingArgs).Select(x => $"{{value.{x.Name}}}"));
                else
                    HttpAgentRoute = WebApiRoute;
            }

            HttpAgentRoute = string.IsNullOrEmpty(Parent!.HttpAgentRoutePrefix) ? HttpAgentRoute : Parent!.HttpAgentRoutePrefix + (string.IsNullOrEmpty(HttpAgentRoute) ? null : "/" + HttpAgentRoute);
            HttpAgentRoute = DefaultWhereNull(HttpAgentRoute, () => WebApiRoute);

            HttpAgentMethod = DefaultWhereNull(HttpAgentMethod, () => WebApiMethod);
            HttpAgentMethod = HttpAgentMethod switch
            {
                "HttpGet" => "Get",
                "HttpPost" => "Post",
                "HttpPut" => "Put",
                "HttpPatch" => "Patch",
                "HttpDelete" => "Delete",
                _ => HttpAgentMethod
            };

            HttpAgentModel = DefaultWhereNull(HttpAgentModel, () => ValueType == null ? null : Parent!.HttpAgentModel);
            HttpAgentReturnModel = DefaultWhereNull(HttpAgentReturnModel, () => BaseReturnType == "void" || BaseReturnType != Parent!.Name ? null : Parent!.HttpAgentReturnModel ?? Parent!.HttpAgentModel);

            if (string.IsNullOrEmpty(HttpAgentModel) && new string[] { "Create", "Update", "Patch" }.Contains(Type))
                throw new CodeGenException(this, nameof(HttpAgentModel), $"Type '{Type}' requires a {nameof(HttpAgentModel)} to be specified.");

            var sb = new StringBuilder();
            if (CompareValue(WithResult, true))
                sb.Append("(await ");
            else if (HttpAgentReturnModel == null)
                sb.Append("await ");
            else
                sb.Append("(await ");

            sb.Append(DataArgs!.Name);

            if (!string.IsNullOrEmpty(Parent.HttpAgentCode))
                sb.Append($".{Parent.HttpAgentCode}");

            if (!string.IsNullOrEmpty(HttpAgentCode))
                sb.Append($".{HttpAgentCode}");

            sb.Append($".{HttpAgentMethod}");
            if (HttpAgentMethod != "Delete")
                sb.Append("MappedAsync");
            else
                sb.Append("Async");

            if (HttpAgentModel != null && HttpAgentReturnModel != null)
                sb.Append($"<{ValueType}, {HttpAgentModel}, {BaseReturnType}, {HttpAgentReturnModel}>(");
            else if (HttpAgentModel != null)
                sb.Append($"<{BaseReturnType}{(HttpAgentMethod == "Get" ? "?" : "")}, {HttpAgentReturnModel}{(HttpAgentMethod == "Get" ? "?" : "")}>(");
            else if (HttpAgentReturnModel != null)
                sb.Append($"<{ReturnType}{(HttpAgentMethod == "Get" ? "?" : "")}, {HttpAgentReturnModel}{(HttpAgentMethod == "Get" ? "?" : "")}>(");
            else
            {
                sb.Append('(');
                HttpAgentRequiresMapper = false;
            }

            sb.Append($"{(HttpAgentRoute != null && HttpAgentRoute.Contains('{') ? "$" : "")}\"{HttpAgentRoute}\"");
            if (ValueType != null)
                sb.Append($", {(CompareValue(WithResult, true) && !DataSingleLine ? "v" : "value")}");

            sb.Append(").ConfigureAwait(false)");
            if (CompareValue(WithResult, true))
                sb.Append(").ToResult()");
            else if (HttpAgentReturnModel != null)
                sb.Append(").Value");

            HttpAgentSendStatement = sb.ToString();
        }

        /// <summary>
        /// Creates the <see cref="ControllerOperationWebApiMethod"/> string.
        /// </summary>
        private string CreateControllerOperationWebApiMethod()
        {
            var sb = new StringBuilder(WebApiMethod![4..]);
            if (CompareValue(WithResult, true))
                sb.Append("WithResult");

            sb.Append("Async");

            if (HasReturnValue || (ValueType != null && WebApiMethod != "HttpPatch"))
                sb.Append('<');

            if (ValueType != null && WebApiMethod != "HttpPatch" && !(WebApiMethod == "HttpPut" && CompareValue(WebApiConcurrency, true)))
            {
                sb.Append(ValueType);
                if (HasReturnValue)
                    sb.Append(", ");
            }

            if (HasReturnValue)
                sb.Append(OperationReturnType);

            if (HasReturnValue || (ValueType != null && WebApiMethod != "HttpPatch"))
                sb.Append('>');

            return sb.ToString();
        }

        /// <summary>
        /// Creates the <see cref="AgentOperationHttpMethod"/> string.
        /// </summary>
        private string CreateAgentOperationHttpMethod()
        {
            var sb = new StringBuilder(WebApiMethod![4..]);
            sb.Append("Async");

            if (HasReturnValue || (ValueType != null && WebApiMethod != "HttpPatch"))
                sb.Append('<');

            if (ValueType != null && WebApiMethod != "HttpPatch")
            {
                sb.Append(ValueType);
                if (HasReturnValue)
                    sb.Append(", ");
            }

            if (HasReturnValue)
                sb.Append(OperationReturnType);

            if (HasReturnValue || (ValueType != null && WebApiMethod != "HttpPatch"))
                sb.Append('>');

            return sb.ToString();
        }

        /// <summary>
        /// Check for any deprecate properties and error.
        /// </summary>
        private void CheckDeprecatedProperties()
        {
            if (ExtraProperties == null || ExtraProperties.Count == 0)
                return;

            var ep = ExtraProperties.Where(x => string.Compare(x.Key, "uniqueKey", StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();
            if (ep.Key != null)
                throw new CodeGenException(this, ep.Key, $"The 'uniqueKey' configuration has been renamed to 'primaryKey'; please update the configuration accordingly.");

            ep = ExtraProperties.Where(x => string.Compare(x.Key, "patchGetOperation", StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();
            if (ep.Key != null)
                throw new CodeGenException(this, ep.Key, $"The 'patchGetOperation' configuration has been renamed to 'webApiGetOperation'; please update the configuration accordingly.");

            ep = ExtraProperties.Where(x => string.Compare(x.Key, "patchUpdateOperation", StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();
            if (ep.Key != null)
                throw new CodeGenException(this, ep.Key, $"The 'patchUpdateOperation' configuration has been renamed to 'webApiUpdateOperation'; please update the configuration accordingly.");

            CodeGenConfig.WarnWhereDeprecated(Root!, this, "eventOutbox", "iValidator");
        }
    }
}