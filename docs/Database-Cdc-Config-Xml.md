# 'Cdc' object (database-driven) - XML

The `Cdc` object enables the definition of the primary table, one-or-more child tables and their relationships, to enable Change Data Capture (CDC) event publishing. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.

<br/>

## Property categories
The `Cdc` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`CDC`](#CDC) | Provides the _Change Data Capture (CDC)_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`Name`** | The name of the primary table.
`Schema` | The schema name of the primary table. Defaults to `dbo`.
`Alias` | The `Schema.Table` alias name. Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.
**`AliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming. Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`StoredProcedureName` | The `CDC` get envelope data stored procedure name. Defaults to `spExecute` (literal) + `Name` + `CdcEnvelope` (literal); e.g. `spExecuteTableNameCdcEnvelope`.
`CdcSchema` | The schema name for the generated `CDC`-related database artefacts. Defaults to `CodeGenConfig.CdcSchema`.
`EnvelopeTableName` | The corresponding `CDC` Envelope table name. Defaults to `Name` + `Envelope` (literal).
`ModelName` | The .NET model name. Defaults to `Name`.
`DataConstructor` | The access modifier for the generated CDC `Data` constructor. Valid options are: `Public`, `Private`, `Protected`. Defaults to `Public`.
`DatabaseName` | The .NET database interface name. Defaults to `IDatabase`.
`EventSubject` | The event subject. Defaults to `ModelName`. Note: when used in code-generation the `CodeGenConfig.EventSubjectRoot` will be prepended where specified.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`Joins`** | The corresponding [`CdcJoin`](Database-CdcJoin-Config-Xml.md) collection. A `Join` object provides the configuration for a joining table.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
