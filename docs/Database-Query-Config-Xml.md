# 'Query' object (database-driven)

The `Query` object enables the definition of more complex multi-table queries (`Joins`) that would primarily result in a database _View_. The primary table `Name` for the query is required to be specified. Multiple queries can be specified for the same table(s). The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name). Additional `Where` and `Order` configuration can also be added as required.

<br/>

## Property categories
The `Query` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`View`](#View) | Provides the _View_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The name of the primary table of the query. [Mandatory]
`Schema` | The schema name of the primary table of the view.<br/><br/>Defaults to `CodeGeneration.dbo`.
`Alias` | The `Schema.Table` alias name.<br/><br/>Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The comma separated list of `Column` names to be included in the underlying generated output.<br/><br/>Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The comma seperated list of `Column` names to be excluded from the underlying generated output.<br/><br/>Where not specified this indicates no `Columns` are to be excluded.
**`AliasColumns`** | The comma seperated list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.<br/><br/>Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## View
Provides the _View_ configuration.

Property | Description
-|-
`View` | Indicates whether a `View` is to be generated.
`ViewName` | The `View` name.<br/><br/>Defaults to `vw` + `Name`; e.g. `vwTableName`.
`ViewSchema` | The schema name for the `View`.<br/><br/>Defaults to `Schema`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`Permission`** | The permission to be used for security permission checking.<br/><br/>The suffix is optional, and where not specified will default to `.READ`.

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`ColumnNameIsDeleted` | The column name for the `IsDeleted` capability.<br/><br/>Defaults to `CodeGeneration.IsDeleted`.
`ColumnNameTenantId` | The column name for the `TenantId` capability.<br/><br/>Defaults to `CodeGeneration.TenantId`.
`ColumnNameOrgUnitId` | The column name for the `OrgUnitId` capability.<br/><br/>Defaults to `CodeGeneration.OrgUnitId`.
`ColumnNameRowVersion` | The column name for the `RowVersion` capability.<br/><br/>Defaults to `CodeGeneration.RowVersion`.
`ColumnNameCreatedBy` | The column name for the `CreatedBy` capability.<br/><br/>Defaults to `CodeGeneration.CreatedBy`.
`ColumnNameCreatedDate` | The column name for the `CreatedDate` capability.<br/><br/>Defaults to `CodeGeneration.CreatedDate`.
`ColumnNameUpdatedBy` | The column name for the `UpdatedBy` capability.<br/><br/>Defaults to `CodeGeneration.UpdatedBy`.
`ColumnNameUpdatedDate` | The column name for the `UpdatedDate` capability.<br/><br/>Defaults to `CodeGeneration.UpdatedDate`.
`ColumnNameDeletedBy` | The column name for the `DeletedBy` capability.<br/><br/>Defaults to `CodeGeneration.UpdatedBy`.
`ColumnNameDeletedDate` | The column name for the `DeletedDate` capability.<br/><br/>Defaults to `CodeGeneration.UpdatedDate`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`Joins`** | The corresponding [`QueryJoin`](Database-QueryJoin-Config-Xml.md) collection.<br/><br/>A `Join` object provides the configuration for a joining table.
`Order` | The corresponding [`QueryOrder`](Database-QueryOrder-Config-Xml.md) collection.<br/><br/>An `Order` object defines the order (sequence).
`Where` | The corresponding [`QueryWhere`](Database-QueryWhere-Config-Xml.md) collection.<br/><br/>A `Where` object defines the selection/filtering.

