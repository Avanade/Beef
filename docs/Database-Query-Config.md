# 'Query' object (database-driven) - YAML/JSON

The `Query` object enables the definition of more complex multi-table queries (`Joins`) that would primarily result in a database _View_. The primary table `Name` for the query is required to be specified. Multiple queries can be specified for the same table(s). The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name). Additional `Where` and `Order` configuration can also be added as required.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml
queries:
- { name: Table, schema: Test, view: true, viewName: vwTestQuery, excludeColumns: [CreatedBy, UpdatedBy], permission: TestSec,
    joins: [
      { name: Person, schema: Demo, excludeColumns: [CreatedDate, UpdatedDate], aliasColumns: [RowVersion ^ RowVersionP],
        on: [
          { name: PersonId, toColumn: TableId }
        ]
      }
    ]
  }
```

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
**`name`** | The name of the primary table of the query.
`schema` | The schema name of the primary table of the view. Defaults to `CodeGeneration.dbo`.
`alias` | The `Schema.Table` alias name. Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## View
Provides the _View_ configuration.

Property | Description
-|-
`view` | Indicates whether a `View` is to be generated.
`viewName` | The `View` name. Defaults to `vw` + `Name`; e.g. `vwTableName`.
`viewSchema` | The schema name for the `View`. Defaults to `Schema`.

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`cdc` | Indicates whether the Change Data Capture (CDC) related artefacts are to be generated.
`cdcName` | The `View` name. Defaults to `CodeGeneration.sp` (literal) + `Name` + `Outbox` (literal); e.g. `spTableNameOutbox`.
`cdcSchema` | The schema name for the `Cdc`-related database artefacts. Defaults to `CodeGeneration.Schema` + `Cdc` (literal).
`cdcEnvelope` | The corresponding `Cdc` Outbox Envelope table name. Defaults to `CodeGeneration.Name` + `OutboxEnvelope` (literal).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`permission`** | The permission to be used for security permission checking. The suffix is optional, and where not specified will default to `.READ`.

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

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`joins`** | The corresponding [`QueryJoin`](Database-QueryJoin-Config.md) collection. A `Join` object provides the configuration for a joining table.
`order` | The corresponding [`QueryOrder`](Database-QueryOrder-Config.md) collection. An `Order` object defines the order (sequence).
`where` | The corresponding [`QueryWhere`](Database-QueryWhere-Config.md) collection. A `Where` object defines the selection/filtering.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
