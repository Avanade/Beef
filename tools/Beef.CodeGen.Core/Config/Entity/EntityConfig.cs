// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx.Caching;
using CoreEx.Entities;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Entity</b> code-generation configuration.
    /// </summary>
    [CodeGenClass("Entity", Title = "'Entity' object (entity-driven)",
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
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CodeGenCategory("Entity", Title = "Provides the _Entity class_ configuration.")]
    [CodeGenCategory("Collection", Title = "Provides the _Entity collection class_ configuration.")]
    [CodeGenCategory("Operation", Title = "Provides the _Operation_ configuration.", Description = "These primarily provide a shorthand to create the standard `Get`, `Create`, `Update` and `Delete` operations (versus having to specify directly).")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Events", Title = "Provides the _Events_ configuration.")]
    [CodeGenCategory("WebApi", Title = "Provides the data _Web API_ configuration.")]
    [CodeGenCategory("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CodeGenCategory("DataSvc", Title = "Provides the _Data Services-layer_ configuration.")]
    [CodeGenCategory("Data", Title = "Provides the generic _Data-layer_ configuration.")]
    [CodeGenCategory("Database", Title = "Provides the specific _Database (ADO.NET)_ configuration where `AutoImplement` is `Database`.")]
    [CodeGenCategory("EntityFramework", Title = "Provides the specific _Entity Framework (EF)_ configuration where `AutoImplement` is `EntityFramework`.")]
    [CodeGenCategory("Cosmos", Title = "Provides the specific _Cosmos_ configuration where `AutoImplement` is `Cosmos`.")]
    [CodeGenCategory("OData", Title = "Provides the specific _OData_ configuration where `AutoImplement` is `OData`.")]
    [CodeGenCategory("HttpAgent", Title = "Provides the specific _HTTP Agent_ configuration where `AutoImplement` is `HttpAgent`.")]
    [CodeGenCategory("Model", Title = "Provides the data _Model_ configuration.")]
    [CodeGenCategory("gRPC", Title = "Provides the _gRPC_ configuration.")]
    [CodeGenCategory("Exclude", Title = "Provides the _Exclude_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
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
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The unique entity name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonPropertyName("text")]
        [CodeGenProperty("Key", Title = "The overriding text for use in comments.",
            Description = "Overrides the Name (as sentence text) for the summary comments. It will be formatted as: `Represents the {Text} entity.`. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}). To have the text used as-is prefix with a `+` plus-sign character.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the overriding file name.
        /// </summary>
        [JsonPropertyName("fileName")]
        [CodeGenProperty("Key", Title = "The overriding file name.",
            Description = "Overrides the Name as the code-generated file name.")]
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonPropertyName("privateName")]
        [CodeGenProperty("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.")]
        public string? PrivateName { get; set; }

        /// <summary>
        /// Gets or sets the overriding argument name.
        /// </summary>
        [JsonPropertyName("argumentName")]
        [CodeGenProperty("Key", Title = "The overriding argument name.",
            Description = "Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.")]
        public string? ArgumentName { get; set; }

        /// <summary>
        /// Gets or sets the Const Type option.
        /// </summary>
        [JsonPropertyName("constType")]
        [CodeGenProperty("Key", Title = "The Const .NET Type option.", Options = ["int", "long", "Guid", "string"],
            Description = "The .NET Type to be used for the `const` values. Defaults to `string`.")]
        public string? ConstType { get; set; }

        /// <summary>
        /// Indicates whether to override the <see cref="IInitial.IsInitial"/> property.
        /// </summary>
        [JsonPropertyName("isInitialOverride")]
        [CodeGenProperty("Key", Title = "Indicates whether to override the `IInitial.IsInitial` property.",
            Description = "Set to either `true` or `false` to override as specified; otherwise, `null` to check each property. Defaults to `null`.")]
        public bool? IsInitialOverride { get; set; }

        /// <summary>
        /// Indicates whether to use Results.
        /// </summary>
        [JsonPropertyName("withResult")]
        [CodeGenProperty("Key", Title = "Indicates whether to use `CoreEx.Results` (aka Railway-oriented programming).",
            Description = "Defaults to `CodeGeneration.WithResult`. This can be overridden within the Operation`(s).")]
        public bool? WithResult { get; set; }

        #endregion

        #region RefData

        /// <summary>
        /// Gets or sets the Reference Data identifier Type option.
        /// </summary>
        [JsonPropertyName("refDataType")]
        [CodeGenProperty("RefData", Title = "The Reference Data identifier Type option.", IsImportant = true, Options = ["int", "long", "Guid", "string"],
            Description = "Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates whether a corresponding <i>text</i> property is added when generating a Reference Data property overriding the <c>CodeGeneration.RefDataText</c> selection.
        /// </summary>
        [JsonPropertyName("refDataText")]
        [CodeGenProperty("RefData", Title = "Indicates whether a corresponding `Text` property is added when generating a Reference Data `Property` overriding the `CodeGeneration.RefDataText` selection.",
            Description = "This is used where serializing within the Web API`Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`). Defaults from `CodeGeneration.RefDataText`.")]
        public bool? RefDataText { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data sort order option.
        /// </summary>
        [JsonPropertyName("refDataSortOrder")]
        [CodeGenProperty("RefData", Title = "The Reference Data sort order option.", Options = ["SortOrder", "Id", "Code", "Text"],
            Description = "Specifies the default sort order for the underlying Reference Data collection. Defaults to `SortOrder`.")]
        public string? RefDataSortOrder { get; set; }

        #endregion

        #region Entity

        /// <summary>
        /// Gets or sets the base class that the entity inherits from.
        /// </summary>
        [JsonPropertyName("inherits")]
        [CodeGenProperty("Entity", Title = "The base class that the entity inherits from.",
            Description = "Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseEx<xxx>` depending on the corresponding `RefDataType` value. " +
                          "See `OmitEntityBase` if the desired outcome is to not inherit from any of the aforementioned base classes.")]
        public string? Inherits { get; set; }

        /// <summary>
        /// Gets or sets the list of comma separated interfaces that are to be declared for the entity class.
        /// </summary>
        [JsonPropertyName("implements")]
        [CodeGenProperty("Entity", Title = "The list of comma separated interfaces that are to be declared for the entity class.")]
        public string? Implements { get; set; }

        /// <summary>
        /// Indicates whether to automatically infer the interface implements for the entity from the properties declared.
        /// </summary>
        [JsonPropertyName("implementsAutoInfer")]
        [CodeGenProperty("Entity", Title = "Indicates whether to automatically infer the interface implements for the entity from the properties declared.",
            Description = "Will attempt to infer the following: `IIdentifier<Guid>`, `IIdentifier<int>`, `IIdentifier<long>`, `IIdentifier<string>`, `IETag` and `IChangeLog`. Defaults to `true`.")]
        public bool? ImplementsAutoInfer { get; set; }

        /// <summary>
        /// Indicates whether the class should be defined as abstract.
        /// </summary>
        [JsonPropertyName("abstract")]
        [CodeGenProperty("Entity", Title = "Indicates whether the class should be defined as abstract.")]
        public bool? Abstract { get; set; }

        /// <summary>
        /// Indicates whether the class should be defined as a generic with a single parameter <c>T</c>.
        /// </summary>
        [JsonPropertyName("genericWithT")]
        [CodeGenProperty("Entity", Title = "Indicates whether the class should be defined as a generic with a single parameter `T`.")]
        public bool? GenericWithT { get; set; }

        /// <summary>
        /// Gets or sets the entity namespace to be appended.
        /// </summary>
        [JsonPropertyName("namespace")]
        [CodeGenProperty("Entity", Title = "The entity namespace to be appended.",
            Description = "Appended to the end of the standard structure as follows: `{Company}.{AppName}.Business.Entities.{Namespace}`.")]
        public string? Namespace { get; set; }

        /// <summary>
        /// Indicates that the entity should not inherit from `EntityBase`.
        /// </summary>
        [JsonPropertyName("omitEntityBase")]
        [CodeGenProperty("Entity", Title = "Indicates that the entity should not inherit from `EntityBase`.",
            Description = "As such any of the `EntityBase` related capabilites are not supported (are omitted from generation). The intention for this is more for the generation of simple internal entities.")]
        public bool? OmitEntityBase { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonPropertyName("jsonSerializer")]
        [CodeGenProperty("Entity", Title = "The JSON Serializer to use for JSON property attribution.", Options = ["SystemText", "Newtonsoft"],
            Description = "Defaults to the `CodeGeneration.JsonSerializer` configuration property where specified; otherwise, `SystemText`.")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Indicates whether the entity is for internal use only; declared in the Business entities only.
        /// </summary>
        [JsonPropertyName("internalOnly")]
        [CodeGenProperty("Entity", Title = "Indicates whether the entity is for internal use only; declared in Business entities only.")]
        public bool? InternalOnly { get; set; }

        #endregion

        #region Collection

        /// <summary>
        /// Indicates whether a corresponding entity collection class should be created.
        /// </summary>
        [JsonPropertyName("collection")]
        [CodeGenProperty("Collection", Title = "Indicates whether a corresponding entity collection class should be created.", IsImportant = true)]
        public bool? Collection { get; set; }

        /// <summary>
        /// Indicates whether a corresponding entity collection result class should be created
        /// </summary>
        [JsonPropertyName("collectionResult")]
        [CodeGenProperty("Collection", Title = "Indicates whether a corresponding entity collection result class should be created", IsImportant = true,
            Description = "Enables the likes of additional paging state to be stored with the underlying collection.")]
        public bool? CollectionResult { get; set; }

        /// <summary>
        /// Gets or sets the entity collection type used where <see cref="CollectionInherits"/> is not specified.
        /// </summary>
        [JsonPropertyName("collectionType")]
        [CodeGenProperty("Collection", Title = "The entity collection type used where `CollectionInherits` is not specified.", Options = ["Standard", "Keyed", "Dictionary"])]
        public string? CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the base class that a <see cref="Collection"/> inherits from.
        /// </summary>
        [JsonPropertyName("collectionInherits")]
        [CodeGenProperty("Collection", Title = "The base class that a `Collection` inherits from.",
            Description = "Defaults to `EntityBaseCollection` or `EntityBaseKeyedCollection` depending on `CollectionKeyed`. For Reference Data it will default to `ReferenceDataCollectionBase`.")]
        public string? CollectionInherits { get; set; }

        /// <summary>
        /// Gets or sets the base class that a <see cref="CollectionResult"/> inherits from.
        /// </summary>
        [JsonPropertyName("collectionResultInherits")]
        [CodeGenProperty("Collection", Title = "The base class that a `CollectionResult` inherits from.",
            Description = "Defaults to `EntityCollectionResult`.")]
        public string? CollectionResultInherits { get; set; }

        #endregion

        #region Operation

        /// <summary>
        /// Gets or sets the key CRUDBA behaviors (operations) will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("behavior")]
        [CodeGenProperty("Operation", Title = "Defines the key CRUD-style behavior (operation types), being 'C'reate, 'G'et (or 'R'ead), 'U'pdate, 'P'atch and 'D'elete). Additionally, GetByArgs ('B') and GetAll ('A') operations that will be automatically generated where not otherwise explicitly specified.",
            Description = "Value may only specifiy one or more of the `CGRUDBA` characters (in any order) to define the automatically generated behavior (operations); for example: `CRUPD` or `CRUP` or `rba` (case insensitive). " +
                          "This is shorthand for setting one or more of the following properties: `Get`, `GetByArgs`, `GetAll`, 'Create', `Update`, `Patch` and `Delete`. Where one of these properties is set to either `true` or `false` this will take precedence over the value set for `Behavior`.")]
        public string? Behavior { get; set; }

        /// <summary>
        /// Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("get")]
        [CodeGenProperty("Operation", Title = "Indicates that a `Get` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Get { get; set; }

        /// <summary>
        /// Indicates that a `GetByArgs` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("getByArgs")]
        [CodeGenProperty("Operation", Title = "Indicates that a `GetByArgs` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? GetByArgs { get; set; }

        /// <summary>
        /// Indicates that a `GetAll` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("getAll")]
        [CodeGenProperty("Operation", Title = "Indicates that a `GetAll` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? GetAll { get; set; }

        /// <summary>
        /// Indicates that a `Create` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("create")]
        [CodeGenProperty("Operation", Title = "Indicates that a `Create` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Create { get; set; }

        /// <summary>
        /// Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("update")]
        [CodeGenProperty("Operation", Title = "Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Update { get; set; }

        /// <summary>
        /// Indicates that a `Update` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("patch")]
        [CodeGenProperty("Operation", Title = "Indicates that a `Patch` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Patch { get; set; }

        /// <summary>
        /// Indicates that a `Delete` operation will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonPropertyName("delete")]
        [CodeGenProperty("Operation", Title = "Indicates that a `Delete` operation will be automatically generated where not otherwise explicitly specified.")]
        public bool? Delete { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the data source auto-implementation option. 
        /// </summary>
        [JsonPropertyName("autoImplement")]
        [CodeGenProperty("Data", Title = "The data source auto-implementation option.", IsImportant = true, Options = ["Database", "EntityFramework", "Cosmos", "OData", "HttpAgent", "None"],
            Description = "Defaults to `CodeGeneration.AutoImplement` (where `RefDataType` or `EntityFrameworkModel` or `CosmosModel` or `HttpAgent` is not null; otherwise, `None`. " 
            + "Indicates that the implementation for the underlying `Operations` will be auto-implemented using the selected data source (unless explicitly overridden). When selected some of the related attributes will also be required (as documented). " 
            + "Additionally, the `AutoImplement` can be further specified/overridden per `Operation`.")]
        public string? AutoImplement { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated `Data` constructor.
        /// </summary>
        [JsonPropertyName("dataCtor")]
        [CodeGenProperty("Data", Title = "The access modifier for the generated `Data` constructor.", Options = ["Public", "Private", "Protected"],
            Description = "Defaults to `Public`.")]
        public string? DataCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.
        /// </summary>
        [JsonPropertyName("dataCtorParams")]
        [CodeGenPropertyCollection("Data", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? DataCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `Data` extensions logic should be generated.
        /// </summary>
        [JsonPropertyName("dataExtensions")]
        [CodeGenProperty("Data", Title = "Indicates whether the `Data` extensions logic should be generated.",
            Description = "This can be overridden using `Operation.DataExtensions`.")]
        public bool? DataExtensions { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data identifier data name.
        /// </summary>
        [JsonPropertyName("refDataIdDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `Id` data name.",
            Description = "Defaults to `Name` + `Id` (literal).")]
        public string? RefDataIdDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data code data name
        /// </summary>
        [JsonPropertyName("refDataCodeDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `Code` data name.",
            Description = "Defaults to `Code` (literal).")]
        public string? RefDataCodeDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data text data name.
        /// </summary>
        [JsonPropertyName("refDataTextDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `Text` data name.",
            Description = "Defaults to `Text` (literal).")]
        public string? RefDataTextDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data is active name.
        /// </summary>
        [JsonPropertyName("refDataIsActiveDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `IsActive` data name.",
            Description = "Defaults to `IsActive` (literal).")]
        public string? RefDataIsActiveDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data sort order data name.
        /// </summary>
        [JsonPropertyName("refDataSortOrderDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `SortOrder` data name.",
            Description = "Defaults to `SortOrder` (literal).")]
        public string? RefDataSortOrderDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data is ETag name.
        /// </summary>
        [JsonPropertyName("refDataETagDataName")]
        [CodeGenProperty("RefData", Title = "The Reference Data `ETag` data name.",
            Description = "Defaults to `RowVersion` (literal).")]
        public string? RefDataETagDataName { get; set; }

        /// <summary>
        /// Gets or sets the Reference Data stored procedure name.
        /// </summary>
        [JsonPropertyName("refDataStoredProcedureName")]
        [CodeGenProperty("RefData", Title = "The Reference Data database stored procedure name.",
            Description = "Defaults to `sp` (literal) + `Name` + `GetAll` (literal).")]
        public string? RefDataStoredProcedureName { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the .NET database interface name used where `AutoImplement` is `Database`.
        /// </summary>
        [JsonPropertyName("databaseName")]
        [CodeGenProperty("Database", Title = "The .NET database interface name (used where `AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.DatabaseName` configuration property (its default value is `IDatabase`).")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database schema name (used where `AutoImplement` is `Database`).
        /// </summary>
        [JsonPropertyName("databaseSchema")]
        [CodeGenProperty("Database", Title = "The database schema name (used where `AutoImplement` is `Database`).", IsImportant = true,
            Description = "Defaults to `dbo`.")]
        public string? DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>Mapper</c> that the generated Database <c>Mapper</c> inherits from.
        /// </summary>
        [JsonPropertyName("databaseMapperInheritsFrom")]
        [CodeGenProperty("Database", Title = "The name of the `Mapper` that the generated Database `Mapper` inherits from.")]
        public string? DatabaseMapperInheritsFrom { get; set; }

        /// <summary>
        /// Indicates that a custom Database <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonPropertyName("databaseCustomMapper")]
        [CodeGenProperty("Database", Title = "Indicates that a custom Database `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? DatabaseCustomMapper { get; set; }

        /// <summary>
        /// Indicates that a `DatabaseMapperEx` will be used versus `DatabaseMapper` (uses Reflection).
        /// </summary>
        [JsonPropertyName("databaseMapperEx")]
        [CodeGenProperty("Database", Title = "Indicates that a `DatabaseMapperEx` (extended/explicit) will be used; versus, `DatabaseMapper` (which uses Reflection internally).",
            Description = "Defaults to `CodeGeneration.DatabaseMapperEx` (its default value is `true`). The `DatabaseMapperEx` essentially replaces the `DatabaseMapper` as it is more performant; this option can be used where leagcy/existing behavior is required.")]
        public bool? DatabaseMapperEx { get; set; }

        #endregion

        #region EntityFramework

        /// <summary>
        /// Gets or sets the .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`.
        /// </summary>
        [JsonPropertyName("entityFrameworkName")]
        [CodeGenProperty("EntityFramework", Title = "The .NET Entity Framework interface name used where `AutoImplement` is `EntityFramework`.", IsImportant = true,
            Description = "Defaults to `CodeGeneration.EntityFrameworkName`.")]
        public string? EntityFrameworkName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding Entity Framework model name required where <see cref="AutoImplement"/> is <c>EntityFramework</c>.
        /// </summary>
        [JsonPropertyName("entityFrameworkModel")]
        [CodeGenProperty("EntityFramework", Title = "The corresponding Entity Framework model name (required where `AutoImplement` is `EntityFramework`).", IsImportant = true)]
        public string? EntityFrameworkModel { get; set; }

        /// <summary>
        /// Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.
        /// </summary>
        [JsonPropertyName("entityFrameworkCustomMapper")]
        [CodeGenProperty("EntityFramework", Title = "Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? EntityFrameworkCustomMapper { get; set; }

        /// <summary>
        /// Gets or sets the EntityFramework data-layer name that should be used for base mappings.
        /// </summary>
        [JsonPropertyName("entityFrameworkMapperBase")]
        [CodeGenProperty("EntityFramework", Title = "The EntityFramework data-layer name that should be used for base mappings.")]
        public string? EntityFrameworkMapperBase { get; set; }

        #endregion

        #region Cosmos

        /// <summary>
        /// Gets or sets the .NET Cosmos interface name used where `AutoImplement` is `Cosmos`.
        /// </summary>
        [JsonPropertyName("cosmosName")]
        [CodeGenProperty("Cosmos", Title = "The .NET Cosmos interface name used where `AutoImplement` is `Cosmos`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.CosmosName` configuration property (its default value is `ICosmosDb`).")]
        public string? CosmosName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding Cosmos model name required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonPropertyName("cosmosModel")]
        [CodeGenProperty("Cosmos", Title = "The corresponding Cosmos model name (required where `AutoImplement` is `Cosmos`).", IsImportant = true)]
        public string? CosmosModel { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos <c>ContainerId</c> required where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonPropertyName("cosmosContainerId")]
        [CodeGenProperty("Cosmos", Title = "The Cosmos `ContainerId` required where `AutoImplement` is `Cosmos`.", IsImportant = true)]
        public string? CosmosContainerId { get; set; }

        /// <summary>
        /// Gets or sets the C# code to be used for setting the optional Cosmos <c>PartitionKey</c> where <see cref="AutoImplement"/> is <c>Cosmos</c>.
        /// </summary>
        [JsonPropertyName("cosmosPartitionKey")]
        [CodeGenProperty("Cosmos", Title = "The C# code to be used for setting the optional Cosmos `PartitionKey` where `AutoImplement` is `Cosmos`.",
            Description = "The value `PartitionKey.None` can be specified. Literals will need to be quoted.")]
        public string? CosmosPartitionKey { get; set; }

        /// <summary>
        /// Indicates whether the <c>CosmosDbValueContainer</c> is to be used; otherwise, <c>CosmosDbContainer</c>.
        /// </summary>
        [JsonPropertyName("cosmosValueContainer")]
        [CodeGenProperty("Cosmos", Title = "Indicates whether the `CosmosDbValueContainer` is to be used; otherwise, `CosmosDbContainer`.")]
        public bool? CosmosValueContainer { get; set; }

        /// <summary>
        /// Indicates that a custom Cosmos <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonPropertyName("cosmosCustomMapper")]
        [CodeGenProperty("Cosmos", Title = "Indicates that a custom Cosmos `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? CosmosCustomMapper { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos data-layer name that should be used for base mappings.
        /// </summary>
        [JsonPropertyName("cosmosMapperBase")]
        [CodeGenProperty("Cosmos", Title = "The Cosmos data-layer name that should be used for base mappings.")]
        public string? CosmosMapperBase { get; set; }

        #endregion

        #region OData

        /// <summary>
        /// Gets or sets the .NET OData interface name used where `AutoImplement` is `OData`.
        /// </summary>
        [JsonPropertyName("odataName")]
        [CodeGenProperty("OData", Title = "The .NET OData interface name used where `AutoImplement` is `OData`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.ODataName` configuration property (its default value is `IOData`).")]
        public string? ODataName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding OData model name required where <see cref="AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonPropertyName("odataModel")]
        [CodeGenProperty("OData", Title = "The corresponding OData model name (required where `AutoImplement` is `OData`).", IsImportant = true)]
        public string? ODataModel { get; set; }

        /// <summary>
        /// Gets or sets the name of the underlying OData collection name where <see cref="AutoImplement"/> is <c>OData</c>.
        /// </summary>
        [JsonPropertyName("odataCollectionName")]
        [CodeGenProperty("OData", Title = "The name of the underlying OData collection where `AutoImplement` is `OData`.", IsImportant = true,
            Description = "The underlying `Simple.OData.Client` will attempt to infer.")]
        public string? ODataCollectionName { get; set; }

        /// <summary>
        /// Indicates that a custom OData <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonPropertyName("odataCustomMapper")]
        [CodeGenProperty("OData", Title = "Indicates that a custom OData `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? ODataCustomMapper { get; set; }

        #endregion

        #region HttpAgent

        /// <summary>
        /// Gets or sets the default .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonPropertyName("httpAgentName")]
        [CodeGenProperty("HttpAgent", Title = "The .NET HTTP Agent interface name used where `Operation.AutoImplement` is `HttpAgent`.", IsImportant = true,
            Description = "Defaults to `CodeGeneration.HttpAgentName` configuration property (its default value is `IHttpAgent`).")]
        public string? HttpAgentName { get; set; }

        /// <summary>
        /// Gets or sets the HttpAgent API route prefix where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonPropertyName("httpAgentRoutePrefix")]
        [CodeGenProperty("HttpAgent", Title = "The base HTTP Agent API route where `Operation.AutoImplement` is `HttpAgent`.",
            Description = "This is the base (prefix) `URI` for the HTTP Agent endpoint and can be further extended when defining the underlying `Operation`(s).")]
        public string? HttpAgentRoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonPropertyName("httpAgentModel")]
        [CodeGenProperty("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).", IsImportant = true,
            Description = "This can be overridden within the `Operation`(s).")]
        public string? HttpAgentModel { get; set; }

        /// <summary>
        /// Gets or sets the corresponding HTTP Agent model name required where <see cref="AutoImplement"/> is `HttpAgent`.
        /// </summary>
        [JsonPropertyName("httpAgentReturnModel")]
        [CodeGenProperty("HttpAgent", Title = "The corresponding HTTP Agent model name (required where `AutoImplement` is `HttpAgent`).",
            Description = "This can be overridden within the `Operation`(s).")]
        public string? HttpAgentReturnModel { get; set; }

        /// <summary>
        /// Gets or sets the fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.
        /// </summary>
        [JsonPropertyName("httpAgentCode")]
        [CodeGenProperty("HttpAgent", Title = "The fluent-style method-chaining C# HTTP Agent API code to include where `Operation.AutoImplement` is `HttpAgent`.",
            Description = "Prepended to `Operation.HttpAgentCode` where specified to enable standardized functionality.")]
        public string? HttpAgentCode { get; set; }

        /// <summary>
        /// Indicates that a custom HTTP Agent <c>Mapper</c> will be used; i.e. not generated.
        /// </summary>
        [JsonPropertyName("httpAgentCustomMapper")]
        [CodeGenProperty("OData", Title = "Indicates that a custom HTTP Agent `Mapper` will be used; i.e. not generated.",
            Description = "Otherwise, by default, a `Mapper` will be generated.")]
        public bool? HttpAgentCustomMapper { get; set; }

        /// <summary>
        /// Gets or sets the `HttpAgent` data-layer name that should be used for base mappings.
        /// </summary>
        [JsonPropertyName("httpAgentMapperBase")]
        [CodeGenProperty("Cosmos", Title = "The HTTP Agent data-layer name that should be used for base mappings.")]
        public string? HttpAgentMapperBase { get; set; }

        #endregion 

        #region DataSvc

        /// <summary>
        /// Indicates whether request-based <see cref="IRequestCache"/> caching is to be performed at the <c>DataSvc</c> layer to improve performance (i.e. reduce chattiness).
        /// </summary>
        [JsonPropertyName("dataSvcCaching")]
        [CodeGenProperty("DataSvc", Title = "Indicates whether request-based `IRequestCache` caching is to be performed at the `DataSvc` layer to improve performance (i.e. reduce chattiness).",
            Description = "Defaults to `true`.")]
        public bool? DataSvcCaching { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated `DataSvc` constructor.
        /// </summary>
        [JsonPropertyName("dataSvcCtor")]
        [CodeGenProperty("DataSvc", Title = "The access modifier for the generated `DataSvc` constructor.", Options = ["Public", "Private", "Protected"],
            Description = "Defaults to `Public`.")]
        public string? DataSvcCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.
        /// </summary>
        [JsonPropertyName("dataSvcCtorParams")]
        [CodeGenPropertyCollection("DataSvc", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? DataSvcCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `DataSvc` extensions logic should be generated.
        /// </summary>
        [JsonPropertyName("dataSvcExtensions")]
        [CodeGenProperty("DataSvc", Title = "Indicates whether the `DataSvc` extensions logic should be generated.",
            Description = "This can be overridden using `Operation.DataSvcExtensions`.")]
        public bool? DataSvcExtensions { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the layer to add logic to publish an event for a <c>Create</c>, <c>Update</c> or <c>Delete</c> operation.
        /// </summary>
        [JsonPropertyName("eventPublish")]
        [CodeGenProperty("Events", Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", IsImportant = true, Options = ["None", "DataSvc", "Data"],
            Description = "Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc. This can be overridden within the `Operation`(s).")]
        public string? EventPublish { get; set; }

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonPropertyName("eventSource")]
        [CodeGenProperty("Events", Title = "The Event Source.",
            Description = "Defaults to `Name` (as lowercase) appended with the `/{$key}` placeholder. Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified. " +
            "To include the entity id/key include a `{$key}` placeholder (`Create`, `Update` or `Delete` operation only); for example: `person/{$key}`. This can be overridden for the `Operation`.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Indicates whether a `System.TransactionScope` should be created and orchestrated whereever generating event publishing logic.
        /// </summary>
        [JsonPropertyName("eventTransaction")]
        [CodeGenProperty("Events", Title = "Indicates whether a `System.TransactionScope` should be created and orchestrated whereever generating event publishing logic.", IsImportant = true,
            Description = "Usage will force a rollback of any underlying data transaction (where the provider supports TransactionScope) on failure, such as an `EventPublish` error. " +
                "This is by no means implying a Distributed Transaction (DTC) should be invoked; this is only intended for a single data source that supports a TransactionScope to guarantee reliable event publishing. " +
                "Defaults to `CodeGeneration.EventTransaction`. This essentially defaults the `Operation.DataSvcTransaction` where not otherwise specified. This should only be used where a transactionally-aware data source is being used.")]
        public bool? EventTransaction { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the access modifier for the generated `Manager` constructor.
        /// </summary>
        [JsonPropertyName("managerCtor")]
        [CodeGenProperty("Manager", Title = "The access modifier for the generated `Manager` constructor.", Options = ["Public", "Private", "Protected"],
            Description = "Defaults to `Public`.")]
        public string? ManagerCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.
        /// </summary>
        [JsonPropertyName("managerCtorParams")]
        [CodeGenPropertyCollection("Manager", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.", IsImportant = true,
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? ManagerCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the `Manager` extensions logic should be generated.
        /// </summary>
        [JsonPropertyName("managerExtensions")]
        [CodeGenProperty("Manager", Title = "Indicates whether the `Manager` extensions logic should be generated.", 
            Description = "This can be overridden using `Operation.ManagerExtensions`.")]
        public bool? ManagerExtensions { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET Type that will perform the validation.
        /// </summary>
        [JsonPropertyName("validator")]
        [CodeGenProperty("Manager", Title = "The name of the .NET implementing `Type` or interface `Type` that will perform the validation.", IsImportant = true,
            Description = "Only used for defaulting the `Create` and `Update` operation types (`Operation.Type`) where not specified explicitly.")]
        public string? Validator { get; set; }

        /// <summary>
        /// Indicates whether the `IIdentifierGenerator` should be used to generate the `Id` property on `Create`.
        /// </summary>
        [JsonPropertyName("identifierGenerator")]
        [CodeGenProperty("Manager", Title = "Indicates whether the `IIdentifierGenerator` should be used to generate the `Id` property where the operation types (`Operation.Type`) is `Create`.")]
        public bool? IdentifierGenerator { get; set; }

        /// <summary>
        /// Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.
        /// </summary>
        [JsonPropertyName("managerCleanUp")]
        [CodeGenProperty("Manager", Title = "Indicates whether a `Cleaner.Cleanup` is performed for the operation parameters within the Manager-layer.",
            Description = "This can be overridden within the `CodeGeneration` and `Operation`(s).")]
        public bool? ManagerCleanUp { get; set; }

        /// <summary>
        /// Gets or sets the `Validation` framework. 
        /// </summary>
        [JsonPropertyName("validationFramework")]
        [CodeGenProperty("Manager", Title = "The `Validation` framework to use for the entity-based validation.", Options = ["CoreEx", "FluentValidation"],
            Description = "Defaults to `CodeGeneration.ValidationFramework`. This can be overridden within the `Operation`(s) and `Parameter`(s).")]
        public string? ValidationFramework { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the <c>RoutePrefixAtttribute</c> for the corresponding entity Web API controller.
        /// </summary>
        [JsonPropertyName("webApiRoutePrefix")]
        [CodeGenProperty("WebApi", Title = "The `RoutePrefixAtttribute` for the corresponding entity Web API controller.", IsImportant = true,
            Description = "This is the base (prefix) `URI` for the entity and can be further extended when defining the underlying `Operation`(s). The `CodeGeneration.WebApiRoutePrefix` will be prepended where specified. " 
            + "Where not specified will automatically default to the pluralized `Name` (as lowercase).")]
        public string? WebApiRoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the authorize attribute value to be used for the corresponding entity Web API controller; generally either <c>Authorize</c> or <c>AllowAnonynous</c>.
        /// </summary>
        [JsonPropertyName("webApiAuthorize")]
        [CodeGenProperty("WebApi", Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally either `Authorize` or `AllowAnonymous`.", IsImportant = true,
            Description = "Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also.")]
        public string? WebApiAuthorize { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated Web API `Controller` constructor.
        /// </summary>
        [JsonPropertyName("webApiCtor")]
        [CodeGenProperty("WebApi", Title = "The access modifier for the generated Web API `Controller` constructor.", Options = ["Public", "Private", "Protected"],
            Description = "Defaults to `Public`.")]
        public string? WebApiCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.
        /// </summary>
        [JsonPropertyName("webApiCtorParams")]
        [CodeGenPropertyCollection("WebApi", Title = "The list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.", IsImportant = true,
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                "Where the `Type` matches an already inferred value it will be ignored.")]
        public List<string>? WebApiCtorParams { get; set; }

        /// <summary>
        /// Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`)` is automatically inferred.
        /// </summary>
        [JsonPropertyName("webApiAutoLocation")]
        [CodeGenProperty("WebApi", Title = "Indicates whether the HTTP Response Location Header route (`Operation.WebApiLocation`) is automatically inferred.",
            Description = "This will automatically set the `Operation.WebApiLocation` for an `Operation` named `Create` where there is a corresponding named `Get`. This is defaulted from the `CodeGen.WebApiAutoLocation`.")]
        public bool? WebApiAutoLocation { get; set; }

        /// <summary>
        /// Indicates whether the Web API is responsible for managing concurrency via auto-generated ETag.
        /// </summary>
        [JsonPropertyName("webApiConcurrency")]
        [CodeGenProperty("WebApi", Title = "Indicates whether the Web API is responsible for managing (simulating) concurrency via auto-generated ETag.",
            Description = "This provides an alternative where the underlying data source does not natively support optimistic concurrency (native support should always be leveraged as a priority). Where the `Operation.Type` is `Update` or `Patch`, the request ETag will " +
            "be matched against the response for a corresponding `Get` operation to verify no changes have been made prior to updating. For this to function correctly the .NET response Type for the `Get` must be the same as that returned from " +
            "the corresponding `Create`, `Update` and `Patch` (where applicable) as the generated ETag is a SHA256 hash of the resulting JSON. This defaults the `Operation.WebApiConcurrency`.")]
        public bool? WebApiConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the override for the corresponding `Get` method name (in the `XxxManager`) either where, the `Operation.Type` is `Update` and `WebApiConcurrency` is `true`, or the `Operation.Type` is `Patch`.
        /// </summary>
        [JsonPropertyName("webApiGetOperation")]
        [CodeGenProperty("WebApi", Title = "The corresponding `Get` method name (in the `XxxManager`) where the `Operation.Type` is `Update` and `SimulateConcurrency` is `true`.",
            Description = "Defaults to `Get`. Specify either just the method name (e.g. `OperationName`) or, interface and method name (e.g. `IXxxManager.OperationName`) to be invoked where in a different `YyyManager.OperationName`.")]
        public string? WebApiGetOperation { get; set; }

        /// <summary>
        /// The list of tags to add for the generated `WebApi`.
        /// </summary>
        [JsonPropertyName("webApiTags")]
        [CodeGenPropertyCollection("WebApi", Title = "The list of tags to add for the generated `WebApi` controller.")]
        public List<string>? WebApiTags { get; set; }

        #endregion

        #region Model

        /// <summary>
        /// Indicates whether a data <i>model</i> version of the entity should also be generated (output to <c>.\Business\Data\Model</c>).
        /// </summary>
        [JsonPropertyName("dataModel")]
        [CodeGenProperty("Model", Title = "Indicates whether a data `model` version of the entity should also be generated (output to `.\\Business\\Data\\Model`).",
            Description = "The model will be generated with `OmitEntityBase = true`. Any reference data properties will be defined using their `RefDataType` intrinsic `Type` versus their corresponding (actual) reference data `Type`.")]
        public bool? DataModel { get; set; }

        /// <summary>
        /// Gets or sets the override for the data <i>model</i> inherits.
        /// </summary>
        [JsonPropertyName("dataModelInherits")]
        [CodeGenProperty("Model", Title = "Overrides the default data `model` inherits value.")]
        public string? DataModelInherits { get; set; }

        #endregion

        #region Exclude

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Entity</c> class (<c>Xxx.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeEntity")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `Entity` class (`Xxx.cs`).", IsImportant = true)]
        public bool? ExcludeEntity { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of <b>all</b> <c>Operation</c> related code; excluding the <c>Entity</c> class.
        /// </summary>
        [JsonPropertyName("excludeAll")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class.", IsImportant = true,
            Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to exclude.")]
        public bool? ExcludeAll { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Data</c> interface (<c>IXxxData.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeIData")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `Data` interface (`IXxxData.cs`).")]
        public bool? ExcludeIData { get; set; }

        /// <summary>
        /// Gets or sets the option to exclude the generation of the <c>Data</c> class (<c>XxxData.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeData")]
        [CodeGenProperty("Exclude", Title = "The option to exclude the generation of the `Data` class (`XxxData.cs`).", Options = ["Include", "Exclude", "RequiresMapper"],
            Description = "Defaults to `Include` indicating _not_ to exclude. A value of `Exclude` indicates to exclude all output; alternatively, `RequiresMapper` indicates to at least output the corresponding `Mapper` class.")]
        public string? ExcludeData { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>DataSvc</c> interface (<c>IXxxDataSvc.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeIDataSvc")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `DataSvc` interface (`IXxxDataSvc.cs`).")]
        public bool? ExcludeIDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>DataSvc</c> class (<c>XxxDataSvc.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeDataSvc")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `DataSvc` class (`XxxDataSvc.cs`).")]
        public bool? ExcludeDataSvc { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Manager</c> interface (<c>IXxxManager.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeIManager")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `Manager` interface (`IXxxManager.cs`).")]
        public bool? ExcludeIManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the <c>Manager</c> class (<c>XxxManager.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeManager")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`).")]
        public bool? ExcludeManager { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the WebAPI <c>Controller</c> class (<c>XxxController.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeWebApi")]
        [CodeGenProperty("Exclude", Title = "The option to exclude the generation of the WebAPI `Controller` class (`XxxController.cs`).")]
        public bool? ExcludeWebApi { get; set; }

        /// <summary>
        /// The option to exclude the generation of the WebAPI <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeWebApiAgent")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the WebAPI consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeWebApiAgent { get; set; }

        /// <summary>
        /// Indicates whether to exclude the generation of the gRPC <c>Agent</c> class (<c>XxxAgent.cs</c>).
        /// </summary>
        [JsonPropertyName("excludeGrpcAgent")]
        [CodeGenProperty("Exclude", Title = "Indicates whether to exclude the generation of the gRPC consuming `Agent` class (`XxxAgent.cs`).")]
        public bool? ExcludeGrpcAgent { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the role (permission) used by the <c>ExecutionContext.IsInRole(role)</c> for each <c>Operation</c>.
        /// </summary>
        [JsonPropertyName("authRole")]
        [CodeGenProperty("Auth", Title = "The role (permission) used by the `ExecutionContext.IsInRole(role)` for each `Operation`.", IsImportant = true,
            Description = "Used where not overridden specifically for an `Operation`; i.e. acts as the default.")]
        public string? AuthRole { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Indicates whether gRPC support (more specifically service-side) is required for the Entity.
        /// </summary>
        [JsonPropertyName("grpc")]
        [CodeGenProperty("gRPC", Title = "Indicates whether gRPC support (more specifically service-side) is required for the Entity.", IsImportant = true,
            Description = "gRPC support is an explicit opt-in model (see `CodeGeneration.Grpc` configuration); therefore, each corresponding `Property` and `Operation` will also need to be opted-in specifically.")]
        public bool? Grpc { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="PropertyConfig"/> collection.
        /// </summary>
        [JsonPropertyName("properties")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Property` collection.")]
        public List<PropertyConfig>? Properties { get; set; }

        #endregion

        /// <summary>
        /// Gets the list of private properties to be implemented (that are not inherited).
        /// </summary>
        public List<PropertyConfig>? PrivateProperties => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && (x.RefDataMapping == null || !x.RefDataMapping.Value)).ToList();

        /// <summary>
        /// Gets the list of core properties to be implemented (that are not inherited).
        /// </summary>
        public List<PropertyConfig>? CoreProperties => Properties!.Where(x => (x.Inherited == null || !x.Inherited.Value) && !(x.InternalOnly == true && Root!.RuntimeEntityScope == "Common" && Root.IsDataModel == false) && (!Root!.IsDataModel || (Root.IsDataModel && x.DataModelIgnore != true))).ToList();

        /// <summary>
        /// Gets the list of properties that form the unique key; excluding inherited.
        /// </summary>
        public List<PropertyConfig> PrimaryKeyProperties => Properties!.Where(x => (x.PrimaryKey.HasValue && x.PrimaryKey.Value) && (x.Inherited == null || !x.Inherited.Value)).ToList();

        /// <summary>
        /// Gets the list of properties that form the unique key; including inherited.
        /// </summary>
        public List<PropertyConfig> PrimaryKeyPropertiesIncludeInherited => Properties!.Where(x => x.PrimaryKey.HasValue && x.PrimaryKey.Value).ToList();

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
        public List<PropertyConfig>? DatabaseMapperProperties => Properties!.Where(x => CompareNullOrValue(x.DatabaseIgnore, false) && x.Name != "ETag" && !x.IsChangeLog).ToList();

        /// <summary>
        /// Gets the list of properties that form the unique key; excluding inherited and specific exclusion for reference data inheritence.
        /// </summary>
        public List<PropertyConfig>? DatabasePrimaryKeyProperties => DatabaseMapperProperties!.Where(x => x.PrimaryKey.HasValue && x.PrimaryKey.Value).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? EntityFrameworkMapperProperties => Properties!.Where(x => !CompareValue(x.EntityFrameworkMapper, "Ignore")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? EntityFrameworkMapperIsSourceProperties => Properties!.Where(x => !CompareValue(x.EntityFrameworkMapper, "Ignore") && !CompareValue(x.EntityFrameworkMapper, "Flatten")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for entity framework mapping.
        /// </summary>
        public List<PropertyConfig>? EntityFrameworkMapperIsDestinationProperties => Properties!.Where(x => !CompareValue(x.EntityFrameworkMapper, "Ignore") && !CompareValue(x.EntityFrameworkMapper, "Expand")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for cosmos mapping.
        /// </summary>
        public List<PropertyConfig>? CosmosMapperProperties => Properties!.Where(x => !CompareValue(x.CosmosMapper, "Ignore")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for cosmos mapping.
        /// </summary>
        public List<PropertyConfig>? CosmosMapperIsSourceProperties => Properties!.Where(x => !CompareValue(x.CosmosMapper, "Ignore") && !CompareValue(x.CosmosMapper, "Flatten")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for cosmos mapping.
        /// </summary>
        public List<PropertyConfig>? CosmosMapperIsDestinationProperties => Properties!.Where(x => !CompareValue(x.CosmosMapper, "Ignore") && !CompareValue(x.CosmosMapper, "Expand")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for odata mapping.
        /// </summary>
        public List<PropertyConfig>? ODataMapperProperties => Properties!.Where(x => !CompareValue(x.ODataMapper, "Ignore")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for http agent mapping.
        /// </summary>
        public List<PropertyConfig>? HttpAgentMapperProperties => Properties!.Where(x => !CompareValue(x.HttpAgentMapper, "Ignore")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for http agent mapping.
        /// </summary>
        public List<PropertyConfig>? HttpAgentMapperIsSourceProperties => Properties!.Where(x => !CompareValue(x.HttpAgentMapper, "Ignore") && !CompareValue(x.HttpAgentMapper, "Flatten")).ToList();

        /// <summary>
        /// Gets the list of properties that are to be used for http agent mapping.
        /// </summary>
        public List<PropertyConfig>? HttpAgentMapperIsDestinationProperties => Properties!.Where(x => !CompareValue(x.HttpAgentMapper, "Ignore") && !CompareValue(x.HttpAgentMapper, "Expand")).ToList();

        /// <summary>
        /// Indicates where there is a <see cref="IChangeLog"/> property.
        /// </summary>
        public bool HasDatabaseChangeLogProperty => Properties!.Any(x => x.IsChangeLog && CompareNullOrValue(x.DatabaseIgnore, false));

        /// <summary>
        /// Indicates where there is a <see cref="IETag"/> property.
        /// </summary>
        public bool HasDatabaseETagProperty => Properties!.Any(x => x.Name == "ETag" && CompareNullOrValue(x.DatabaseIgnore, false));

        /// <summary>
        /// Indicates where there is a <see cref="IChangeLog"/> property.
        /// </summary>
        public bool HasEntityFrameworkChangeLogProperty => Properties!.Any(x => x.IsChangeLog && !CompareValue(x.EntityFrameworkMapper, "Ignore"));

        /// <summary>
        /// Indicates where there is a <see cref="IETag"/> property.
        /// </summary>
        public bool HasEntityFrameworkETagProperty => Properties!.Any(x => x.Name == "ETag" && !CompareValue(x.EntityFrameworkMapper, "Ignore"));

        /// <summary>
        /// Gets the list of properties that are to be used for gRPC.
        /// </summary>
        public List<PropertyConfig>? GrpcProperties => Properties!.Where(x => x.GrpcFieldNo.HasValue && x.GrpcFieldNo.Value > 0).ToList();

        /// <summary>
        /// Gets or sets the corresponding <see cref="OperationConfig"/> collection.
        /// </summary>
        [JsonPropertyName("operations")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Operation` collection.")]
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
        public bool HasManagerExtensions => Operations!.Any(x => x.ManagerExtensions == true);

        /// <summary>
        /// Indicates where there are any <see cref="OperationConfig.DataSvcExtensions"/>.
        /// </summary>
        public bool HasDataSvcExtensions => Operations!.Any(x => x.DataSvcExtensions == true);

        /// <summary>
        /// Indicates where there are any <see cref="OperationConfig.DataExtensions"/>.
        /// </summary>
        public bool HasDataExtensions => Operations!.Any(x => x.DataExtensions == true);

        /// <summary>
        /// Gets the Manager constructor parameters.
        /// </summary>
        public List<ParameterConfig> ManagerCtorParameters { get; } = [];

        /// <summary>
        /// Gets the DataSvc constructor parameters.
        /// </summary>
        public List<ParameterConfig> DataSvcCtorParameters { get; } = [];

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
        public List<ParameterConfig> DataCtorParameters { get; } = [];

        /// <summary>
        /// Gets the <see cref="DatabaseName"/> as a <see cref="ParameterConfig"/>.
        /// </summary>
        public ParameterConfig? DatabaseDataParameter { get; set; }

        /// <summary>
        /// Gets the <see cref="EntityFrameworkName"/> as a <see cref="ParameterConfig"/>.
        /// </summary>
        public ParameterConfig? EntityFrameworkDataParameter { get; set; }

        /// <summary>
        /// Gets the <see cref="CosmosName"/> as a <see cref="ParameterConfig"/>.
        /// </summary>
        public ParameterConfig? CosmosDataParameter { get; set; }

        /// <summary>
        /// Gets the EntityController <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? WebApiOperations => Operations!.Where(x => IsFalse(x.ExcludeWebApi)).ToList();

        /// <summary>
        /// Gets the EntityAgent <see cref="OperationConfig"/> collection.
        /// </summary>
        public List<OperationConfig>? WebApiAgentOperations => Operations!.Where(x => IsFalse(x.ExcludeWebApiAgent)).ToList();

        /// <summary>
        /// Gets the WebApi Constructor parameters.
        /// </summary>
        public List<ParameterConfig> WebApiCtorParameters { get; } = [];

        /// <summary>
        /// Gets or sets the corresponding <see cref="ConstConfig"/> collection.
        /// </summary>
        [JsonPropertyName("consts")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Consts` collection.")]
        public List<ConstConfig>? Consts { get; set; }

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => CodeGenConfig.GetSentenceText(StringConverter.ToComments(CodeGenConfig.GetFormattedText(Text, text => $"Represents the {text} entity.")));

        /// <summary>
        /// Gets the formatted model text.
        /// </summary>
        public string? ModelSummaryText => CodeGenConfig.GetSentenceText(StringConverter.ToComments(CodeGenConfig.GetFormattedText(Text, text => $"Represents the {text} model.")));

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
        public string? EntityNameSeeComments => IsTrue(ExcludeEntity) ? $"<b>{Name}</b>" : StringConverter.ToSeeComments(Name);

        /// <summary>
        /// Indicates whether the extended inherits logic is required.
        /// </summary>
        public bool ExtendedInherits { get; set; }

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
        /// Indicates whether the entity has an inferred identifier column.
        /// </summary>
        public bool HasIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the computed entity implements.
        /// </summary>
        public string? EntityImplements { get; set; }

        /// <summary>
        /// Gets or sets the computed common implements.
        /// </summary>
        public string? CommonImplements { get; set; }

        /// <summary>
        /// Gets or sets the computed model implements.
        /// </summary>
        public string? ModelImplements { get; set; }

        /// <summary>
        /// Indicates whether any of the operations use validators.
        /// </summary>
        public bool UsesValidators => Validator != null || Operations!.Any(x => x.Validator != null || x.Parameters!.Any(y => y.Validator != null));

        /// <summary>
        /// Indicates whether at least one operation needs a Manager.
        /// </summary>
        public bool RequiresManager => !(CompareValue(ExcludeManager, true) && CompareValue(ExcludeIManager, true));

        /// <summary>
        /// Indicates whether at least one operation needs a DataSvc.
        /// </summary>
        public bool RequiresDataSvc => !(CompareValue(ExcludeDataSvc, true) && CompareValue(ExcludeIDataSvc, true)) || Operations!.Any(x => CompareNullOrValue(x.ManagerCustom, false));

        /// <summary>
        /// Indicates whether at least one operation needs a Data.
        /// </summary>
        public bool RequiresData => (CompareValue(ExcludeData, "Exclude") && CompareValue(ExcludeIData, true)) || Operations!.Any(x => CompareNullOrValue(x.DataSvcCustom, false));

        /// <summary>
        /// Indicates whether any of the operations will raise an event within the DataSvc-layer. 
        /// </summary>
        public bool SupportsDataSvcEvents => Operations!.Any(x => x.Events.Count > 0 && x.EventPublish == "DataSvc");

        /// <summary>
        /// Indicates whether any of the operations will raise an event within the Data-layer.
        /// </summary>
        public bool SupportsDataEvents => Operations!.Any(x => x.Events.Count > 0 && x.EventPublish == "Data");

        /// <summary>
        /// Indicates whether auto-implementing 'Database'.
        /// </summary>
        public bool UsesDatabase => AutoImplement == "Database" || Operations!.Any(x => x.AutoImplement == "Database");

        /// <summary>
        /// Indicates whether auto-implementing 'EntityFramework'.
        /// </summary>
        public bool UsesEntityFramework => AutoImplement == "EntityFramework" || EntityFrameworkModel != null || Operations!.Any(x => x.AutoImplement == "EntityFramework");

        /// <summary>
        /// Indicates whether auto-implementing 'Cosmos'.
        /// </summary>
        public bool UsesCosmos => AutoImplement == "Cosmos" || CosmosModel != null || Operations!.Any(x => x.AutoImplement == "Cosmos");

        /// <summary>
        /// Indicates whether auto-implementing 'OData'.
        /// </summary>
        public bool UsesOData => AutoImplement == "OData" || ODataModel != null || Operations!.Any(x => x.AutoImplement == "OData");

        /// <summary>
        /// Indicates whether auto-implementing 'HttpAgent'.
        /// </summary>
        public bool UsesHttpAgent => AutoImplement == "HttpAgent" || HttpAgentModel != null || Operations!.Any(x => x.AutoImplement == "HttpAgent");

        /// <summary>
        /// Indicates whether the data extensions section is required.
        /// </summary>
        public bool DataExtensionsRequired => HasDataExtensions || UsesCosmos || DataOperations!.Any(x => x.Type == "GetColl");

        /// <summary>
        /// Gets the reference data qualified Entity name.
        /// </summary>
        public string RefDataQualifiedEntityCollectionName => RefDataQualifiedEntityName + "Collection";

        /// <summary>
        /// Gets the reference data qualified Entity name.
        /// </summary>
        public string RefDataQualifiedEntityName => string.IsNullOrEmpty(RefDataType) ? Name! : $"RefDataNamespace.{Name}";

        /// <summary>
        /// Gets the reference data fully qualified stored procedure name.
        /// </summary>
        public string RefDataFullyQualifiedStoredProcedureName => Root!.DatabaseProvider switch
        {
            "MySQL" => $"`{RefDataStoredProcedureName}`",
            _ => $"[{DatabaseSchema}].[{RefDataStoredProcedureName}]"
        };

        /// <summary>
        /// Indicates whether the Manager needs a DataSvc using statement.
        /// </summary>
        public bool ManagerNeedsUsingDataSvc => Operations!.Any(x => x.ManagerCustom == null || x.ManagerCustom == false);

        /// <summary>
        /// Indicates whether the DataSvc needs a DataSvc using statement.
        /// </summary>
        public bool DataSvcNeedsUsingData => Operations!.Any(x => x.DataSvcCustom == null || x.DataSvcCustom == false);

        /// <summary>
        /// Gets the Cosmos PartitionKey as C# code.
        /// </summary>
        public string? CosmosPartitionKeyCode => CosmosPartitionKey == null ? null : (CosmosPartitionKey!.StartsWith("PartitionKey.", StringComparison.InvariantCulture) ? $"Mac.{CosmosPartitionKey}" : $"new Mac.PartitionKey({CosmosPartitionKey})");

        /// <summary>
        /// Indicates whether a using statement has been generated (set by codegen template).
        /// </summary>
        public bool HasUsingStatement { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            RefDataType = DefaultWhereNull(RefDataType, () => Parent!.RefDataType);
            if (!string.IsNullOrEmpty(RefDataType) && CompareValue(OmitEntityBase, true))
                throw new CodeGenException(this, nameof(OmitEntityBase), $"An {nameof(OmitEntityBase)} is not allowed where a {nameof(RefDataType)} has been specified.");

            Text = StringConverter.ToComments(DefaultWhereNull(Text, () =>
            {
                // Replace any 'Args' with 'Arguments'; where more than one word, and first word is an entity as well, then use that as the moustache entity name; i.e. 'PersonAddress' becomes '{{Person}} Address'.
                var sc = StringConverter.ToSentenceCase(Name)!;
                var words = sc.Split(' ').Select(x => x.Replace("Args", "Arguments")).ToArray();
                return words.Length > 1 && Parent!.Entities!.Any(x => x.Name == words[0]) ? string.Join(" ", new string[] { "{{" + words[0] + "}}" }.Concat(words[1..])) : string.Join(" ", words);
            }));

            FileName = DefaultWhereNull(FileName, () => Name);
            PrivateName = DefaultWhereNull(PrivateName, () => StringConverter.ToPrivateCase(Name));
            ArgumentName = DefaultWhereNull(ArgumentName, () => StringConverter.ToCamelCase(Name));
            WithResult = DefaultWhereNull(WithResult, () => Parent!.WithResult);
            ConstType = DefaultWhereNull(ConstType, () => "string");
            RefDataText = DefaultWhereNull(RefDataText, () => Parent!.RefDataText);
            RefDataSortOrder = DefaultWhereNull(RefDataSortOrder, () => "SortOrder");
            ImplementsAutoInfer = DefaultWhereNull(ImplementsAutoInfer, () => true);
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => Parent!.JsonSerializer);
            AutoImplement = DefaultWhereNull(AutoImplement, () => RefDataType is not null || EntityFrameworkModel is not null || CosmosModel is not null || ODataModel is not null || HttpAgentModel is not null ? Parent!.AutoImplement : "None");
            DataCtor = DefaultWhereNull(DataCtor, () => "Public");
            DatabaseName = DefaultWhereNull(DatabaseName, () => Parent!.DatabaseName);
            DatabaseSchema = DefaultWhereNull(DatabaseSchema, () => Parent!.DatabaseSchema);
            DatabaseMapperEx = DefaultWhereNull(DatabaseMapperEx, () => Parent!.DatabaseMapperEx);
            EntityFrameworkName = DefaultWhereNull(EntityFrameworkName, () => Parent!.EntityFrameworkName);
            CosmosName = DefaultWhereNull(CosmosName, () => Parent!.CosmosName);
            ODataName = DefaultWhereNull(ODataName, () => Parent!.ODataName);
            HttpAgentName = DefaultWhereNull(HttpAgentName, () => Parent!.HttpAgentName);
            DataSvcCaching = DefaultWhereNull(DataSvcCaching, () => true);
            DataSvcCtor = DefaultWhereNull(DataSvcCtor, () => "Public");
            EventSource = DefaultWhereNull(EventSource, () => $"{Name!.ToLowerInvariant()}/{{$key}}");
            EventPublish = DefaultWhereNull(EventPublish, () => Parent!.EventPublish);
            EventTransaction = DefaultWhereNull(EventTransaction, () => Parent!.EventTransaction);
            ManagerCtor = DefaultWhereNull(ManagerCtor, () => "Public");
            ManagerCleanUp = DefaultWhereNull(ManagerCleanUp, () => Parent!.ManagerCleanUp);
            ValidationFramework = DefaultWhereNull(ValidationFramework, () => Parent!.ValidationFramework);
            WebApiAuthorize = DefaultWhereNull(WebApiAuthorize, () => Parent!.WebApiAuthorize);
            WebApiCtor = DefaultWhereNull(WebApiCtor, () => "Public");
            WebApiAutoLocation = DefaultWhereNull(WebApiAutoLocation, () => Parent!.WebApiAutoLocation);
            WebApiConcurrency = DefaultWhereNull(WebApiConcurrency, () => false);
            RefDataStoredProcedureName = DefaultWhereNull(RefDataStoredProcedureName, () => $"sp{Name}GetAll");
            RefDataIdDataName = DefaultWhereNull(RefDataIdDataName, () => AutoImplement == "EntityFramework" || AutoImplement == "Database" ? $"{Name}Id" : "Id");
            RefDataCodeDataName = DefaultWhereNull(RefDataCodeDataName, () => Root!.RefDataCodeDataName);
            RefDataTextDataName = DefaultWhereNull(RefDataTextDataName, () => Root!.RefDataTextDataName);
            RefDataIsActiveDataName = DefaultWhereNull(RefDataIsActiveDataName, () => Root!.RefDataIsActiveDataName);
            RefDataSortOrderDataName = DefaultWhereNull(RefDataSortOrderDataName, () => Root!.RefDataSortOrderDataName);
            RefDataETagDataName = DefaultWhereNull(RefDataETagDataName, () => Root!.RefDataETagDataName == "*" ? (AutoImplement == "EntityFramework" || AutoImplement == "Database" ? $"RowVersion" : "ETag") : Root!.RefDataETagDataName);
            Collection = DefaultWhereNull(Collection, () => Parent!.RefDataType is not null);

            WebApiRoutePrefix = DefaultWhereNull(WebApiRoutePrefix, () => StringConverter.ToPlural(Name).ToLowerInvariant());

            if (RefDataType is not null && !string.IsNullOrEmpty(Parent!.WebApiRoutePrefix))
                WebApiRoutePrefix = string.IsNullOrEmpty(WebApiRoutePrefix) ? Parent!.WebApiRoutePrefix :
                    $"{(Parent!.WebApiRoutePrefix.EndsWith('/') ? Parent!.WebApiRoutePrefix[..^1] : Parent!.WebApiRoutePrefix)}{(string.IsNullOrEmpty(WebApiRoutePrefix) ? "" : "/")}{(WebApiRoutePrefix.StartsWith('/') ? WebApiRoutePrefix[1..] : WebApiRoutePrefix)}";

            if (IsTrue(DataModel))
            {
                switch (AutoImplement)
                {
                    case "EntityFramework":
                        EntityFrameworkModel = DefaultWhereNull(EntityFrameworkModel, () => $"Model.{Name}");
                        break;

                    case "Cosmos":
                        CosmosModel = DefaultWhereNull(CosmosModel, () => $"Model.{Name}");
                        break;

                    case "OData":
                        ODataModel = DefaultWhereNull(ODataModel, () => $"Model.{Name}");
                        break;

                    case "HttpAgent":
                        HttpAgentModel = DefaultWhereNull(HttpAgentModel, () => $"Model.{Name}");
                        break;
                }
            }

            HttpAgentReturnModel = DefaultWhereNull(HttpAgentReturnModel, () => HttpAgentModel);

            // Ensure the WebApiTags are set correctly.
            if (WebApiTags is not null && WebApiTags.Count != 0)
            {
                var list = new List<string>();
                foreach (var tag in WebApiTags)
                {
                    if (tag == "^")
                        list.AddRange(Parent!.WebApiTags!);
                    else
                        list.Add(tag);
                }

                WebApiTags = list;
            }
            else
                WebApiTags ??= [];

            InferInherits();
            Consts = await PrepareCollectionAsync(Consts).ConfigureAwait(false);
            await PreparePropertiesAsync().ConfigureAwait(false);
            await PrepareOperationsAsync().ConfigureAwait(false);
            InferImplements();
            await PrepareConstructorsAsync().ConfigureAwait(false);

            ExcludeEntity = DefaultWhereNull(ExcludeEntity, () => false);
            ExcludeAll = DefaultWhereNull(ExcludeAll, () => Operations!.Count == 0);
            ExcludeIData = DefaultWhereNull(ExcludeIData, () => CompareValue(ExcludeAll, true));
            ExcludeData = DefaultWhereNull(ExcludeData, () => CompareValue(ExcludeAll, true) ? (AutoImplement == "None" || RefDataType is not null ? "Exclude" : "RequiresMapper") : "Include");
            ExcludeIDataSvc = DefaultWhereNull(ExcludeIDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeDataSvc = DefaultWhereNull(ExcludeDataSvc, () => CompareValue(ExcludeAll, true));
            ExcludeIManager = DefaultWhereNull(ExcludeIManager, () => CompareValue(ExcludeAll, true));
            ExcludeManager = DefaultWhereNull(ExcludeManager, () => CompareValue(ExcludeAll, true));
            ExcludeWebApi = DefaultWhereNull(ExcludeWebApi, () => CompareValue(ExcludeAll, true));
            ExcludeWebApiAgent = DefaultWhereNull(ExcludeWebApiAgent, () => CompareValue(ExcludeAll, true));
            ExcludeGrpcAgent = DefaultWhereNull(ExcludeGrpcAgent, () => CompareValue(ExcludeAll, true));

            CheckDeprecatedProperties();
        }

        /// <summary>
        /// Infers the <see cref="Inherits"/>, <see cref="CollectionInherits"/> and <see cref="CollectionResultInherits"/> values.
        /// </summary>
        private void InferInherits()
        {
            ExtendedInherits = Inherits != null && CompareNullOrValue(OmitEntityBase, false);
            if (RefDataType != null && Properties != null && Properties.Count > 0)
                ExtendedInherits = true;

            EntityInherits = Inherits;
            EntityInherits = DefaultWhereNull(EntityInherits, () => RefDataType switch
            {
                "int" => $"ReferenceDataBaseEx<int, {Name}>",
                "long" => $"ReferenceDataBaseEx<long, {Name}>",
                "Guid" => $"ReferenceDataBaseEx<Guid, {Name}>",
                "string" => $"ReferenceDataBaseEx<string?, {Name}>",
                _ => CompareNullOrValue(OmitEntityBase, false) ? $"EntityBase" : null
            });

            DataModelInherits = DefaultWhereNull(DataModelInherits, () => RefDataType switch
            {
                "int" => "ReferenceDataBase<int>",
                "long" => "ReferenceDataBase<long>",
                "Guid" => "ReferenceDataBase<Guid>",
                "string" => "ReferenceDataBase<string?>",
                _ => EntityInherits == "EntityBase" ? null : EntityInherits
            });

            ModelInherits = RefDataType switch
            {
                "int" => "ReferenceDataBase<int>",
                "long" => "ReferenceDataBase<long>",
                "Guid" => "ReferenceDataBase<Guid>",
                "string" => "ReferenceDataBase<string?>",
                _ => EntityInherits == "EntityBase" ? null : EntityInherits
            };

            CollectionType = DefaultWhereNull(CollectionType, () => "Standard");
            if (!string.IsNullOrEmpty(EntityCollectionInherits))
                CollectionType = "Overridden";

            EntityCollectionInherits = CompareValue(OmitEntityBase, true) ? (CollectionType == "Dictionary" ? $"Dictionary<string, {EntityName}>" : $"List<{EntityName}>") : CollectionInherits;
            EntityCollectionInherits = DefaultWhereNull(EntityCollectionInherits, () =>
            {
                if (RefDataType == null)
                    return CollectionType switch
                    {
                        "Keyed" => $"EntityKeyBaseCollection<{EntityName}, {EntityCollectionName}>",
                        "Dictionary" => $"EntityBaseDictionary<{EntityName}, {EntityCollectionName}>",
                        _ => $"EntityBaseCollection<{EntityName}, {EntityCollectionName}>"
                    };
                else
                    return $"ReferenceDataCollectionBase<{RefDataType}{(RefDataType == "string" ? "?" : "")}, {EntityName}, {EntityCollectionName}>";
            });

            EntityCollectionResultInherits = CompareValue(OmitEntityBase, true) ? $"CollectionResult<{EntityCollectionName}, {EntityName}>" : CollectionResultInherits;
            EntityCollectionResultInherits = DefaultWhereNull(CollectionResultInherits, () => $"EntityCollectionResult<{EntityCollectionName}, {EntityName}, {EntityCollectionResultName}>");

            CollectionInherits = DefaultWhereNull(CollectionInherits, () => CollectionType == "Dictionary" ? $"Dictionary<string, {EntityName}>" : $"List<{EntityName}>");
            CollectionResultInherits = DefaultWhereNull(CollectionResultInherits, () => $"CollectionResult<{EntityCollectionName}, {EntityName}>");
        }

        /// <summary>
        /// Prepares the properties.
        /// </summary>
        private async Task PreparePropertiesAsync()
        {
            Properties ??= [];

            if (RefDataType != null)
            {
                var i = 0;
                AddInheritedProperty("Id", ref i, () => new PropertyConfig { Text = $"{{{{{Name}}}}} identifier", Type = RefDataType, PrimaryKey = true, DataAutoGenerated = true, DataName = RefDataIdDataName == "Id" ? null : RefDataIdDataName });
                AddInheritedProperty("Code", ref i, () => new PropertyConfig { Type = "string", DataName = RefDataCodeDataName == "Code" ? null : RefDataCodeDataName });
                AddInheritedProperty("Text", ref i, () => new PropertyConfig { Type = "string", DataName = RefDataTextDataName == "Text" ? null : RefDataTextDataName });

                if (!string.IsNullOrEmpty(RefDataIsActiveDataName))
                    AddInheritedProperty("IsActive", ref i, () => new PropertyConfig { Type = "bool", DataName = RefDataIsActiveDataName == "IsActive" ? null : RefDataIsActiveDataName });

                if (!string.IsNullOrEmpty(RefDataSortOrderDataName))
                    AddInheritedProperty("SortOrder", ref i, () => new PropertyConfig { Type = "int", DataName = RefDataSortOrderDataName == "SortOrder" ? null : RefDataSortOrderDataName });

                if (!string.IsNullOrEmpty(RefDataETagDataName))
                    AddInheritedProperty("ETag", ref i, () => new PropertyConfig { Type = "string", DataName = RefDataETagDataName == "ETag" ? null : RefDataETagDataName });
            }

            foreach (var property in Properties)
            {
                await property.PrepareAsync(Root!, this).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds the named (inherited) property if it does not already exist.
        /// </summary>
        private void AddInheritedProperty(string name, ref int index, Func<PropertyConfig> func)
        {
            if (Properties!.Any(x => x.Name == name))
                return;

            var p = func();
            p.Name = name;
            p.Inherited = true;
            Properties!.Insert(index++, p);
        }

        /// <summary>
        /// Prepares the Operations.
        /// </summary>
        private async Task PrepareOperationsAsync()
        {
            Operations ??= [];

            // Enables/supports existing 'crud: true'; will error on deprecation at some point in the future.
            if (!string.IsNullOrEmpty(Behavior))
            {
                foreach (var c in Behavior.ToLowerInvariant())
                {
                    switch (c)
                    {
                        case 'c': Create = DefaultWhereNull(Create, () => true); break;
                        case 'r': Get = DefaultWhereNull(Get, () => true); break;
                        case 'g': Get = DefaultWhereNull(Get, () => true); break;
                        case 'u': Update = DefaultWhereNull(Update, () => true); break;
                        case 'p': Patch = DefaultWhereNull(Patch, () => true); break;
                        case 'd': Delete = DefaultWhereNull(Delete, () => true); break;
                        case 'b': GetByArgs = DefaultWhereNull(GetByArgs, () => true); break;
                        case 'a': GetAll = DefaultWhereNull(GetAll, () => true); break;
                        default: throw new CodeGenException(this, nameof(Behavior), $"The '{c}' character does not map to a supported underlying behavior (operation) type; valid values are: 'CRGUPDBA' (case-insensitive).");
                    }
                }
            }

            // Add in selected operations where applicable (in reverse order in which output).
            if (CompareValue(Delete, true) && !Operations.Any(x => x.Name == "Delete"))
                Operations.Insert(0, new OperationConfig { Name = "Delete", Type = "Delete", PrimaryKey = true });

            if (CompareValue(Patch, true) && !Operations.Any(x => x.Name == "Patch"))
                Operations.Insert(0, new OperationConfig { Name = "Patch", Type = "Patch", PrimaryKey = true });

            if (CompareValue(Update, true) && !Operations.Any(x => x.Name == "Update"))
                Operations.Insert(0, new OperationConfig { Name = "Update", Type = "Update", PrimaryKey = true });

            if (CompareValue(Create, true) && !Operations.Any(x => x.Name == "Create"))
                Operations.Insert(0, new OperationConfig { Name = "Create", Type = "Create", WebApiRoute = "" });

            if (CompareValue(Get, true) && !Operations.Any(x => x.Name == "Get"))
                Operations.Insert(0, new OperationConfig { Name = "Get", Type = "Get", PrimaryKey = true });

            if (CompareValue(GetByArgs, true) && !Operations.Any(x => x.Name == "GetByArgs"))
            {
                var at = $"{Name}Args";
                Operations.Insert(0, new OperationConfig { Name = "GetByArgs", Type = "GetColl", Paging = true, WebApiRoute = "", Parameters = [new ParameterConfig() { Name = "Args", Type = at, Validator = $"{Name}ArgsValidator" }] });
                if (!Parent!.Entities!.Any(x => x.Name == at))
                    Root!.CodeGenArgs?.Logger?.LogWarning("{Warning}", $"Warning: Config [{BuildFullyQualifiedName(nameof(GetByArgs))}] references entity '{at}' that has not been defined; this is needed to complete implementation.");
            }

            if (CompareValue(GetAll, true) && !Operations.Any(x => x.Name == "GetAll"))
                Operations.Insert(0, new OperationConfig { Name = "GetAll", Type = "GetColl", WebApiRoute = GetByArgs is not null && GetByArgs.Value ? "all" : "" });

            // Prepare each operations.
            foreach (var operation in Operations)
            {
                await operation.PrepareAsync(Root!, this);
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
            var commonImplements = new List<string>();
            var modelImplements = new List<string>();
            var i = 0;
            var c = 0;
            var m = 0;

            if (CompareValue(ImplementsAutoInfer, true))
            {
                if (Implements != null)
                {
                    foreach (var str in Implements!.Split(",", StringSplitOptions.RemoveEmptyEntries))
                    {
                        var txt = str?.Trim();
                        if (!string.IsNullOrEmpty(txt))
                        {
                            implements.Add(txt!);
                            commonImplements.Add(txt!);
                        }
                    }
                }

                if (PrimaryKeyProperties.Count == 1)
                {
                    var id = PrimaryKeyProperties!.FirstOrDefault(x => x.Name == "Id" && CompareNullOrValue(x.Inherited, false));
                    if (id != null)
                    {
                        var iid = id.Type switch
                        {
                            "Guid" => "IIdentifier<Guid>",
                            "int" => "IIdentifier<int>",
                            "long" => "IIdentifier<long>",
                            "string" => "IIdentifier<string>",
                            _ => "???",
                        };

                        if (iid != "???")
                        {
                            implements.Insert(i++, iid);
                            commonImplements.Insert(c++, iid);
                            modelImplements.Insert(m++, iid);
                            HasIdentifier = true;
                        }
                    }
                }
                else if (PrimaryKeyPropertiesIncludeInherited.Count == 1)
                {
                    var id = PrimaryKeyPropertiesIncludeInherited!.FirstOrDefault(x => x.Name == "Id");
                    if (id != null)
                        HasIdentifier = true;
                }
            }

            if (!HasIdentifier && Properties!.Any(x => CompareValue(x.PrimaryKey, true) && CompareNullOrValue(x.Inherited, false)))
            {
                implements.Insert(i++, "IPrimaryKey");
                commonImplements.Insert(c++, "IPrimaryKey");
                modelImplements.Insert(m++, "IPrimaryKey");
            }

            if (Properties!.Any(x => CompareValue(x.PartitionKey, true) && CompareNullOrValue(x.Inherited, false)))
            {
                implements.Insert(i++, "IPartitionKey");
                modelImplements.Insert(m++, "IPartitionKey");
            }

            if (CompareValue(ImplementsAutoInfer, true))
            {
                if (Properties!.Any(x => x.Name == "ETag" && x.Type == "string" && CompareNullOrValue(x.Inherited, false)))
                {
                    implements.Insert(i++, "IETag");
                    if (Properties!.Any(x => x.Name == "ETag" && x.Type == "string" && CompareNullOrValue(x.Inherited, false) && CompareNullOrValue(x.DataModelIgnore, false)))
                        modelImplements.Insert(m++, "IETag");

                    if (Properties!.Any(x => x.Name == "ETag" && x.Type == "string" && CompareNullOrValue(x.Inherited, false) && CompareNullOrValue(x.InternalOnly, false)))
                        commonImplements.Insert(c++, "IETag");
                }

                if (Properties!.Any(x => x.IsChangeLog && CompareNullOrValue(x.Inherited, false)))
                {
                    implements.Insert(i++, "IChangeLogEx");
                    if (Properties!.Any(x => x.IsChangeLog && CompareNullOrValue(x.Inherited, false) && CompareNullOrValue(x.DataModelIgnore, false)))
                        modelImplements.Insert(m++, "IChangeLog");

                    if (Properties!.Any(x => x.IsChangeLog && CompareNullOrValue(x.Inherited, false) && CompareNullOrValue(x.InternalOnly, false)))
                        commonImplements.Insert(c++, "IChangeLog");
                }

                if (Properties!.Any(x => x.Name == "TenantId" && x.Type == "string" && CompareNullOrValue(x.Inherited, false)))
                    modelImplements.Insert(m++, "ITenantId");

                if (Properties!.Any(x => x.Name == "IsDeleted" && x.Type == "bool" && CompareNullOrValue(x.Nullable, true) && CompareNullOrValue(x.Inherited, false)))
                    modelImplements.Insert(m++, "ILogicallyDeleted");
            }

            EntityImplements = implements.Count == 0 ? null : string.Join(", ", implements.GroupBy(x => x).Select(y => y.First()).ToArray());
            CommonImplements = commonImplements.Count == 0 ? null : string.Join(", ", commonImplements.GroupBy(x => x).Select(y => y.First()).ToArray());
            ModelImplements = modelImplements.Count == 0 ? null : string.Join(", ", modelImplements.GroupBy(x => x).Select(y => y.First()).ToArray());
        }

        /// <summary>
        /// Prepare the constructors.
        /// </summary>
        private async Task PrepareConstructorsAsync()
        {
            // Manager constructors.
            var oc = new OperationConfig { Name = "<internal>" };
            await oc.PrepareAsync(Root!, this).ConfigureAwait(false);

            // Manager constructors.
            if (RequiresDataSvc)
                ManagerCtorParameters.Add(new ParameterConfig { Name = "DataService", Type = $"I{Name}DataSvc", Text = $"{{{{I{Name}DataSvc}}}}" });

            AddConfiguredParameters(ManagerCtorParams, ManagerCtorParameters);

            // Include `IIdentifierGenerator` where has identifier property and there is at least one create operation.
            if (CompareValue(IdentifierGenerator, true) && HasIdentifier && Operations!.Any(x => x.Type! == "Create"))
                ManagerCtorParameters.Add(new ParameterConfig { Name = "identifierGenerator", Type = "IIdentifierGenerator", Text = $"{{{{IIdentifierGenerator}}}}" });

            foreach (var ctor in ManagerCtorParameters)
            {
                await ctor.PrepareAsync(Root!, oc).ConfigureAwait(false);
            }

            // DataSvc constructors.
            if (RequiresData)
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "Data", Type = $"I{Name}Data", Text = $"{{{{I{Name}Data}}}}" });

            if (SupportsDataSvcEvents)
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "Events", Type = $"IEventPublisher", Text = "{{IEventPublisher}}" });

            if (CompareValue(DataSvcCaching, true) && Operations!.Any(x => x.SupportsCaching))
                DataSvcCtorParameters.Add(new ParameterConfig { Name = "Cache", Type = "IRequestCache", Text = "{{IRequestCache}}" });
            else 
                DataSvcCaching = false;

            AddConfiguredParameters(DataSvcCtorParams, DataSvcCtorParameters);
            foreach (var ctor in DataSvcCtorParameters)
            {
                await ctor.PrepareAsync(Root!, oc).ConfigureAwait(false);
            }

            // Data constructors.
            if (UsesDatabase)
                DataCtorParameters.Add(DatabaseDataParameter = new ParameterConfig { Name = "Db", Type = DatabaseName, Text = $"{{{{{DatabaseName}}}}}" });

            if (UsesEntityFramework)
                DataCtorParameters.Add(EntityFrameworkDataParameter = new ParameterConfig { Name = "Ef", Type = EntityFrameworkName, Text = $"{{{{{EntityFrameworkName}}}}}" });

            if (UsesCosmos)
                DataCtorParameters.Add(CosmosDataParameter = new ParameterConfig { Name = "Cosmos", Type = CosmosName, Text = $"{{{{{CosmosName}}}}}" });

            if (UsesOData)
                DataCtorParameters.Add(new ParameterConfig { Name = "OData", Type = ODataName, Text = $"{{{{{ODataName}}}}}" });

            if (UsesHttpAgent)
                DataCtorParameters.Add(new ParameterConfig { Name = "HttpAgent", Type = HttpAgentName, Text = $"{{{{{HttpAgentName}}}}}" });

            if (SupportsDataEvents)
                DataCtorParameters.Add(new ParameterConfig { Name = "Events", Type = $"IEventPublisher", Text = "{{IEventPublisher}}" });

            AddConfiguredParameters(DataCtorParams, DataCtorParameters);
            foreach (var ctor in DataCtorParameters)
            {
                await ctor.PrepareAsync(Root!, oc).ConfigureAwait(false);
            }

            // WebAPI constructors.
            if (RequiresManager)
                WebApiCtorParameters.Insert(0, new ParameterConfig { Name = "Manager", Type = $"I{Name}Manager", Text = $"{{{{I{Name}Manager}}}}" });

            WebApiCtorParameters.Insert(0, new ParameterConfig { Name = "WebApi", Type = RefDataType is null ? "WebApi" : "ReferenceDataContentWebApi", Text = $"{{{{WebApi}}}}" });
            AddConfiguredParameters(WebApiCtorParams, WebApiCtorParameters);
            foreach (var ctor in WebApiCtorParameters)
            {
                await ctor.PrepareAsync(Root!, oc).ConfigureAwait(false);
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
                pc.Name = StringConverter.ToPascalCase(parts[1]);

            return pc;
        }

        /// <summary>
        /// Check for any deprecated properties and error.
        /// </summary>
        private void CheckDeprecatedProperties() => CodeGenConfig.WarnWhereDeprecated(Root!, this,
            ("entityScope", null, false),
            ("entityUsing", null, false),
            ("collectionKeyed", " Use the new 'collectionType' property with a value of 'Keyed' to achieve same functionality.", true),
            ("refDataStringFormat", null, false),
            ("entityFrameworkMapperInheritsFrom", null, false),
            ("cosmosMapperInheritsFrom", null, false),
            ("odataMapperInheritsFrom", null, false),
            ("eventOutbox", null, false),
            ("eventSubjectFormat", null, false),
            ("eventCasing", null, false),
            ("iValidator", null, false),
            ("crud", " Use the new 'behavior' property with a value of 'crupd' to achieve same functionality.", true),
            ("databaseCustomerMapper", "Use 'databaseCustomMappper' instead; this was a spelling mistake that has now been corrected.", true));
    }
}