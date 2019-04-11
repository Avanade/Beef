# `Entity`  element (entity-driven)

The **`Entity`** element is the primary configuration for driving the entity-driven code generation.

An example for a _standard_ entity is as follows:

```xml
<Entity Name="Person" Text="Person" Implements="IETag, IChangeLog" Collection="true" CollectionResult="true" WebApiRoutePrefix="api/v1/demo/persons" AutoImplement="Database"/>
```

<br>

An example for a _Reference Data_ entity is as follows:

```xml
<Entity Name="Gender" RefDataType="int" Collection="true" WebApiRoutePrefix="api/v1/demo/ref/genders" AutoImplement="Database" DatabaseSchema="Ref">
```

<br>

## Attributes

The **`Entity`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** entity attributes: 

Attribute | Description
-|-
**`Name`** | Unique entity name. This is mandatory.
**`Text`** | Text to be used in comments. Defaults to the `Name` converted to sentence case. To create a `<see cref="XXX"/>` use `{{XXX}}` shorthand.
`FileName` | Unique entity file name overriding the `Name` where the file name should be different.
`EntityScope` | Determines whether the entity is considered `Common` (default) or should be scoped to the `Business` namespace/assembly only (i.e. not externally visible).
`PrivateName` | Name to be used for private fields; e.g. 'FirstName' reformatted as '_firstName'. Defaults from `Name`.
`ArgumentName` | Name to be used for argument parameters; e.q. 'FirstName' reformatted as 'firstName'. Defaults from `Name`).
`ConstType` | The .NET data type to be used for the `Const` values. Options are: `int`, `Guid` and `string`.
`IsInitialOverride` | Indicates whether to override the entity `IsInitial` property result with `true` or `false`; otherwise, `null` to check each property to determine. Defaults to `null`.

<br>

### Reference data attributes

The following represent the attibutes where specifically outputing a **Reference Data** entity:

Attribute | Description
---|---
**`RefDataType`** | Required to identify an entity as being Reference Data. Specifies the underlying .NET Type used for the Reference Data identifier. Options are `int` or `Guid`.
`RefDataSortOrder` | Identifies the default sort order for the underlying Reference Data collection. Options are: `SortOrder` (default), `Id`, `Code` and `Text`.

<br>

### Entity class attributes

The following represents the **entity class** attributes:

Attribute | Description
---|---
`Inherits` | Defines the base class that the entity inherits from. Defaults to `EntityBase` for a standard entity. For Reference Data it will default to `ReferenceDataBaseInt` or `ReferenceDataBaseGuid` depending on the corresponding `RefDataType` value.
`Implements` | List of comma separated interfaces that are to be declared for the class.
`Abstract` | Indicates whether the class should be defined as `abstract`.
`GenericWithT` | Indicates whether the class should be defined as a generic with a single parameter `T`.
`Namespace` | Name of the entity namespace appended to end of the standard `[company].[appname].Common.Entities.Namespace`.

<br>

### Entity collection class attributes

The following represents the corresponding **entity collection class** attributes:

Attribute | Description
---|---
**`Collection`** | Indicates whether a corresponding entity collection class should be created.
`CollectionKeyed` | Indicates whether the entity collection is keyed using the properties defined as forming part of the unique key. 
`CollectionInherits` | Defines the base class that a standard entity collection inherits from. Defaults to `EntityBaseCollection` or `EntityBaseKeyedCollection` depending on `CollectionKeyed`. For Reference Data it will default to `ReferenceDataCollectionBase`.
**`CollectionResult`** | Indicates whether a corresponding entity collection result class should be created; enables the likes of additional paging state to be stored with the underlying collection. There is no Reference Data equivalent and therefore should not be selected.
`CollectionResultInherits` | Defines the base class that entity collection result inherits from. Defaults to `EntityCollectionResult`. 

<br>

### Operation attributes

The following represents the corresponding **Operation** attributes. These primarily provide a shorthand to create the standard Get, Create, Update and Delete operations (versus having to specify directly):

Attribute | Description
---|---
**`Validator`** | The name of the Type that will perform the value validation; only used for a 'Create' and 'Update' operation types (used where not specified for the [`Operation`](Entity-Operation-element.md)).
`Get` | Indicates that a **Get** operation will be automatically generated where not otherwise specified.
`Create` | Indicates that a **Create** operation will be automatically generated where not otherwise specified.
`Update` | Indicates that a **Update** operation will be automatically generated where not otherwise specified.
`Delete` | Indicates that a **Delete** operation will be automatically generated where not otherwise specified.

<br>

### Data attributes

The following represents the corresponding **data** attributes:

