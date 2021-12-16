# 'QueryJoin' object (database-driven)

The `QueryJoin` object defines a join to another (or same) table within a query. The `Type` defines the join type, such as inner join, etc. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name).

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
The `QueryJoin` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the table to join. [Mandatory]
`schema` | The schema name of the table to join.<br/>&dagger; Defaults to `Table.Schema`; i.e. same schema.
`alias` | The `Schema.Table` alias name.<br/>&dagger; Will automatically default where not specified.
**`type`** | The SQL join type. Valid options are: `Inner`, `Left`, `Right`, `Full`.<br/>&dagger; Defaults to `Inner`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output.<br/>&dagger; Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output.<br/>&dagger; Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.<br/>&dagger; Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

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

