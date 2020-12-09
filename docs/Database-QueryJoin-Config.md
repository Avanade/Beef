# 'QueryJoin' object (database-driven) - YAML/JSON

The `QueryJoin` object defines a join to another (or same) table within a query. The `Type` defines the join type, such as inner join, etc. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name).

<br/>

## Property categories
The `QueryJoin` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`CDC`](#CDC) | Provides the _Change Data Capture (CDC)_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the table to join.
`schema` | The schema name of the table to join. Defaults to `Table.Schema`; i.e. same schema.
`alias` | The `Schema.Table` alias name. Will automatically default where not specified.
**`type`** | The SQL join type. Valid options are: `Inner`, `Left`, `Right`, `Full`. Defaults to `Inner`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`cdc` | Indicates whether the joined table is also being monitored for Change Data Capture (CDC) and should be included accordingly. Otherwise, the `Join` is purely for filtering and/or column addition.

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`columnNameIsDeleted` | The column name for the `IsDeleted` capability. Defaults to `CodeGeneration.IsDeleted`.
`columnNameTenantId` | The column name for the `TenantId` capability. Defaults to `CodeGeneration.TenantId`.
`columnNameOrgUnitId` | The column name for the `OrgUnitId` capability. Defaults to `CodeGeneration.OrgUnitId`.
`columnNameRowVersion` | The column name for the `RowVersion` capability. Defaults to `CodeGeneration.RowVersion`.
`columnNameCreatedBy` | The column name for the `CreatedBy` capability. Defaults to `CodeGeneration.CreatedBy`.
`columnNameCreatedDate` | The column name for the `CreatedDate` capability. Defaults to `CodeGeneration.CreatedDate`.
`columnNameUpdatedBy` | The column name for the `UpdatedBy` capability. Defaults to `CodeGeneration.UpdatedBy`.
`columnNameUpdatedDate` | The column name for the `UpdatedDate` capability. Defaults to `CodeGeneration.UpdatedDate`.
`columnNameDeletedBy` | The column name for the `DeletedBy` capability. Defaults to `CodeGeneration.UpdatedBy`.
`columnNameDeletedDate` | The column name for the `DeletedDate` capability. Defaults to `CodeGeneration.UpdatedDate`.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
