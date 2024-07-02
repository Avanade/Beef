# 'Parameter' object (entity-driven)

The `Parameter` object defines an `Operation` parameter and its charateristics.

<br/>

## Example

A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
parameters: [
  { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
]
```

<br/>

## Property categories
The `Parameter` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Property`](#Property) | Provides the _Property_ reference configuration.
[`RefData`](#RefData) | Provides the _Reference Data_ configuration.
[`Manager`](#Manager) | Provides the _Manager-layer_ configuration.
[`Data`](#Data) | Provides the _data_ configuration.
[`WebApi`](#WebApi) | Provides the _Web API_ configuration.
[`gRPC`](#gRPC) | Provides the _gRPC_ configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique parameter name. [Mandatory]
`text` | The overriding text for use in comments.<br/>&dagger; By default the `Text` will be the `Name` reformatted as sentence casing. To have the text used as-is prefix with a `+` plus-sign character.
**`type`** | The .NET `Type`.<br/>&dagger; Defaults to `string`. To reference a Reference Data `Type` always prefix with `RefDataNamespace` (e.g. `RefDataNamespace.Gender`) or shortcut `^` (e.g. `^Gender`). This will ensure that the appropriate Reference Data `using` statement is used. _Shortcut:_ Where the `Type` starts with (prefix) `RefDataNamespace.` or `^`, and the correspondong `RefDataType` attribute is not specified it will automatically default the `RefDataType` to `string.`
**`nullable`** | Indicates whether the .NET Type should be declared as nullable; e.g. `int?`. Will be inferred where the `Type` is denoted as nullable; i.e. suffixed by a `?`. Where the .NET Type is not considered as an intrinsic type then will default to `true`.
`default` | The C# code to default the value.<br/>&dagger; Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.
`privateName` | The overriding private name.<br/>&dagger; Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.
`argumentName` | The overriding argument name.<br/>&dagger; Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.

<br/>

## Property
Provides the _Property_ reference configuration.

Property | Description
-|-
`property` | The `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.

<br/>

## RefData
Provides the _Reference Data_ configuration.

Property | Description
-|-
`refDataType` | The underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID). Valid options are: `string`, `int`, `Guid`.<br/>&dagger; Defaults to `string` where not specified and the corresponding `Type` starts with (prefix) `RefDataNamespace.`.
`refDataList` | Indicates that the Reference Data property is to be a serializable list (`ReferenceDataSidList`).<br/>&dagger; This is required to enable a list of Reference Data values (as per `RefDataType`) to be passed as an argument for example.

<br/>

## Manager
Provides the _Manager-layer_ configuration.

Property | Description
-|-
**`validator`** | The name of the .NET `Type` that will perform the validation.
`validatorCode` | The fluent-style method-chaining C# validator code to append to `IsMandatory` and `Validator` (where specified).
`validationFramework` | The `Validation` framework to use for the entity-based validation. Valid options are: `CoreEx`, `FluentValidation`.<br/>&dagger; Defaults to `Operation.ValidationFramework`.
`isMandatory` | Indicates whether a `ValidationException` should be thrown when the parameter value has its default value (null, zero, etc).
`layerPassing` | The option that determines the layers in which the parameter is passed. Valid options are: `All`, `ToManagerSet`, `ToManagerCollSet`.<br/>&dagger; Defaults to `All`. To further describe, `All` passes the parameter through all layeys, `ToManagerSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` parameter, `ToManagerCollSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` collection parameter. Where using the `PrimaryKey` option to automatically set `Parameters`, and the `Operation.Type` is `Create` or `Update` it will default to `ToManagerSet`.

<br/>

## Data
Provides the _data_ configuration.

Property | Description
-|-
`dataConverter` | The data `Converter` class name where specific data conversion is required.<br/>&dagger; A `Converter` is used to convert a data source value to/from a .NET `Type` where no standard data conversion can be applied. Where this value is suffixed by `<T>` or `{T}` this will automatically set `Type`.

<br/>

## WebApi
Provides the _Web API_ configuration.

Property | Description
-|-
`webApiFrom` | The option for how the parameter will be delcared within the Web API Controller. Valid options are: `FromQuery`, `FromBody`, `FromRoute`, `FromEntityProperties`.<br/>&dagger; Defaults to `FromQuery`; unless the parameter `Type` has also been defined as an `Entity` within the code-gen config file then it will default to `FromEntityProperties`. Specifies that the parameter will be declared with corresponding `FromQueryAttribute`, `FromBodyAttribute` or `FromRouteAttribute` for the Web API method. The `FromEntityProperties` will declare all properties of the `Entity` as query parameters.
`webApiText` | The overriding text for use in the Web API comments.<br/>&dagger; By default the `WbeApiText` will be the `Name` reformatted as sentence casing. To have the text used as-is prefix with a `+` plus-sign character.

<br/>

## gRPC
Provides the _gRPC_ configuration.

Property | Description
-|-
`grpcType` | The underlying gRPC data type; will be inferred where not specified.

