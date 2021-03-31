# 'CodeGeneration' object (database-driven) - XML

The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.

<br/>

## Property categories
The `CodeGeneration` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`CDC`](#CDC) | Provides the _Change Data Capture (CDC)_ configuration.
[`Path`](#Path) | Provides the _Path (Directory)_ configuration for the generated artefacts.
[`Namespace`](#Namespace) | Provides the _.NET Namespace_ configuration for the generated artefacts.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`ColumnNameIsDeleted` | The column name for the `IsDeleted` capability. Defaults to `IsDeleted`.
`ColumnNameTenantId` | The column name for the `TenantId` capability. Defaults to `TenantId`.
`ColumnNameOrgUnitId` | The column name for the `OrgUnitId` capability. Defaults to `OrgUnitId`.
`ColumnNameRowVersion` | The column name for the `RowVersion` capability. Defaults to `RowVersion`.
`ColumnNameCreatedBy` | The column name for the `CreatedBy` capability. Defaults to `CreatedBy`.
`ColumnNameCreatedDate` | The column name for the `CreatedDate` capability. Defaults to `CreatedDate`.
`ColumnNameUpdatedBy` | The column name for the `UpdatedBy` capability. Defaults to `UpdatedBy`.
`ColumnNameUpdatedDate` | The column name for the `UpdatedDate` capability. Defaults to `UpdatedDate`.
`ColumnNameDeletedBy` | The column name for the `DeletedBy` capability. Defaults to `UpdatedBy`.
`ColumnNameDeletedDate` | The column name for the `DeletedDate` capability. Defaults to `UpdatedDate`.
`OrgUnitJoinSql` | The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification. Defaults to `[Sec].[fnGetUserOrgUnits]()`.
`CheckUserPermissionSql` | The SQL stored procedure that is to be used for `Permission` verification. Defaults to `[Sec].[spCheckUserHasPermission]`.
`GetUserPermissionSql` | The SQL function that is to be used for `Permission` verification. Defaults to `[Sec].[fnGetUserHasPermission]`.

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`CdcSchema` | The schema name for the generated `CDC`-related database artefacts. Defaults to `Cdc` (literal).
`CdcAuditTableName` | The table name for the `Cdc`-Tracking. Defaults to `CdcTracking` (literal).
`HasCdcIdentifierMapping` | The option to exclude the generation of the generic `Cdc`-IdentifierMapping database capabilities. Valid options are: `No`, `Yes`.
`CdcIdentifierMappingTableName` | The table name for the `Cdc`-IdentifierMapping. Defaults to `CdcIdentifierMapping` (literal).
`CdcIdentifierMappingStoredProcedureName` | The table name for the `Cdc`-IdentifierMapping. Defaults to `spCreateCdcIdentifierMapping` (literal).
**`EventSubjectRoot`** | The root for the event name by prepending to all event subject names. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s).
**`EventActionFormat`** | The formatting for the Action when an Event is published. Valid options are: `None`, `UpperCase`, `PastTense`, `PastTenseUpperCase`. Defaults to `None` (no formatting required).
`JsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `None`, `Newtonsoft`. Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).
`PluralizeCollectionProperties` | Indicates whether the .NET collection properties should be pluralized.
`HasBeefDbo` | Indicates whether the database has (contains) the standard _Beef_ `dbo` schema objects. Defaults to `true`.

<br/>

## Path
Provides the _Path (Directory)_ configuration for the generated artefacts.

Property | Description
-|-
`PathBase` | The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`PathDatabaseSchema` | The path (directory) for the Schema Database-related artefacts. Defaults to `PathBase` + `.Database/Schema` (literal). For example `Beef.Demo.Database/Schema`.
`PathDatabaseMigrations` | The path (directory) for the Schema Database-related artefacts. Defaults to `PathBase` + `.Database/Migrations` (literal). For example `Beef.Demo.Database/Migrations`.
`PathBusiness` | The path (directory) for the Business-related (.NET) artefacts. Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`PathCdc` | The path (directory) for the CDC-related (.NET) artefacts. Defaults to `PathBase` + `.Cdc` (literal). For example `Beef.Demo.Cdc`.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`NamespaceBase` | The base Namespace (root) for the .NET artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`NamespaceCommon` | The Namespace (root) for the Common-related .NET artefacts. Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.
`NamespaceBusiness` | The Namespace (root) for the Business-related .NET artefacts. Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`NamespaceCdc` | The Namespace (root) for the CDC-related .NET artefacts. Defaults to `NamespaceBase` + `.Cdc` (literal). For example `Beef.Demo.Cdc`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`Tables`** | The corresponding [`Table`](Database-Table-Config-Xml.md) collection. A `Table` object provides the relationship to an existing table within the database.
**`Queries`** | The corresponding [`Query`](Database-Query-Config-Xml.md) collection. A `Query` object provides the primary configuration for a query, including multiple table joins.
**`Cdc`** | The corresponding [`Cdc`](Database-Cdc-Config-Xml.md) collection. A `Cdc` object provides the primary configuration for Change Data Capture (CDC), including multiple table joins to form a composite entity.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
