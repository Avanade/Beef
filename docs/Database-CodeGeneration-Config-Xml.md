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
[`DotNet`](#DotNet) | Provides the _.NET_ configuration.
[`Event`](#Event) | Provides the _Event_ configuration.
[`Outbox`](#Outbox) | Provides the _Event Outbox_ configuration.
[`Auth`](#Auth) | Provides the _Authorization_ configuration.
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
`CdcSchema` | The schema name for the generated `CDC`-related database artefacts. Defaults to `XCdc` (literal).
`CdcAuditTableName` | The table name for the `Cdc`-Tracking. Defaults to `CdcTracking` (literal).
`CdcIdentifierMapping` | Indicates whether to include the generation of the generic `Cdc`-IdentifierMapping database capabilities.
`CdcIdentifierMappingTableName` | The table name for the `Cdc`-IdentifierMapping. Defaults to `CdcIdentifierMapping` (literal).
`CdcIdentifierMappingStoredProcedureName` | The table name for the `Cdc`-IdentifierMapping. Defaults to `spCreateCdcIdentifierMapping` (literal).
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
`PathCdcPublisher` | The path (directory) for the CDC-related (.NET) artefacts. Defaults to `PathBase` + `.Cdc` (literal). For example `Beef.Demo.Cdc`.

<br/>

## DotNet
Provides the _.NET_ configuration.

Property | Description
-|-
`CdcExcludeColumnsFromETag` | The default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking)
`AutoDotNetRename` | The option to automatically rename the SQL Tables and Columns for use in .NET. Valid options are: `None`, `PascalCase`, `SnakeKebabToPascalCase`. Defaults `SnakeKebabToPascalCase` that will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.
`EntityScope` | The entity scope option. Valid options are: `Common`, `Business`, `Autonomous`. Defaults to `Common` for backwards compatibility; `Autonomous` is recommended. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible).

<br/>

## Event
Provides the _Event_ configuration.

Property | Description
-|-
**`EventSubjectRoot`** | The root for the event name by prepending to all event subject names via CDC. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be extended within the `Entity`(s).
`EventSubjectFormat` | The default formatting for the Subject when an Event is published via CDC. Valid options are: `NameOnly`, `NameAndKey`. Defaults to `NameAndKey` (being the event subject name appended with the corresponding unique key.)`.
**`EventActionFormat`** | The formatting for the Action when an Event is published via CDC. Valid options are: `None`, `PastTense`. Defaults to `None` (no formatting required, i.e. as-is).
`EventSourceRoot` | The URI root for the event source by prepending to all event source URIs for CDC. The event source is only updated where an `EventSourceKind` is not `None`. This can be extended within the `Entity`(s).
`EventSourceKind` | The URI kind for the event source URIs for CDC. Valid options are: `None`, `Absolute`, `Relative`, `RelativeOrAbsolute`. Defaults to `None` (being the event source is not updated).
`EventSourceFormat` | The default formatting for the Source when an Event is published via CDC. Valid options are: `NameOnly`, `NameAndKey`, `NameAndGlobalId`. Defaults to `NameAndKey` (being the event subject name appended with the corresponding unique key.)`.

<br/>

## Outbox
Provides the _Event Outbox_ configuration.

Property | Description
-|-
`EventOutbox` | Indicates whether events will publish using the outbox pattern and therefore the event outbox artefacts are required.
`EventOutboxTableName` | The table name for the `EventOutbox`. Defaults to `EventOutbox` (literal).

<br/>

## Auth
Provides the _Authorization_ configuration.

Property | Description
-|-
**`OrgUnitImmutable`** | Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set. This is only applicable for stored procedures.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`NamespaceBase` | The base Namespace (root) for the .NET artefacts. Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.
`NamespaceCommon` | The Namespace (root) for the Common-related .NET artefacts. Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.
`NamespaceBusiness` | The Namespace (root) for the Business-related .NET artefacts. Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.
`NamespaceCdcPublisher` | The Namespace (root) for the CDC-related publisher .NET artefacts. Defaults to `NamespaceBase` + `.CdcPublisher` (literal). For example `Beef.Demo.CdcPublisher`.

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
