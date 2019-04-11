# `Parameter` element (table-driven)

The **`Parameter`** element will add the specified parameters to the generated SQL; these are in addition to those that are automatically inferred by the selected stored procedure type.

An example is as follows:

```xml
<Parameter Name="FirstName" IsNullable="true" IsCollection="false" Operator="LIKE" />
<Parameter Name="WorkHistoryList" SqlType="[dbo].[udtWorkHistoryList] READONLY" />
```

<br>

## Attributes

The **`Parameter`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Name`** | Unqiue parameter name (without the @ prefix). This is mandatory.
**`Column`** | The corresponding column name (defaults to Name where not specified) to infer characteristics.

<br>

### Type characteristics attributes

The following represents the **type charateristics** attributes: 

Attribute | Description
-|-
`SqlType` | The SQL type definition (overrides inferred Column definition) including length/precision/scale.

<br>

### Query attributes

The following represents the **query** attributes: 

Attribute | Description
-|-
**`Operator`** | The query operator. Options are: `EQ`, `NE`,`LT`,`LE`,`GT`,`GE` or`LIKE`. Defaults to `EQ`.
`IsNullable` | Indicates whether the parameter is nullable (when the parameter value is `NULL` it will not be included in the query).
`TreatColumnNullAs` | Indicates that the column value where `null` should be treated as the specified *value*; results in: `ISNULL([schema].[column], specified-value)`.
`IsCollection` | Indicates whether the parameter is a collection (one or more values to be included in the query).