Attribute | Description
---|---
**`AutoImplement`** | Indicates that the implementation for the underlying [`Operations`](Entity-Operation-element.md) can be auto-implemented using the selected data source; options are `Database`, `EntityFramework` or `OData`. When selected some of the folllowing related attributes are also required (as documented). Additionally, the `AutoImplement` indicator must be selected for each **`Operation`** that is to be auto-implemented.
`DatabaseName` | Specifies the .NET database wrapper instance name. Defaults to `Database` where `AutoImplement` is `Database`. 
`DatabaseSchema` | Specifies the database schema name where `AutoImplement` is `Database`. Defaults to `dbo`.
`EntityFrameworkName` | Specifies the entity framework instance name where `AutoImplement` is `EntityFramework`. Defaults to `EfDb`.
`EntityFrameworkEntity` | Specifies the corresponding Entity Framework entity model name (required where `AutoImplement` is `EntityFramework`).
`ODataName` | Specifies the .NET OData wrapper instance name where `AutoImplement` is `OData`. Defaults to `OData`.
`ODataEntity` | Specifies the corresponding OData entity name. Required where `AutoImplement` is `OData`.
`DataDatabaseMapperInheritsFrom` | Specifies the mapper that the generated Database mapper inherits from.
`DataODataMapperInheritsFrom` | Specifies the mapper that the generated OData mapper inherits from.
`DataEntityFrameworkMapperInheritsFrom` | Specifies the mapper that the generated Entity Framework mapper inherits from.
`DataDatabaseCustomMapper` | Indicates that a custom Database `Mapper` will be used; i.e. not generated. Otherwise, by default a `Mapper` is generated.
`DataODataCustomMapper` | Indicates that a custom OData `Mapper` will be used; i.e. not generated. Otherwise, by default a `Mapper` is generated.
`DataEntityFrameworkCustomMapper` | Indicates that a custom Entity Framework `Mapper` will be used; i.e. not generated. Otherwise, by default a `Mapper` is generated.
`MapperAddStandardProperties` | Indicates that the AddStandardProperties method call is to be included for the `Mapper`. Defaults to `true`.

<br>

### Data service attributes

The following represents the corresponding **Data Service** attributes:

Attribute | Description
---|---
`DataSvcCaching` | Indicates whether ExecutionContext-based (request) caching is to be performed at the `DataSvc` layer to improve performance (i.e. reduce chattiness).
`EventPublish` | Indicates whether to add logic to publish an event on the successful completion of the DataSvc layer invocation for a Create, Update or Delete. Uses `Config` value (inherits) where not specified. Used to enable the sending of messages to the likes of EventGrid, Service Broker, SignalR, etc.

<br>

### Web API attributes

The following represents the corresponding **Web API** attributes:

Attribute | Description
---|---
**`WebApiRoutePrefix`** |  Specifies the `RoutePrefixAtttribute` for the corresponding entity Web API controller. This is the base (prefix) URI for the entity and is extended when defining the underlying [`Operations`](Entity-Operation-element.md).
**`WebApiAuthorize`** | Indicates whether the Web API controller should use the `Authorize` or `AllowAnonynous`. Uses `Config` value (inherits) where not specified; can be overridden at the [`Operation`](Entity-Operation-element.md) level also.

The following represents the attributes for controlling (selecting) which specific code **artefacts** are to be generated; where none are specified all artefacts are implied:

<br>

### Exclusion attributes

The following represents the corresponding **Exclusion** attributes:

Attribute | Description
---|---
`ExcludeEntity` | Indicates whether to exclude the creation of the Entity class (`Xxx.cs`).
**`ExcludeAll`** | Indicates whether to exclude the creation of all Operations versus specifying each layer (below).
`ExcludeIData` | Indicates whether to exclude the creation of the Data interface (`IXxxData.cs`).
`ExcludeData` | Indicates whether to exclude the creation of the Data class (`XxxData.cs`). This also supports specialised output where `false` is used; will result in the Data class with only the underlying `Mapper` generated where no operations have been specified.
`ExcludeDataSvc` | Indicates whether to exclude the creation of the DataSvc class (`XxxDataSvc.cs`).
`ExcludeIManager` | Indicates whether to exclude the creation of the Manager interface (`IXxxManager.cs`).
`ExcludeManager` | Indicates whether to exclude the creation of the Manager class (`XxxManager.cs`).
`ExcludeWebApi` | Indicates whether to exclude the creation of the Web API Controller class (`XxxController.cs`).
`ExcludeWebApiAgent` | Indicates whether to exclude the creation of the Web API Agent class (`XxxAgent.cs`).
