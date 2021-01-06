# 'CodeGeneration' object (database-driven) - YAML/JSON

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
`columnNameIsDeleted` | The column name for the `IsDeleted` capability. Defaults to `IsDeleted`.
`columnNameTenantId` | The column name for the `TenantId` capability. Defaults to `TenantId`.
`columnNameOrgUnitId` | The column name for the `OrgUnitId` capability. Defaults to `OrgUnitId`.
`columnNameRowVersion` | The column name for the `RowVersion` capability. Defaults to `RowVersion`.
`columnNameCreatedBy` | The column name for the `CreatedBy` capability. Defaults to `CreatedBy`.
`columnNameCreatedDate` | The column name for the `CreatedDate` capability. Defaults to `CreatedDate`.
`columnNameUpdatedBy` | The column name for the `UpdatedBy` capability. Defaults to `UpdatedBy`.
`columnNameUpdatedDate` | The column name for the `UpdatedDate` capability. Defaults to `UpdatedDate`.
`columnNameDeletedBy` | The column name for the `DeletedBy` capability. Defaults to `UpdatedBy`.
`columnNameDeletedDate` | The column name for the `DeletedDate` capability. Defaults to `UpdatedDate`.
`orgUnitJoinSql` | The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification. Defaults to `[Sec].[fnGetUserOrgUnits]()`.
`checkUserPermissionSql` | The SQL stored procedure that is to be used for `Permission` verification. Defaults to `[Sec].[spCheckUserHasPermission]`.
`getUserPermissionSql` | The SQL function that is to be used for `Permission` verification. Defaults to `[Sec].[fnGetUserHasPermission]`.

<br/>

## CDC
Provides the _Change Data Capture (CDC)_ configuration.

Property | Description
-|-
`cdcSchema` | The schema name for the generated `CDC`-related database artefacts. Defaults to `Cdc` (literal).
`cdcAuditTableName` | The table name for the `Cdc`-Tracking. Defaults to `CdcTracking` (literal).
**`eventSubjectRoot`** | The root for the event name by prepending to all event subject names. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overidden within the `Entity`(s).
**`eventActionFormat`** | The formatting for the Action when an Event is published. Valid options are: `None`, `UpperCase`, `PastTense`, `PastTenseUpperCase`. Defaults to `None` (no formatting required).
`jsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `None`, `Newtonsoft`. Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).
`pluralizeCollectionProperties` | Indicates whether the .NET collection properties should be pluralized.

<br/>

## Path
Provides the _Path (Directory)_ configuration for the generated artefacts.

Property | Description
-|-
`pathDatabase` | The path (directory) for the Database-related artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Database` (literal). For example `Beef.Demo.Database`.
`pathBusiness` | The path (directory) for the Business-related (.NET) artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Business` (literal). For example `Beef.Demo.Business`.
`pathCdc` | The path (directory) for the CDC-related (.NET) artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Cdc` (literal). For example `Beef.Demo.Cdc`.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`namespaceCommon` | The root Namespace for the Common-related (.NET) artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Common` (literal). For example `Beef.Demo.Common`.
`namespaceBusiness` | The root Namespace for the Business-related (.NET) artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Business` (literal). For example `Beef.Demo.Business`.
`namespaceCdc` | The path (directory) for the CDC-related (.NET) artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter) + `.Cdc` (literal). For example `Beef.Demo.Cdc`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`tables`** | The corresponding [`Table`](Database-Table-Config.md) collection. A `Table` object provides the relationship to an existing table within the database.
**`queries`** | The corresponding [`Query`](Database-Query-Config.md) collection. A `Query` object provides the primary configuration for a query, including multiple table joins.
**`cdc`** | The corresponding [`Cdc`](Database-Cdc-Config.md) collection. A `Cdc` object provides the primary configuration for Change Data Capture (CDC), including multiple table joins to form a composite entity.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
