// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the global entity code-generation configuration.
    /// </summary>
    [ClassSchema("CodeGeneration", Title = "'CodeGeneration' object (entity-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying entity-driven code generation.",
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
refDataNamespace: My.Hr.Common.Entities
refDataText: true
eventSubjectRoot: My
eventActionFormat: PastTense
entities:
```")]
    [CategorySchema("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CategorySchema("Entity", Title = "Provides the _Entity class_ configuration.")]
    [CategorySchema("Events", Title = "Provides the _Events_ configuration.")]
    [CategorySchema("WebApi", Title = "Provides the _Web API (Controller)_ configuration.")]
    [CategorySchema("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CategorySchema("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CategorySchema("Database", Title = "Provides the _Database Data-layer_ configuration.")]
    [CategorySchema("EntityFramework", Title = "Provides the _Entity Framewotrk (EF) Data-layer_ configuration.")]
    [CategorySchema("Cosmos", Title = "Provides the _CosmosDB Data-layer_ configuration.")]
    [CategorySchema("OData", Title = "Provides the _OData Data-layer_ configuration.")]
    [CategorySchema("HttpAgent", Title = "Provides the _HTTP Agent Data-layer_ configuration.")]
    [CategorySchema("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CategorySchema("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CategorySchema("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, IRootConfig
    {
        #region RefData

        /// <summary>
        /// Gets or sets the namespace for the Reference Data entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("refDataNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement).", IsImportant = true,
            Description = "Defaults to `Company` + `.` (literal) + AppName + `.` (literal) + `EntityUsing` + `.Entities` (literal).")]
        public string? RefDataNamespace { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the Reference Data common entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("refDataCommonNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The namespace for the Reference Data common entities (adds as a c# `using` statement).", IsImportant = true,
            Description = "Defaults to `Company` + `.` (literal) + AppName + `.Common.Entities` (literal).")]
        public string? RefDataCommonNamespace { get; set; }

        /// <summary>
        /// Indicates whether a corresponding <i>text</i> property is added by default when generating a Reference Data `Property` for an `Entity`.
        /// </summary>
        [JsonProperty("refDataText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` for an `Entity`.", IsImportant = true,
            Description = "This is used where serializing within the Web API `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`).")]
        public bool? RefDataText { get; set; }

        /// <summary>
        /// Gets or sets the <c>RouteAtttribute</c> for the Reference Data Web API controller required for named pre-fetching.
        /// </summary>
        [JsonProperty("refDataWebApiRoute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The `RouteAtttribute` for the Reference Data Web API controller required for named pre-fetching. The `WebApiRoutePrefix` will be prepended where specified.", IsImportant = true)]
        public string? RefDataWebApiRoute { get; set; }

        /// <summary>
        /// Gets or sets the cache used for the ReferenceData providers.
        /// </summary>
        [JsonProperty("refDataCache", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The cache used for the ReferenceData providers.", Options = new string[] { "ReferenceDataCache", "ReferenceDataMultiTenantCache" },
            Description = "Defaults to `ReferenceDataCache`. A value of `ReferenceDataCache` specifies a single-tenant cache; otherwise, `ReferenceDataMultiTenantCache` for a multi-tenant cache leverageing the `ExecutionContext.TenantId`.")]
        public string? RefDataCache { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data entity namespace appended to end of the standard <c>company.appname.Common.Entities.{AppendToNamespace}</c>.
        /// </summary>
        [JsonProperty("refDataAppendToNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The Reference Data entity namespace appended to end of the standard `company.appname.Common.Entities.{AppendToNamespace}`.",
            Description = "Defaults to `null`; being nothing to append.")]
        public string? RefDataAppendToNamespace { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the Reference Data entities (adds as a c# <c>using</c> statement) for additional business-layer inclusion where requried.
        /// </summary>
        [JsonProperty("refDataBusNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement) for additional business-layer inclusion where requried.")]
        public string? RefDataBusNamespace { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Gets or sets the entity scope option.
        /// </summary>
        [JsonProperty("entityScope", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The entity scope option.", Options = new string[] { "Common", "Business", "Autonomous" },
            Description = "Defaults to `Common` for backwards compatibility; `Autonomous` is **recommended**. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible). Additionally, there is the special case of `Autonomous` " +
            "where both a `Common` and `Business` entity are generated (where only the latter inherits from `EntityBase`, etc).")]
        public string? EntityScope { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("entityUsing", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).", Options = new string[] { "Common", "Business", "All", "None" },
            Description = "Defaults to `Common` (unless `EntityScope` is `Autonomous` and then it will default to `Business`) which will add `.Common.Entities`. Additionally, `Business` to add `.Business.Entities`, `All` to add both, and `None` to exclude any. This can be overridden for each `Entity`.")]
        public string? EntityUsing { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "None", "Newtonsoft" },
            Description = "Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets the default JSON name for the ETag property.
        /// </summary>
        [JsonProperty("etagJsonName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The default JSON name for the `ETag` property.", Options = new string[] { "etag", "eTag", "_etag", "_eTag", "ETag", "ETAG" },
            Description = "Defaults to `etag`. Note that the `JsonName` can be set individually per property where required.")]
        public string? ETagJsonName { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace1 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace2 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to be added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace3 { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.",
            Description = "This can be overridden within the `Entity`(s) and/or their corresponding `Operation`(s).")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Indicates whether to create and use an application-based (domain) <see cref="WebApi.WebApiAgentArgs"/> to simplify dependency injection usage.
        /// </summary>
        [JsonProperty("appBasedAgentArgs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "Indicates whether to create and use a domain-specific `WebApi.WebApiAgentArgs` to simplify dependency injection usage.")]
        public bool? AppBasedAgentArgs { get; set; }

        /// <summary>
        /// Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`)` is automatically inferred.
        /// </summary>
        [JsonProperty("webApiAutoLocation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.",
            Description = "This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This can be overridden within the `Entity`(s).")]
        public bool? WebApiAutoLocation { get; set; }

        /// <summary>
        /// Gets or sets the <c>RoutePrefixAtttribute</c> for the corresponding entity Web API controller.
        /// </summary>
        [JsonProperty("webApiRoutePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The `RoutePrefixAtttribute` for the corresponding entity Web API controller.", IsImportant = true,
            Description = "This is the base (prefix) `URI` prepended to all entity and underlying `Operation`(s).")]
        public string? WebApiRoutePrefix { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the layer namespace where the Validators are defined.
        /// </summary>
        [JsonProperty("validatorLayer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement).", Options = new string[] { "Business", "Common" },
            Description = "Defaults to `Business`. A value of `Business` indicates that the Validators will be defined within the `Business` namespace/assembly; otherwise, defined within the `Common` namespace/assembly.")]
        public string? ValidatorLayer { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the default .NET database interface name used where `Operation.AutoImplement` is `Database`.
        /// </summary>
        [JsonProperty("databaseName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The .NET database interface name (used where `Operation.AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to `IDatabase`. This can be overridden within the `Entity`(s).")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the default database schema name.
        /// </summary>
        [JsonProperty("databaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The default database schema name.", IsImportant = true,
            Description = "Defaults to `dbo`.")]
        public string? DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the default .NET Entity Framework interface name used where `Operation.AutoImplement` is `EntityFramework`.
        /// </summary>
        [JsonProperty("entityFrameworkName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The .NET Entity Framework interface name used where `Operation.AutoImplement` is `EntityFramework`.",
            Description = "Defaults to `IEfDb`. This can be overridden within the `Entity`(s).")]
        public string? EntityFrameworkName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET Cosmos interface name used where `Operation.AutoImplement` is `Cosmos`.
        /// </summary>
        [JsonProperty("cosmosName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The .NET Entity Framework interface name used where `Operation.AutoImplement` is `Cosmos`.", IsImportant = true,
            Description = "Defaults to `ICosmosDb`. This can be overridden within the `Entity`(s).")]
        public string? CosmosName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET OData interface name used where `Operation.AutoImplement` is `OData`.
        /// </summary>
        [JsonProperty("odataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The .NET OData interface name used where `Operation.AutoImplement` is `OData`.", IsImportant = true,
            Description = "Defaults to `IOData`. This can be overridden within the `Entity`(s).")]
        public string? ODataName { get; set; }

        /// <summary>
        /// Gets or sets the default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpRest`.
        /// </summary>
        [JsonProperty("httpAgentName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("HttpAgent", Title = "The default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.", IsImportant = true,
            Description = "Defaults to `IHttpAgent`. This can be overridden within the `Entity`(s).")]
        public string? HttpAgentName { get; set; }

        /// <summary>
        /// Gets or sets the default Reference Data property Converter used by the generated Mapper(s) where not specifically defined.
        /// </summary>
        [JsonProperty("refDataDefaultMapperConverter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The default Reference Data property `Converter` used by the generated `Mapper`(s) where not specifically defined.", 
            Options = new string[] { 
                "ReferenceDataCodeConverter", "ReferenceDataCodeConverter{T}", "ReferenceDataCodeConverter<T>",
                "ReferenceDataInt32IdConverter", "ReferenceDataInt32IdConverter{T}", "ReferenceDataInt32IdConverter<T>",
                "ReferenceDataNullableInt32IdConverter", "ReferenceDataNullableInt32IdConverter{T}", "ReferenceDataNullableInt32IdConverter<T>",
                "ReferenceDataInt64IdConverter", "ReferenceDataInt64IdConverter{T}", "ReferenceDataInt64IdConverter<T>",
                "ReferenceDataNullableInt64IdConverter", "ReferenceDataNullableInt64IdConverter{T}", "ReferenceDataNullableInt64IdConverter<T>",
                "ReferenceDataGuidIdConverter", "ReferenceDataGuidIdConverter{T}", "ReferenceDataGuidIdConverter<T>",
                "ReferenceDataNullableGuidIdConverter", "ReferenceDataNullableGuidIdConverter{T}", "ReferenceDataNullableGuidIdConverter<T>" },
            Description = "Defaults to `ReferenceDataCodeConverter<T>`. Where this value is suffixed by `<T>` or `{T}` this will automatically set `Property.DataConverterIsGeneric` to `true`.")]
        public string? RefDataDefaultMapperConverter { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code.
        /// </summary>
        [JsonProperty("dataUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The additional Namespace using statement to be added to the generated `Data` code.")]
        public string? DataUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>Database</c>.
        /// </summary>
        [JsonProperty("databaseUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The additional Namespace using statement to be added to the generated `Data` code where `Operation.AutoImplement` is `Database`.")]
        public string? DatabaseUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>EntityFramework</c>.
        /// </summary>
        [JsonProperty("entityFrameworkUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The additional Namespace using statement to be added to the generated `Data` code where `Operation.AutoImplement` is `EntityFramework`.")]
        public string? EntityFrameworkUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "additional Namespace using statement to be added to the generated `Data` code where `Operation.AutoImplement` is `Cosmos`.")]
        public string? CosmosUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "additional Namespace using statement to be added to the generated `Data` code where `Operation.AutoImplement` is `OData`.")]
        public string? ODataUsingNamespace { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = new string[] { "None", "DataSvc", "Data" },
            Description = "Defaults to `DataSvc`; unless the `EventOutbox` is not `None` where it will default to `Data`. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s).")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the data-tier event outbox persistence technology (where the events will be transactionally persisted in an outbox as part of the data-tier processing).
        /// </summary>
        [JsonProperty("eventOutbox", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The the data-tier event outbox persistence technology (where the events will be transactionally persisted in an outbox as part of the data-tier processing).", IsImportant = true, Options = new string[] { "None", "Database" },
            Description = "Defaults to `None`. A value of `Database` will result in the `DatabaseEventOutboxInvoker` being used to orchestrate.")]
        public string? EventOutbox { get; set; }

        /// <summary>
        /// Gets or sets the URI root for the event source by prepending to all event source URIs.
        /// </summary>
        [JsonProperty("eventSourceRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The URI root for the event source by prepending to all event source URIs.",
            Description = "The event source is only updated where an `EventSourceKind` is not `None`. This can be extended within the `Entity`(s).")]
        public string? EventSourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the URI kind for the event source URIs.
        /// </summary>
        [JsonProperty("eventSourceKind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The URI kind for the event source URIs.", Options = new string[] { "None", "Absolute", "Relative", "RelativeOrAbsolute" },
            Description = "Defaults to `None` (being the event source is not updated).")]
        public string? EventSourceKind { get; set; }

        /// <summary>
        /// Gets or sets the root for the event Subject name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The root for the event Subject name by prepending to all event subject names.", IsImportant = true,
            Description = "Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s).")]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The default formatting for the Subject when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey" },
            Description = "Defaults to `NameAndKey` (being the event subject name appended with the corresponding unique key.)`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the subject path separator.
        /// </summary>
        [JsonProperty("eventSubjectSeparator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Event", Title = "The subject path separator.",
            Description = "Defaults to `.`. Used only where the subject is automatically inferred.")]
        public string? EventSubjectSeparator { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Event", Title = "The formatting for the Action when an Event is published.", Options = new string[] { "None", "PastTense" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required, i.e. as-is)`.")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.
        /// </summary>
        [JsonProperty("eventTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Event", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.", IsImportant = true,
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
        [PropertySchema("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model. Must be set to `true` for any of the subordinate gRPC capabilities to be code-generated. Will require each `Entity`, and corresponding `Property` and `Operation` to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonProperty("pathBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Path", Title = "The base path (directory) prefix for the artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Common-related artefacts.
        /// </summary>
        [JsonProperty("pathCommon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Path", Title = "The path (directory) for the Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? PathCommon { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Business-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("pathBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Path", Title = "The path (directory) for the Business-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? PathBusiness { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the CDC-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("pathApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Path", Title = "The path (directory) for the API-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.")]
        public string? PathApi { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Common-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceCommon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Namespace", Title = "The Namespace (root) for the Common-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? NamespaceCommon { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Business-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Namespace", Title = "The Namespace (root) for the Business-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? NamespaceBusiness { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Api-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Namespace", Title = "The Namespace (root) for the Api-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.` + `ApiName` (runtime parameter). For example `Beef.Demo.Api`.")]
        public string? NamespaceApi { get; set; }

        #endregion

        #region RuntimeParameters

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        public Dictionary<string, string> RuntimeParameters { get; internal set; } = new Dictionary<string, string>();

        /// <summary>
        /// Replaces the <see cref="RuntimeParameters"/> with the specified <paramref name="parameters"/> (copies values).
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public void ReplaceRuntimeParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return;

            foreach (var p in parameters)
            {
                if (RuntimeParameters.ContainsKey(p.Key))
                    RuntimeParameters[p.Key] = p.Value;
                else
                    RuntimeParameters.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Resets the runtime parameters.
        /// </summary>
        public void ResetRuntimeParameters() => RuntimeParameters.Clear();

        /// <summary>
        /// Gets the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetRuntimeParameter<T>(string key, T defaultValue = default!)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val.ToString(), typeof(T));
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> if the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetRuntimeParameter<T>(string key, out T value)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val.ToString(), typeof(T));
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="EntityConfig"/> collection.
        /// </summary>
        [JsonProperty("entities", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Entity` collection.", IsImportant = true,
            Markdown = "An `Entity` object provides the primary configuration for an entity, its properties and operations.")]
        public List<EntityConfig>? Entities { get; set; }

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxManager.  
        /// </summary>
        public List<EntityConfig>? IManagerEntities => Entities.Where(x => CompareNullOrValue(x.ExcludeIManager, false) && x.Operations!.Count > 0).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataSvcEntities => Entities.Where(x => CompareNullOrValue(x.ExcludeIDataSvc, false) && x.Operations!.Count > 0).ToList();
 
        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataEntities => Entities.Where(x => CompareNullOrValue(x.ExcludeIData, false) && x.Operations!.Count > 0).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for Reference Data.  
        /// </summary>
        public List<EntityConfig>? RefDataEntities => Entities.Where(x => !string.IsNullOrEmpty(x.RefDataType) && CompareNullOrValue(x.Abstract, false)).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for Grpc.  
        /// </summary>
        public List<EntityConfig>? GrpcEntities => Entities.Where(x => CompareValue(x.Grpc, true) && CompareNullOrValue(x.Abstract, false)).ToList();

        /// <summary>
        /// Gets the company name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string? Company => GetRuntimeParameter<string?>("Company")!;

        /// <summary>
        /// Gets the application name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string? AppName => GetRuntimeParameter<string?>("AppName")!;

        /// <summary>
        /// Gets the API name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string? ApiName => DefaultWhereNull(GetRuntimeParameter<string?>("ApiName"), () => "Api")!;

        /// <summary>
        /// Gets the entity scope from the from the <see cref="RuntimeParameters"/> (defaults to 'Common').
        /// </summary>
        public string RuntimeEntityScope => DefaultWhereNull(GetRuntimeParameter<string?>("EntityScope"), () => "Common")!;

        /// <summary>
        /// Indicates whether to generate an <c>Entity</c> as a <c>DataModel</c> where the <see cref="EntityConfig.DataModel"/> is selected (from the <see cref="RuntimeParameters"/>).
        /// </summary>
        public bool ModelFromEntity => GetRuntimeParameter<bool>("ModelFromEntity");

        /// <summary>
        /// Indicates whether the intended Entity code generation is a Data Model and therefore should not inherit from <see cref="EntityBase"/> (from the <see cref="RuntimeParameters"/>).
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
        protected override void Prepare()
        {
            CheckOptionsProperties();

            PathBase = DefaultWhereNull(PathBase, () => $"{Company}.{AppName}");
            PathCommon = DefaultWhereNull(PathCommon, () => $"{PathBase}.Common");
            PathBusiness = DefaultWhereNull(PathBusiness, () => $"{PathBase}.Business");
            PathApi = DefaultWhereNull(PathApi, () => $"{PathBase}.{ApiName}");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => $"{Company}.{AppName}");
            NamespaceCommon = DefaultWhereNull(NamespaceCommon, () => $"{NamespaceBase}.Common");
            NamespaceBusiness = DefaultWhereNull(NamespaceBusiness, () => $"{NamespaceBase}.Business");
            NamespaceApi = DefaultWhereNull(NamespaceApi, () => $"{NamespaceBase}.{ApiName}");

            WebApiAutoLocation = DefaultWhereNull(WebApiAutoLocation, () => false);
            RefDataCache = DefaultWhereNull(RefDataCache, () => "ReferenceDataCache");
            ValidatorLayer = DefaultWhereNull(ValidatorLayer, () => "Business");
            EventSourceKind = DefaultWhereNull(EventSourceKind, () => "None");
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => "NameAndKey");
            EventSubjectSeparator = DefaultWhereNull(EventSubjectSeparator, () => ".");
            EventOutbox = DefaultWhereNull(EventOutbox, () => "None");
            EventPublish = DefaultWhereNull(EventPublish, () => EventOutbox == "Database" ? "Data" : "DataSvc");
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "None");
            EntityScope = DefaultWhereNull(EntityScope, () => "Common");
            EntityUsing = DefaultWhereNull(EntityUsing, () => EntityScope == "Autonomous" ? "Business" : "Common");
            RefDataNamespace = DefaultWhereNull(RefDataNamespace, () => $"{Company}.{AppName}.{EntityUsing}.Entities");
            RefDataCommonNamespace = DefaultWhereNull(RefDataCommonNamespace, () => $"{Company}.{AppName}.Common.Entities");
            DatabaseSchema = DefaultWhereNull(DatabaseSchema, () => "dbo");
            DatabaseName = DefaultWhereNull(DatabaseName, () => "IDatabase");
            EntityFrameworkName = DefaultWhereNull(EntityFrameworkName, () => "IEfDb");
            CosmosName = DefaultWhereNull(CosmosName, () => "ICosmosDb");
            ODataName = DefaultWhereNull(ODataName, () => "IOData");
            HttpAgentName = DefaultWhereNull(HttpAgentName, () => "IHttpAgent");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "Newtonsoft");
            ETagJsonName = DefaultWhereNull(ETagJsonName, () => "etag");
            RefDataDefaultMapperConverter = DefaultWhereNull(RefDataDefaultMapperConverter, () => "ReferenceDataCodeConverter<T>");

            if (!string.IsNullOrEmpty(WebApiRoutePrefix))
                RefDataWebApiRoute = string.IsNullOrEmpty(RefDataWebApiRoute) ? WebApiRoutePrefix :
                    $"{(WebApiRoutePrefix.EndsWith('/') ? WebApiRoutePrefix[..^1] : WebApiRoutePrefix)}/{(RefDataWebApiRoute.StartsWith('/') ? RefDataWebApiRoute[1..] : RefDataWebApiRoute)}";

            if (Entities != null && Entities.Count > 0)
            {
                foreach (var entity in Entities)
                {
                    entity.Prepare(Root!, this);
                }
            }

            RefData = new RefDataConfig();
            RefData.Prepare(Root!, this);

            if (Entities != null && Entities.Count > 0)
            {
                foreach (var e in Entities)
                {
                    foreach (var o in e.Operations!)
                    {
                        foreach (var p in o.Parameters!.Where(x => x.IValidator != null))
                        {
                            var pc = new ParameterConfig { Name = p.Validator, Type = p.IValidator };
                            if (!Validators.Any(x => x.Type == pc.Type))
                                Validators.Add(pc);
                        }
                    }
                }
            }
        }
    }
}