// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx.Entities.Extended;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the global entity code-generation configuration.
    /// </summary>
    [CodeGenClass("CodeGeneration", Title = "'CodeGeneration' object (entity-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying entity-driven code generation.",
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
refDataNamespace: My.Hr.Common.Entities
refDataText: true
eventSubjectRoot: My
eventActionFormat: PastTense
entities:
```")]
    [CodeGenCategory("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CodeGenCategory("Entity", Title = "Provides the _Entity class_ configuration.")]
    [CodeGenCategory("Events", Title = "Provides the _Events_ configuration.")]
    [CodeGenCategory("WebApi", Title = "Provides the _Web API (Controller)_ configuration.")]
    [CodeGenCategory("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CodeGenCategory("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CodeGenCategory("Database", Title = "Provides the _Database Data-layer_ configuration.")]
    [CodeGenCategory("EntityFramework", Title = "Provides the _Entity Framewotrk (EF) Data-layer_ configuration.")]
    [CodeGenCategory("Cosmos", Title = "Provides the _CosmosDB Data-layer_ configuration.")]
    [CodeGenCategory("OData", Title = "Provides the _OData Data-layer_ configuration.")]
    [CodeGenCategory("HttpAgent", Title = "Provides the _HTTP Agent Data-layer_ configuration.")]
    [CodeGenCategory("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CodeGenCategory("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CodeGenCategory("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigRootBase<CodeGenConfig>
    {
        #region RefData

        /// <summary>
        /// Gets or sets the namespace for the Reference Data entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("refDataNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement).", IsImportant = true,
            Description = "Defaults to `Company` + `.` (literal) + AppName + `.Business.Entities` (literal).")]
        public string? RefDataNamespace { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the Reference Data common entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("refDataCommonNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The namespace for the Reference Data common entities (adds as a c# `using` statement).", IsImportant = true,
            Description = "Defaults to `Company` + `.` (literal) + AppName + `.Common.Entities` (literal).")]
        public string? RefDataCommonNamespace { get; set; }

        /// <summary>
        /// Indicates whether a corresponding <i>text</i> property is added by default when generating a Reference Data `Property` for an `Entity`.
        /// </summary>
        [JsonProperty("refDataText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` for an `Entity`.", IsImportant = true,
            Description = "This is used where serializing within the Web API `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`). This can be further configured on the `Entity` and for each `Property`.")]
        public bool? RefDataText { get; set; }

        /// <summary>
        /// Gets or sets the <c>RouteAtttribute</c> for the Reference Data Web API controller required for named pre-fetching.
        /// </summary>
        [JsonProperty("refDataWebApiRoute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The `RouteAtttribute` for the Reference Data Web API controller required for named pre-fetching. The `WebApiRoutePrefix` will be prepended where specified.", IsImportant = true)]
        public string? RefDataWebApiRoute { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `ReferenceDataData` constructor.
        /// </summary>
        [JsonProperty("refDataDataCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Data", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `ReferenceDataData` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? RefDataDataCtorParams { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Entity", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "SystemText", "Newtonsoft" },
            Description = "Defaults to `SystemText`. This can be overridden within the `Entity`(s).")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets the default JSON name for the ETag property.
        /// </summary>
        [JsonProperty("etagJsonName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Entity", Title = "The default JSON name for the `ETag` property.", Options = new string[] { "etag", "eTag", "_etag", "_eTag", "ETag", "ETAG" },
            Description = "Defaults to `etag`. Note that the `JsonName` can be set individually per property where required.")]
        public string? ETagJsonName { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace1 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace2 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace3 { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.",
            Description = "This can be overridden within the `Entity`(s) and/or their corresponding `Operation`(s).")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`)` is automatically inferred.
        /// </summary>
        [JsonProperty("webApiAutoLocation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.",
            Description = "This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This can be overridden within the `Entity`(s).")]
        public bool? WebApiAutoLocation { get; set; }

        /// <summary>
        /// Gets or sets the <c>RoutePrefixAtttribute</c> for the corresponding entity Web API controller.
        /// </summary>
        [JsonProperty("webApiRoutePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("WebApi", Title = "The `RoutePrefixAtttribute` for the corresponding entity Web API controller.", IsImportant = true,
            Description = "This is the base (prefix) `URI` prepended to all entity and underlying `Operation`(s).")]
        public string? WebApiRoutePrefix { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.
        /// </summary>
        [JsonProperty("managerCleanUp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.",
            Description = "This can be overridden within the `Entity`(s) and `Operation`(s).")]
        public bool? ManagerCleanUp { get; set; }

        /// <summary>
        /// Gets or sets the `Validation` framework. 
        /// </summary>
        [JsonProperty("validationFramework", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Manager", Title = "The `Validation` framework to use for the entity-based validation.", Options = new string[] { "CoreEx", "FluentValidation" },
            Description = "Defaults to `CoreEx` (literal). This can be overridden within the `Entity`(s), `Operation`(s) and `Parameter`(s).")]
        public string? ValidationFramework { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the default .NET database interface name used where `Operation.AutoImplement` is `Database`.
        /// </summary>
        [JsonProperty("databaseName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The .NET database interface name (used where `Operation.AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to `IDatabase`. This can be overridden within the `Entity`(s).")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the default database schema name.
        /// </summary>
        [JsonProperty("databaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The default database schema name.", IsImportant = true,
            Description = "Defaults to `dbo`.")]
        public string? DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the database provider.
        /// </summary>
        [JsonProperty("databaseProvider", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The default database schema name.", IsImportant = true, Options = new string[] { "SqlServer", "MySQL" },
            Description = "Defaults to `SqlServer`. Enables specific database provider functionality/formatting/etc. where applicable.")]
        public string? DatabaseProvider { get; set; }

        /// <summary>
        /// Gets or sets the default .NET Entity Framework interface name used where `Operation.AutoImplement` is `EntityFramework`.
        /// </summary>
        [JsonProperty("entityFrameworkName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("EntityFramework", Title = "The .NET Entity Framework interface name used where `Operation.AutoImplement` is `EntityFramework`.",
            Description = "Defaults to `IEfDb`. This can be overridden within the `Entity`(s).")]
        public string? EntityFrameworkName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET Cosmos interface name used where `Operation.AutoImplement` is `Cosmos`.
        /// </summary>
        [JsonProperty("cosmosName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Cosmos", Title = "The .NET Entity Framework interface name used where `Operation.AutoImplement` is `Cosmos`.", IsImportant = true,
            Description = "Defaults to `ICosmosDb`. This can be overridden within the `Entity`(s).")]
        public string? CosmosName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET OData interface name used where `Operation.AutoImplement` is `OData`.
        /// </summary>
        [JsonProperty("odataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("OData", Title = "The .NET OData interface name used where `Operation.AutoImplement` is `OData`.", IsImportant = true,
            Description = "Defaults to `IOData`. This can be overridden within the `Entity`(s).")]
        public string? ODataName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpRest`.
        /// </summary>
        [JsonProperty("httpAgentName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("HttpAgent", Title = "The default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.", IsImportant = true,
            Description = "Defaults to `IHttpAgent`. This can be overridden within the `Entity`(s).")]
        public string? HttpAgentName { get; set; }

        /// <summary>
        /// Gets or sets the default ETag to/from RowVersion Mapping Converter used.
        /// </summary>
        [JsonProperty("etagDefaultMapperConverter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "The default ETag to/from RowVersion column Mapping Converter used where `Operation.AutoImplement` is `Database` or `EntityFramework`.", IsImportant = true,
            Description = "Defaults to `StringToBase64Converter`.")]
        public string? ETagDefaultMapperConverter { get; set; }

        /// <summary>
        /// Gets or sets the default Reference Data property Converter used by the generated Mapper(s) where not specifically defined.
        /// </summary>
        [JsonProperty("refDataDefaultMapperConverter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Data", Title = "The default Reference Data property `Converter` used by the generated `Mapper`(s) where not specifically defined.", 
            Options = new string[] { 
                "ReferenceDataCodeConverter", "ReferenceDataCodeConverter{T}", "ReferenceDataCodeConverter<T>",
                "ReferenceDataIdConverter{T, int}", "ReferenceDataIdConverter<T, int>", "ReferenceDataIdConverter{T, int?}", "ReferenceDataIdConverter<T, int?>",
                "ReferenceDataIdConverter{T, long}", "ReferenceDataIdConverter<T, long>", "ReferenceDataIdConverter{T, long?}", "ReferenceDataIdConverter<T, long?>",
                "ReferenceDataIdConverter{T, Guid}", "ReferenceDataIdConverter<T, Guid>", "ReferenceDataIdConverter{T, Guid?}", "ReferenceDataIdConverter<T, Guid?>",
                "ReferenceDataInt32IdConverter", "ReferenceDataInt32IdConverter{T}", "ReferenceDataInt32IdConverter<T>",
                "ReferenceDataNullableInt32IdConverter", "ReferenceDataNullableInt32IdConverter{T}", "ReferenceDataNullableInt32IdConverter<T>",
                "ReferenceDataInt64IdConverter", "ReferenceDataInt64IdConverter{T}", "ReferenceDataInt64IdConverter<T>",
                "ReferenceDataNullableInt64IdConverter", "ReferenceDataNullableInt64IdConverter{T}", "ReferenceDataNullableInt64IdConverter<T>",
                "ReferenceDataGuidIdConverter", "ReferenceDataGuidIdConverter{T}", "ReferenceDataGuidIdConverter<T>",
                "ReferenceDataNullableGuidIdConverter", "ReferenceDataNullableGuidIdConverter{T}", "ReferenceDataNullableGuidIdConverter<T>" },
            Description = "Defaults to `ReferenceDataCodeConverter<T>`. Where this value is suffixed by `<T>` or `{T}` this will automatically be set to the `Type`.")]
        public string? RefDataDefaultMapperConverter { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data code data name
        /// </summary>
        [JsonProperty("refDataCodeDataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The Reference Data `Code` data name.",
            Description = "Defaults to `Code` (literal).")]
        public string? RefDataCodeDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data text data name.
        /// </summary>
        [JsonProperty("refDataTextDataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The Reference Data `Text` data name.",
            Description = "Defaults to `Text` (literal).")]
        public string? RefDataTextDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data is active name.
        /// </summary>
        [JsonProperty("refDataIsActiveDataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The Reference Data `IsActive` data name.",
            Description = "Defaults to `IsActive` (literal).")]
        public string? RefDataIsActiveDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data sort order data name.
        /// </summary>
        [JsonProperty("refDataSortOrderDataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The Reference Data `SortOrder` data name.",
            Description = "Defaults to `SortOrder` (literal).")]
        public string? RefDataSortOrderDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data is ETag name.
        /// </summary>
        [JsonProperty("refDataETagDataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("RefData", Title = "The Reference Data `ETag` data name.",
            Description = "Defaults to `RowVersion` (literal).")]
        public string? RefDataETagDataName { get; set; } = "*";

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = new string[] { "None", "DataSvc", "Data" },
            Description = "Defaults to `DataSvc`. Used to enable the sending of messages to the likes of EventHub, ServiceBus, SignalR, etc. This can be overridden within the `Entity`(s).")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the URI root for the event source by prepending to all event source URIs.
        /// </summary>
        [JsonProperty("eventSourceRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The URI root for the event source by prepending to all event source URIs.",
            Description = "The event source is only updated where an `EventSourceKind` is not `None`. This can be extended within the `Entity`(s).")]
        public string? EventSourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the URI kind for the event source URIs.
        /// </summary>
        [JsonProperty("eventSourceKind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The URI kind for the event source URIs.", Options = new string[] { "None", "Absolute", "Relative", "RelativeOrAbsolute" },
            Description = "Defaults to `None` (being the event source is not updated).")]
        public string? EventSourceKind { get; set; }

        /// <summary>
        /// Gets or sets the root for the event Subject name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Events", Title = "The root for the event Subject name by prepending to all event subject names.", IsImportant = true,
            Description = "Used to enable the sending of messages to the likes of EventHub, ServiceBus, SignalR, etc. This can be overridden within the `Entity`(s).")]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the subject path separator.
        /// </summary>
        [JsonProperty("eventSubjectSeparator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The subject path separator.",
            Description = "Defaults to `.`. Used only where the subject is automatically inferred.")]
        public string? EventSubjectSeparator { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The formatting for the Action when an Event is published.", Options = new string[] { "None", "PastTense" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required, i.e. as-is)`.")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.
        /// </summary>
        [JsonProperty("eventTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.", IsImportant = true,
            Description = "Usage will force a rollback of any underlying data transaction (where the provider supports TransactionScope) on failure, such as an `EventPublish` error. " +
                "This is by no means implying a Distributed Transaction (DTC) should be invoked; this is only intended for a single data source that supports a TransactionScope to guarantee reliable event publishing. " +
                "Defaults to `false`. This essentially defaults the `Entity.EventTransaction` where not otherwise specified. This should only be used where `EventPublish` is `DataSvc` and a transactionally-aware data source is being used.")]
        public bool? EventTransaction { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required.
        /// </summary>
        [JsonProperty("grpc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model. Must be set to `true` for any of the subordinate gRPC capabilities to be code-generated. Will require each `Entity`, and corresponding `Property` and `Operation` to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonProperty("pathBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The base path (directory) prefix for the artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Common-related artefacts.
        /// </summary>
        [JsonProperty("pathCommon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? PathCommon { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Business-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("pathBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Business-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? PathBusiness { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the API-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("pathApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the API-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.")]
        public string? PathApi { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Common-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceCommon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Common-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? NamespaceCommon { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Business-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Business-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? NamespaceBusiness { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Api-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Api-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.")]
        public string? NamespaceApi { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="EntityConfig"/> collection.
        /// </summary>
        [JsonProperty("entities", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Entity` collection.", IsImportant = true,
            Markdown = "An `Entity` object provides the primary configuration for an entity, its properties and operations.")]
        public List<EntityConfig>? Entities { get; set; }

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxManager.  
        /// </summary>
        public List<EntityConfig>? IManagerEntities => Entities!.Where(x => CompareNullOrValue(x.ExcludeIManager, false) && x.Operations!.Count > 0).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataSvcEntities => Entities!.Where(x => CompareNullOrValue(x.ExcludeIDataSvc, false) && x.Operations!.Count > 0).ToList();
 
        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataEntities => Entities!.Where(x => CompareNullOrValue(x.ExcludeIData, false) && x.Operations!.Count > 0).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for Reference Data.  
        /// </summary>
        public List<EntityConfig>? RefDataEntities => Entities!.Where(x => !string.IsNullOrEmpty(x.RefDataType) && CompareNullOrValue(x.Abstract, false)).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for Grpc.  
        /// </summary>
        public List<EntityConfig>? GrpcEntities => Entities!.Where(x => CompareValue(x.Grpc, true) && CompareNullOrValue(x.Abstract, false)).ToList();

        /// <summary>
        /// Gets the company name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? Company => CodeGenArgs!.GetCompany(true);

        /// <summary>
        /// Gets the application name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? AppName => CodeGenArgs!.GetAppName(true);

        /// <summary>
        /// Gets the API name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? ApiName => DefaultWhereNull(CodeGenArgs!.GetParameter<string>(CodeGenConsole.ApiNameParamName), () => "Api")!;

        /// <summary>
        /// Gets the entity scope from the from the <see cref="IRootConfig.RuntimeParameters"/> (defaults to 'Common').
        /// </summary>
        public string RuntimeEntityScope => GetRuntimeParameter<string?>("EntityScope") ?? "Business";

        /// <summary>
        /// Indicates whether to generate an <c>Entity</c> as a <c>DataModel</c> where the <see cref="EntityConfig.DataModel"/> is selected (from the <see cref="IRootConfig.RuntimeParameters"/>).
        /// </summary>
        public bool ModelFromEntity => GetRuntimeParameter<bool>("ModelFromEntity");

        /// <summary>
        /// Indicates whether the intended Entity code generation is a Data Model and therefore should not inherit from <see cref="EntityBase"/> (from the <see cref="IRootConfig.RuntimeParameters"/>).
        /// </summary>
        public bool IsDataModel => GetRuntimeParameter<bool>("IsDataModel");

        /// <summary>
        /// Indicates whether the intended code generation is explicitly for Reference Data.
        /// </summary>
        public bool IsRefData => GetRuntimeParameter<bool>("IsRefData");

        /// <summary>
        /// Gets the reference data specific properties.
        /// </summary>
        public RefDataConfig? RefData { get; private set; }

        /// <summary>
        /// Gets the list of all the used validators.
        /// </summary>
        public List<ParameterConfig> Validators { get; } = new List<ParameterConfig>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            PathBase = DefaultWhereNull(PathBase, () => $"{Company}.{AppName}");
            PathCommon = DefaultWhereNull(PathCommon, () => $"{PathBase}.Common");
            PathBusiness = DefaultWhereNull(PathBusiness, () => $"{PathBase}.Business");
            PathApi = DefaultWhereNull(PathApi, () => $"{PathBase}.{ApiName}");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => $"{Company}.{AppName}");
            NamespaceCommon = DefaultWhereNull(NamespaceCommon, () => $"{NamespaceBase}.Common");
            NamespaceBusiness = DefaultWhereNull(NamespaceBusiness, () => $"{NamespaceBase}.Business");
            NamespaceApi = DefaultWhereNull(NamespaceApi, () => $"{NamespaceBase}.{ApiName}");

            ManagerCleanUp = DefaultWhereNull(ManagerCleanUp, () => false);
            ValidationFramework = DefaultWhereNull(ValidationFramework, () => "CoreEx");
            WebApiAutoLocation = DefaultWhereNull(WebApiAutoLocation, () => false);
            EventSourceKind = DefaultWhereNull(EventSourceKind, () => "None");
            EventSubjectSeparator = DefaultWhereNull(EventSubjectSeparator, () => ".");
            EventPublish = DefaultWhereNull(EventPublish, () => "DataSvc");
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "None");
            RefDataNamespace = DefaultWhereNull(RefDataNamespace, () => $"{Company}.{AppName}.Business.Entities");
            RefDataCommonNamespace = DefaultWhereNull(RefDataCommonNamespace, () => $"{Company}.{AppName}.Common.Entities");
            DatabaseProvider = DefaultWhereNull(DatabaseProvider, () => "SqlServer");
            DatabaseSchema = DefaultWhereNull(DatabaseSchema, () => DatabaseProvider == "SqlServer" ? "dbo" : "");
            DatabaseName = DefaultWhereNull(DatabaseName, () => "IDatabase");
            EntityFrameworkName = DefaultWhereNull(EntityFrameworkName, () => "IEfDb");
            CosmosName = DefaultWhereNull(CosmosName, () => "ICosmosDb");
            ODataName = DefaultWhereNull(ODataName, () => "IOData");
            HttpAgentName = DefaultWhereNull(HttpAgentName, () => "IHttpAgent");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "SystemText");
            ETagJsonName = DefaultWhereNull(ETagJsonName, () => "etag");
            ETagDefaultMapperConverter = DefaultWhereNull(ETagDefaultMapperConverter, () => nameof(CoreEx.Mapping.Converters.StringToBase64Converter));
            RefDataDefaultMapperConverter = DefaultWhereNull(RefDataDefaultMapperConverter, () => "ReferenceDataCodeConverter<T>");
            RefDataCodeDataName = DefaultWhereNull(RefDataCodeDataName, () => "Code");
            RefDataTextDataName = DefaultWhereNull(RefDataTextDataName, () => "Text");
            RefDataIsActiveDataName = DefaultWhereNull(RefDataIsActiveDataName, () => "IsActive");
            RefDataSortOrderDataName = DefaultWhereNull(RefDataSortOrderDataName, () => "SortOrder");

            if (!string.IsNullOrEmpty(WebApiRoutePrefix))
                RefDataWebApiRoute = string.IsNullOrEmpty(RefDataWebApiRoute) ? WebApiRoutePrefix :
                    $"{(WebApiRoutePrefix.EndsWith('/') ? WebApiRoutePrefix[..^1] : WebApiRoutePrefix)}/{(RefDataWebApiRoute.StartsWith('/') ? RefDataWebApiRoute[1..] : RefDataWebApiRoute)}";

            Entities = await PrepareCollectionAsync(Entities).ConfigureAwait(false);

            RefData = new RefDataConfig();
            await RefData.PrepareAsync(Root!, this).ConfigureAwait(false);

            foreach (var e in Entities)
            {
                foreach (var o in e.Operations!)
                {
                    foreach (var p in o.Parameters!.Where(x => x.Validator != null))
                    {
                        var pc = new ParameterConfig { Name = p.Validator, Type = p.Validator, ValidationFramework = p.ValidationFramework };
                        if (!Validators.Any(x => x.Type == pc.Type))
                            Validators.Add(pc);
                    }
                }
            }

            // Check for any deprecate properties and warn.
            WarnWhereDeprecated(this, this,
                "refDataCache",
                "refDataAppendToNamespace",
                "refDataBusNamespace",
                "entityScope",
                "entityUsing",
                "appBasedAgentArgs",
                "validatorLayer",
                "dataUsingNamespace",
                "databaseUsingNamespace",
                "entityFrameworkUsingNamespace",
                "cosmosUsingNamespace",
                "odataUsingNamespace",
                "eventOutbox",
                "eventSubjectFormat",
                "eventCasing");
        }

        /// <summary>
        /// Warn where the property has been deprecated.
        /// </summary>
        /// <param name="root">The root <see cref="CodeGenConfig"/>.</param>
        /// <param name="config">The <see cref="ConfigBase"/>.</param>
        /// <param name="names">The list of deprecated properties.</param>
        internal static void WarnWhereDeprecated(CodeGenConfig root, ConfigBase config, params string[] names)
        {
            if (config.ExtraProperties == null || config.ExtraProperties.Count == 0 || names.Length == 0)
                return;

            foreach (var xp in config.ExtraProperties)
            {
                if (names.Contains(xp.Key))
                    root.CodeGenArgs?.Logger?.LogWarning("{Deprecated}", $"Warning: Config [{config.BuildFullyQualifiedName(xp.Key)}] has been deprecated and will be ignored.");
            }
        }

        /// <summary>
        /// Warn where the property has been deprecated.
        /// </summary>
        /// <param name="root">The root <see cref="CodeGenConfig"/>.</param>
        /// <param name="config">The <see cref="ConfigBase"/>.</param>
        /// <param name="properties">The list of deprecated properties.</param>
        internal static void WarnWhereDeprecated(CodeGenConfig root, ConfigBase config, params (string Property, string? Message)[] properties)
        {
            if (config.ExtraProperties == null || config.ExtraProperties.Count == 0 || properties.Length == 0)
                return;

            foreach (var xp in config.ExtraProperties)
            {
                var (Property, Message) = properties!.FirstOrDefault(x => x.Property == xp.Key);
                if (Property != null)
                    root.CodeGenArgs?.Logger?.LogWarning("{Deprecated}", $"Warning: Config [{config.BuildFullyQualifiedName(xp.Key)}] has been deprecated and will be ignored.{(string.IsNullOrEmpty(Message) ? string.Empty : Message)}");
            }
        }
    }
}