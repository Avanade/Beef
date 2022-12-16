# Reference Data

The _Beef_ code-generation is _reference data_ aware and will generate the appropriate code appropriately. This leverages the underlying [`CoreEx.RefData`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/RefData) runtime capabilities to enable.

<br/>

## Reference data usage

When the code generation is used to create an entity, any properties that are marked up with the `RefDataType` attribute will have code generated to enable multiple options to access the reference data values.

There will be a single field and two properties (where `Xxx` is the name of the reference data item) generated. The Serialization Identifier (SID) is the selected value used for actual over the wire serialization.

Additionally, if there is a need for the reference data `Text` to also be made available within the entity as a read-only property then the property can be marked up with the `RefDataText` attribute. The property will only be serialized when the [`ExecutionContext.IsTextSerializationEnabled`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ExecutionContext.cs) is set to `true` (automatically performed where URL query string contains '$text=true'). The reason this is not the default behaviour is that it is believed that these should generally be retrieved and cached (further minimizes payload size).

Name | Description
-|-
`_xxxSid` | This is the private Serialization Identifier (SID) field used internally to store the `XxxSid` value.
`XxxSid` | This is the Serialization Identifier (SID) that is used where serializing the reference data value; this is generally the unique reference data `Code`. <br/> As per sample below this property has the `JsonPropertyNameAttribute` specified so that this property is marked for serialization.
`XxxText` | The related reference data text (where optionally enabled). This will return the appropriate `Text` _only_ when selected to do so.
`Xxx` | This is the reference data object (inherits from [`ReferenceDataBaseEx`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/Extended/ReferenceDataBaseEx.cs)) that provides the rich capabilities that a developer would typically interact with internally within the underlying business logic. This value is _never_ serialized. <br/> This is casted from the `_xxxSid` when referenced; this will force the underlying reference data to be loaded (lazy) on first access. This will result in a small performance cost, although this data is generally cached so for most access this is not an issue. 

<br/>

The following represents a [snippet of the generated code](../samples/My.Hr/My.Hr.Business/Entities/Generated/EmployeeBase.cs) for an reference data property named `Gender`:

``` csharp
private string? _genderSid;

/// <summary>
/// Gets or sets the <see cref="Gender"/> using the underlying Serialization Identifier (SID).
/// </summary>
[JsonPropertyName("gender")]
public string? GenderSid { get => _genderSid; set => SetValue(ref _genderSid, value); }

/// <summary>
/// Gets the corresponding <see cref="Gender"/> text (read-only where selected).
/// </summary>
public string? GenderText => RefDataNamespace.Gender.GetRefDataText(_genderSid);

/// <summary>
/// Gets or sets the Gender (see <see cref="RefDataNamespace.Gender"/>).
/// </summary>
[DebuggerBrowsable(DebuggerBrowsableState.Never)]
[JsonIgnore]
public RefDataNamespace.Gender? Gender { get => _genderSid; set => SetValue(ref _genderSid, value); }
```

<br/>

There are multiple means that a developer would typically interact with reference data; for example:

``` csharp
// Accessing the 'Sid' directly will bypass the object casting (and corresponding load) and offers a small performance benefit.
person.GenderSid = "M";

// Casting from string to reference data item is supported (will result in reference data load + cache on first access).
person.Gender = "M";

// Other properties can be accessed directly making the developer experience more natural.
var text = person.Gender.Text;

// Casting an invalid value will create a dummy reference data item with 'IsInvalid' set to true.
person.Gender = "%";
bool isInvalid = person.Gender.IsInvalid;
```

<br/>

## Reference data APIs

The code-generation will create the end-to-end capabilities to expose the _reference data_ entities as APIs, including all the requisite business logic to load data, etc.

See the [`My.Hr`](../samples/My.Hr/Readme.md) and [`MyEf.Hr`](../samples/MyEf.Hr/Readme.md) solutions for guidance and examples.