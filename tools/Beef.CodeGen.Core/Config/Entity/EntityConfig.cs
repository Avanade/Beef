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
    [ClassSchema("Entity", Title = "'Entity' object (entity-driven)",
        Description = "The `Entity` is used as the primary configuration for driving the entity-driven code generation.",
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) for a _standard_ entity is as follows:
``` yaml
entities:
- { name: Employee, inherits: EmployeeBase, validator: EmployeeValidator, webApiRoutePrefix: api/v1/employees, autoImplement: Database, databaseSchema: Hr, databaseMapperInheritsFrom: EmployeeBaseData.DbMapper, entityFrameworkModel: EfModel.Employee, entityFrameworkCustomMapper: true,
```

<br/>

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/refdata.beef.yaml) for a _reference data_ entity is as follows:
``` yaml
entities:
- { name: Gender, refDataType: Guid, collection: true, webApiRoutePrefix: api/v1/demo/ref/genders, autoImplement: Database, databaseSchema: Ref }
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CategorySchema("Entity", Title = "Provides the _Entity class_ configuration.")]
    [CategorySchema("Collection", Title = "Provides the _Entity collection class_ configuration.")]
    [CategorySchema("Operation", Title = "Provides the _Operation_ configuration.", Description = "These primarily provide a shorthand to create the standard `Get`, `Create`, `Update` and `Delete` operations (versus having to specify directly).")]
    [CategorySchema("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CategorySchema("Events", Title = "Provides the _Events_ configuration.")]
    [CategorySchema("WebApi", Title = "Provides the data _Web API_ configuration.")]
    [CategorySchema("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CategorySchema("DataSvc", Title = "Provides the _Data Services-layer_ configuration.")]
    [CategorySchema("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CategorySchema("Database", Title = "Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.")]
    [CategorySchema("EntityFramework", Title = "Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.")]
    [CategorySchema("Cosmos", Title = "Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.")]
    [CategorySchema("OData", Title = "Provides the specific _OData_ configuration where `AutoImplement` is `OData`.")]
    [CategorySchema("HttpAgent", Title = "Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.")]
    [CategorySchema("Model", Title = "Provides the data _Model_ configuration.")]
    [CategorySchema("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CategorySchema("Exclude", Title = "Provides the _Exclude_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class EntityConfig : ConfigBase<CodeGenConfig, CodeGenConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Entity", Name);

        #region Key

        /// <summary>
        /// Gets or sets the unique entity name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique entity name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
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
        [PropertySchema("Key", Title = "The entity scope option.", Options = new string[] { "Common", "Business", "Autonomous" },
            Description = "Defaults to the `CodeGeneration.EntityScope`. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible). Additionally, there is a special case of `Autonomous` " +
            "where both a `Common` and `Business` entity are generated (where only the latter inherits from `EntityBase`, etc).")]
        public string? EntityScope { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).
        /// </summary>
        [JsonProperty("entityUsing", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The namespace for the non Reference Data entities (adds as a c# <c>using</c> statement).", Options = new string[] { "Common", "Business", "All", "None" },
            Description = "Defaults to `EntityScope` (`Autonomous` will result in `Business`). A value of `Common` will add `.Common.Entities`, `Business` will add `.Business.Entities`, `All` to add both, and `None` to exclude any.")]
        public string? EntityUsing { get; set; }

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
        [PropertySchema("Key", Title = "The Const .NET Type option.", Options = new string[] { "int", "long", "Guid", "string" },
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
        [PropertySchema("RefData", Title = "The Reference Data identifier Type option.", IsImportant = true, Options = new string[] { "int", "long", "Guid", "string" },
            Description = "Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates whether a corresponding <i>text</i> property is added when generating a Reference Data property overriding the <c>CodeGeneration.RefDataText</c> selection.
        /// </summary>
        [JsonProperty("refDataText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` overriding the `CodeGeneration.RefDataText` selection.",
            Description = "This is used where serializing within the Web API`Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`).")]
        public bool? RefDataText { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data sort order option.
        /// </summary>
        [JsonProperty("refDataSortOrder", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The Reference Data sort order option.", Options = new string[] { "SortOrder", "Id", "Code", "Text" },
            Description = "Specifies the default sort order for the underlying Reference Data collection. Defaults to `SortOrder`.")]
        public string? RefDataSortOrder { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data <c>ToString</c> composite <see cref="Beef.RefData.ReferenceDataBase.StringFormat"/>.
        /// </summary>
        [JsonProperty("refDataStringFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The Reference Data `ToString` composite format.",
            Description = "The string format supports the standard composite formatting; where the following indexes are used: `{0}` for `Id`, `{1}` for `Code` and `{2}` for `Text`. Defaults to `{2}`.")]
        public string? RefDataStringFormat { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Gets or sets the base class that the entity inherits from.
        /// </summary>
        [JsonProperty("inherits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "The base class that the entity inherits from.",
            Description = "Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseXxx` depending on the corresponding `RefDataType` value. " +
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
        [JsonProperty("implementsAutoInfer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Entity", Title = "Indicates whether to automatically infer the interface implements for the entity from the properties declared.",
            Description = "Will attempt to infer the following: `IGuidIdentifier`, `IInt32Identifier`, `IInt64Identifier`, `IStringIdentifier`, `IETag` and `IChangeLog`. Defaults to `true`.")]
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
        [PropertySchema("Data", Title = "The data source auto-implementation option.", IsImportant = true, Options = new string[] { "Database", "EntityFramework", "Cosmos", "OData", "HttpAgent", "None" },
            Description = "Defaults to `None`. Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicity overridden). When selected some of the related attributes will also be required (as documented). " +
                          "Additionally, the `AutoImplement` indicator must be selected for each underlying `Operation` that is to be auto-implemented.")]
        public string? AutoImplement { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated `Data` constructor.
        /// </summary>
        [JsonProperty("dataCtor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The access modifier for the generated `Data` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.
        /// </summary>
        [JsonProperty("dataCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Data", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? DataCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `Data` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the `Data` extensions logic should be generated.",
            Description = "This can be overridden using `Operation.DataExtensions`.")]
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
        /// Gets or sets the corresponding Entity Framework model name required where <see cref="AutoImplement"/> is <c>EntityFramework</c>.
        /// </summary>
        [JsonProperty("entityFrameworkModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).", IsImportant = true)]
        public string? EntityFrameworkModel { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated Entity Framework <c>Mapper</c> inherits from.
        /// </summary>
        [JsonProperty("entityFrameworkMapperInheritsFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "The name of the `Mapper  that the generated Entity Framework `Mapper` inherits from.",
            Description = "Defaults to `Model.{Name}`; i.e. an entity with the same name in the `Model` namespace.")]
        public string? EntityFrameworkMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.
        /// </summary>
        [JsonProperty("entityFrameworkCustomMapper", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("EntityFramework", Title = "Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? EntityFrameworkCustomMapper { get; set; }

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
        /// Gets or sets the corresponding Cosmos model name required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonProperty("cosmosModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Cosmos", Title = "The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).", IsImportant = true)]
        public string? CosmosModel { get; set; }

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
        [PropertySchema("OData", Title = "The .NET OData interface name used where `AutoImplement` is `OData`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.ODataName` configuration property (its default value is `IOData`).")]
        public string? ODataName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding OData model name required where <see cref="AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonProperty("odataModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("OData", Title = "The corresponding OData model name (required where `AutoImplement` is `OData`).", IsImportant = true)]
        public string? ODataModel { get; set; }

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

        #region HttpAgent

        /// <summary>
        /// Gets or sets the default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("HttpAgent", Title = "The .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.", IsImportant = true,
            Description = "Defaults to `CodeGeneration.HttpAgentName` configuration property (its default value is `IHttpAgent`).")]
        public string? HttpAgentName { get; set; }

        /// <summary>
        /// Gets or sets the HttpAgent API route prefix where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentRoutePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("HttpAgent", Title = "The base HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`.",
            Description = "This is the base (prefix) `URI` for the HTTP Agent endpoint and can be further extended when defining the underlying `Operation`(s).")]
        public string? HttpAgentRoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).", IsImportant = true,
            Description = "This can be overridden within the `Operation`(s).")]
        public string? HttpAgentModel { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonProperty("httpAgentReturnModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).",
            Description = "This can be overridden within the `Operation`(s).")]
        public string? HttpAgentReturnModel { get; set; }

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
        /// Gets or sets the access modifier for the generated `DataSvc` constructor.
        /// </summary>
        [JsonProperty("dataSvcCtor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The access modifier for the generated `DataSvc` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataSvcCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.
        /// </summary>
        [JsonProperty("dataSvcCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("DataSvc", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? DataSvcCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `DataSvc` extensions logic should be generated.
        /// </summary>
        [JsonProperty("dataSvcExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "Indicates whether the `DataSvc` extensions logic should be generated.",
            Description = "This can be overridden using `Operation.DataSvcExtensions`.")]
        public bool? DataSvcExtensions { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonProperty("eventPublish", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = new string[] { "None", "DataSvc", "Data" },
            Description = "Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc. This can be overridden within the `Operation`(s).")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the data-tier event outbox persistence technology (where the events will be transactionally persisted in an outbox as part of the data-tier processing).
        /// </summary>
        [JsonProperty("eventOutbox", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The the data-tier event outbox persistence technology (where the events will be transactionally persisted in an outbox as part of the data-tier processing).", IsImportant = true, Options = new string[] { "None", "Database" },
            Description = "Defaults to `CodeGeneration.EventOutbox` configuration property (inherits) where not specified. A value of `Database` will result in the `DatabaseEventOutboxInvoker` being used to orchestrate.")]
        public string? EventOutbox { get; set; }

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonProperty("eventSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The Event Source.",
            Description = "Defaults to `Name` (as lowercase) appended with the `/{$key}` placeholder. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. " +
            "To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. This can be overridden for the `Operation`.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "The default formatting for the Subject when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey" },
            Description = "Defaults to `CodeGeneration.EventSubjectFormat`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.
        /// </summary>
        [JsonProperty("eventTransaction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Events", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated at the `DataSvc`-layer whereever generating event publishing logic.", IsImportant = true,
            Description = "Usage will force a rollback of any underlying data transaction (where the provider supports TransactionScope) on failure, such as an `EventPublish` error. " +
                "This is by no means implying a Distributed Transaction (DTC) should be invoked; this is only intended for a single data source that supports a TransactionScope to guarantee reliable event publishing. " +
                "Defaults to `CodeGeneration.EventTransaction`. This essentially defaults the `Operation.DataSvcTransaction` where not otherwise specified. This should only be used where `EventPublish` is `DataSvc` and a transactionally-aware data source is being used.")]
        public bool? EventTransaction { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the access modifier for the generated `Manager` constructor.
        /// </summary>
        [JsonProperty("managerCtor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The access modifier for the generated `Manager` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? ManagerCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.
        /// </summary>
        [JsonProperty("managerCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Manager", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.", IsImportant = true,
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? ManagerCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `Manager` extensions logic should be generated.
        /// </summary>
        [JsonProperty("managerExtensions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "Indicates whether the `Manager` extensions logic should be generated.", 
            Description = "This can be overridden using `Operation.ManagerExtensions`.")]
        public bool? ManagerExtensions { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET `Type` that will perform the validation.", IsImportant = true,
            Description = "Only used for defaulting the `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.")]
        public string? Validator { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Interface that the `Validator` implements/inherits.
        /// </summary>
        [JsonProperty("iValidator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET Interface that the `Validator` implements/inherits.",
            Description = "Only used for defaulting the `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.")]
        public string? IValidator { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the <c>RoutePrefixAtttribute</c> for the corresponding entity Web API controller.
        /// </summary>
        [JsonProperty("webApiRoutePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The `RoutePrefixAtttribute` for the corresponding entity Web API controller.", IsImportant = true,
            Description = "This is the base (prefix) `URI` for the entity and can be further extended when defining the underlying `Operation`(s). The `CodeGeneration.WebApiRoutePrefix` will be prepended where specified.")]
        public string? WebApiRoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonProperty("webApiAuthorize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also.")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated Web API `Controller` constructor.
        /// </summary>
        [JsonProperty("webApiCtor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "The access modifier for the generated Web API `Controller` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? WebApiCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.
        /// </summary>
        [JsonProperty("webApiCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("WebApi", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.", IsImportant = true,
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? WebApiCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`)` is automatically inferred.
        /// </summary>
        [JsonProperty("webApiAutoLocation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("WebApi", Title = "Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.",
            Description = "This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This is defaulted from the `CodeGen.WebApiAutoLocation`.")]
        public bool? WebApiAutoLocation { get; set; }

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
        /// Indicates whether to exclude the generation of the <c>Entity</c> class (<c>Xxx.cs</c>).
        /// </summary>
        [JsonProperty("excludeEntity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `Entity` class (`Xxx.cs`).", IsImportant = true)]
        public bool? ExcludeEntity { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of <b>all</b> <c>Operation</c> related code; excluding the <c>Entity</c> class.
        /// </summary>
        [JsonProperty("excludeAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class.", IsImportant = true,
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to exclude.")]
        public bool? ExcludeAll { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeIData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `Data` interface (`IXxxData.cs`).")]
        public bool? ExcludeIData { get; set; }

        /// <summary>
        /// Gets or sets the option to exclude the generation of the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonProperty("excludeData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the `Data` class (`XxxData.cs`).", Options = new string[] { "Include", "Exclude", "RequiresMapper" },
            Description = "Defaults to `Include` indicating _not_ to exlude. A value of `Exclude` indicates to exclude all output; alternatively, `RequiresMapper` indicates to at least output the corresponding `Mapper` class.")]
        public string? ExcludeData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeIDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `DataSvc` interface (`IXxxDataSvc.cs`).")]
        public bool? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonProperty("excludeDataSvc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `DataSvc` class (`XxxDataSvc.cs`).")]
        public bool? ExcludeDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeIManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `Manager` interface (`IXxxManager.cs`).")]
        public bool? ExcludeIManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonProperty("excludeManager", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`).")]
        public bool? ExcludeManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "The option to exclude the generation of the WebAPI `Controller` class (`XxxController.cs`).")]
        public bool? ExcludeWebApi { get; set; }

        /// <summary>
        /// The option to exclude the generation of the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeWebApiAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the WebAPI consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonProperty("excludeGrpcAgent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Exclude", Title = "Indicates whether to exclude the generation of the gRPC consuming `Agent` class (`XxxAgent.cs`).")]
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
        [PropertySchema("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Entity.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="PropertyConfig"/> collection.
        /// </summary>
        [JsonProperty("properties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Property` collection.")]
        public List<PropertyConfig>? Properties { get; set; }

        #endregion

        /// <summary>
        /// Gets the list of private properties to be implemented (that are not inherited).
        /// </summary>
        public List<PropertyConfig>? PrivateProperties => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && (x.RefDataMapping == null || !x.RefDataMapping.Value)).ToList();

        /// <summary>
        /// Gets the list of core properties to be implemented (that are not inherited).
        /// </summary>
        public List<PropertyConfig>? CoreProperties => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && !(x.InternalOnly == true && Root!.RuntimeEntityScope == "Common" && Root.IsDataModel == false)).ToList();

        /// <summary>
        /// Gets the list of properties that form the unique key.
        /// </summary>
        public List<PropertyConfig>? UniqueKeyProperties => Properties!.Where(x => (x.UniqueKey.HasValue && x.UniqueKey.Value) && (x.Inherited == null || !x.Inherited.Value)).ToList();

        /// <summary>
        /// Gets the list of properties that form the partition key.
        /// </summary>
        public List<PropertyConfig>? PartitionKeyProperties => Properties!.Where(x => (x.PartitionKey.HasValue && x.PartitionKey.Value) && (x.Inherited == null || !x.Inherited.Value)).ToList();

        /// <summary>
        /// Gets the list of properties that are sub-entities.
        /// </summary>
        public List<PropertyConfig>? EntityProperties => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && x.IsEntity.HasValue && x.IsEntity.Value).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for database mapping.
        /// </summary>
        public List<PropertyConfig>? DatabaseMapperProperties => Properties!.Where(x => CompareNullOrValue(x.DatabaseIgnore, false) && x.Name != "ETag" && x.Name != "ChangeLog").ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? EntityFrameworkMapperProperties => Properties!.Where(x => CompareValue(x.EntityFrameworkMapper, "Map") && x.Name != "ETag" && x.Name != "ChangeLog").ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? EntityFrameworkAutoMapperProperties => Properties!.Where(x => !CompareValue(x.EntityFrameworkMapper, "Skip") && x.Name != "ETag" && x.Name != "ChangeLog").ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? CosmosMapperProperties => Properties!.Where(x => CompareValue(x.CosmosMapper, "Map") && x.Name != "ETag" && x.Name != "ChangeLog").ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? CosmosAutoMapperProperties => Properties!.Where(x => !CompareValue(x.CosmosMapper, "Skip")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? ODataMapperProperties => Properties!.Where(x => !CompareValue(x.ODataMapper, "Skip")).ToList();

        /// <summary>
        /// Indicates where there is a <see cref="IChangeLog"/> property.
        /// </summary>
        public bool HasDatabaseChangeLogProperty => Properties!.Any(x => x.Name == "ChangeLog" && CompareNullOrValue(x.DatabaseIgnore, false));

        /// <summary>
        /// Indicates where there is a <see cref="IETag"/> property.
        /// </summary>
        public bool HasDatabaseETagProperty => Properties!.Any(x => x.Name == "ETag" && CompareNullOrValue(x.DatabaseIgnore, false));

        /// <summary>
        /// Indicates where there is a <see cref="IChangeLog"/> property.
        /// </summary>
        public bool HasEntityFrameworkChangeLogProperty => Properties!.Any(x => x.Name == "ChangeLog" && !CompareValue(x.EntityFrameworkMapper, "Skip"));

        /// <summary>
        /// Indicates where there is a <see cref="IETag"/> property.
        /// </summary>
        public bool HasEntityFrameworkETagProperty => Properties!.Any(x => x.Name == "ETag" && !CompareValue(x.EntityFrameworkMapper, "Skip"));

        /// <summary>
        /// Gets the list of properties that are to be used for gRPC.
        /// </summary>
        public List<PropertyConfig>? GrpcProperties => Properties!.Where(x => x.GrpcFieldNo.HasValue && x.GrpcFieldNo.Value > 0).ToList();

        /// <summary>
        /// Gets or sets the corresponding <see cref="OperationConfig"/> collection.
        /// </summary>
        [JsonProperty("operations", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Operation` collection.")]
        public List<OperationConfig>? Operations { get; set; }

        /// <summary>
        /// Gets the IEntityManager <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? IManagerOperations => Operations!.Where(x => IsFalse(x.ExcludeIManager)).ToList();

        /// <summary>
        /// Gets the EntityManager <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? ManagerOperations => Operations!.Where(x => IsFalse(x.ExcludeManager)).ToList();

        /// <summary>
        /// Gets the EntityManager <see cref="OperationConfig"/> collection where the Manager is not custom.
        /// </summary>
        public List<OperationConfig>? ManagerAutoOperations => Operations!.Where(x => IsFalse(x.ExcludeManager) && CompareNullOrValue(x.ManagerCustom, false)).ToList();

        /// <summary>
        /// Gets the IEntityDataSvc <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? IDataSvcOperations => Operations!.Where(x => IsFalse(x.ExcludeIDataSvc)).ToList();

        /// <summary>
        /// Gets the EntityDataSvc <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? DataSvcOperations => Operations!.Where(x => IsFalse(x.ExcludeDataSvc)).ToList();

        /// <summary>
        /// Gets the EntityDataSvc <see cref="OperationConfig"/> collection where the DataSvc is not custom.
        /// </summary>
        public List<OperationConfig>? DataSvcAutoOperations => Operations!.Where(x => IsFalse(x.ExcludeDataSvc) && CompareNullOrValue(x.DataSvcCustom, false)).ToList();

        /// <summary>
        /// Indicates where there are any <see cref="OperationConfig.ManagerExtensions"/>.
        /// </summary>
        public bool HasManagerExtensions => Operations.Any(x => x.ManagerExtensions == true);

        /// <summary>
        /// Indicates where there are any <see cref="OperationConfig.DataSvcExtensions"/>.
        /// </summary>
        public bool HasDataSvcExtensions => Operations.Any(x => x.DataSvcExtensions == true);

        /// <summary>
        /// Indicates where there are any <see cref="OperationConfig.DataExtensions"/>.
        /// </summary>
        public bool HasDataExtensions => Operations.Any(x => x.DataExtensions == true);

        /// <summary>
        /// Gets the Manager constructor parameters.
        /// </summary>
        public List<ParameterConfig> ManagerCtorParameters { get; } = new List<ParameterConfig>();

        /// <summary>
        /// Gets the DataSvc constructor parameters.
        /// </summary>
        public List<ParameterConfig> DataSvcCtorParameters { get; } = new List<ParameterConfig>();

        /// <summary>
        /// Gets the IEntityData <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? IDataOperations => Operations!.Where(x => IsFalse(x.ExcludeIData)).ToList();

        /// <summary>
        /// Gets the IEntityData <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? DataOperations => Operations!.Where(x => IsFalse(x.ExcludeData)).ToList();

        /// <summary>
        /// Gets the Grpc <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? GrpcOperations => Operations!.Where(x => CompareValue(x.Grpc, true) && !x.IsPatch).ToList();

        /// <summary>
        /// Gets the Data constructor parameters.
        /// </summary>
        public List<ParameterConfig> DataCtorParameters { get; } = new List<ParameterConfig>();

        /// <summary>
        /// Gets the EntityController <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? WebApiOperations => Operations!.Where(x => IsFalse(x.ExcludeWebApi)).ToList();

        /// <summary>
        /// Gets the EntityAgent <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? WebApiAgentOperations => Operations!.Where(x => IsFalse(x.ExcludeWebApiAgent)).ToList();

        /// <summary>
        /// Gets the WebApi Contructor parameters.
        /// </summary>
        public List<ParameterConfig> WebApiCtorParameters { get; } = new List<ParameterConfig>();

        /// <summary>
        /// Gets or sets the corresponding <see cref="ConstConfig"/> collection.
        /// </summary>
        [JsonProperty("consts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Consts` collection.")]
        public List<ConstConfig>? Consts { get; set; }

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => ToComments($"Represents the {Text} entity.");

        /// <summary>
        /// Gets the entity name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityName => !GenericWithT.HasValue || !GenericWithT.Value ? Name : $"{Name}<T>";

        /// <summary>
        /// Gets the entity collection name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityCollectionName => !GenericWithT.HasValue || !GenericWithT.Value ? $"{Name}Collection" : $"{Name}Collection<T>";

        /// <summary>
        /// Gets the entity collection result name (accounts for <see cref="GenericWithT"/>).
        /// </summary>
        public string? EntityCollectionResultName => !GenericWithT.HasValue || !GenericWithT.Value ? $"{Name}CollectionResult" : $"{Name}CollectionResult<T>";

        /// <summary>
        /// Gets the <see cref="Name"/> formatted as see comments.
        /// </summary>
        public string? EntityNameSeeComments => IsTrue(ExcludeEntity) ? $"<b>{Name}</b>" : ToSeeComments(Name);

        /// <summary>
        /// Gets or sets the computed entity inherits.
        /// </summary>
        public string? EntityInherits { get; set; }

        /// <summary>
        /// Gets or sets the computed model inherits.
        /// </summary>
        public string? ModelInherits { get; set; }

        /// <summary>
        /// Gets or sets the computed entity collection inherits.
        /// </summary>
        public string? EntityCollectionInherits { get; set; }

        /// <summary>
        /// Gets or sets the computed entity collection result inherits.
        /// </summary>
        public string? EntityCollectionResultInherits { get; set; }

        /// <summary>
        /// Gets or sets the computed entity inherits.
        /// </summary>
        public string? EntityImplements { get; set; }

        /// <summary>
        /// Gets or sets the computed model inherits.
        /// </summary>
        public string? ModelImplements { get; set; }

        /// <summary>
        /// Indicates whether any of the operations use validators.
        /// </summary>
        public bool UsesValidators => Validator != null || Operations.Any(x => x.Validator != null || x.Parameters.Any(y => y.Validator != null));

        /// <summary>
        /// Indicates whether at least one operation needs a Manager.
        /// </summary>
        public bool RequiresManager => !(CompareValue(ExcludeManager, true) && CompareValue(ExcludeIManager, true));

        /// <summary>
        /// Indicates whether at least one operation needs a DataSvc.
        /// </summary>
        public bool RequiresDataSvc => !(CompareValue(ExcludeDataSvc, true) && CompareValue(ExcludeIDataSvc, true)) || Operations.Any(x => CompareNullOrValue(x.ManagerCustom, false));

        /// <summary>
        /// Indicates whether at least one operation needs a Data.
        /// </summary>
        public bool RequiresData => (CompareValue(ExcludeData, "Exclude") && CompareValue(ExcludeIData, true)) || Operations.Any(x => CompareNullOrValue(x.DataSvcCustom, false));

        /// <summary>
        /// Indicates whether any of the operations will raise an event within the DataSvc-layer. 
        /// </summary>
        public bool SupportsDataSvcEvents => Operations.Any(x => x.Events.Count > 0 && x.EventPublish == "DataSvc");

        /// <summary>
        /// Indicates whether any of the operations will raise an event within the Data-layer.
        /// </summary>
        public bool SupportsDataEvents => Operations.Any(x => x.Events.Count > 0 && x.EventPublish == "Data");

        /// <summary>
        /// Indicates whether auto-implementing 'Database'.
        /// </summary>
        public bool UsesDatabase => AutoImplement == "Database" || Operations.Any(x => x.AutoImplement == "Database");

        /// <summary>
        /// Indicates whether auto-implementing 'EntityFramework'.
        /// </summary>
        public bool UsesEntityFramework => AutoImplement == "EntityFramework" || EntityFrameworkModel != null || Operations.Any(x => x.AutoImplement == "EntityFramework");

        /// <summary>
        /// Indicates whether auto-implementing 'Cosmos'.
        /// </summary>
        public bool UsesCosmos => AutoImplement == "Cosmos" || CosmosModel != null || Operations.Any(x => x.AutoImplement == "Cosmos");

        /// <summary>
        /// Indicates whether auto-implementing 'OData'.
        /// </summary>
        public bool UsesOData => AutoImplement == "OData" || ODataModel != null || Operations.Any(x => x.AutoImplement == "OData");

        /// <summary>
        /// Indicates whether auto-implementing 'HttpAgent'.
        /// </summary>
        public bool UsesHttpAgent => AutoImplement == "HttpAgent" || HttpAgentModel != null || Operations.Any(x => x.AutoImplement == "HttpAgent");

        /// <summary>
        /// Indicates whether AutoMapper is being used.
        /// </summary>
        public bool UsesAutoMapper { get; set; }

        /// <summary>
        /// Indicates whether the data extensions section is required.
        /// </summary>
        public bool DataExtensionsRequired => HasDataExtensions || UsesCosmos || DataOperations.Any(x => x.Type == "GetColl");

        /// <summary>
        /// Gets the reference data qualified Entity name.
        /// </summary>
        public string RefDataQualifiedEntityCollectionName => RefDataQualifiedEntityName + "Collection";

        /// <summary>
        /// Gets the reference data qualified Entity name.
        /// </summary>
        public string RefDataQualifiedEntityName => string.IsNullOrEmpty(RefDataType) ? Name! : $"{(string.IsNullOrEmpty(Root?.RefDataNamespace) ? "RefDataBusNamespace" : "RefDataNamespace")}.{Name}";

        /// <summary>
        /// Indicates whether the Manager needs a DataSvc using statement.
        /// </summary>
        public bool ManagerNeedsUsingDataSvc => Operations.Any(x => x.ManagerCustom == null || x.ManagerCustom == false);

        /// <summary>
        /// Indicates whether the DataSvc needs a DataSvc using statement.
        /// </summary>
        public bool DataSvcNeedsUsingData => Operations.Any(x => x.DataSvcCustom == null || x.DataSvcCustom == false);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();
            Text = ToComments(DefaultWhereNull(Text, () => StringConversion.ToSentenceCase(Name)));
            FileName = DefaultWhereNull(FileName, () => Name);
            EntityScope = DefaultWhereNull(EntityScope, () => Root!.EntityScope);
            EntityUsing = DefaultWhereNull(EntityUsing, () => EntityScope == "Autonomous" ? "Business" : EntityScope);
            PrivateName = DefaultWhereNull(PrivateName, () => StringConversion.ToPrivateCase(Name));
            ArgumentName = DefaultWhereNull(ArgumentName, () => StringConversion.ToCamelCase(Name));
            ConstType = DefaultWhereNull(ConstType, () => "string");
            RefDataText = DefaultWhereNull(RefDataText, () => Parent!.RefDataText);
            RefDataSortOrder = DefaultWhereNull(RefDataSortOrder, () => "SortOrder");
            ImplementsAutoInfer = DefaultWhereNull(ImplementsAutoInfer, () => true);
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => Parent!.JsonSerializer);
            AutoImplement = DefaultWhereNull(AutoImplement, () => "None");
            DataCtor = DefaultWhereNull(DataCtor, () => "Public");
            DatabaseName = InterfaceiseName(DefaultWhereNull(DatabaseName, () => Parent!.DatabaseName));
            DatabaseSchema = DefaultWhereNull(DatabaseSchema, () => Parent!.DatabaseSchema);
            EntityFrameworkName = InterfaceiseName(DefaultWhereNull(EntityFrameworkName, () => Parent!.EntityFrameworkName));
            CosmosName = InterfaceiseName(DefaultWhereNull(CosmosName, () => Parent!.CosmosName));
            CosmosPartitionKey = DefaultWhereNull(CosmosPartitionKey, () => "PartitionKey.None");
            ODataName = InterfaceiseName(DefaultWhereNull(ODataName, () => Parent!.ODataName));
            HttpAgentName = InterfaceiseName(DefaultWhereNull(HttpAgentName, () => Parent!.HttpAgentName));
            DataSvcCaching = DefaultWhereNull(DataSvcCaching, () => true);
            DataSvcCtor = DefaultWhereNull(DataSvcCtor, () => "Public");
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => Parent!.EventSubjectFormat);
            EventSource = DefaultWhereNull(EventSource, () => $"{Name!.ToLowerInvariant()}/{{$key}}");
            EventPublish = DefaultWhereNull(EventPublish, () => Parent!.EventPublish);
            EventOutbox = DefaultWhereNull(EventOutbox, () => Parent!.EventOutbox);
            EventTransaction = DefaultWhereNull(EventTransaction, () => Parent!.EventTransaction);
            ManagerCtor = DefaultWhereNull(ManagerCtor, () => "Public");
            WebApiAuthorize = DefaultWhereNull(WebApiAuthorize, () => Parent!.WebApiAuthorize);
            WebApiCtor = DefaultWhereNull(WebApiCtor, () => "Public");
            WebApiAutoLocation = DefaultWhereNull(WebApiAutoLocation, () => Parent!.WebApiAutoLocation);

            if (!string.IsNullOrEmpty(Parent!.WebApiRoutePrefix))
                WebApiRoutePrefix = string.IsNullOrEmpty(WebApiRoutePrefix) ? Parent!.WebApiRoutePrefix :
                    $"{(Parent!.WebApiRoutePrefix.EndsWith('/') ? Parent!.WebApiRoutePrefix[..^1] : Parent!.WebApiRoutePrefix)}/{(WebApiRoutePrefix.StartsWith('/') ? WebApiRoutePrefix[1..] : WebApiRoutePrefix)}";

            InferInherits();
            PrepareConsts();
            PrepareProperties();
            PrepareOperations();
            InferImplements();
            PrepareConstructors();

            ExcludeEntity = DefaultWhereNull(ExcludeEntity, () => false);
            ExcludeAll = DefaultWhereNull(ExcludeAll, () => Operations!.Count == 0);
            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, true));
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, true) ? "Exclude" : "Include");
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, true));
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, true));
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, true));
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, true));
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, true));
        }

        /// <summary>
        /// Interface-ise the name; i.e. prefix with an 'I'.
        /// </summary>
        private static string? InterfaceiseName(string? name) => string.IsNullOrEmpty(name) || (name.StartsWith("I", StringComparison.Ordinal) && name.Length >= 2 && char.IsUpper(name[1])) ? name : "I" + name;

        /// <summary>
        /// Infers the <see cref="Inherits"/>, <see cref="CollectionInherits"/> and <see cref="CollectionResultInherits"/> values.
        /// </summary>
        private void InferInherits()
        {
            EntityInherits = Inherits;
            EntityInherits = DefaultWhereNull(EntityInherits, () => RefDataType switch
            {
                "int" => "ReferenceDataBaseInt32",
                "long" => "ReferenceDataBaseInt64",
                "Guid" => "ReferenceDataBaseGuid",
                "string" => "ReferenceDataBaseString",
                _ => CompareNullOrValue(OmitEntityBase, false) ? "EntityBase" : null
            });

            ModelInherits = RefDataType switch
            {
                "int" => "ReferenceDataBaseInt32",
                "long" => "ReferenceDataBaseInt64",
                "Guid" => "ReferenceDataBaseGuid",
                "string" => "ReferenceDataBaseString",
                _ => EntityInherits == "EntityBase" ? null : EntityInherits
            };

            EntityCollectionInherits = CollectionInherits;
            EntityCollectionInherits = DefaultWhereNull(EntityCollectionInherits, () =>
            {
                if (RefDataType == null)
                    return CompareValue(CollectionKeyed, true) ? $"EntityBaseKeyedCollection<UniqueKey, {EntityName}>" : $"EntityBaseCollection<{EntityName}>";
                else
                    return $"ReferenceDataCollectionBase<{EntityName}>";
            });

            EntityCollectionResultInherits = CollectionResultInherits;
            EntityCollectionResultInherits = DefaultWhereNull(CollectionResultInherits, () => $"EntityCollectionResult<{EntityCollectionName}, {EntityName}>");

            CollectionInherits = DefaultWhereNull(CollectionInherits, () => $"List<{EntityName}>");
            CollectionResultInherits = DefaultWhereNull(CollectionResultInherits, () => $"CollectionResult<{EntityCollectionName}, {EntityName}>");
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
                constant.Prepare(Root!, this);
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
                AddInheritedProperty("Id", ref i, () => new PropertyConfig { Text = $"{{{{{Name}}}}} identifier", Type = RefDataType, UniqueKey = true, DataAutoGenerated = true, DataName = $"{Name}Id" });
                AddInheritedProperty("Code", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("Text", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("IsActive", ref i, () => new PropertyConfig { Type = "bool" });
                AddInheritedProperty("SortOrder", ref i, () => new PropertyConfig { Type = "int" });
                AddInheritedProperty("ETag", ref i, () => new PropertyConfig { Type = "string" });
                AddInheritedProperty("ChangeLog", ref i, () => new PropertyConfig { Type = "ChangeLog" });
            }

            foreach (var property in Properties)
            {
                property.Prepare(Root!, this);
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
                operation.Prepare(Root!, this);
            }

            if (WebApiAutoLocation == true)
            {
                var co = Operations!.FirstOrDefault(x => x.Type == "Create" && x.Name == "Create");
                if (co != null)
                {
                    var go = Operations!.FirstOrDefault(x => x.Type == "Get" && x.Name == "Get");
                    if (go != null && co.WebApiLocation == null)
                        co.WebApiLocation = "^";
                }
            }

            // Go back and do another pass.
            foreach (var operation in Operations)
            {
                operation.PrepareAfter();
            }
        }

        /// <summary>
        /// Infer the implements from the properties configured.
        /// </summary>
        private void InferImplements()
        {
            var implements = new List<string>();
            var modelImplements = new List<string>();
            var i = 0;
            var m = 0;

            if (CompareValue(ImplementsAutoInfer, true))
            {
                if (Implements != null)
                {
                    foreach (var str in Implements!.Split(",", StringSplitOptions.RemoveEmptyEntries))
                    {
                        var txt = str?.Trim();
                        if (!string.IsNullOrEmpty(txt))
                            implements.Add(txt!);
                    }
                }

                var id = Properties.FirstOrDefault(x => x.Name == "Id" && CompareNullOrValue(x.Inherited, false));
                if (id != null)
                {
                    var iid = id.Type switch
                    {
                        "Guid" => "IGuidIdentifier",
                        "int" => "IInt32Identifier",
                        "long" => "IInt64Identifier",
                        "string" => "IStringIdentifier",
                        _ => "IIdentifier",
                    };

                    implements.Insert(i++, iid);
                    modelImplements.Insert(m++, iid);
                }
            }

            if (Properties.Any(x => CompareValue(x.UniqueKey, true) && CompareNullOrValue(x.Inherited, false)))
            {
                implements.Insert(i++, "IUniqueKey");
                modelImplements.Insert(m++, "IUniqueKey");
            }

            if (Properties.Any(x => CompareValue(x.PartitionKey, true) && CompareNullOrValue(x.Inherited, false)))
                implements.Insert(i++, "IPartitionKey");

            if (CompareValue(ImplementsAutoInfer, true))
            {
                if (Properties.Any(x => x.Name == "ETag" && x.Type == "string" && CompareNullOrValue(x.Inherited, false)))
                {
                    implements.Insert(i++, "IETag");
                    modelImplements.Insert(m++, "IETag");
                }

                if (Properties.Any(x => x.Name == "ChangeLog" && x.Type == "ChangeLog" && CompareNullOrValue(x.Inherited, false)))
                {
                    implements.Insert(i++, "IChangeLog");
                    modelImplements.Insert(m++, "IChangeLog");
                }
            }

            if (RefDataType == null)
                implements.Insert(i++, $"IEquatable<{EntityName}>");

            EntityImplements = implements.Count == 0 ? null : string.Join(", ", implements.GroupBy(x => x).Select(y => y.First()).ToArray());
            ModelImplements = modelImplements.Count == 0 ? null : string.Join(", ", modelImplements.GroupBy(x => x).Select(y => y.First()).ToArray());
        }

        /// <summary>
        /// Prepare the constructors.
        /// </summary>
        private void PrepareConstructors()
        {
            // Manager constructors.
            var oc = new OperationConfig { Name = "<internal>" };
            oc.Prepare(Root!, this);

            // Manager constructors.
            if (RequiresDataSvc)
                ManagerCtorParameters.Add(new ParameterConfig { Name = "DataService", Type = $"I{Name}DataSvc", Text = $"{{{{I{Name}DataSvc}}}}" });

            AddConfiguredParameters(ManagerCtorParams, ManagerCtorParameters);

            // Get the identifier generator(s).
            foreach (var p in Properties!.Where(x => !string.IsNullOrEmpty(x.IdentifierGenerator)))
            {
                if (!ManagerCtorParameters.Any(x => x.Name == p.IdentifierGeneratorName))
                    ManagerCtorParameters.Add(new ParameterConfig { Name = p.IdentifierGeneratorName, Type = p.IdentifierGenerator, Text = $"{{{{{p.IdentifierGenerator}}}}}" });
            }

            foreach (var ctor in ManagerCtorParameters)
            {
                ctor.Prepare(Root!, oc);
            }

            // DataSvc constructors.
            if (RequiresData)
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "Data", Type = $"I{Name}Data", Text = $"{{{{I{Name}Data}}}}" });

            if (SupportsDataSvcEvents)
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "EvtPub", Type = $"IEventPublisher", Text = "{{IEventPublisher}}" });

            if (CompareValue(DataSvcCaching, true) && Operations.Any(x => x.SupportsCaching))
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "Cache", Type = "IRequestCache", Text = "{{IRequestCache}}" });
            else 
                DataSvcCaching = false;

            AddConfiguredParameters(DataSvcCtorParams, DataSvcCtorParameters);
            foreach (var ctor in DataSvcCtorParameters)
            {
                ctor.Prepare(Root!, oc);
            }

            // Data constructors.
            if (UsesDatabase)
                DataCtorParameters.Add(new ParameterConfig { Name = "Db", Type = DatabaseName, Text = $"{{{{{DatabaseName}}}}}" });

            if (UsesEntityFramework)
                DataCtorParameters.Add(new ParameterConfig { Name = "Ef", Type = EntityFrameworkName, Text = $"{{{{{EntityFrameworkName}}}}}" });

            if (UsesCosmos)
                DataCtorParameters.Add(new ParameterConfig { Name = "Cosmos", Type = CosmosName, Text = $"{{{{{CosmosName}}}}}" });

            if (UsesOData)
                DataCtorParameters.Add(new ParameterConfig { Name = "OData", Type = ODataName, Text = $"{{{{{ODataName}}}}}" });

            if (UsesHttpAgent)
                DataCtorParameters.Add(new ParameterConfig { Name = "HttpAgent", Type = HttpAgentName, Text = $"{{{{{HttpAgentName}}}}}" });

            if (UsesEntityFramework || UsesCosmos || UsesOData || UsesHttpAgent)
            { 
                DataCtorParameters.Add(new ParameterConfig { Name = "Mapper", Type = "AutoMapper.IMapper", Text = $"{{{{AutoMapper.IMapper}}}}" });
                UsesAutoMapper = true;
            }

            if (SupportsDataEvents)
                DataCtorParameters.Add(new ParameterConfig { Name = "EvtPub", Type = $"IEventPublisher", Text = "{{IEventPublisher}}" });

            AddConfiguredParameters(DataCtorParams, DataCtorParameters);
            foreach (var ctor in DataCtorParameters)
            {
                ctor.Prepare(Root!, oc);
            }

            // WebAPI contstructors.
            if (RequiresManager)
                WebApiCtorParameters.Insert(0, new ParameterConfig { Name = "Manager", Type = $"I{Name}Manager", Text = $"{{{{I{Name}Manager}}}}" });

            AddConfiguredParameters(WebApiCtorParams, WebApiCtorParameters);
            foreach (var ctor in WebApiCtorParameters)
            {
                ctor.Prepare(Root!, oc);
            }
        }

        /// <summary>
        /// Add configured list to the parameters.
        /// </summary>
        internal static void AddConfiguredParameters(List<string>? configList, List<ParameterConfig> paramList)
        {
            if (configList == null)
                return;

            foreach (var p in configList)
            {
                var pc = CreateParameterConfigFromInterface(p);
                if (pc != null && !paramList.Any(x => x.Name == pc.Name))
                    paramList.Add(pc);
            }
        }

        /// <summary>
        /// Create parameter configuration from interface definition.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static ParameterConfig? CreateParameterConfigFromInterface(string text)
        {
            var parts = text.Split("^", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return null;

            var pc = new ParameterConfig { Type = parts[0], Text = $"{{{{{parts[0]}}}}}" };
            if (parts.Length == 1)
            {
                var nsparts = parts[0].Split(".", StringSplitOptions.RemoveEmptyEntries);
                pc.Name = nsparts.Last().Replace("<", "", StringComparison.InvariantCulture).Replace(">", "", StringComparison.InvariantCulture);
                if (pc.Name[0] == 'I' && pc.Name.Length > 1 && char.IsUpper(pc.Name[1]))
                    pc.Name = pc.Name[1..];
            }
            else
                pc.Name = StringConversion.ToPascalCase(parts[1]);

            return pc;
        }
    }
}