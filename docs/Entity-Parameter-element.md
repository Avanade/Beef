# 'Parameter' element (entity-driven)

The **`Parameter`** element enables parameters to be configured for an Operation.

An example is as follows:

```xml
<Operation Name="UpdateBatch" Text="Upserts a {{CustomerGroupCollection}} as a batch" OperationType="Custom" WebApiRoute="{company}" AutoImplement="false" WebApiMethod="HttpPut">
  <Parameter Name="Value" Type="CustomerGroupCollection" IsMandatory="true" WebApiFrom="FromBody" ValidatorFluent="EntityCollection(CustomerGroupValidator.Default, 1, 100)" />
  <Parameter Name="Company" Property="Company" IsMandatory="true" LayerPassing="ToManagerCollSet"/>
</Operation>
```

<br>

## Attributes

The **Parameter** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** property attributes: 

Attribute | Description
---|---
**`Name`** | Unique property name. This is mandatory.
`Property` | The corresponding Property name within the owning Entity to initially set up the Parameter configuration/characteristics.
`Text` | Text to be used in comments. Defaults to the Name converted to sentence case. Depending on whether the `Type` is bool, will appear in one of the two generated sentences: 'Gets or sets a value indicating whether {text}.' or 'Gets or sets the {text}.'. To create a <see cref="XXX"/> use {{XXX}} shorthand.
`Inherited` | Indicates whether the property is inherited and therefore should not be output within the generated entity class.
`PrivateName` | Name to be used for private fields; e.g. 'FirstName' reformatted as '_firstName'. Defaults from `Name`.
`ArgumentName` | Name to be used for argument parameters; e.g. 'FirstName' reformatted as 'firstName'. Defaults from `Name`).

<br>

### Parameter definition attributes

The following represents the key .NET **parameter definition** attributes:

Attribute | Description
---|---
**`Type`** | Specifies the .NET type. Defaults to `string`.
**`Nullable`** | Indicates that the .NET Type should be declared as nullable; e.g. `Nullable<T>`.
**`RefDataType`** | Identifies the `Type` as being a Reference Data type as well as specifying the underlying .NET Type used for the ReferenceData identifier serialization. Options are: `string`, `int` and `Guid`.
`Default` | Specifies the default value. Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.

<br>

### Manager attributes

The following represents the **manager** layer attributes:

Attribute | Description
---|---
`IsMandatory` | Indicates that a `ValidationException` should be thrown when the parameter value has its default value (null, zero, etc).
`Validator` | The name of the .NET `Type` that will perform the value validation.
`ValidatorFluent` | Fluent validator C# code to append to IsMandatory and Validator (where specified).
`LayerPassing` | Determines the layers in which the parameters is passed. Where using the `UniqueKey` option to automatically set `Parameters`, and the `OperationType` is `Create` or `Update` it will default to `ToManagerSet`). Options are: `All` (passes the argument through all layers), `ToManager` (only passes the argument to the Manager layer), `ToManagerSet` (only passes the argument to the Manager layer and overrides the same named property within the corresponding 'value' parameter), and `ToManagerCollSet` (only passes the argument to the Manager layer and overrides the same named property within the 'value' collection parameter). The default is `All`.

<br>

### Web API attributes

The following represents the **Web API** attributes:

Attribute | Description
---|---
`WebApiFrom` | Specifies how the parameter will be declared. Options are: `FromUri` (passed as part of the URI), `FromBody` (passed as the content body), `FromUriUseProperties` (passed as individual parameters as defined within the specified entity - within the same XML configuration file), and `FromUriJsonBinder` (now obsolete). The default is `FromUri`.

<br>

### Data attributes

The following represents the corresponding **data** attributes:

Attribute | Description
---|---
`DataConverter` | Specifies the data converter class name (where specific data conversion is required). A `Converter` is used to convert a data source value to/from a .NET type where no standard data type conversion can be applied.