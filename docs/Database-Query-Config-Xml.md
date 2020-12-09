# 'Query' object (database-driven) - XML

The `Query` object enables the definition of more complex multi-table queries (`Joins`) that would primarily result in a database _View_. The primary table `Name` for the query is required to be specified. Multiple queries can be specified for the same table(s). The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name). Additional `Where` and `Order` configuration can also be added as required.

<br/>

## Property categories
The `Query` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`View`](#View) | Provides the _View_ configuration.
[`CDC`](#CDC) | Provides the _Change Data Capture (CDC)_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The name of the primary table of the query.
`Schema` | The schema name of the primary table of the view. Defaults to `CodeGeneration.dbo`.
`Alias` | The `Schema.Table` alias name. Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The comma separated list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The comma seperated list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`AliasColumns`** | The comma seperated list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## View
Provides the _View_ configuration.

Property | Description
-|-
`View` | Indicates whether a `View` is to be generated.
`ViewName` | The `View` name. Defaults to `vw` + `Name`; e.g. `vwTableName`.
`ViewSchema` | The schema name for the `View`. Defaults to `Schema`.

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`Cdc` | Indicates whether the Change Data Capture (CDC) related artefacts are to be generated.
`CdcName` | The `View` name. Defaults to `CodeGeneration.sp` (literal) + `Name` + `Outbox` (literal); e.g. `spTableNameOutbox`.
`CdcSchema` | The schema name for the `Cdc`-related database artefacts. Defaults to `CodeGeneration.Schema` + `Cdc` (literal).
`CdcEnvelope` | The corresponding `Cdc` Outbox Envelope table name. Defaults to `CodeGeneration.Name` + `OutboxEnvelope` (literal).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`Permission`** | The permission to be used for security permission checking. The suffix is optional, and where not specified will default to `.READ`.

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

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`Joins`** | The corresponding [`QueryJoin`](Database-QueryJoin-Config-Xml.md) collection. A `Join` object provides the configuration for a joining table.
`Order` | The corresponding [`QueryOrder`](Database-QueryOrder-Config-Xml.md) collection. An `Order` object defines the order (sequence).
`Where` | The corresponding [`QueryWhere`](Database-QueryWhere-Config-Xml.md) collection. A `Where` object defines the selection/filtering.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
