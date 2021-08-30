# 'Property' object (entity-driven) - XML

The `Property` object defines an `Entity` property and its charateristics.

<br/>

## Property categories
The `Property` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Property`](#Property) | Provides additional _Property_ configuration.
[`RefData`](#RefData) | Provides the _Reference Data_ configuration.
[`Serialization`](#Serialization) | Provides the _Serialization_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`Data`](#Data) | Provides the generic _Data-layer_ configuration.
[`Database`](#Database) | Provides the specific _Database (ADO.NET)_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `Database`.
[`EntityFramework`](#EntityFramework) | Provides the specific _Entity Framework (EF)_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `EntityFramework`.
[`Cosmos`](#Cosmos) | Provides the specific _Cosmos DB_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `Cosmos`.
[`OData`](#OData) | Provides the specific _OData_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `OData`.
[`Annotation`](#Annotation) | Provides additional property _Annotation_ configuration.
[`WebApi`](#WebApi) | Provides the data _Web API_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The unique property name.
`Text` | The overriding text for use in comments. By default the `Text` will be the `Name` reformatted as sentence casing. Depending on whether the `Type` is `bool`, will appear in one of the two generated sentences. Where not `bool` it will be: Gets or sets a value indicating whether {text}.'. Otherwise, it will be: Gets or sets the {text}.'. To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. {{Xxx}}).
**`Type`** | The .NET `Type`. Defaults to `string`. To reference a Reference Data `Type` always prefix with `RefDataNamespace` (e.g. `RefDataNamespace.Gender`) or shortcut `^` (e.g. `^Gender`). This will ensure that the appropriate Reference Data `using` statement is used. _Shortcut:_ Where the `Type` starts with (prefix) `RefDataNamespace.` or `^`, and the correspondong `RefDataType` attribute is not specified it will automatically default the `RefDataType` to `string.`
**`Nullable`** | Indicates whether the .NET `Type` should be declared as nullable; e.g. `string?`. Will be inferred where the `Type` is denoted as nullable; i.e. suffixed by a `?`.
`Inherited` | Indicates whether the property is inherited and therefore should not be output within the generated Entity class.
`PrivateName` | The overriding private name. Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.
`ArgumentName` | The overriding argument name. Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.

<br/>

## Property
Provides additional _Property_ configuration.

Property | Description
-|-
**`UniqueKey`** | Indicates whether the property is considered part of the unique (primary) key. This is also used to simplify the parameter specification for an Entity Operation by inferrence.
**`IsEntity`** | Indicates that the property `Type` is another generated entity / collection and therefore specific capabilities can be assumed (e.g. `CopyFrom` and `Clone`). Will be inferred (default to `true`) where the `Type` is `ChangeLog` or the `Type` is found as another `Entity` within the code-generation configuration file.
`Immutable` | Indicates that the value is immutable and therefore cannot be changed once set.
`DateTimeTransform` | The `DateTime` transformation to be performed on `Set` and `CleanUp`. Valid options are: `UseDefault`, `None`, `DateOnly`, `DateTimeLocal`, `DateTimeUtc`, `DateTimeUnspecified`. Defaults to `UseDefault`. This is only applied where the `Type` is `DateTime`.
`StringTrim` | The `string` trimming of white space characters to be performed on `Set` and `CleanUp`. Valid options are: `UseDefault`, `None`, `Start`, `End`, `Both`. Defaults to `UseDefault`. This is only applied where the `Type` is `string`.
`StringTransform` | The `string` transformation to be performed on `Set` and `CleanUp`. Valid options are: `UseDefault`, `None`, `NullToEmpty`, `EmptyToNull`. Defaults to `UseDefault`. This is only applied where the `Type` is `string`.
`AutoCreate` | Indicates whether an instance of the `Type` is to be automatically created/instantiated when the property is first accessed (i.e. lazy instantiation).
`Default` | The C# code to default the value. Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.
`PartitionKey` | Indicates whether the property is considered part of the Partition Key. This will implement `IPartitionKey` for the generated entity.
`SecondaryPropertyChanged` | The names of the secondary property(s), comma delimited, that are to be notified on a property change.
`BubblePropertyChanges` | Indicates whether the value should bubble up property changes versus only recording within the sub-entity itself. Note that the `IsEntity` property is also required to enable.
`ExcludeCleanup` | Indicates that `CleanUp` is not to be performed for the property within the `Entity.CleanUp` method.
`InternalOnly` | Indicates whether the property is for internal use only; declared in Business entities only. This is only applicable where the `Entity.EntityScope` is `Autonomous`. In this instance the `Property` will be excluded from the `Common` entity declaration.

<br/>

## RefData
Provides the _Reference Data_ configuration.

Property | Description
-|-
`RefDataType` | The underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID). Valid options are: `string`, `int`, `Guid`. Defaults to `string` (being the `ReferenceDataBase.Code`) where not specified and the corresponding `Type` starts with (prefix) `RefDataNamespace.` or `^`. Note: an `Id` of type `string` is currently not supported; the use of the `Code` is the recommended approach.
`RefDataList` | Indicates that the Reference Data property is to be a serializable list (`ReferenceDataSidList`). This is required to enable a list of Reference Data values (as per `RefDataType`) to be passed as an argument for example.
`RefDataText` | Indicates whether a corresponding `Text` property is added when generating a Reference Data property, overriding the `Entity.RefDataText` selection. This is used where serializing within the Web API `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (which is automatically set where the url contains `$text=true`).
`RefDataMapping` | Indicates whether the property should use the underlying Reference Data mapping capabilities. Mapped properties are a special Reference Data property type that ensure value uniqueness; this allows the likes of additional to/from mappings to occur between systems where applicable.

