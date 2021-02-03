# 'QueryJoin' object (database-driven) - XML

The `QueryJoin` object defines a join to another (or same) table within a query. The `Type` defines the join type, such as inner join, etc. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name).

<br/>

## Property categories
The `QueryJoin` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The name of the table to join.
`Schema` | The schema name of the table to join. Defaults to `Table.Schema`; i.e. same schema.
`Alias` | The `Schema.Table` alias name. Will automatically default where not specified.
**`Type`** | The SQL join type. Valid options are: `Inner`, `Left`, `Right`, `Full`. Defaults to `Inner`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The comma separated list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The comma seperated list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`AliasColumns`** | The comma seperated list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`ColumnNameIsDeleted` | The column name for the `IsDeleted` capability. Defaults to `CodeGeneration.IsDeleted`.
`ColumnNameTenantId` | The column name for the `TenantId` capability. Defaults to `CodeGeneration.TenantId`.
`ColumnNameOrgUnitId` | The column name for the `OrgUnitId` capability. Defaults to `CodeGeneration.OrgUnitId`.
`ColumnNameRowVersion` | The column name for the `RowVersion` capability. Defaults to `CodeGeneration.RowVersion`.
`ColumnNameCreatedBy` | The column name for the `CreatedBy` capability. Defaults to `CodeGeneration.CreatedBy`.
`ColumnNameCreatedDate` | The column name for the `CreatedDate` capability. Defaults to `CodeGeneration.CreatedDate`.
`ColumnNameUpdatedBy` | The column name for the `UpdatedBy` capability. Defaults to `CodeGeneration.UpdatedBy`.
`ColumnNameUpdatedDate` | The column name for the `UpdatedDate` capability. Defaults to `CodeGeneration.UpdatedDate`.
`ColumnNameDeletedBy` | The column name for the `DeletedBy` capability. Defaults to `CodeGeneration.UpdatedBy`.
`ColumnNameDeletedDate` | The column name for the `DeletedDate` capability. Defaults to `CodeGeneration.UpdatedDate`.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
