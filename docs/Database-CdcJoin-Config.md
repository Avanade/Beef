# 'CdcJoin' object (database-driven) - YAML/JSON

The `CdcJoin` object defines a join to another (or same) table within a CDC entity.  The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml
```

<br/>

## Property categories
The `CdcJoin` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`JoinTo`](#JoinTo) | Provides the _join to_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Database`](#Database) | Provides the _database_ configuration.
[`DotNet`](#DotNet) | Provides the _.NET_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique name.
`schema` | The schema name of the table to join. Defaults to `Table.Schema`; i.e. same schema.
`tableName` | The name of the table to join. Defaults to `Name`. This is used to specify the actual underlying database table name (where the `Name` has been changed to enable uniqueness).
`alias` | The `Schema.Table` alias name. Will automatically default where not specified.
**`type`** | The SQL join type. Valid options are: `Cdc`, `Inner`, `Left`, `Right`, `Full`. Defaults to `Cdc`. The `Cdc` value indicates this is a related secondary table that also has Change Data Capture turned on and equally needs to be monitored for changes.

<br/>

## JoinTo
Provides the _join to_ configuration.

Property | Description
-|-
**`joinTo`** | The name of the table to join to (must be previously specified). Defaults to `Parent.Name`.
**`joinToSchema`** | The schema name of the table to join to. Defaults to `Parent.Schema`.
`joinCardinality` | The join cardinality being whether there is a One-to-Many or One-to-One relationship. Valid options are: `OneToMany`, `OneToOne`. Defaults to `OneToMany`. This represents the Parent (`JoinTo`) to child (_this_) relationship.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## Database
Provides the _database_ configuration.

Property | Description
-|-

<br/>

## DotNet
Provides the _.NET_ configuration.

Property | Description
-|-
`modelName` | The .NET model name. Defaults to `Name`.
`propertyName` | The .NET property name. Defaults to `TableName` where `JoinCardinality` is `OneToOne`; otherwise, it will be `Name` suffixed by an `s` except when already ending in `s` where it will be suffixed by an `es`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`on` | The corresponding [`CdcJoinOn`](Database-CdcJoinOn-Config.md) collection.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
