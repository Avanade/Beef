# `StoredProcedure` element (table-driven)

The code generation for a **Stored Procedure** is primarily driven by the `Type` attribute. This encourages a consistent implementation for the standardised  actions. Options are as follows:
- **`Get`** - indicates a get (read) returning a single row value.
- **`GetAll`** - indicates a get (read) returning one or more rows (collection).
- **`Create`** - indicates the creation of a row.
- **`Update`** - indicates the updating of a row.
- **`Upsert`** - indicates the upserting (create or update) of a row.
- **`Delete`** - indicates the deleting of a row.
- **`Merge`** - indicates the merging (create, update or delete) of one or more rows (collection) through the use of a User Defined Table Type (UDT) parameter.

An example is as follows:

```xml
<Table Name="Contact" Schema="Prac" Permission="Contact">
```

<br>

## Attributes

The **`StoredProcedure`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** table attributes: 

Attribute | Description
-|-
**`Name`** | Unique stored procedure name. This is mandatory.
**`Type`** | The stored procedure type. Options are: `Get`, `GetAll`, `Create`, `Update`, `Upsert`, `Delete` or `Merge`.
`Permission` | The permission (fullname being name.action) override to be used for security permission checking.

<br>

### Additional attributes

The following represents **additional** table attributes: 

Attribute | Description
-|-
`Paging` | Indicates whether paging support should be added. Only valid for a `Type` of `GetAll`.
`IntoTempTable` | Indicates whether to select into a #TempTable to allow other statements to get access to the selected data; a `select * from #TempTable` is also performed immediately. The temporary table is named `#`+`Table.Alias`. Only valid for a `Type` of `GetAll`.
`ReselectStatement` | Overrides the re-select SQL statement to execute (`Create`, `Update` and `Upsert` types only).
`MergeOverrideIdentityColumns` | Overrides the column names used in the **Merge** statement with those specified in the comma separated list to determine whether to insert, update or delete. Defaults to the identity column name inferred from the database schema.