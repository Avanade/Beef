# Reference Data

The _Beef_ framework and runtime enable a rich, first class, experience for _reference data_ given its key role within an application.

<br/>

## Types of data

At a high-level a typical applications deals with different types of data:

- **Reference Data** is data that is managed within an application primarily used to provide lists of valid values. These values provide contextual information that are generally used to trigger business processes, workflows and/or used for grouping / filtering.
This data has a low level of volatility, in that it remains largely static for significant periods of time. There are low volumes of this data within an application. It is a very good candidate for the likes of caching.
Reference Data is generally never deleted; instead it may become inactive. 
Example: Country, Gender, Payment Type, etc. 

- **Master data** is data that is captured and continuously maintained to reflect a current known understanding; there is no historical context other than that provided by an audit process providing a version history over time.
This data has a moderate level of volatility, in that changes generally occur infrequently. There are moderate volumes of this data within an application.
Master data can be deleted (or logically deleted) as required; typically the latter. 
Example: Customer, Vendor, Product, GL Account, etc. 

- **Transactional data** is data that is recorded to capture/manage an event or action, tied to specific business rules, at a point in time. 
The data will typically have a high level of volatility at inception decreasing significantly over time. Once the corresponding workflow has completed the data becomes immutable and serves the purpose of providing a historical context.
Transactional data is generally never deleted as it provides an auditable recording. There are high volumes of this type of data within an application.
Example: Purchase Order, Sales Invoice, GL Posting, etc. 

<br/>

## Base capabilities

The [`ReferenceDataBase`](../src/Beef.Core/RefData/ReferenceDataBase.cs) provides the base capabilities for a reference data item. The following tables describes the key properties:

Property | Description
-|-
`Id` | The internal unique identifier as either an `int` ([`ReferenceDataBaseInt32`](../src/Beef.Core/RefData/ReferenceDataBaseInt.cs)) or a `Guid` ([`ReferenceDataBaseGuid`](../src/Beef.Core/RefData/ReferenceDataBaseGuid.cs)).
`Code` | The unique (immutable) code as a `string`. This is primarily the value that would be used by external parties (applications) to consume. Additionally, it could be used to store the reference in the underlying data source if the above `Id` is not suitable.
`Text` | The textual `string` used for display within an application; e.g. within a drop-down. 
`SortOrder` | Defines the sort order within the underlying reference data collection.
`IsActive` | Indicates whether the value is active or not. It is up to the application what to do when a value is not considered valid.
... | There are other properties on this base type that can also be used; see [codebase](../src/Beef.Core/RefData/ReferenceDataBase.cs) for more information.

<br/>

Additional developer-defined properties can be, and should be, added where required extending on the base class. The Reference Data framework will then make these available within the application to enable simple usage/access by a developer. The property will only be serialized when the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (automatically performed where url contains '$text=true').

The [`ReferenceDataCollectionBase`](../src/Beef.Core/RefData/ReferenceDataCollectionBase.cs) provides the base capabilities for a reference data collection. Including the adding, sorting and additional filtering (e.g. `ActiveList`).

<br/>

## Reference data usage

When the code generation is used to create an entity, any properties that are marked up with the `RefDataType` attribute will have code generated to enable multiple options to access the reference data values.

There will be a single field and two properties (where `Xxx` is the name of the reference data item) generated. The Serialization Identifier (SID) is the selected value used for actual serialization.

Additionally, if there is a need for the reference data `Text` to also be made available within the entity as a read-only property then the property can be marked up with the `RefDataText` attribute.

Name | Description
-|-
`XxxSid` | This is the Serialization Identifier (SID) that is used where serializing the reference data value; this is generally the unique reference data `Code`. <br/> As you can see in the sample below this property has the `JsonPropertyAttribute` specified so that this property is marked for serialization.
`_xxxSid` | This is the private Serialization Identifier (SID) field used internally to store the `XxxSid` above.
`Xxx` | This is the reference data object (inherits from `ReferenceDataBase`) that provides the rich capabilities that a developer would typically interact with. This value is never serialized. <br/> This is casted from the `_xxxSid` when referenced; this will force the underlying reference data to be loaded (lazy) on first access. This will result in a small performance cost, although this data is generally cached so for most access this will not be an issue. 
`XxxText` | The related reference data text (where optionally enabled). This can be either set directly (not encouraged), or will return the appropriate `Text` when selected to do so.
`_xxxText` | This the private variable field used internally to store the `XxxText` above.

<br/>

The following represents a snippet of the generated code, for an reference data item named `Gender`:

``` csharp
private string _genderSid;

/// <summary>
/// Gets or sets the <see cref="Gender"/> using the underlying Serialization Identifier (SID).
/// </summary>
[JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.Ignore)]
[Display(Name="Gender")]
public string GenderSid
{
    get { return this._genderSid; }
    set { SetValue(ref this._genderSid, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_Gender); }
}

/// <summary>
/// Gets or sets the Gender (see <see cref="RefDataNamespace.Gender"/>).
/// </summary>
[DebuggerBrowsable(DebuggerBrowsableState.Never)]
[Display(Name="Gender")]
public RefDataNamespace.Gender Gender
{
    get { return this._genderSid; }
    set { SetValue<string>(ref this._genderSid, value, false, false, Property_Gender); }
}
```

<br/>

There are multiple means that a developer would typically interact with reference data; for example:

``` csharp
// Accessing the 'Sid' directly will bypass the object casting and offers a small performance benefit.
person.GenderSid = "M";

// Casting from string to reference data item is supported (will result in ref data load + cache on first access).
person.Gender = "M";

// Other properties can be accessed directly making the developer experience more natural.
var text = person.Gender.Text;

// Casting an invalid value will create a dummy reference data item with IsInvalid set to true.
person.Gender = "%";
bool isInvalid = person.Gender.IsInvalid;
```

<br/>

## Caching

Given the static nature of the _Reference Data_ it is an excellent candidate for caching. There is a purpose built cache ([`ReferenceDataCache`](../src/Beef.Core/RefData/Caching/ReferenceDataCache.cs)) that ensures a consistent implementation, which then gets leveraged from within the code generation process.

This caching leverages the core [Caching](../src/Beef.Core/Caching) capabilities (i.e. [`CacheCoreBase`](../src/Beef.Core/Caching/CacheCorebase.cs)) within _Beef_; including one of the [Policies](../src/Beef.Core/Caching/Policy) to allow automatic cache expiry/refresh.

<br/>

## Reference data loading

As described above the _reference data_ is loaded and cached on first access (lazy loaded) to improve the overall performance. The physical load occurs within the [Data access](./Layer-Data.md) layer; whilst the caching is managed at the [Service orchestration](./Layer-DataSvc.md) layer.

When the cache is empty, or expired, then a load will occur. There are two options available for the loading:

1. Using the `GetRefData` purpose-built capability built into the [`DatabaseBase`](../src/Beef.Data.Database/DatabaseBase.cs). This uses the [DatabaseRefDataColumns](../src/Beef.Data.Database/DatabaseRefDataColumns.cs) to map the database columns names to the .NET properties. This can be completely code generated to minimise the developer effort.
2. Implement as required (i.e. custom logic).

<br/>

## Reference data providers

The [IReferenceDataProvider](../src/Beef.Core/RefData/IReferenceDataProvider.cs) provides a means to manage and group one or more Reference Data entities for use by the centralised [ReferenceDataManager](../src/Beef.Core/RefData/ReferenceDataManager.cs). There can be one or more providers (perhaps from multiple domains) that are centrally managed. The code-generation will implement this automatically; see [example](../samples/Demo/Beef.Demo.Common/Entities/Generated/ReferenceData.cs).

The [ReferenceDataManager](../src/Beef.Core/RefData/ReferenceDataManager.cs) provides a standard, centralised, mechanism for managing and accessing all the available/possible Reference Data entities via the `Current` property. There is a `Register` method that is used to register one or more providers for use; this is typically performed at start up.

<br/>

## Reference data APIs

By leveraging the code-generation reference data endpoint can be created. By default only the active (`ReferenceDataBase.IsActive`) entries will be returned. To get both the active and inactive the `$inactive=true` URL query string must be used.

### Per referenece data endpoints:

Each reference data entity should have an API endpoint generated; being `/ref/Xxx`. This can also be invoked passing additional URL query string parameters:

Parameter | Description
- | -
`code` | Zero or mode codes can be passed; e.g: `?code=m,f` or `?code=m&code=f` (case insensitive).
`text` | A single text with wildcards can be passed; e.g: `?text=M*` (case insensitive).

### Root reference data endpoints:

Additionally there is a root `/ref` that can be used to return multiple reference data values in a single request; designed to reduce chattiness from a consuming channel to the above endpoints. This must be passed at least a single URL query string parameter to function.

The parameter is either a just the named reference data entity which will result in all corresponding entries being returned (e.g: `?gender` or `?gender&country`). Otherwise, specific codes can be specified (e.g" `?gender=m,f`, `?gender=m&gender=f`, `?gender=m,f&country=au,nz`). The options can be mixed and matched (e.g: `?gender&country=au,nz`).

## Sample

There is an end-to-end example of the _reference data_ implementation within the [`Demo`](../samples/Demo).