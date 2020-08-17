// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Entity</b> code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Entity", Title = "The **Entity** is used as the primary configuration for driving the entity-driven code generation.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("RefData", Title = "Provides the **Reference Data** configuration.")]
    [CategorySchema("Entity", Title = "Provides the **Entity class** configuration.")]
    [CategorySchema("Collection", Title = "Provides the **Entity collection class** configuration.")]
    [CategorySchema("Operation", Title = "Provides the **Operation** configuration.", Description = "These primarily provide a shorthand to create the standard `Get`, `Create`, `Update` and `Delete` operations (versus having to specify directly).")]
    [CategorySchema("Auth", Title = "Provides the **Authorization** configuration.")]
    [CategorySchema("WebApi", Title = "Provides the data **Web API** configuration.")]
    [CategorySchema("Manager", Title = "Provides the **Manager-layer** configuration.")]
    [CategorySchema("DataSvc", Title = "Provides the **Data Services-layer** configuration.")]
    [CategorySchema("Data", Title = "Provides the generic **Data-layer** configuration.")]
    [CategorySchema("Database", Title = "Provides the specific **Database (ADO.NET)** configuration where `AutoImplement` is `Database`.")]
    [CategorySchema("EntityFramework", Title = "Provides the specific **Entity Framework** configuration where `AutoImplement` is `EntityFramework`.")]
    [CategorySchema("Cosmos", Title = "Provides the specific **Cosmos** configuration where `AutoImplement` is `Cosmos`.")]
    [CategorySchema("OData", Title = "Provides the specific **OData** configuration where `AutoImplement` is `OData`.")]
    [CategorySchema("Model", Title = "Provides the data **Model** configuration.")]
    [CategorySchema("Grpc", Title = "Provides the **gRPC** configuration.")]
    [CategorySchema("Exclude", Title = "Provides the **Exclude** configuration.")]
    public class EntityConfig : ConfigBase<CodeGenConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the unique entity name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique entity name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overridding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding text for use in comments.",
            Description = "Overrides the Name (as sentence text) for the summary comments. It will be formatted as: `Represents the {Text} entity.`. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the overriding file name.
        /// </summary>
        [JsonProperty("fileName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding file name.",
            Description = "Overrides the Name as the code-generated file name.")]
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the entity scope option.
        /// </summary>
        [JsonProperty("entityScope", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The entity scope option.", Options = new string[] { "Common", "Business" },
            Description = "Determines whether the entity is considered `Common` (default) or should be scoped to the `Business` namespace/assembly only (i.e. not externally visible).")]
        public string? EntityScope { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonProperty("privateName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.")]
        public string? PrivateName { get; set; }

        /// <summary>
        /// Gets or sets the overriding argument name.
        /// </summary>
        [JsonProperty("argumentName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding argument name.",
            Description = "Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.")]
        public string? ArgumentName { get; set; }

        /// <summary>
        /// Gets or sets the Const Type option.
        /// </summary>
        [JsonProperty("constType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The Const .NET Type option.", Options = new string[] { "int", "Guid", "string" },
            Description = "The .NET Type to be used for the `const` values. Defaults to `string`.")]
        public string? ConstType { get; set; }

        /// <summary>
        /// Indicates whether to override the <see cref="ICleanUp.IsInitial"/> property.
        /// </summary>
        [JsonProperty("isInitialOverride", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether to override the `ICleanup.IsInitial` property.",
            Description = "Set to either `true` or `false` to override as specified; otherwise, `null` to check each property. Defaults to `null`.")]
        public bool? IsInitialOverride { get; set; }

        #endregion

        #region RefData

        /// <summary>
        /// Gets or sets the Reference Data identifier Type option.
        /// </summary>
        [JsonProperty("refDataType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The Reference Data identifier Type option.", IsImportant = true, Options = new string[] { "int", "Guid" },
            Description = "Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates whether a corresponding <i>text</i> property is added when generating a Reference Data property overridding the <c>CodeGeneration.RefDataText</c> selection.
        /// </summary>
        [JsonProperty("refDataText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "Indicates whether a corresponding `text` property is added when generating a Reference Data property overridding the `CodeGeneration.RefDataText` selection.",
            Description = "This is used where serializing within the `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to true (automatically performed where url contains `$text=true`).")]
        public bool? RefDataText { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data sort order option.
        /// </summary>
        [JsonProperty("refDataSortOrder", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The Reference Data sort order option.", Options = new string[] { "SortOrder", "Id", "Code", "Text" },
            Description = "Specifies the default sort order for the underlying Reference Data collection. Defaults to `SortOrder`.")]
        public string? RefDataSortOrder { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Gets or sets the base class that the entity inherits from.
        /// </summary>
        [JsonProperty("inherits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The base class that the entity inherits from.",
            Description = "Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseInt` or `ReferenceDataBaseGuid` depending on the corresponding `RefDataType` value. " +
                          "See `OmitEntityBase` if the desired outcome is to not inherit from any of the aforementioned base classes.")]
        public string? Inherits { get; set; }

        /// <summary>
        /// Gets or sets the list of comma separated interfaces that are to be declared for the entity class.
        /// </summary>
        [JsonProperty("implements", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The list of comma separated interfaces that are to be declared for the entity class.")]
        public string? Implements { get; set; }

        /// <summary>
        /// Indicates whether to automatically infer the interface implements for the entity from the properties declared.
        /// </summary>
        [JsonProperty("autoInferImplements", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "Indicates whether to automatically infer the interface implements for the entity from the properties declared.",
            Description = "Will attempt to infer the following: `IGuidIdentifier`, `IIntIdentifier`, `IStringIdentifier`, `IETag` and `IChangeLog`. Defaults to `true`.")]
        public bool? ImplementsAutoInfer { get; set; }

        /// <summary>
        /// Indicates whether the class should be defined as abstract.
        /// </summary>
        [JsonProperty("abstract", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "Indicates whether the class should be defined as abstract.")]
        public bool? Abstract { get; set; }

        /// <summary>
        /// Indicates whether the class should be defined as a generic with a single parameter <c>T</c>.
        /// </summary>
        [JsonProperty("genericWithT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "Indicates whether the class should be defined as a generic with a single parameter `T`.")]
        public bool? GenericWithT { get; set; }

        /// <summary>
        /// Gets or sets the entity namespace to be appended.
        /// </summary>
        [JsonProperty("namespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The entity namespace to be appended.",
            Description = "Appended to the end of the standard structure as follows: `{Company}.{AppName}.Common.Entities.{Namespace}`.")]
        public string? Namespace { get; set; }

        /// <summary>
        /// Indicates that the entity should not inherit from `EntityBase`.
        /// </summary>
        [JsonProperty("omitEntityBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "Indicates that the entity should not inherit from `EntityBase`.",
            Description = "As such any of the `EntityBase` related capabilites are not supported (are omitted from generation). The intention for this is more for the generation of simple internal entities.")]
        public bool? OmitEntityBase { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "None", "Newtonsoft" },
            Description = "Defaults to the `CodeGeneration.JsonSerializer` configuration property where specified; otherwise, `Newtonsoft`.")]
        public string? JsonSerializer { get; set; }

        #endregion

        #region Collection

        /// <summary>
        /// Indicates whether a corresponding entity collection class should be created.
        /// </summary>
        [JsonProperty("collection", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Collection", Title = "Indicates whether a corresponding entity collection class should be created.", IsImportant = true)]
        public bool? Collection { get; set; }

        /// <summary>
        /// Indicates whether a corresponding entity collection result class should be created
        /// </summary>
        [JsonProperty("collectionResult", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Collection", Title = "Indicates whether a corresponding entity collection result class should be created", IsImportant = true,
            Description = "Enables the likes of additional paging state to be stored with the underlying collection.")]
        public bool? CollectionResult { get; set; }

        /// <summary>
        /// Indicates whether the entity collection is keyed using the properties defined as forming part of the unique key.
        /// </summary>
        [JsonProperty("collectionKeyed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Collection", Title = "Indicates whether the entity collection is keyed using the properties defined as forming part of the unique key.")]
        public bool? CollectionKeyed { get; set; }

        /// <summary>
        /// Gets or sets the base class that a <see cref="Collection"/> inherits from.
        /// </summary>
        [JsonProperty("collectionInherits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Collection", Title = "The base class that a `Collection` inherits from.",
            Description = "Defaults to `EntityBaseCollection` or `EntityBaseKeyedCollection` depending on `CollectionKeyed`. For Reference Data it will default to `ReferenceDataCollectionBase`.")]
        public string? CollectionInherits { get; set; }

        /// <summary>
        /// Gets or sets the base class that a <see cref="CollectionResult"/> inherits from.
        /// </summary>
        [JsonProperty("collectionResultInherits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Collection", Title = "The base class that a `CollectionResult` inherits from.",
            Description = "Defaults to `EntityCollectionResult`.")]
        public string? CollectionResultInherits { get; set; }

        #endregion

        #region Operation

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "The name of the .NET `Type` that will perform the validation.", IsImportant = true,
            Description = "Only used for `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.")]
        public string? Validator { get; set; }

        /// <summary>
        /// Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("get", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Get { get; set; }

        /// <summary>
        /// Indicates that a `GetAll` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("getAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `GetAll` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? GetAll { get; set; }

        /// <summary>
        /// Indicates that a `Create` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("create", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `Create` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Create { get; set; }

        /// <summary>
        /// Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("update", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Update { get; set; }

        /// <summary>
        /// Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("patch", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `Patch` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Patch { get; set; }

        /// <summary>
        /// Indicates that a `Delete` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("delete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Operation", Title = "Indicates that a `Delete` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Delete { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the data source auto-implementation option. 
        /// </summary>
        [JsonProperty("autoImplement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The data source auto-implementation option.", IsImportant = true, Options = new string[] { "Database", "EntityFramework", "Cosmos", "OData", "None" },
            Description = "Defaults to `None`. Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicity overridden). When selected some of the related attributes will also be required (as documented). " +
                          "Additionally, the `AutoImplement` indicator must be selected for each underlying `Operation` that is to be auto-implemented.")]
        public string? AutoImplement { get; set; }

        /// <summary>
        /// Indicates that the `AddStandardProperties` method call is to be included for the generated (corresponding) `Mapper`.
        /// </summary>
        [JsonProperty("mapperAddStandardProperties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates that the `AddStandardProperties` method call is to be included for the generated (corresponding) `Mapper`.",
            Description = "Defaults to `true`.")]
        public bool? MapperAddStandardProperties { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated `Data` constructor.
        /// </summary>
        [JsonProperty("dataConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The access modifier for the generated `Data` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataConstructor { get; set; }

        /// <summary>
        /// Indicates whether the `Data` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the `Data` extensions logic should be generated.")]
        public bool? DataExtensions { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the .NET database interface name used where `AutoImplement` is `Database`.
        /// </summary>
        [JsonProperty("databaseName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The .NET database interface name (used where `AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.DatabaseName` configuration property (its default value is `IDatabase`).")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database schema name (used where `AutoImplement` is `Database`).
        /// </summary>
        [JsonProperty("databaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The database schema name (used where `AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to `dbo`.")]
        public string? DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated Database <c>Mapper</c> inherits from.
        /// </summary>
        [JsonProperty("databaseMapperInheritsFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The name of the `Mapper` that the generated Database `Mapper` inherits from.")]
        public string? DatabaseMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom Database <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonProperty("databaseCustomerMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "Indicates that a custom Database `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? DatabaseCustomMapper { get; set; }

        #endregion

        #region EntityFramework

        /// <summary>
        /// Gets or sets the .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`.
        /// </summary>
        [JsonProperty("entityFrameworkName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.EntityFrameworkName` configuration property (its default value is `IEfDb`).")]
        public string? EntityFrameworkName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding Entity Framework entity model name required where <see cref="AutoImplement"/> is <c>EntityFramework</c>.
        /// </summary>
        [JsonProperty("entityFrameworkEntity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The corresponding Entity Framework entity model name (required where `AutoImplement` is `EntityFramework`).", IsImportant = true)]
        public string? EntityFrameworkEntity { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated Entity Framework <c>Mapper</c> inherits from.
        /// </summary>
        [JsonProperty("dataEntityFrameworkMapperInheritsFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The name of the `Mapper  that the generated Entity Framework `Mapper` inherits from.",
            Description = "Defaults to `Model.{Name}`; i.e. an entity with the same name in the `Model` namespace.")]
        public string? DataEntityFrameworkMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.
        /// </summary>
        [JsonProperty("dataEntityFrameworkCustomMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? DataEntityFrameworkCustomMapper { get; set; }

        #endregion

        #region Cosmos

        /// <summary>
        /// Gets or sets the .NET Cosmos interface name used where `AutoImplement` is `Cosmos`.
        /// </summary>
        [JsonProperty("cosmosName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The .NET Cosmos interface name used where `AutoImplement` is `Cosmos`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.CosmosName` configuration property (its default value is `ICosmosDb`).")]
        public string? CosmosName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding Cosmos entity model name required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosEntity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The corresponding Cosmos entity model name (required where `AutoImplement` is `Cosmos`).", IsImportant = true)]
        public string? CosmosEntity { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos <c>ContainerId</c> required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosContainerId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The Cosmos `ContainerId` required where `AutoImplement` is `Cosmos`.", IsImportant = true)]
        public string? CosmosContainerId { get; set; }

        /// <summary>
        /// Gets or sets the C# code to be used for setting the optional Cosmos <c>PartitionKey</c> where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosPartitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The C# code to be used for setting the optional Cosmos `PartitionKey` where `AutoImplement` is `Cosmos`.",
            Description = "Defaults to `PartitionKey.None`.")]
        public string? CosmosPartitionKey { get; set; }

        /// <summary>
        /// Indicates whether the <c>CosmosDbValueContainer</c> is to be used; otherwise, <c>CosmosDbContainer</c>.
        /// </summary>
        [JsonProperty("cosmosValueContainer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "Indicates whether the `CosmosDbValueContainer` is to be used; otherwise, `CosmosDbContainer`.")]
        public bool? CosmosValueContainer { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated Cosmos <c>Mapper</c> inherits from.
        /// </summary>
        [JsonProperty("cosmosMapperInheritsFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The name of the `Mapper` that the generated Cosmos `Mapper` inherits from.")]
        public string? CosmosMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom Cosmos <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonProperty("cosmosCustomMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "Indicates that a custom Cosmos `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? CosmosCustomMapper { get; set; }

        #endregion

        #region OData

        /// <summary>
        /// Gets or sets the .NET OData interface name used where `AutoImplement` is `OData`.
        /// </summary>
        [JsonProperty("odataName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The .NET OData interface name used where `AutoImplement` is `OData`.", 
            Description = "Defaults to the `CodeGeneration.ODataName` configuration property (its default value is `IOData`).")]
        public string? ODataName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding OData entity model name required where <see cref="AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataEntity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The corresponding OData entity model name (required where `AutoImplement` is `OData`).", IsImportant = true)]
        public string? ODataEntity { get; set; }

        /// <summary>
        /// Gets or sets the name of the underlying OData collection name where <see cref="AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataCollectionName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The name of the underlying OData collection where `AutoImplement` is `OData`.", IsImportant = true,
            Description = "The underlying `Simple.OData.Client` will attempt to infer.")]
        public string? ODataCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated OData <c>Mapper</c> inherits from.
        /// </summary>
        [JsonProperty("odataMapperInheritsFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The name of the `Mapper` that the generated OData `Mapper` inherits from.")]
        public string? ODataMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom OData <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonProperty("odataCustomMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "Indicates that a custom OData `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? ODataCustomMapper { get; set; }

        #endregion

        #region DataSvc

        /// <summary>
        /// Indicates whether request-based <see cref="IRequestCache"/> caching is to be performed at the <c>DataSvc</c> layer to improve performance (i.e. reduce chattiness).
        /// </summary>
        [JsonProperty("dataSvcCaching", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether request-based `IRequestCache` caching is to be performed at the `DataSvc` layer to improve performance (i.e. reduce chattiness).",
            Description = "Defaults to `true`.")]
        public bool? DataSvcCaching { get; set; }

        /// <summary>
        /// Indicates whether to add logic to publish an event on the successful completion of the <c>DataSvc</c> layer invocation for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a `Create`, `Update` or `Delete` operation.",
            Description = "Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.")]
        public bool? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated `DataSvc` constructor.
        /// </summary>
        [JsonProperty("dataSvcConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The access modifier for the generated `DataSvc` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataSvcConstructor { get; set; }

        /// <summary>
        /// Indicates whether the `DataSvc` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataSvcExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether the `DataSvc` extensions logic should be generated.")]
        public bool? DataSvcExtensions { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the access modifier for the generated `Manager` constructor.
        /// </summary>
        [JsonProperty("managerConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The access modifier for the generated `Manager` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? ManagerConstructor { get; set; }

        /// <summary>
        /// Indicates whether the `Manager` extensions logic should be generated.
        /// </summary>
        [JsonProperty("managerExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "Indicates whether the `Manager` extensions logic should be generated.")]
        public bool? ManagerExtensions { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the <c>RoutePrefixAtttribute</c> for the corresponding entity Web API controller.
        /// </summary>
        [JsonProperty("webApiRoutePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The `RoutePrefixAtttribute` for the corresponding entity Web API controller.", IsImportant = true,
            Description = "This is the base (prefix) `URI` for the entity and can be further extended when defining the underlying `Operation`(s).")]
        public string? WebApiRoutePrefix { get; set; }

        /// <summary>
        /// Indicates whether the Web API controller should use <c>Authorize</c> (<c>true</c>); otherwise, <c>AllowAnonynous</c> (<c>false</c>).
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "Indicates whether the Web API controller should use `Authorize` (`true`); otherwise, `AllowAnonynous` (`false`).",
            Description = "Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also.")]
        public bool? WebApiAuthorize { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated Web API `Controller` constructor.
        /// </summary>
        [JsonProperty("webApiConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The access modifier for the generated Web API `Controller` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? WebApiConstructor { get; set; }

        #endregion

        #region Model

        /// <summary>
        /// Indicates whether a data <i>model</i> version of the entity should also be generated (output to <c>.\Business\Data\Model</c>).
        /// </summary>
        [JsonProperty("dataModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Model", Title = "Indicates whether a data `model` version of the entity should also be generated (output to `.\\Business\\Data\\Model`).",
            Description = "The model will be generated with `OmitEntityBase = true`. Any reference data properties will be defined using their `RefDataType` intrinsic `Type` versus their corresponding (actual) reference data `Type`.")]
        public bool? DataModel { get; set; }

        #endregion

        #region Exclude

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>Entity</c> class (<c>Xxx.cs</c>).
        /// </summary>
        [JsonProperty("excludeEntity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `Entity` class (`Xxx.cs`).", IsImportant = true)]
        public bool? ExcludeEntity { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of <b>all</b> <c>Operation</c> related artefacts; excluding the <c>Entity</c> class.
        /// </summary>
        [JsonProperty("excludeAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of all< `Operation` related artefacts; excluding the `Entity` class.", IsImportant = true,
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `true`.")]
        public bool? ExcludeAll { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeIData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `Data` interface (`IXxxData.cs`).")]
        public bool? ExcludeIData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `Data` class (`XxxData.cs`).")]
        public bool? ExcludeData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeIDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `DataSvc` interface (`IXxxDataSvc.cs`).")]
        public bool? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `DataSvc` class (`XxxDataSvc.cs`).")]
        public bool? ExcludeDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeIManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `Manager` interface (`IXxxManager.cs`).")]
        public bool? ExcludeIManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the `Manager` class (`XxxManager.cs`).")]
        public bool? ExcludeManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the WebAPI `Controller` class (`XxxController.cs`).")]
        public bool? ExcludeWebApi { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApiAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the WebAPI consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// Indicates whether to exclude the creation of the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeGrpcAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the creation of the gRPC consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeGrpcAgent { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the role (permission) used by the <c>ExecutionContext.IsInRole(role)</c> for each <c>Operation</c>.
        /// </summary>
        [JsonProperty("authRole", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The role (permission) used by the `ExecutionContext.IsInRole(role)` for each `Operation`.", IsImportant = true,
            Description = "Used where not overridden specifically for an `Operation`; i.e. acts as the default.")]
        public string? AuthRole { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required for the Entity.
        /// </summary>
        [JsonProperty("grpc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Grpc", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Entity.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="PropertyConfig"/> collection.
        /// </summary>
        [JsonProperty("properties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema(Title = "The corresponding `Property` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<PropertyConfig>? Properties { get; set; }


        /// <summary>
        /// Gets the list of properties that are not inherited.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public PropertyConfig[] PropertiesNotInherited => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && (x.RefDataMapping == null || !x.RefDataMapping.Value)).ToArray();

        /// <summary>
        /// Gets or sets the corresponding <see cref="OperationConfig"/> collection.
        /// </summary>
        [JsonProperty("operations", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema(Title = "The corresponding `Operation` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<OperationConfig>? Operations { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="ConstConfig"/> collection.
        /// </summary>
        [JsonProperty("consts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema(Title = "The corresponding `Consts` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<ConstConfig>? Consts { get; set; }

        /// <summary>
        /// Gets or sets the formatted summary text.
        /// </summary>
        public string? SummaryText { get; set; }

        /// <summary>
        /// Gets or sets the entity name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// Gets or sets the entity collection name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the entity collection result name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityCollectionResultName { get; set; }

        /// <summary>
        /// Indicates whether the entity (based on all configurations) should implement the <see cref="EntityBase"/> capabilties.
        /// </summary>
        public bool HasEntityBase => !(CompareValue(OmitEntityBase, true) || Parent!.IsDataModel);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            EntityName = !GenericWithT.HasValue || !GenericWithT.Value ? Name : $"{Name}<T>";
            EntityCollectionName = !GenericWithT.HasValue || !GenericWithT.Value ? Name : $"{Name}Collection<T>";
            EntityCollectionResultName = !GenericWithT.HasValue || !GenericWithT.Value ? Name : $"{Name}CollectionResult<T>";

            Text = DefaultWhereNull(Text, () => CodeGenerator.ToSentenceCase(Name));
            SummaryText = CodeGenerator.ToComments($"Represents the {Text} entity.");
            FileName = DefaultWhereNull(FileName, () => Name);
            EntityScope = DefaultWhereNull(EntityScope, () => "Common");
            PrivateName = DefaultWhereNull(PrivateName, () => CodeGenerator.ToPrivateCase(Name));
            ArgumentName = DefaultWhereNull(ArgumentName, () => CodeGenerator.ToCamelCase(Name));
            ConstType = DefaultWhereNull(ConstType, () => "string");
            RefDataText = DefaultWhereNull(RefDataText, () => Parent!.RefDataText);
            RefDataSortOrder = DefaultWhereNull(RefDataSortOrder, () => "SortOrder");
            ImplementsAutoInfer = DefaultWhereNull(ImplementsAutoInfer, () => true);
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => Parent!.JsonSerializer);
            MapperAddStandardProperties = DefaultWhereNull(MapperAddStandardProperties, () => true);
            AutoImplement = DefaultWhereNull(AutoImplement, () => "None");
            DataConstructor = DefaultWhereNull(DataConstructor, () => "Public");
            DatabaseName = DefaultWhereNull(DatabaseName, () => Parent!.DatabaseName);
            DatabaseSchema = DefaultWhereNull(DatabaseSchema, () => "dbo");
            EntityFrameworkName = DefaultWhereNull(EntityFrameworkName, () => Parent!.EntityFrameworkName);
            EntityFrameworkEntity = DefaultWhereNull(EntityFrameworkEntity, () => $"Model.{Name}");
            CosmosName = DefaultWhereNull(CosmosName, () => Parent!.CosmosName);
            CosmosEntity = DefaultWhereNull(CosmosEntity, () => $"Model.{Name}");
            CosmosPartitionKey = DefaultWhereNull(CosmosPartitionKey, () => "ParitionKey.None");
            ODataName = DefaultWhereNull(ODataName, () => Parent!.ODataName);
            ODataEntity = DefaultWhereNull(ODataEntity, () => $"Model.{Name}");
            DataSvcCaching = DefaultWhereNull(DataSvcCaching, () => true);
            DataSvcConstructor = DefaultWhereNull(DataSvcConstructor, () => "Public");
            EventPublish = DefaultWhereNull(EventPublish, () => Parent!.EventPublish);
            ManagerConstructor = DefaultWhereNull(ManagerConstructor, () => "Public");
            WebApiAuthorize = DefaultWhereNull(WebApiAuthorize, () => Parent!.WebApiAuthorize);
            WebApiConstructor = DefaultWhereNull(WebApiConstructor, () => "Public");
            ExcludeEntity = DefaultWhereNull(ExcludeEntity, () => false);
            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, true));
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, true));
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, true));
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, true));
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, true));
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, true));
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, true));

            InferInherits();
            PrepareConsts();
            PrepareProperties();
            PrepareOperations();
            InferImplements();
        }

        /// <summary>
        /// Infers the <see cref="Inherits"/>, <see cref="CollectionInherits"/> and <see cref="CollectionResultInherits"/> values.
        /// </summary>
        private void InferInherits()
        {
            Inherits = DefaultWhereNull(Inherits, () =>
            {
                if (!CompareNullOrValue(OmitEntityBase, false))
                    return null;

                return RefDataType switch
                {
                    "int" => "ReferenceDataBaseInt",
                    "Guid" => "ReferenceDataBaseGuid",
                    _ => "EntityBase"
                };
            });

            CollectionInherits = DefaultWhereNull(CollectionInherits, () =>
            {
                if (!CompareNullOrValue(OmitEntityBase, false))
                    return $"List<{EntityName}>";

                if (RefDataType == null)
                    return CompareValue(CollectionKeyed, true) ? $"EntityBaseKeyedCollection<UniqueKey, {EntityName}>" : $"EntityBaseCollection<{EntityName}>";
                else
                    return $"ReferenceDataCollectionBase<{EntityName}>";
            });

            CollectionResultInherits = DefaultWhereNull(CollectionResultInherits, () => $"EntityCollectionResult<{EntityCollectionName}, {EntityName}>");
        }

        /// <summary>
        /// Prepares the constants.
        /// </summary>
        private void PrepareConsts()
        {
            if (Consts == null)
                Consts = new List<ConstConfig>();

            foreach (var constant in Consts)
            {
                constant.Prepare(this);
            }
        }

        /// <summary>
        /// Prepares the properties.
        /// </summary>
        private void PrepareProperties()
        {
            if (Properties == null)
                Properties = new List<PropertyConfig>();

            if (RefDataType != null)
            {
                var i = 0;
                AddInheritedProperty("Id", ref i, () => new PropertyConfig { Text = "{{" + Name + "}} identifier", Type = RefDataType, UniqueKey = true, DataAutoGenerated = true, DataName = $"{Name}Id" });
                AddInheritedProperty("Code", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("Text", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("IsActive", ref i, () => new PropertyConfig { Type = "bool" });
                AddInheritedProperty("SortOrder", ref i, () => new PropertyConfig { Type = "int" });
                AddInheritedProperty("ETag", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("ChangeLog", ref i, () => new PropertyConfig { Type = "ChangeLog" });
            }

            foreach (var property in Properties)
            {
                property.Prepare(this);
            }
        }

        /// <summary>
        /// Adds the named (inherited) property if it does not already exist.
        /// </summary>
        private void AddInheritedProperty(string name, ref int index, Func<PropertyConfig> func)
        {
            if (Properties.Any(x => x.Name == name))
                return;

            var p = func();
            p.Name = name;
            p.Inherited = true;
            Properties!.Insert(index++, p);
        }

        /// <summary>
        /// Prepares the Operations.
        /// </summary>
        private void PrepareOperations()
        {
            if (Operations == null)
                Operations = new List<OperationConfig>();

            // Add in selected operations where applicable (in reverse order in which output).
            if (CompareValue(Delete, true) && !Operations.Any(x => x.Name == "Delete"))
                Operations.Insert(0, new OperationConfig { Name = "Delete", Type = "Delete", UniqueKey = true });

            if (CompareValue(Patch, true) && !Operations.Any(x => x.Name == "Patch"))
                Operations.Insert(0, new OperationConfig { Name = "Patch", Type = "Patch", UniqueKey = true });

            if (CompareValue(Update, true) && !Operations.Any(x => x.Name == "Update"))
                Operations.Insert(0, new OperationConfig { Name = "Update", Type = "Update", UniqueKey = true });

            if (CompareValue(Create, true) && !Operations.Any(x => x.Name == "Create"))
                Operations.Insert(0, new OperationConfig { Name = "Create", Type = "Create", UniqueKey = false, WebApiRoute = "" });

            if (CompareValue(Get, true) && !Operations.Any(x => x.Name == "Get"))
                Operations.Insert(0, new OperationConfig { Name = "Get", Type = "Get", UniqueKey = true });

            if (CompareValue(GetAll, true) && !Operations.Any(x => x.Name == "GetAll"))
                Operations.Insert(0, new OperationConfig { Name = "GetAll", Type = "GetColl", UniqueKey = false, WebApiRoute = "" });

            // Prepare each operations.
            foreach (var operation in Operations)
            {
                operation.Prepare(this);
            }
        }

        /// <summary>
        /// Infer the implements from the properties configured.
        /// </summary>
        private void InferImplements()
        {
            if (!ImplementsAutoInfer.HasValue || !ImplementsAutoInfer.Value)
                return;

            var implements = new List<string>();
            if (Implements != null)
            {
                foreach (var str in Implements!.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    var txt = str?.Trim();
                    if (string.IsNullOrEmpty(txt))
                        implements.Add(txt!);
                }
            }

            var i = 0;
            var id = Properties.FirstOrDefault(x => x.Name == "Id" && (!x.Inherited.HasValue || !x.Inherited.Value) && x.UniqueKey.HasValue && x.UniqueKey.Value);
            if (id != null)
            {
                var iid = id.Type switch
                {
                    "Guid" => "IGuidIdentifier",
                    "int" => "IIntIdentifier",
                    "string" => "IStringIdentifier",
                    _ => "IIdentifier",
                };

                if (!implements.Contains(iid))
                    implements.Insert(i++, iid);
            }

            if (Properties.Any(x => x.Name == "ETag" && x.Type == "string") && !implements.Contains("IETag"))
                implements.Insert(i++, "IETag");

            if (Properties.Any(x => x.Name == "ChangeLog" && x.Type == "ChangeLog") && !implements.Contains("IChangeLog"))
                implements.Insert(i++, "IChangeLog");

            Implements = implements.Count == 0 ? null : string.Join(", ", implements.ToArray());
        }
    }
}