# 'CodeGeneration' object (database-driven)

The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.

<br/>

## Property categories
The `CodeGeneration` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Path`](#Path) | Provides the _Path (Directory)_ configuration for the generated artefacts.
[`DotNet`](#DotNet) | Provides the _.NET_ configuration.
[`EntityFramework`](#EntityFramework) | Provides the _Entity Framework (EF) model_ configuration.
[`Outbox`](#Outbox) | Provides the _Event Outbox_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
[`Namespace`](#Namespace) | Provides the _.NET Namespace_ configuration for the generated artefacts.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`columnNameIsDeleted` | The column name for the `IsDeleted` capability.<br/>&dagger; Defaults to `IsDeleted`.
`columnNameTenantId` | The column name for the `TenantId` capability.<br/>&dagger; Defaults to `TenantId`.
`columnNameOrgUnitId` | The column name for the `OrgUnitId` capability.<br/>&dagger; Defaults to `OrgUnitId`.
`columnNameRowVersion` | The column name for the `RowVersion` capability.<br/>&dagger; Defaults to `RowVersion`.
`columnNameCreatedBy` | The column name for the `CreatedBy` capability.<br/>&dagger; Defaults to `CreatedBy`.
`columnNameCreatedDate` | The column name for the `CreatedDate` capability.<br/>&dagger; Defaults to `CreatedDate`.
`columnNameUpdatedBy` | The column name for the `UpdatedBy` capability.<br/>&dagger; Defaults to `UpdatedBy`.
`columnNameUpdatedDate` | The column name for the `UpdatedDate` capability.<br/>&dagger; Defaults to `UpdatedDate`.
`columnNameDeletedBy` | The column name for the `DeletedBy` capability.<br/>&dagger; Defaults to `UpdatedBy`.
`columnNameDeletedDate` | The column name for the `DeletedDate` capability.<br/>&dagger; Defaults to `UpdatedDate`.
`orgUnitJoinSql` | The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.<br/>&dagger; Defaults to `[Sec].[fnGetUserOrgUnits]()`.
`checkUserPermissionSql` | The SQL stored procedure that is to be used for `Permission` verification.<br/>&dagger; Defaults to `[Sec].[spCheckUserHasPermission]`.
`getUserPermissionSql` | The SQL function that is to be used for `Permission` verification.<br/>&dagger; Defaults to `[Sec].[fnGetUserHasPermission]`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
`aliasColumns` | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.<br/>&dagger; Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.

<br/>

## Path
Provides the _Path (Directory)_ configuration for the generated artefacts.

Property | Description
-|-
`pathBase` | The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.<br/>&dagger; Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`pathDatabaseSchema` | The path (directory) for the Schema Database-related artefacts.<br/>&dagger; Defaults to `PathBase` + `.Database/Schema` (literal). For example `Beef.Demo.Database/Schema`.
`pathDatabaseMigrations` | The path (directory) for the Schema Database-related artefacts.<br/>&dagger; Defaults to `PathBase` + `.Database/Migrations` (literal). For example `Beef.Demo.Database/Migrations`.
`pathBusiness` | The path (directory) for the Business-related (.NET) artefacts.<br/>&dagger; Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.

<br/>

## DotNet
Provides the _.NET_ configuration.

Property | Description
-|-
`autoDotNetRename` | The option to automatically rename the SQL Tables and Columns for use in .NET. Valid options are: `None`, `PascalCase`, `SnakeKebabToPascalCase`.<br/>&dagger; Defaults `SnakeKebabToPascalCase` that will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.
`preprocessorDirectives` | Indicates whether to use preprocessor directives in the generated output.
**`collectionType`** | The collection type. Valid options are: `JSON`, `UDT`.<br/>&dagger; Values are `JSON` being a JSON array (preferred) or `UDT` for a User-Defined Type (legacy). Defaults to `JSON`.

<br/>

## EntityFramework
Provides the _Entity Framework (EF) model_ configuration.

Property | Description
-|-
`efModel` | Indicates whether an `Entity Framework` .NET (C#) model is to be generated for all tables.<br/>&dagger; This can be overridden within the `Table`(s).

<br/>

## Outbox
Provides the _Event Outbox_ configuration.

Property | Description
-|-
`outbox` | Indicates whether to generate the event outbox SQL and .NET artefacts.<br/>&dagger; Defaults to `false`.
`outboxSchema` | The schema name of the event outbox table.<br/>&dagger; Defaults to `Outbox` (literal).
`outboxSchemaCreate` | Indicates whether to create the `OutboxSchema` within the database.<br/>&dagger; Defaults to `true`.
`outboxTable` | The name of the event outbox table.<br/>&dagger; Defaults to `EventOutbox` (literal).
`outboxEnqueueStoredProcedure` | The stored procedure name for the event outbox enqueue.<br/>&dagger; Defaults to `spEventOutboxEnqueue` (literal).
`outboxDequeueStoredProcedure` | The stored procedure name for the event outbox dequeue.<br/>&dagger; Defaults to `spEventOutboxDequeue` (literal).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`orgUnitImmutable`** | Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.<br/>&dagger; This is only applicable for stored procedures.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`namespaceBase` | The base Namespace (root) for the .NET artefacts.<br/>&dagger; Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`namespaceCommon` | The Namespace (root) for the Common-related .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.
`namespaceBusiness` | The Namespace (root) for the Business-related .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`namespaceOutbox` | The Namespace (root) for the Outbox-related Publisher .NET artefacts.<br/>&dagger; Defaults to `NamespaceBusiness`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`tables`** | The corresponding [`Table`](Database-Table-Config.md) collection.<br/><br/>A `Table` object provides the relationship to an existing table within the database.
**`queries`** | The corresponding [`Query`](Database-Query-Config.md) collection.<br/><br/>A `Query` object provides the primary configuration for a query, including multiple table joins.

