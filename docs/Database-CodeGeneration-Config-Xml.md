# 'CodeGeneration' object (database-driven) - XML

The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.

<br/>

## Property categories
The `CodeGeneration` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories. The properties with a bold name are those that are more typically used (considered more important).

Category | Description
-|-
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`CDC`](#CDC) | Provides the _Change Data Capture (CDC)_ configuration.
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
**`EventSubjectRoot`** | The root for the event name by prepending to all event subject names. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overidden within the `Entity`(s).
**`EventActionFormat`** | The formatting for the Action when an Event is published. Valid options are: `None`, `UpperCase`, `PastTense`, `PastTenseUpperCase`. Defaults to `None` (no formatting required)`.
`JsonSerializer` | The JSON Serializer to use for JSON property attribution. Valid options are: `None`, `Newtonsoft`. Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).
`PluralizeCollectionProperties` | Indicates whether the .NET collection properties should be pluralized.

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
