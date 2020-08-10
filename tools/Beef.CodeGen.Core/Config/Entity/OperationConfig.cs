// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Operation</b> code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Operation", Title = "The **Operation** is used to define an operation and its charateristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("Auth", Title = "Provides the **Authorization** configuration.")]
    [CategorySchema("WebApi", Title = "Provides the data **Web API** configuration.")]
    [CategorySchema("Manager", Title = "Provides the **Manager-layer** configuration.")]
    [CategorySchema("DataSvc", Title = "Provides the **Data Services-layer** configuration.")]
    [CategorySchema("Data", Title = "Provides the generic **Data-layer** configuration.")]
    [CategorySchema("Grpc", Title = "Provides the **gRPC** configuration.")]
    [CategorySchema("Exclude", Title = "Provides the **Exclude** configuration.")]
    public class OperationConfig : ConfigBase<EntityConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the unique property name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique operation name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the operation type.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The operation type.", IsMandatory = true, IsImportant = true,
            Options = new string[] { "Get", "GetColl", "Create", "Update", "Patch", "Delete", "Custom" })]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the overridding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The text for use in comments.",
            Description = "The `Text` will be defaulted for all the `OperationType` options with the exception of `Custom`. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the .NET Type that will perform the validation.", IsImportant = true,
            Description = "Defaults to the `Entity.Validator` where not specified explicitly. Only used for `OperationType` options `Create` or `Update`.")]
        public string? Validator { get; set; }

        /// <summary>
        /// Indicates that the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters. 
        /// </summary>
        [JsonProperty("uniqueKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates that the properties marked as a unique key (`Property.UniqueKey`) are to be used as the parameters.", IsImportant = true,
            Description = "This simplifies the specification of these properties versus having to declare each specifically.")]
        public bool? UniqueKey { get; set; }

        /// <summary>
        /// Indicates that a PagingArgs argument is to be added to the operation to enable paging related logic.
        /// </summary>
        [JsonProperty("pagingArgs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates that a `PagingArgs` argument is to be added to the operation to enable (standardized) paging related logic.", IsImportant = true)]
        public bool? PagingArgs { get; set; }

        /// <summary>
        /// Gets or sets the .NET value parameter <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("valueType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The .NET value parameter `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `OperationType` options are `Create` or `Update`.")]
        public string? ValueType { get; set; }

        /// <summary>
        /// Gets or sets the .NET value parameter <see cref="System.Type"/> for the operation.
        /// </summary>
        [JsonProperty("returnType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The .NET return `Type` for the operation.",
            Description = "Defaults to the parent `Entity.Name` where the `OperationType` options are `Get`, `GetColl`, `Create` or `Update`; otherwise, defaults to `void`.")]
        public string? ReturnType { get; set; }

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
        /// Indicates whether the data logic for the operation is to be auto-implemented.
        /// </summary>
        [JsonProperty("autoImplement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the data logic for the operation is to be auto-implemented.", IsImportant = true,
            Description = "The corresponding `Entity.AutoImplement` must be defined for this to enacted. Auto-implementation is applicable for all `OperationType` options with the exception of `Custom`.")]
        public bool? AutoImplement { get; set; }

        /// <summary>
        /// Gets or sets the database stored procedure name.
        /// </summary>
        [JsonProperty("databaseStoredProc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The database stored procedure name used where the `Entity.AutoImplement` is `Database` and `Operation.AutoImplement` is `true`.",
            Description = "Defaults to `sp` + `Entity.Name` + `Operation.Name`; e.g. `spPersonCreate`.")]
        public string? DatabaseStoredProc { get; set; }

        /// <summary>
        /// Gets or sets the override for the data entity <c>Mapper</c>.
        /// </summary>
        [JsonProperty("dataEntityMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The override for the data entity `Mapper`.",
            Description = "Used where the default generated `Mapper` is not applicable.")]
        public string? DataEntityMapper { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos <c>ContainerId</c> override.
        /// </summary>
        [JsonProperty("cosmosContainerId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The Cosmos `ContainerId` override used where `Entity.AutoImplement` is `Cosmos` and `Operation.AutoImplement` is `true`.",
            Description = "Overrides the `Entity.CosmosContainerId`.")]
        public string? CosmosContainerId { get; set; }

        /// <summary>
        /// Gets or sets the C# code override to be used for setting the optional Cosmos <c>PartitionKey</c>.
        /// </summary>
        [JsonProperty("cosmosPartitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The C# code override to be used for setting the optional Cosmos `PartitionKey` used where `Entity.AutoImplement` is `Cosmos` and `Operation.AutoImplement` is `true`.",
            Description = "Overrides the `Entity.CosmosPartitionKey`.")]
        public string? CosmosPartitionKey { get; set; }

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
        /// Indicates whether to add logic to publish an event on the successful completion of the <c>DataSvc</c> layer invocation for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a `Create`, `Update` or `Delete` operation.",
            Description = "Defaults to the `CodeGeneration.EventPublish` or `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.")]
        public bool? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the event subject template and corresponding event action pair (separated by a colon).
        /// </summary>
        [JsonProperty("eventSubject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The event subject template and corresponding event action pair (separated by a colon).",
            Description = "The event subject template defaults to `{AppName}.{Entity.Name}` plus each of the unique key placeholders comma separated; e.g. Domain.Entity.{id1},{id2}. " +
            "The event action defaults to `WebApiOperationType` or `OperationType` where not specified. Multiple events can be raised by specifying more than one subject/action pair separated by a semicolon. " +
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
        /// Gets or sets the HTTP Method for the operation.
        /// </summary>
        [JsonProperty("webApiMethod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The HTTP Method for the operation.", IsImportant = true, Options = new string[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete" },
            Description = "The value defaults as follows: `HttpGet` for `OperationType` value `Get` or `GetColl`, `HttpPost` for `OperationType` value `Create` or `Custom`, " +
            "`HttpPut` for `OperationType` value `Update`, and `HttpDelete` for `OperationType` value `Delete`. An `OperationType` value `Patch` can not be specified and will always default to `HttpPatch`.")]
        public string? WebApiMethod { get; set; }

        /// <summary>
        /// Gets or sets the primary HTTP Status Code that will be returned for the operation where there is a non-null return value. 
        /// </summary>
        [JsonProperty("webApiStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a non-`null` return value. ", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound" },
            Description = "The value defaults as follows: `OK` for `OperationType` value `Get`, `GetColl`, `Update`, `Delete` or `Custom`, `Created` for `OperationType` value `Create`.")]
        public string? WebApiStatus { get; set; }

        /// <summary>
        /// Gets or sets the alternate HTTP Status Code that will be returned for the operation where there is a null return value. 
        /// </summary>
        [JsonProperty("webApiAlternateStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The primary HTTP Status Code that will be returned for the operation where there is a `null` return value. ", Options = new string[] { "OK", "Accepted", "Created", "NoContent", "NotFound", "ThrowException" },
            Description = "The value defaults as follows: `NotFound` for `OperationType` value `Get`, `NoContent` for `OperationType` value `GetColl`, `Create`, `Update` or `Patch`; otherwise, `ThrowException` which will result in an `InvalidOperationException`.")]
        public string? WebApiAlternateStatus { get; set; }

        /// <summary>
        /// Gets or sets the `ExecutionContext.OperationType` (CRUD denotation) where the `OperationType` is `Custom` (i.e. can not be inferred).
        /// </summary>
        [JsonProperty("webApiOperationType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The `ExecutionContext.OperationType` (CRUD denotation)  where the `OperationType` is `Custom` (i.e. can not be inferred).", Options = new string[] { "Create", "Read", "Updated", "Delete", "Unspecified" },
            Description = "The default will be inferred where possible; otherwise, set to `Unspecified`.")]
        public string? WebApiOperationType { get; set; }

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
        /// Indicates whether to exclude the generation of <b>all</b> <c>Operation</c> related output.
        /// </summary>
        [JsonProperty("excludeAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of all `Operation` related output.", IsImportant = true,
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties to `true`.")]
        public bool? ExcludeAll { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeIData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `Data` interface (`IXxxData.cs`).")]
        public bool? ExcludeIData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `Data` class (`XxxData.cs`).")]
        public bool? ExcludeData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeIDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `DataSvc` interface (`IXxxDataSvc.cs`).")]
        public bool? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `DataSvc` class (`XxxDataSvc.cs`).")]
        public bool? ExcludeDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeIManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `Manager` interface (`IXxxManager.cs`).")]
        public bool? ExcludeIManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the `Manager` class (`XxxManager.cs`).")]
        public bool? ExcludeManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the WebAPI `Controller` class (`XxxController.cs`).")]
        public bool? ExcludeWebApi { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApiAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the WebAPI consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the operation within the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeGrpcAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the operation within the gRPC consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeGrpcAgent { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required for the Operation.
        /// </summary>
        [JsonProperty("grpc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Operation.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Entity`, `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="ParameterConfig"/> collection.
        /// </summary>
        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema(Title = "The corresponding `Parameter` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<ParameterConfig>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the formatted summary text.
        /// </summary>
        public string? SummaryText { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            ReturnType = DefaultWhereNull(ReturnType, () => Type switch
            {
                "Get" => Parent!.EntityName,
                "GetColl" => Parent!.EntityCollectionResultName,
                "Create" => Parent!.EntityName,
                "Update" => Parent!.EntityName,
                "Delete" => "void",
                _ => "void"
            });

            ValueType = DefaultWhereNull(ReturnType, () => Type switch
            {
                "Create" => Parent!.EntityName,
                "Update" => Parent!.EntityName,
                _ => null
            });

            Text = DefaultWhereNull(Text, () => CodeGenerator.ToSentenceCase(Name));
            SummaryText = CodeGenerator.ToComments(DefaultWhereNull(Text, () => Type switch
            {
                "Get" => $"Gets the specified {{{{{ReturnType}}}}}.",
                "GetColl" => $"Gets the {{{{{ReturnType}}}}} that includes the items that match the selection criteria.",
                "Create" => $"Creates a new {{{{{ValueType}}}}}.",
                "Update" => $"Updates an existing {{{{{ValueType}}}}}.",
                "Delete" => $"Deletes the specified {{{{{Parent!.EntityName}}}}}",
                _ => Text
            }));

            ReturnText = CodeGenerator.ToComments(DefaultWhereNull(ReturnText, () => Type switch
            {
                "Get" => $"The selected {{{{{ReturnType}}}}} where found; otherwise, <c>null</c>.",
                "GetColl" => $"The {{{{{ReturnType}}}}}",
                "Create" => $"A refreshed {{{{{ReturnType}}}}}.",
                "Update" => $"A refreshed {{{{{ReturnType}}}}}.",
                "Delete" => null,
                _ => "???"
            }));

            PrivateName = DefaultWhereNull(PrivateName, () => CodeGenerator.ToPrivateCase(Name));
            AutoImplement = Parent!.AutoImplement == null ? false : AutoImplement;
            DatabaseStoredProc = DefaultWhereNull(DatabaseStoredProc, () => $"sp{Parent!.Name}{Name}");
            CosmosContainerId = DefaultWhereNull(CosmosContainerId, () => Parent!.CosmosContainerId);
            CosmosPartitionKey = DefaultWhereNull(CosmosPartitionKey, () => Parent!.CosmosPartitionKey);
            EventPublish = DefaultWhereNull(EventPublish, () => Parent!.EventPublish);
            WebApiStatus = DefaultWhereNull(WebApiStatus, () => Type! == "Create" ? "Created" : "OK");
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
                "Create" => "NoContent",
                "Update" => "NoContent",
                "Delete" => "NoContent",
                _ => "ThrowException"
            });

            WebApiOperationType = DefaultWhereNull(WebApiOperationType, () => Type switch
            {
                "Get" => "Read",
                "GetColl" => "Read",
                "Create" => "Create",
                "Update" => "Update",
                "Delete" => "Delete",
                _ => "Unspecified"
            });

            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, true));
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, true));
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, true));
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, true));
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, true));
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, true));
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, true));

            PrepareParameters();

            WebApiRoute = DefaultWhereNull(WebApiRoute, () => Type switch
            {
                "GetColl" => "",
                "Custom" => "",
                _ => string.Join(",", Parameters.Select(x => x.ArgumentName))
            });
        }

        /// <summary>
        /// Prepares the properties.
        /// </summary>
        private void PrepareParameters()
        {
            if (Parameters == null)
                Parameters = new List<ParameterConfig>();

            var i = 0;
            var isCreateUpdate = new string[] { "Create", "Update", "Patch" }.Contains(Type);

            if (isCreateUpdate)
                Parameters.Insert(i++, new ParameterConfig { Name = "Value", Type = Parent!.Name, Text = $"{{{{{Parent.Name}}}}}", IsMandatory = true});

            if (UniqueKey.HasValue && UniqueKey.Value)
            {
                foreach (var pc in Parent!.Properties.Where(p => p.UniqueKey.HasValue && p.UniqueKey.Value))
                {
                    Parameters.Insert(i++, new ParameterConfig { Name = pc.Name, IsMandatory = !isCreateUpdate, LayerPassing = isCreateUpdate ? "ToManagerSet" : "All", Property = pc.Name });
                }
            }

            foreach (var parameter in Parameters)
            {
                parameter.Prepare(this);
            }
        }
    }
}