<br/>

## Serialization
Provides the _Serialization_ configuration.

Property | Description
-|-
`JsonName` | The JSON property name. Defaults to `ArgumentName` where not specified (i.e. camelCase); however, where the property is `ETag` it will default to the `Config.ETagJsonName`.
`JsonDataModelName` | The JSON property name for the corresponding data model (see `Entity.DataModel`). Defaults to `JsonName` where not specified.
`IgnoreSerialization` | Indicates whether the property is not to be serialized. All properties are serialized by default.
`EmitDefaultValue` | Indicates whether to emit the default value when serializing.
`DataModelJsonName` | The override JSON property name where outputting as a data model. Defaults to `JsonName` where not specified.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
`IdentifierGenerator` | The Identifier Generator Type to generate the identifier on create via Dependency Injection. Should be formatted as `Type` + `^` + `Name`; e.g. `IGuidIdentifierGenerator^GuidIdGen`. Where the `Name` portion is not specified it will be inferred. Where the `Type` matches an already inferred value it will be ignored. See `Beef.Entities.IInt32IdentifierGenerator`, `Beef.Entities.IInt64IdentifierGenerator`, `Beef.Entities.IGuidIdentifierGenerator` or `Beef.Entities.IStringIdentifierGenerator` for underlying implementation requirements.

<br/>

## Data
Provides the generic _Data-layer_ configuration.

Property | Description
-|-
**`DataName`** | The data name where Entity.AutoImplement is selected. Defaults to the property `Name`. Represents the column name for a `Database`, or the correspinding property name for the other options.
**`DataConverter`** | The data `Converter` class name where `Entity.AutoImplement` is selected. A `Converter` is used to convert a data source value to/from a .NET `Type` where no standard data conversion can be applied. Where this value is suffixed by `<T>` or `{T}` this will automatically set `DataConverterIsGeneric` to `true`.
`IsDataConverterGeneric` | Indicates whether the data `Converter` is a generic class and will automatically use the corresponding property `Type` as the generic `T`.
`DataMapperIgnore` | Indicates whether the property should be ignored (excluded) from the `Data`-layer / data `Mapper` generated output. All properties are included by default.
`DataAutoGenerated` | Indicates whether the `UniqueKey` property value is automatically generated by the data source on `Create`.
`DataOperationTypes` | The operations types (`ExecutionContext.OperationType`) selection to enable inclusion and exclusion of property mapping. Valid options are: `Any`, `AnyExceptCreate`, `AnyExceptUpdate`, `AnyExceptGet`, `Get`, `Create`, `Update`, `Delete`. Defaults to `Any`.

<br/>

## Database
Provides the specific _Database (ADO.NET)_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `Database`.

Property | Description
-|-
`DataDatabaseMapper` | The database property `Mapper` class name where `Entity.AutoImplement` is selected. A `Mapper` is used to map a data source value to/from a .NET complex `Type` (i.e. class with one or more properties).
`DataDatabaseIgnore` | Indicates whether the property should be ignored (excluded) from the database `Mapper` generated output.
**`DatabaseDbType`** | The database `DbType` override (versus inferring from the corresponding .NET Type). Overrides the inferred database type; i.e. can specify `Date` or `DateTime2`, for .NET Type `System.DateTime`.

<br/>

## EntityFramework
Provides the specific _Entity Framework (EF)_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `EntityFramework`.

Property | Description
-|-
`EntityFrameworkMapper` | The Entity Framework `Mapper` approach for the property. Valid options are: `Map`, `Ignore`, `Skip`. Defaults to `Map` which indicates the property will be explicitly mapped. A value of `Ignore` will explicitly `Ignore`, whilst a value of `Skip` will skip code-generated mapping altogether.

<br/>

## Cosmos
Provides the specific _Cosmos DB_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `Cosmos`.

Property | Description
-|-
`CosmosMapper` | The Cosmos `Mapper` approach for the property. Valid options are: `Map`, `Ignore`, `Skip`. Defaults to `Map` which indicates the property will be explicitly mapped. A value of `Ignore` will explicitly `Ignore`, whilst a value of `Skip` will skip code-generated mapping altogether.

<br/>

## OData
Provides the specific _OData_ configuration where `Entity.AutoImplement` or `Operation.AutoImplement` is `OData`.

Property | Description
-|-
`ODataMapper` | The OData `Mapper` approach for the property. Valid options are: `Map`, `Ignore`, `Skip`. Defaults to `Map` which indicates the property will be explicitly mapped. A value of `Ignore` will explicitly `Ignore`, whilst a value of `Skip` will skip code-generated mapping altogether.

<br/>

## Annotation
Provides additional property _Annotation_ configuration.

Property | Description
-|-
`DisplayName` | The display name used in the likes of error messages for the property. Defaults to the `Name` as sentence case.
`Annotation1` | The property annotation (e.g. attribute) declaration code.
`Annotation2` | The property annotation (e.g. attribute) declaration code.
`Annotation3` | The property annotation (e.g. attribute) declaration code.

<br/>

## WebApi
Provides the data _Web API_ configuration.

Property | Description
-|-
`WebApiQueryStringConverter` | The `IPropertyMapperConverter` to perform `Type` to `string` conversion for writing to and parsing from the query string.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
**`GrpcFieldNo`** | The unique (immutable) field number required to enable gRPC support.
`GrpcType` | The underlying gRPC data type; will be inferred where not specified.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
