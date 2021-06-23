# 'Table' object (entity-driven) - YAML/JSON

The `Table` object identifies an existing database `Table` (or `View`) and defines its code-generation characteristics.

The columns for the table (or view) are inferred from the database schema definition. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns from all the `StoredProcedure` children. A table can be defined more that once to enable different column configurations as required.

In addition to the primary [`Stored Procedures`](#Collections) generation, the following types of artefacts can also be generated:
- [Entity Framework](#EntityFramework) - Enables the generation of C# model code for [Entity Framework](https://docs.microsoft.com/en-us/ef/) data access.
- [UDT and TVP](#UDT) - Enables the [User-Defined Tables (UDT)](https://docs.microsoft.com/en-us/sql/relational-databases/server-management-objects-smo/tasks/using-user-defined-tables) and [Table-Valued Parameters (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) to enable collections of data to be passed bewteen SQL Server and .NET.

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
The `Table` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`CodeGen`](#CodeGen) | Provides the _Code Generation_ configuration.
[`EntityFramework`](#EntityFramework) | Provides the _Entity Framework (EF) model_ configuration.
[`UDT`](#UDT) | Provides the _User Defined Table_ and _Table-Valued Parameter_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the `Table` in the database.
**`schema`** | The name of the `Schema` where the `Table` is defined in the database. Defaults to `CodeGeneration.Schema`.
`alias` | The `Schema.Table` alias name. Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.

<br/>

## CodeGen
Provides the _Code Generation_ configuration. These primarily provide a shorthand to create the standard `Get`, `GetAll`, `Create`, `Update`, `Upsert`, `Delete` and `Merge`.

Property | Description
-|-
`get` | Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.
`getAll` | Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified. The `GetAllOrderBy` is used to specify the `GetAll` query sort order.
`getAllOrderBy` | The list of `Column` names (including sort order `ASC`/`DESC` literal) to be used as the `GetAll` query sort order. This relates to the `GetAll` selection.
`create` | Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.
`update` | Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.
`upsert` | Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.
`delete` | Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.
`merge` | Indicates whether a `Merge` (insert/update/delete of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified. This will also require a `Udt` (SQL User Defined Table) and `Tvp` (.NET Table-Valued Parameter) to function.

<br/>

## EntityFramework
Provides the _Entity Framework (EF) model_ configuration.

Property | Description
-|-
`efModel` | Indicates whether an `Entity Framework` .NET (C#) model is to be generated.
`efModelName` | The .NET (C#) EntityFramework (EF) model name. Defaults to `Name`.

<br/>

## UDT
Provides the _User Defined Table_ and _Table-Valued Parameter_ configuration.

Property | Description
-|-
**`udt`** | Indicates whether a `User Defined Table (UDT)` type should be created.
`udtExcludeColumns` | The list of `Column` names to be excluded from the `User Defined Table (UDT)`. Where not specified this indicates that no `Columns` are to be excluded.
**`tvp`** | The name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.
`entityScope` | The entity scope option. Valid options are: `Common`, `Business`, `Autonomous`. Defaults to `CodeGeneration.EntityScope`. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`permission`** | The permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).
**`orgUnitImmutable`** | Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set. Defaults to `CodeGeneration.OrgUnitImmutable`. This is only applicable for stored procedures.

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
`storedProcedures` | The corresponding [`StoredProcedure`](Database-StoredProcedure-Config.md) collection. A `StoredProcedure` object defines the stored procedure code-generation characteristics.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
