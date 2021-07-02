# 'Table' object (entity-driven) - XML

The `Table` object identifies an existing database `Table` (or `View`) and defines its code-generation characteristics.

The columns for the table (or view) are inferred from the database schema definition. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns from all the `StoredProcedure` children. A table can be defined more that once to enable different column configurations as required.

In addition to the primary [`Stored Procedures`](#Collections) generation, the following types of artefacts can also be generated:
- [Entity Framework](#EntityFramework) - Enables the generation of C# model code for [Entity Framework](https://docs.microsoft.com/en-us/ef/) data access.
- [UDT and TVP](#UDT) - Enables the [User-Defined Tables (UDT)](https://docs.microsoft.com/en-us/sql/relational-databases/server-management-objects-smo/tasks/using-user-defined-tables) and [Table-Valued Parameters (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) to enable collections of data to be passed bewteen SQL Server and .NET.

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
**`Name`** | The name of the `Table` in the database.
**`Schema`** | The name of the `Schema` where the `Table` is defined in the database. Defaults to `CodeGeneration.Schema`.
`Alias` | The `Schema.Table` alias name. Will automatically default where not specified.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`IncludeColumns`** | The comma separated list of `Column` names to be included in the underlying generated output. Where not specified this indicates that all `Columns` are to be included.
**`ExcludeColumns`** | The comma seperated list of `Column` names to be excluded from the underlying generated output. Where not specified this indicates no `Columns` are to be excluded.

<br/>

## CodeGen
Provides the _Code Generation_ configuration. These primarily provide a shorthand to create the standard `Get`, `GetAll`, `Create`, `Update`, `Upsert`, `Delete` and `Merge`.

Property | Description
-|-
`Get` | Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.
`GetAll` | Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified. The `GetAllOrderBy` is used to specify the `GetAll` query sort order.
`GetAllOrderBy` | The comma seperated list of `Column` names (including sort order ASC/DESC) to be used as the `GetAll` query sort order.
`Create` | Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.
`Update` | Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.
`Upsert` | Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.
`Delete` | Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.
`Merge` | Indicates whether a `Merge` (insert/update/delete of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified. This will also require a `Udt` (SQL User Defined Table) and `Tvp` (.NET Table-Valued Parameter) to function.

<br/>

## EntityFramework
Provides the _Entity Framework (EF) model_ configuration.

Property | Description
-|-
`EfModel` | Indicates whether an `Entity Framework` .NET (C#) model is to be generated.
`EfModelName` | The .NET (C#) EntityFramework (EF) model name. Defaults to `Name`.

<br/>

## UDT
Provides the _User Defined Table_ and _Table-Valued Parameter_ configuration.

Property | Description
-|-
**`Udt`** | Indicates whether a `User Defined Table (UDT)` type should be created.
`UdtExcludeColumns` | The comma seperated list of `Column` names to be excluded from the `User Defined Table (UDT)`. Where not specified this indicates that no `Columns` are to be excluded.
**`Tvp`** | The name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.
`EntityScope` | The entity scope option. Valid options are: `Common`, `Business`, `Autonomous`. Defaults to `CodeGeneration.EntityScope`. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`Permission`** | The permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).
**`OrgUnitImmutable`** | Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set. Defaults to `CodeGeneration.OrgUnitImmutable`. This is only applicable for stored procedures.

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
`StoredProcedures` | The corresponding [`StoredProcedure`](Database-StoredProcedure-Config-Xml.md) collection. A `StoredProcedure` object defines the stored procedure code-generation characteristics.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
