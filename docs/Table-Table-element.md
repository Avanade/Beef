# `Table` element (table-driven)

The **`Table`** element specifies that one or more stored procedures are to be generated for the specified table (or view), including other characteristics defined.

The columns for the table (or view) are inferred from the database schema definition; unless explicitly overridden using the **`Column`** element. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude the selected columns.

An example is as follows:

```xml
<Table Name="Contact" Schema="Prac" Permission="Contact">
```

<br>

## Attributes

The **`Table`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Name`** | The database table/view name used to identify the table/view within the specified schema. This is mandatory.
**`Schema`** | The database schema used to identify the table/view.
`Alias` | The alias for the schema/name combination used in the generated output (automatically defaults).
`Permission` | The permission (prefix) to be used for security permission checking (suffix defaults to 'Read', 'Write' or 'Delete'). This can be overidden in the `StoredProcedure` configuration as required.
`View` | Indicates whether a database View will be automatically generated (only applies for a Table).

<br>

### Column selection attributes

The following represents the corresponding **Column selction** attributes:

Attribute | Description
---|---
`IncludeColumns` | A comma separated list of columns names to be included.
`ExcludeColumns` | A comma separated list of columns names to be excluded.

<br>

### Stored procedure attributes

The following represents the corresponding **StoredProcedure** attributes. These provide a shorthand to create the standard stored proceures (versus having to specify directly):

Attribute | Description
---|---
`Get` | Indicates that a **Get** stored procedure will be automatically generated where not otherwise specified.
`GetAll` | Indicates that a **GetAll** stored procedure will be automatically generated where not otherwise specified.
`Create` | Indicates that a **Create** stored procedure will be automatically generated where not otherwise specified.
`Update` | Indicates that a **Update** stored procedure will be automatically generated where not otherwise specified.
`Upsert` | Indicates that a **Upsert** (creates or updates) stored procedure will be automatically generated where not otherwise specified.
`Delete` | Indicates that a **Delete** stored procedure will be automatically generated where not otherwise specified.
`Merge` | Indicates that a **Merge** stored procedure (using a User Defined Table Type) will be automatically generated where not otherwise specified.

<br>

### User defined table type (UDT) attributes

The following represents the corresponding **User Defined Table Type (UDT)** attributes:

Attribute | Description
---|---
`Udt` | Indicates whether a user defined table type should be created.
`UdtExcludeColumns` | A comma separated list of columns names to be excluded from the Udt.
`Tvp` | Specifies the .NET entity name associated with the Udt so that it can ibe expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`. Results in generated C# code named `EntityDataTvp.cs` that extends the existing (corresponding) data partial classes.

<br>

### Entity framework attributes

The following represents the corresponding **Entity Framework** attributes:

Attribute | Description
---|---
`EfModel` | Indicates whether an Entity Framework c# model class should be created.