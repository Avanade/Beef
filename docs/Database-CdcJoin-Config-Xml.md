# 'CdcJoin' object (database-driven) - XML

The `CdcJoin` object defines a join to another (or same) table within a CDC entity.  The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.

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
[`IdentifierMapping`](#IdentifierMapping) | Provides the _identifier mapping_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The unique name.
`Schema` | The schema name of the table to join. Defaults to `Table.Schema`; i.e. same schema.
`TableName` | The name of the table to join. Defaults to `Name`. This is used to specify the actual underlying database table name (where the `Name` has been changed to enable uniqueness).
`Alias` | The `Schema.Table` alias name. Will automatically default where not specified.
**`Type`** | The SQL join type. Valid options are: `Cdc`, `Inner`, `Left`, `Right`, `Full`. Defaults to `Cdc`. The `Cdc` value indicates this is a related secondary table that also has Change Data Capture turned on and equally needs to be monitored for changes.

<br/>

## JoinTo
Provides the _join to_ configuration.

Property | Description
-|-
**`JoinTo`** | The name of the table to join to (must be previously specified). Defaults to `Parent.Name`.
**`JoinToSchema`** | The schema name of the table to join to. Defaults to `Parent.Schema`.
`JoinCardinality` | The join cardinality being whether there is a One-to-Many or One-to-One relationship. Valid options are: `OneToMany`, `OneToOne`. Defaults to `OneToMany`. This represents the Parent (`JoinTo`) to child (_this_) relationship.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`AliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

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
`ModelName` | The .NET model name. Defaults to `Name`.
`PropertyName` | The .NET property name. Defaults to `TableName` where `JoinCardinality` is `OneToOne`; otherwise, it will be `Name` suffixed by an `s` except when already ending in `s` where it will be suffixed by an `es`.
`IncludeColumnsOnDelete` | The list of `Column` names that should be included (in addition to the primary key) for a logical delete. Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.

<br/>

## IdentifierMapping
Provides the _identifier mapping_ configuration.

Property | Description
-|-
**`IdentifierMapping`** | Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key. This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s).
**`IdentifierMappingColumns`** | The list of `Column` with related `Schema`/`Table` values (all split by a `^` lookup character) to enable column one-to-one identifier mapping. Each value is formatted as `Column` + `^` + `Schema` + `^` + `Table` where the schema is optional; e.g. `ContactId^dbo^Contact` or `ContactId^Contact`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`On` | The corresponding [`CdcJoinOn`](Database-CdcJoinOn-Config-Xml.md) collection.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
