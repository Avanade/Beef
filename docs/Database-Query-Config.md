# 'Query' object (database-driven)

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
**`name`** | The name of the primary table of the query. [Mandatory]
`schema` | The schema name of the primary table of the view.<br/>&dagger; Defaults to `CodeGeneration.Schema`.
`alias` | The `Schema.Table` alias name.<br/>&dagger; Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output.<br/>&dagger; Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output.<br/>&dagger; Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.<br/>&dagger; Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## View
Provides the _View_ configuration.

Property | Description
-|-
`view` | Indicates whether a `View` is to be generated.
`viewName` | The `View` name.<br/>&dagger; Defaults to `vw` + `Name`; e.g. `vwTableName`.
`viewSchema` | The schema name for the `View`.<br/>&dagger; Defaults to `Schema`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`permission`** | The permission to be used for security permission checking.<br/>&dagger; The suffix is optional, and where not specified will default to `.READ`.

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`columnNameIsDeleted` | The column name for the `IsDeleted` capability.<br/>&dagger; Defaults to `CodeGeneration.IsDeleted`.
`columnNameTenantId` | The column name for the `TenantId` capability.<br/>&dagger; Defaults to `CodeGeneration.TenantId`.
`columnNameOrgUnitId` | The column name for the `OrgUnitId` capability.<br/>&dagger; Defaults to `CodeGeneration.OrgUnitId`.
`columnNameRowVersion` | The column name for the `RowVersion` capability.<br/>&dagger; Defaults to `CodeGeneration.RowVersion`.
`columnNameCreatedBy` | The column name for the `CreatedBy` capability.<br/>&dagger; Defaults to `CodeGeneration.CreatedBy`.
`columnNameCreatedDate` | The column name for the `CreatedDate` capability.<br/>&dagger; Defaults to `CodeGeneration.CreatedDate`.
`columnNameUpdatedBy` | The column name for the `UpdatedBy` capability.<br/>&dagger; Defaults to `CodeGeneration.UpdatedBy`.
`columnNameUpdatedDate` | The column name for the `UpdatedDate` capability.<br/>&dagger; Defaults to `CodeGeneration.UpdatedDate`.
`columnNameDeletedBy` | The column name for the `DeletedBy` capability.<br/>&dagger; Defaults to `CodeGeneration.UpdatedBy`.
`columnNameDeletedDate` | The column name for the `DeletedDate` capability.<br/>&dagger; Defaults to `CodeGeneration.UpdatedDate`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`joins`** | The corresponding [`QueryJoin`](Database-QueryJoin-Config.md) collection.<br/><br/>A `Join` object provides the configuration for a joining table.
`order` | The corresponding [`QueryOrder`](Database-QueryOrder-Config.md) collection.<br/><br/>An `Order` object defines the order (sequence).
`where` | The corresponding [`QueryWhere`](Database-QueryWhere-Config.md) collection.<br/><br/>A `Where` object defines the selection/filtering.

