// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the global entity code-generation configuration.
    /// </summary>
    [ClassSchema("CodeGeneration", Title = "'CodeGeneration' object (entity-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying entity-driven code generation.",
        Markdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
refDataNamespace: My.Hr.Common.Entities
refDataText: true
eventSubjectRoot: My
eventActionFormat: PastTense
entities:
```")]
    [CategorySchema("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CategorySchema("Entity", Title = "Provides the _Entity class_ configuration.")]
    [CategorySchema("WebApi", Title = "Provides the _Web API (Controller)_ configuration.")]
    [CategorySchema("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CategorySchema("DataSvc", Title = "Provides the _Data Services-layer_ configuration.")]
    [CategorySchema("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CategorySchema("Database", Title = "Provides the _Database Data-layer_ configuration.")]
    [CategorySchema("EntityFramework", Title = "Provides the _Entity Framewotrk (EF) Data-layer_ configuration.")]
    [CategorySchema("Cosmos", Title = "Provides the _CosmosDB Data-layer_ configuration.")]
    [CategorySchema("OData", Title = "Provides the _OData Data-layer_ configuration.")]
    [CategorySchema("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, IRootConfig
    {
        #region RefData

        /// <summary>
        /// Gets or sets the namespace for the Reference Data entities (adds as a c# <c>using</c> statement) where the <see cref="EntityConfig.EntityScope"/> is `Common`.
        /// </summary>
        [JsonProperty("refDataNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement) where the `Entity.EntityScope` property configuration is `Common`.", IsImportant = true)]
        public string? RefDataNamespace { get; set; }

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
        [PropertySchema("RefData", Title = "The `RouteAtttribute` for the Reference Data Web API controller required for named pre-fetching.", IsImportant = true)]
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
        /// Gets or sets the namespace for the Reference Data entities (adds as a c# <c>using</c> statement) where the <see cref="EntityConfig.EntityScope"/> is `Business`.
        /// </summary>
        [JsonProperty("refDataBusNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The namespace for the Reference Data entities (adds as a c# `using` statement) where the `Entity.EntityScope` property configuration is `Business`.")]
        public string? RefDataBusNamespace { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "None", "Newtonsoft" },
            Description = "Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("entityUsing", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).", Options = new string[] { "Common", "Business", "All", "None" },
            Description = "Defaults to `Common` which will add `.Common.Entities`. Otherwise, `Business` to add `.Business.Entities`, `All` to add both, and `None` to exclude any.")]
        public string? EntityUsing { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to the added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace1 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to the added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace2 { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Entity</c> code.
        /// </summary>
        [JsonProperty("usingNamespace3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The additional Namespace using statement to the added to the generated `Entity` code.",
            Description = "Typically used where referening a `Type` from a Namespace that is not generated by default.")]
        public string? UsingNamespace3 { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.", IsImportant = true,
            Description = "Defaults to `AllowAnonymous`. This can be overidden within the `Entity`(s) and/or their corresponding `Operation`(s).")]
        public string? WebApiAuthorize { get; set; }

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
        /// Gets or sets the default Reference Data property Converter used by the generated Mapper(s) where not specifically defined.
        /// </summary>
        [JsonProperty("refDataDefaultMapperConverter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The default Reference Data property `Converter` used by the generated `Mapper`(s) where not specifically defined.", Options = new string[] { "ReferenceDataCodeConverter", "ReferenceDataInt32IdConverter", "ReferenceDataNullableInt32IdConverter", "ReferenceDataGuidIdConverter", "ReferenceDataNullableGuidIdConverter" },
            Description = "Defaults to `ReferenceDataCodeConverter`.")]
        public string? RefDataDefaultMapperConverter { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code.
        /// </summary>
        [JsonProperty("dataUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The additional Namespace using statement to the added to the generated `Data` code.")]
        public string? DataUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>Database</c>.
        /// </summary>
        [JsonProperty("databaseUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The additional Namespace using statement to the added to the generated `Data` code where `Operation.AutoImplement` is `Database`.")]
        public string? DatabaseUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>EntityFramework</c>.
        /// </summary>
        [JsonProperty("entityFrameworkUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The additional Namespace using statement to the added to the generated `Data` code where `Operation.AutoImplement` is `EntityFramework`.")]
        public string? EntityFrameworkUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "additional Namespace using statement to the added to the generated `Data` code where `Operation.AutoImplement` is `Cosmos`.")]
        public string? CosmosUsingNamespace { get; set; }

        /// <summary>
        /// Gets or sets the additional Namespace using statement to the added to the generated <c>Data</c> code where <c>Operation.AutoImplement</c> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataUsingNamespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "additional Namespace using statement to the added to the generated `Data` code where `Operation.AutoImplement` is `OData`.")]
        public string? ODataUsingNamespace { get; set; }

        #endregion

        #region DataSvc

        /// <summary>
        /// Indicates whether to add logic to publish an event on the successful completion of the <c>DataSvc</c> layer invocation for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a `Create`, `Update` or `Delete` operation.", IsImportant = true,
            Description = "Defaults to `true`. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overidden within the `Entity`(s).")]
        public bool? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the root for the event name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The root for the event name by prepending to all event subject names.",
            Description = "Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overidden within the `Entity`(s).", IsImportant = true)]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The formatting for the Action when an Event is published.", Options = new string[] { "None", "UpperCase", "PastTense", "PastTenseUpperCase" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required)`.")]
        public string? EventActionFormat { get; set; }

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
        /// Gets the specified runtime parameter value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="isRequired">Indicates whether the parameter is mandatory and therefore must exist and have non-<c>null</c> value.</param>
        /// <returns>The runtime parameter value.</returns>
        internal string? GetRuntimeParameter(string name, bool isRequired = false)
        {
            if ((!RuntimeParameters.TryGetValue(name, out var value) && isRequired) || (isRequired && string.IsNullOrEmpty(value)))
                throw new CodeGenException($"Runtime parameter '{name}' was not found or had no value; this is required to function.");
            else
                return value;
        }

        /// <summary>
        /// Gets the specified runtime parameter value as a <see cref="bool"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The runtime parameter value.</returns>
        internal bool GetRuntimeBoolParameter(string name)
        {
            var val = GetRuntimeParameter(name);
            if (string.IsNullOrEmpty(val))
                return false;

            if (bool.TryParse(val, out var value))
                return value;

            throw new CodeGenException($"Runtime parameter '{name}' must be a boolean; value '{val}' is invalid.");
        }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="EntityConfig"/> collection.
        /// </summary>
        [JsonProperty("entities", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Entity` collection.", IsImportant = true,
            Markdown = "An `Entity` object provides the primary configuration for an entity, its properties and operations.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<EntityConfig>? Entities { get; set; }

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxManager.  
        /// </summary>
        public List<EntityConfig>? IManagerEntities => Entities.Where(x => IsNoOption(x.ExcludeIManager) && x.Operations!.Count > 0).ToList();

        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataSvcEntities => Entities.Where(x => IsNoOption(x.ExcludeIDataSvc) && x.Operations!.Count > 0).ToList();
 
        /// <summary>
        /// Gets the <see cref="Entities"/> that are selected for IXxxData.  
        /// </summary>
        public List<EntityConfig>? IDataEntities => Entities.Where(x => IsNoOption(x.ExcludeIData) && x.Operations!.Count > 0).ToList();

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
        public string Company => GetRuntimeParameter("Company", true)!;

        /// <summary>
        /// Gets the application name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string AppName => GetRuntimeParameter("AppName", true)!;

        /// <summary>
        /// Gets the API name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string ApiName => DefaultWhereNull(GetRuntimeParameter("ApiName"), () => "Api")!;

        /// <summary>
        /// Gets the entity scope from the from the <see cref="RuntimeParameters"/> (defaults to 'Common').
        /// </summary>
        public string EntityScope => DefaultWhereNull(GetRuntimeParameter("EntityScope"), () => "Common")!;

        /// <summary>
        /// Indicates whether to generate an <c>Entity</c> as a <c>DataModel</c> where the <see cref="EntityConfig.DataModel"/> is selected (from the <see cref="RuntimeParameters"/>).
        /// </summary>
        public bool ModelFromEntity => GetRuntimeBoolParameter("ModelFromEntity");

        /// <summary>
        /// Indicates whether the intended Entity code generation is a Data Model and therefore should not inherit from <see cref="EntityBase"/> (from the <see cref="RuntimeParameters"/>).
        /// </summary>
        public bool IsDataModel => GetRuntimeBoolParameter("IsDataModel");

        /// <summary>
        /// Indicates whether the intended code generation is explicitly for Reference Data.
        /// </summary>
        public bool IsRefData => GetRuntimeBoolParameter("IsRefData");

        /// <summary>
        /// Gets the reference data specific properties.
        /// </summary>
        public RefDataConfig? RefData { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            RefDataCache = DefaultWhereNull(RefDataCache, () => "ReferenceDataCache");
            ValidatorLayer = DefaultWhereNull(ValidatorLayer, () => "Business");
            EventPublish = DefaultWhereNull(EventPublish, () => true);
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "None");
            EntityUsing = DefaultWhereNull(EntityUsing, () => "Common");
            DatabaseName = DefaultWhereNull(DatabaseName, () => "IDatabase");
            EntityFrameworkName = DefaultWhereNull(EntityFrameworkName, () => "IEfDb");
            CosmosName = DefaultWhereNull(CosmosName, () => "ICosmosDb");
            ODataName = DefaultWhereNull(ODataName, () => "IOData");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "Newtonsoft");
            RefDataDefaultMapperConverter = DefaultWhereNull(RefDataDefaultMapperConverter, () => "ReferenceDataCodeConverter");
            WebApiAuthorize = DefaultWhereNull(WebApiAuthorize, () => "AllowAnonymous");

            if (Entities != null && Entities.Count > 0)
            {
                foreach (var entity in Entities)
                {
                    entity.Prepare(Root!, this);
                }
            }

            RefData = new RefDataConfig();
            RefData.Prepare(Root!, this);
        }
    }
}