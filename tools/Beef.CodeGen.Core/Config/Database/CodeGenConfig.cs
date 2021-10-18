// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using OnRamp.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("CodeGeneration", Title = "'CodeGeneration' object (database-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.",
        Markdown = "")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory("CDC", Title = "Provides the _Change Data Capture (CDC)_ configuration.")]
    [CodeGenCategory("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CodeGenCategory("DotNet", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("Event", Title = "Provides the _Event_ configuration.")]
    [CodeGenCategory("Outbox", Title = "Provides the _Event Outbox_ configuration.")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigRootBase<CodeGenConfig>, ISpecialColumnNames
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the `Schema` where the artefacts are defined in the database.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the `Schema` where the artefacts are defined in, or should be created in, the database.", IsImportant = true,
            Description = "This is used as the default `Schema` for all child objects.")]
        public string? Schema { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnNameIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonProperty("columnNameTenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `TenantId`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonProperty("columnNameOrgUnitId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `OrgUnitId`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonProperty("columnNameRowVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `RowVersion`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CreatedBy`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CreatedDate`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameDeletedDate { get; set; }

        /// <summary>
        /// Gets or sets the SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.
        /// </summary>
        [JsonProperty("orgUnitJoinSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.",
            Description = "Defaults to `[Sec].[fnGetUserOrgUnits]()`.")]
        public string? OrgUnitJoinSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL stored procedure that is to be used for `Permission` verification.
        /// </summary>
        [JsonProperty("checkUserPermissionSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The SQL stored procedure that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[spCheckUserHasPermission]`.")]
        public string? CheckUserPermissionSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL function that is to be used for `Permission` verification.
        /// </summary>
        [JsonProperty("getUserPermissionSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The SQL function that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[fnGetUserHasPermission]`.")]
        public string? GetUserPermissionSql { get; set; }

        #endregion

        #region CDC

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonProperty("cdcSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The schema name for the generated `CDC`-related database artefacts.",
            Description = "Defaults to `XCdc` (literal).")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-Tracking.
        /// </summary>
        [JsonProperty("cdcAuditTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The table name for the `Cdc`-Tracking.",
            Description = "Defaults to `CdcTracking` (literal).")]
        public string? CdcTrackingTableName { get; set; }

        /// <summary>
        /// Indicates whether to include the generation of the generic `Cdc`-IdentifierMapping database capabilities.
        /// </summary>
        [JsonProperty("cdcIdentifierMapping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "Indicates whether to include the generation of the generic `Cdc`-IdentifierMapping database capabilities.")]
        public bool? CdcIdentifierMapping { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-IdentifierMapping.
        /// </summary>
        [JsonProperty("cdcIdentifierMappingTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The table name for the `Cdc`-IdentifierMapping.",
            Description = "Defaults to `CdcIdentifierMapping` (literal).")]
        public string? CdcIdentifierMappingTableName { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the `Cdc`-IdentifierMapping.
        /// </summary>
        [JsonProperty("cdcIdentifierMappingStoredProcedureName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The table name for the `Cdc`-IdentifierMapping.",
            Description = "Defaults to `spCreateCdcIdentifierMapping` (literal).")]
        public string? CdcIdentifierMappingStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("cdcExcludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("DotNet", Title = "The default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking)")]
        public List<string>? CdcExcludeColumnsFromETag { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "None", "Newtonsoft" },
            Description = "Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Indicates whether the .NET collection properties should be pluralized.
        /// </summary>
        [JsonProperty("pluralizeCollectionProperties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "Indicates whether the .NET collection properties should be pluralized.")]
        public bool? PluralizeCollectionProperties { get; set; }

        /// <summary>
        /// Indicates whether the database has (contains) the standard _Beef_ `dbo` schema objects.
        /// </summary>
        [JsonProperty("hasBeefDbo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "Indicates whether the database has (contains) the standard _Beef_ `dbo` schema objects.",
            Description = "Defaults to `true`.")]
        public bool? HasBeefDbo { get; set; }

        #endregion

        #region DotNet

        /// <summary>
        /// Gets or sets the option to automatically rename the SQL Tables and Columns for use in .NET.
        /// </summary>
        [JsonProperty("autoDotNetRename", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DotNet", Title = "The option to automatically rename the SQL Tables and Columns for use in .NET.", Options = new string[] { "None", "PascalCase", "SnakeKebabToPascalCase" },
            Description = "Defaults `SnakeKebabToPascalCase` that will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.")]
        public string? AutoDotNetRename { get; set; }

        /// <summary>
        /// Gets or sets the entity scope option.
        /// </summary>
        [JsonProperty("entityScope", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("DotNet", Title = "The entity scope option.", Options = new string[] { "Common", "Business", "Autonomous" },
            Description = "Defaults to `Common` for backwards compatibility; `Autonomous` is recommended. Determines where the entity is scoped/defined, being `Common` or `Business` (i.e. not externally visible).")]
        public string? EntityScope { get; set; }

        #endregion

        #region Event

        /// <summary>
        /// Gets or sets the root for the event name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The root for the event name by prepending to all event subject names via CDC.",
            Description = "Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be extended within the `Entity`(s).", IsImportant = true)]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Subject when an Event is published via CDC.", Options = new string[] { "NameOnly", "NameAndKey" },
            Description = "Defaults to `NameAndKey` (being the event subject name appended with the corresponding unique key.)`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The formatting for the Action when an Event is published via CDC.", Options = new string[] { "None", "PastTense" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required, i.e. as-is).")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Gets or sets the URI root for the event source by prepending to all event source URIs.
        /// </summary>
        [JsonProperty("eventSourceRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The URI root for the event source by prepending to all event source URIs for CDC.",
            Description = "The event source is only updated where an `EventSourceKind` is not `None`. This can be extended within the `Entity`(s).")]
        public string? EventSourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the URI kind for the event source URIs.
        /// </summary>
        [JsonProperty("eventSourceKind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The URI kind for the event source URIs for CDC.", Options = new string[] { "None", "Absolute", "Relative", "RelativeOrAbsolute" },
            Description = "Defaults to `None` (being the event source is not updated).")]
        public string? EventSourceKind { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Source when an Event is published.
        /// </summary>
        [JsonProperty("eventSourceFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Source when an Event is published via CDC.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndGlobalId" },
            Description = "Defaults to `NameAndKey` (being the event subject name appended with the corresponding unique key.)`.")]
        public string? EventSourceFormat { get; set; }

        #endregion

        #region Outbox

        /// <summary>
        /// Indicates whether events will publish using the outbox pattern and therefore the event outbox artefacts are required.
        /// </summary>
        [JsonProperty("eventOutbox", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Outbox", Title = "Indicates whether events will publish using the outbox pattern and therefore the event outbox artefacts are required.")]
        public bool? EventOutbox { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `EventOutbox`.
        /// </summary>
        [JsonProperty("eventOutboxTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Outbox", Title = "The table name for the `EventOutbox`.",
            Description = "Defaults to `EventOutbox` (literal).")]
        public string? EventOutboxTableName { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonProperty("pathBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonProperty("pathDatabaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database/Schema` (literal). For example `Beef.Demo.Database/Schema`.")]
        public string? PathDatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonProperty("pathDatabaseMigrations", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database/Migrations` (literal). For example `Beef.Demo.Database/Migrations`.")]
        public string? PathDatabaseMigrations { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Business-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("pathBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Business-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? PathBusiness { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the CDC-related (.NET) artefacts.
        /// </summary>
        [JsonProperty("PathCdcPublisher", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the CDC-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Cdc` (literal). For example `Beef.Demo.Cdc`.")]
        public string? PathCdcPublisher { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.
        /// </summary>
        [JsonProperty("orgUnitImmutable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.", IsImportant = true,
            Description = "This is only applicable for stored procedures.")]
        public bool? OrgUnitImmutable { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Common-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceCommon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Common-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? NamespaceCommon { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Business-related .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBusiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Business-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? NamespaceBusiness { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the CDC-related publisher .NET artefacts.
        /// </summary>
        [JsonProperty("NamespaceCdcPublisher", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the CDC-related publisher .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.CdcPublisher` (literal). For example `Beef.Demo.CdcPublisher`.")]
        public string? NamespaceCdcPublisher { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableConfig"/> collection.
        /// </summary>
        [JsonProperty("tables", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Table` collection.", IsImportant = true,
            Markdown = "A `Table` object provides the relationship to an existing table within the database.")]
        public List<TableConfig>? Tables { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryConfig"/> collection.
        /// </summary>
        [JsonProperty("queries", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Query` collection.", IsImportant = true,
            Markdown = "A `Query` object provides the primary configuration for a query, including multiple table joins.")]
        public List<QueryConfig>? Queries { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="CdcConfig"/> collection.
        /// </summary>
        [JsonProperty("cdc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Cdc` collection.", IsImportant = true,
            Markdown = "A `Cdc` object provides the primary configuration for Change Data Capture (CDC), including multiple table joins to form a composite entity.")]
        public List<CdcConfig>? Cdc { get; set; }

        /// <summary>
        /// Gets all the tables that require an EfModel to be generated.
        /// </summary>
        public List<TableConfig> EFModels => Tables!.Where(x => CompareValue(x.EfModel, true)).ToList();

        /// <summary>
        /// Gets or sets the list of tables that exist within the database.
        /// </summary>
        public List<DbTable>? DbTables { get; private set; }

        /// <summary>
        /// Gets the company name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? Company => CodeGenArgs!.GetCompany(true);

        /// <summary>
        /// Gets the application name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? AppName => CodeGenArgs!.GetAppName(true);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            LoadDbTablesConfig();

            Schema = DefaultWhereNull(Schema, () => "dbo");

            PathBase = DefaultWhereNull(PathBase, () => $"{Company}.{AppName}");
            PathDatabaseSchema = DefaultWhereNull(PathDatabaseSchema, () => $"{PathBase}.Database/Schema");
            PathDatabaseMigrations = DefaultWhereNull(PathDatabaseMigrations, () => $"{PathBase}.Database/Migrations");
            PathBusiness = DefaultWhereNull(PathBusiness, () => $"{PathBase}.Business");
            PathCdcPublisher = DefaultWhereNull(PathCdcPublisher, () => $"{PathBase}.CdcPublisher");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => $"{Company}.{AppName}");
            NamespaceCommon = DefaultWhereNull(NamespaceCommon, () => $"{NamespaceBase}.Common");
            NamespaceBusiness = DefaultWhereNull(NamespaceBusiness, () => $"{NamespaceBase}.Business");
            NamespaceCdcPublisher = DefaultWhereNull(NamespaceCdcPublisher, () => $"{NamespaceBase}.CdcPublisher");

            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => "IsDeleted");
            ColumnNameTenantId = DefaultWhereNull(ColumnNameTenantId, () => "TenantId");
            ColumnNameOrgUnitId = DefaultWhereNull(ColumnNameOrgUnitId, () => "OrgUnitId");
            ColumnNameRowVersion = DefaultWhereNull(ColumnNameRowVersion, () => "RowVersion");
            ColumnNameCreatedBy = DefaultWhereNull(ColumnNameCreatedBy, () => "CreatedBy");
            ColumnNameCreatedDate = DefaultWhereNull(ColumnNameCreatedDate, () => "CreatedDate");
            ColumnNameUpdatedBy = DefaultWhereNull(ColumnNameUpdatedBy, () => "UpdatedBy");
            ColumnNameUpdatedDate = DefaultWhereNull(ColumnNameUpdatedDate, () => "UpdatedDate");
            ColumnNameDeletedBy = DefaultWhereNull(ColumnNameDeletedBy, () => "UpdatedBy");
            ColumnNameDeletedDate = DefaultWhereNull(ColumnNameDeletedDate, () => "UpdatedDate");
            OrgUnitJoinSql = DefaultWhereNull(OrgUnitJoinSql, () => "[Sec].[fnGetUserOrgUnits]()");
            CheckUserPermissionSql = DefaultWhereNull(CheckUserPermissionSql, () => "[Sec].[spCheckUserHasPermission]");
            GetUserPermissionSql = DefaultWhereNull(GetUserPermissionSql, () => "[Sec].[fnGetUserHasPermission]");
            CdcSchema = DefaultWhereNull(CdcSchema, () => "XCdc");
            CdcTrackingTableName = DefaultWhereNull(CdcTrackingTableName, () => "CdcTracking");
            CdcIdentifierMappingTableName = DefaultWhereNull(CdcIdentifierMappingTableName, () => "CdcIdentifierMapping");
            CdcIdentifierMappingStoredProcedureName = DefaultWhereNull(CdcIdentifierMappingStoredProcedureName, () => "spCreateCdcIdentifierMapping");
            HasBeefDbo = DefaultWhereNull(HasBeefDbo, () => true);
            EntityScope = DefaultWhereNull(EntityScope, () => "Common");
            EventSourceKind = DefaultWhereNull(EventSourceKind, () => "None");
            EventSourceFormat = DefaultWhereNull(EventSourceFormat, () => "NameAndKey");
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => "NameAndKey");
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "None");
            EventOutboxTableName = DefaultWhereNull(EventOutboxTableName, () => "EventOutbox");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "Newtonsoft");
            AutoDotNetRename = DefaultWhereNull(AutoDotNetRename, () => "SnakeKebabToPascalCase");

            if (Queries == null)
                Queries = new List<QueryConfig>();

            foreach (var query in Queries)
            {
                query.Prepare(Root!, this);
            }

            if (Tables == null)
                Tables = new List<TableConfig>();

            foreach (var table in Tables)
            {
                table.Prepare(Root!, this);
            }

            if (Cdc == null)
                Cdc = new List<CdcConfig>();

            foreach (var cdc in Cdc)
            {
                cdc.Prepare(Root!, this);
            }
        }

        /// <summary>
        /// Load the database table and columns configuration.
        /// </summary>
        private void LoadDbTablesConfig()
        {
            CodeGenArgs?.Logger?.Log(LogLevel.Information, $"  Querying database to infer table(s)/column(s) configuration...");

            var cs = CodeGenArgs?.ConnectionString ?? throw new CodeGenException($"Connection string must be explicitly specified using Environment Variable '{CodeGenArgs?.ConnectionStringEnvironmentVariableName}' or as a runtime option.");

            var sw = Stopwatch.StartNew();
            using var db = new SqlConnection(cs);
            DbTables = DbTable.LoadTablesAndColumnsAsync(db, false).GetAwaiter().GetResult();

            sw.Stop();
            CodeGenArgs?.Logger?.Log(LogLevel.Information, $"    Database query complete [{sw.ElapsedMilliseconds}ms]");
            CodeGenArgs?.Logger?.Log(LogLevel.Information, string.Empty);
        }

        /// <summary>
        /// Renames for usage in .NET using the <see cref="AutoDotNetRename"/> option.
        /// </summary>
        /// <param name="name">The value to rename.</param>
        /// <returns>The renamed value.</returns>
        public string? RenameForDotNet(string? name)
        {
            if (string.IsNullOrEmpty(name) || AutoDotNetRename == "None")
                return name;

            if (AutoDotNetRename == "PascalCase")
                return StringConversion.ToPascalCase(name);

            // That only leaves SnakeKebabToPascalCase.
            var sb = new StringBuilder();
            name.Split(new char[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries).ForEach(part => sb.Append(StringConversion.ToPascalCase(part)));
            return sb.ToString();
        }
    }
}