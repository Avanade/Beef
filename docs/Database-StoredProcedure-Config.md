# 'StoredProcedure' object (database-driven) - YAML/JSON

The code generation for an `StoredProcedure` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting `Upsert`, `Merge` and ad-hoc queries as required.

The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single row value. The primary key is automatically added as a `Parameter`.
- **`GetAll`** - indicates an ad-hoc query/get (read) returning one or more rows (collection). No `Parameter`s are automatically added.
- **`Create`** - indicates the creation of a row. All columns are added as `Parameter`s.
- **`Update`** - indicates the updating of a row. All columns are added as `Parameter`s.
- **`Upsert`** - indicates the upserting (create or update) of a row. All columns are added as `Parameter`s.
- **`Delete`** - indicates the deleting of a row. The primary key is automatically added as a `Parameter`.
- **`Merge`** - indicates the merging (create, update or delete) of one or more rows (collection) through the use of a [Table-Valued Parameter (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) Type parameter.

<br/>

## Example

A YAML example is as follows:
``` yaml
tables:
- { name: Table, schema: Test, create: true, update: true, upsert: true, delete: true, merge: true, udt: true, getAll: true, getAllOrderBy: [ Name Des ], excludeColumns: [ Other ], permission: TestSec,
    storedProcedures: [
      { name: GetByArgs, type: GetColl, excludeColumns: [ Count ],
        parameters: [
          { name: Name, nullable: true, operator: LIKE },
          { name: MinCount, operator: GE, column: Count },
          { name: MaxCount, operator: LE, column: Count, nullable: true }
        ]
      },
      { name: Get, type: Get, withHints: NOLOCK,
        execute: [
          { statement: EXEC Demo.Before, location: Before },
          { statement: EXEC Demo.After }
        ]
      },
      { name: Update, type: Update }
    ]
  }
```

<br/>

## Property categories
The `StoredProcedure` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Merge`](#Merge) | Provides _Merge_ configuration (where `Type` is `Merge`).
[`Additional`](#Additional) | Provides _additional ad-hoc_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the `StoredProcedure`; generally the verb/action, i.e. `Get`, `Update`, etc. See `StoredProcedureName` for the actual name used in the database.
**`type`** | The stored procedure operation type. Valid options are: `Get`, `GetColl`, `Create`, `Update`, `Upsert`, `Delete`, `Merge`. Defaults to `GetColl`.
**`paging`** | Indicates whether standardized paging support should be added. This only applies where the stored procedure operation `Type` is `GetColl`.
`storedProcedureName` | The `StoredProcedure` name in the database. Defaults to `sp` + `Table.Name` + `Name`; e.g. `spTableName` or `spPersonGet`.

<br/>

## Merge
Provides _Merge_ configuration (where `Type` is `Merge`).

Property | Description
-|-
`mergeOverrideIdentityColumns` | The list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_. This is used to override the default behaviour of using the primary key column(s).

<br/>

## Additional
Provides _additional ad-hoc_ configuration.

Property | Description
-|-
`reselectStatement` | The SQL statement to perform the reselect after a `Create`, `Update` or `Upsert` stored procedure operation `Type`. Defaults to `[{{Table.Schema}}].[sp{{Table.Name}}Get]` passing the primary key column(s).
`intoTempTable` | Indicates whether to select into a `#TempTable` to allow other statements access to the selected data. A `Select * from #TempTable` is also performed (code-generated) where the stored procedure operation `Type` is `GetColl`.
`withHints` | the table hints using the SQL Server `WITH()` statement; the value specified will be used as-is; e.g. `NOLOCK` will result in `WITH(NOLOCK)`.

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
`permission` | The name of the `StoredProcedure` in the database.
**`orgUnitImmutable`** | Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set. Defaults to `Table.OrgUnitImmutable`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated _settable_ output (further filters `Table.IncludeColumns`). Where not specified this indicates that all `Columns` are to be included. Only filters the columns where `Type` is `Get`, `GetColl`, `Create`, `Update` or `Upsert`.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated _settable_ output (further filters `Table.ExcludeColumns`). Where not specified this indicates no `Columns` are to be excluded. Only filters the columns where `Type` is `Get`, `GetColl`, `Create`, `Update` or `Upsert`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`parameters` | The corresponding [`Parameter`](Database-Parameter-Config.md) collection.
`where` | The corresponding [`Where`](Database-Where-Config.md) collection.
`orderby` | The corresponding [`OrderBy`](Database-OrderBy-Config.md) collection.
`execute` | The corresponding [`Execute`](Database-Execute-Config.md) collection.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
