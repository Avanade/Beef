// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using DbEx.DbSchema;
using DbEx.Migration;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [CodeGenClass("CodeGeneration", Title = "'CodeGeneration' object (database-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.",
        Markdown = "")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory("Columns", Title = "Provides the _Columns_ configuration.")]
    [CodeGenCategory("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CodeGenCategory("DotNet", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("EntityFramework", Title = "Provides the _Entity Framework (EF) model_ configuration.")]
    [CodeGenCategory("Outbox", Title = "Provides the _Event Outbox_ configuration.")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigRootBase<CodeGenConfig>, ISpecialColumnNames
    {
        private static readonly char[] _snakeKebabSeparators = ['_', '-'];

        #region Key

        /// <summary>
        /// Gets or sets the name of the `Schema` where the artefacts are defined in the database.
        /// </summary>
        [JsonPropertyName("schema")]
        [CodeGenProperty("Key", Title = "The name of the `Schema` where the artefacts are defined in, or should be created in, the database.", IsImportant = true,
            Description = "This is used as the default `Schema` for all child objects.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Indicates whether the existing database object should be replaced/altered or whether the object is dropped and recreated.
        /// </summary>
        [JsonPropertyName("replace")]
        [CodeGenProperty("Key", Title = "Indicates whether the existing database object should be replaced/altered or whether the object is dropped and recreated.",
            Description = "Defaults to `true`.")]
        public bool? Replace { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonPropertyName("columnNameIsDeleted")]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonPropertyName("columnNameTenantId")]
        [CodeGenProperty("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `TenantId`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonPropertyName("columnNameOrgUnitId")]
        [CodeGenProperty("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `OrgUnitId`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonPropertyName("columnNameRowVersion")]
        [CodeGenProperty("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `RowVersion`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameCreatedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CreatedBy`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameCreatedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CreatedDate`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameUpdatedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameUpdatedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameDeletedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameDeletedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameDeletedDate { get; set; }

        /// <summary>
        /// Gets or sets the SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.
        /// </summary>
        [JsonPropertyName("orgUnitJoinSql")]
        [CodeGenProperty("Infer", Title = "The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.",
            Description = "Defaults to `[Sec].[fnGetUserOrgUnits]()`.")]
        public string? OrgUnitJoinSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL stored procedure that is to be used for `Permission` verification.
        /// </summary>
        [JsonPropertyName("checkUserPermissionSql")]
        [CodeGenProperty("Infer", Title = "The SQL stored procedure that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[spCheckUserHasPermission]`.")]
        public string? CheckUserPermissionSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL function that is to be used for `Permission` verification.
        /// </summary>
        [JsonPropertyName("getUserPermissionSql")]
        [CodeGenProperty("Infer", Title = "The SQL function that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[fnGetUserHasPermission]`.")]
        public string? GetUserPermissionSql { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonPropertyName("aliasColumns")]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.",
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region DotNet

        /// <summary>
        /// Gets or sets the option to automatically rename the SQL Tables and Columns for use in .NET.
        /// </summary>
        [JsonPropertyName("autoDotNetRename")]
        [CodeGenProperty("DotNet", Title = "The option to automatically rename the SQL Tables and Columns for use in .NET.", Options = ["None", "PascalCase", "SnakeKebabToPascalCase"],
            Description = "Defaults `SnakeKebabToPascalCase` that will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.")]
        public string? AutoDotNetRename { get; set; }

        /// <summary>
        /// Indicates whether to use preprocessor directives in the generated output.
        /// </summary>
        [JsonPropertyName("preprocessorDirectives")]
        [CodeGenProperty("DotNet", Title = "Indicates whether to use preprocessor directives in the generated output.")]
        public bool? PreprocessorDirectives { get; set; }

        /// <summary>
        /// Gets or sets the collection type.
        /// </summary>
        [JsonPropertyName("collectionType")]
        [CodeGenProperty("DotNet", Title = "The collection type.", IsImportant = true, Options = ["JSON", "UDT"],
            Description = "Values are `JSON` being a JSON array (preferred) or `UDT` for a User-Defined Type (legacy). Defaults to `JSON`.")]
        public string? CollectionType { get; set; }

        #endregion

        #region EntityFramework

        /// <summary>
        /// Indicates whether an `Entity Framework` .NET (C#) model is to be generated.
        /// </summary>
        [JsonPropertyName("efModel")]
        [CodeGenProperty("EntityFramework", Title = "Indicates whether an `Entity Framework` .NET (C#) model is to be generated for all tables.",
            Description = "This can be overridden within the `Table`(s).")]
        public bool? EfModel { get; set; }

        #endregion

        #region Outbox

        /// <summary>
        /// Indicates whether to generate the event outbox SQL and .NET artefacts.
        /// </summary>
        [JsonPropertyName("outbox")]
        [CodeGenProperty("Outbox", Title = "Indicates whether to generate the event outbox SQL and .NET artefacts.",
            Description = "Defaults to `false`.")]
        public bool? Outbox { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the event outbox table.
        /// </summary>
        [JsonPropertyName("outboxSchema")]
        [CodeGenProperty("Outbox", Title = "The schema name of the event outbox table.",
            Description = "Defaults to `Outbox` (literal).")]
        public string? OutboxSchema { get; set; }

        /// <summary>
        /// Indicates whether to create the `Outbox`-Schema within the database.
        /// </summary>
        [JsonPropertyName("outboxSchemaCreate")]
        [CodeGenProperty("Outbox", Title = "Indicates whether to create the `OutboxSchema` within the database.",
            Description = "Defaults to `true`.")]
        public bool? OutboxSchemaCreate { get; set; }

        /// <summary>
        /// Gets or sets the name of the event outbox table.
        /// </summary>
        [JsonPropertyName("outboxTable")]
        [CodeGenProperty("Outbox", Title = "The name of the event outbox table.",
            Description = "Defaults to `EventOutbox` (literal).")]
        public string? OutboxTable { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the event outbox enqueue.
        /// </summary>
        [JsonPropertyName("outboxEnqueueStoredProcedure")]
        [CodeGenProperty("Outbox", Title = "The stored procedure name for the event outbox enqueue.",
            Description = "Defaults to `spEventOutboxEnqueue` (literal).")]
        public string? OutboxEnqueueStoredProcedure { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the event outbox dequeue.
        /// </summary>
        [JsonPropertyName("outboxDequeueStoredProcedure")]
        [CodeGenProperty("Outbox", Title = "The stored procedure name for the event outbox dequeue.",
            Description = "Defaults to `spEventOutboxDequeue` (literal).")]
        public string? OutboxDequeueStoredProcedure { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonPropertyName("pathBase")]
        [CodeGenProperty("Path", Title = "The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonPropertyName("pathDatabaseSchema")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database/Schema` (literal). For example `Beef.Demo.Database/Schema`.")]
        public string? PathDatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonPropertyName("pathDatabaseMigrations")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database/Migrations` (literal). For example `Beef.Demo.Database/Migrations`.")]
        public string? PathDatabaseMigrations { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Business-related (.NET) artefacts.
        /// </summary>
        [JsonPropertyName("pathBusiness")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Business-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? PathBusiness { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.
        /// </summary>
        [JsonPropertyName("orgUnitImmutable")]
        [CodeGenProperty("Auth", Title = "Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.", IsImportant = true,
            Description = "This is only applicable for stored procedures.")]
        public bool? OrgUnitImmutable { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceBase")]
        [CodeGenProperty("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `Company` (runtime parameter) + `.` + `AppName` (runtime parameter). For example `Beef.Demo`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Common-related .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceCommon")]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Common-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Common` (literal). For example `Beef.Demo.Common`.")]
        public string? NamespaceCommon { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the Business-related .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceBusiness")]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Business-related .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Business` (literal). For example `Beef.Demo.Business`.")]
        public string? NamespaceBusiness { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the outbox-related .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceOutbox")]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Outbox-related Publisher .NET artefacts.",
            Description = "Defaults to `NamespaceBusiness`.")]
        public string? NamespaceOutbox { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableConfig"/> collection.
        /// </summary>
        [JsonPropertyName("tables")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Table` collection.", IsImportant = true,
            Markdown = "A `Table` object provides the relationship to an existing table within the database.")]
        public List<TableConfig>? Tables { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryConfig"/> collection.
        /// </summary>
        [JsonPropertyName("queries")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Query` collection.", IsImportant = true,
            Markdown = "A `Query` object provides the primary configuration for a query, including multiple table joins.")]
        public List<QueryConfig>? Queries { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseMigrationBase"/>.
        /// </summary>
        public DatabaseMigrationBase? Migrator { get; set; }

        /// <summary>
        /// Gets all the tables that require an EfModel to be generated.
        /// </summary>
        public List<TableConfig> EFModels => Tables!.Where(x => CompareValue(x.EfModel, true)).ToList();

        /// <summary>
        /// Gets or sets the list of tables that exist within the database.
        /// </summary>
        public List<DbTableSchema>? DbTables { get; private set; }

        /// <summary>
        /// Gets the company name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? Company => CodeGenArgs!.GetCompany(true);

        /// <summary>
        /// Gets the application name from the <see cref="IRootConfig.RuntimeParameters"/>.
        /// </summary>
        public string? AppName => CodeGenArgs!.GetAppName(true);

        /// <summary>
        /// Gets the database provider name.
        /// </summary>
        public string? DatabaseProvider => Migrator?.Provider;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            await LoadDbTablesConfigAsync().ConfigureAwait(false);

            Schema = DefaultWhereNull(Schema, () => "dbo");
            Replace = DefaultWhereNull(Replace, () => true);
            EfModel = DefaultWhereNull(EfModel, () => false);

            PathBase = DefaultWhereNull(PathBase, () => $"{Company}.{AppName}");
            PathDatabaseSchema = DefaultWhereNull(PathDatabaseSchema, () => $"{PathBase}.Database/Schema");
            PathDatabaseMigrations = DefaultWhereNull(PathDatabaseMigrations, () => $"{PathBase}.Database/Migrations");
            PathBusiness = DefaultWhereNull(PathBusiness, () => $"{PathBase}.Business");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => $"{Company}.{AppName}");
            NamespaceCommon = DefaultWhereNull(NamespaceCommon, () => $"{NamespaceBase}.Common");
            NamespaceBusiness = DefaultWhereNull(NamespaceBusiness, () => $"{NamespaceBase}.Business");
            NamespaceOutbox = DefaultWhereNull(NamespaceOutbox, () => NamespaceBusiness);

            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => Migrator!.Args.IsDeletedColumnName);
            ColumnNameTenantId = DefaultWhereNull(ColumnNameTenantId, () => Migrator!.Args.TenantIdColumnName);
            ColumnNameOrgUnitId = DefaultWhereNull(ColumnNameOrgUnitId, () => "OrgUnitId");
            ColumnNameRowVersion = DefaultWhereNull(ColumnNameRowVersion, () => Migrator!.Args.RowVersionColumnName);
            ColumnNameCreatedBy = DefaultWhereNull(ColumnNameCreatedBy, () => Migrator!.Args.CreatedByColumnName);
            ColumnNameCreatedDate = DefaultWhereNull(ColumnNameCreatedDate, () => Migrator!.Args.CreatedDateColumnName);
            ColumnNameUpdatedBy = DefaultWhereNull(ColumnNameUpdatedBy, () => Migrator!.Args.UpdatedByColumnName);
            ColumnNameUpdatedDate = DefaultWhereNull(ColumnNameUpdatedDate, () => Migrator!.Args.UpdatedDateColumnName);
            ColumnNameDeletedBy = DefaultWhereNull(ColumnNameDeletedBy, () => Migrator!.Args.UpdatedByColumnName);
            ColumnNameDeletedDate = DefaultWhereNull(ColumnNameDeletedDate, () => Migrator!.Args.UpdatedDateColumnName);

            OrgUnitJoinSql = DefaultWhereNull(OrgUnitJoinSql, () => "[Sec].[fnGetUserOrgUnits]()");
            CheckUserPermissionSql = DefaultWhereNull(CheckUserPermissionSql, () => "[Sec].[spCheckUserHasPermission]");
            GetUserPermissionSql = DefaultWhereNull(GetUserPermissionSql, () => "[Sec].[fnGetUserHasPermission]");
            CollectionType = DefaultWhereNull(CollectionType, () => "JSON");

            OutboxSchema = DefaultWhereNull(OutboxSchema, () => "Outbox");
            OutboxSchemaCreate = DefaultWhereNull(OutboxSchemaCreate, () => true);
            OutboxTable = DefaultWhereNull(OutboxTable, () => "EventOutbox");
            OutboxEnqueueStoredProcedure = DefaultWhereNull(OutboxEnqueueStoredProcedure, () => $"sp{OutboxTable}Enqueue");
            OutboxDequeueStoredProcedure = DefaultWhereNull(OutboxDequeueStoredProcedure, () => $"sp{OutboxTable}Dequeue");

            AutoDotNetRename = DefaultWhereNull(AutoDotNetRename, () => "SnakeKebabToPascalCase");
            PreprocessorDirectives = DefaultWhereNull(PreprocessorDirectives, () => false);

            Queries = await PrepareCollectionAsync(Queries).ConfigureAwait(false);
            Tables = await PrepareCollectionAsync(Tables).ConfigureAwait(false);

            WarnWhereDeprecated(this, this,
                "cdcSchema",
                "cdcAuditTableName",
                "cdcIdentifierMapping",
                "cdcIdentifierMappingTableName",
                "cdcIdentifierMappingStoredProcedureName",
                "cdcExcludeColumnsFromETag",
                "eventSubjectRoot",
                "eventSubjectFormat",
                "eventActionFormat",
                "eventSourceRoot",
                "eventSourceKind",
                "eventSourceFormat",
                "PathCdcPublisher",
                "jsonSerializer",
                "pluralizeCollectionProperties",
                "hasBeefDbo",
                "entityScope",
                "eventOutbox",
                "eventOutboxTableName",
                "namespaceCdcPublisher",
                "entityScope");
        }

        /// <summary>
        /// Load the database table and columns configuration.
        /// </summary>
        private async Task LoadDbTablesConfigAsync()
        {
            CodeGenArgs!.Logger?.Log(LogLevel.Information, "{Content}", string.Empty);
            CodeGenArgs.Logger?.Log(LogLevel.Information, "{Content}", $"Querying database to infer table(s)/column(s) schema configuration...");

            _ = CodeGenArgs.ConnectionString ?? throw new CodeGenException("Connection string must be specified via an environment variable or as a command-line option.");

            var sw = Stopwatch.StartNew();
            Migrator = CodeGenArgs.GetDatabaseMigrator();
            var db = Migrator.Database;
            DbTables = await db.SelectSchemaAsync(Migrator).ConfigureAwait(false);

            sw.Stop();
            CodeGenArgs.Logger?.Log(LogLevel.Information, "{Content}", $"  Database schema query complete [{sw.ElapsedMilliseconds}ms]");
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

            // Try to find the global alias and use.
            var ca = AliasColumns?.Where(x => x.StartsWith(name + "^", StringComparison.Ordinal)).FirstOrDefault();
            if (ca != null)
            {
                var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && parts[1].Length > 0)
                    return parts[1];
            }

            // Now apply the rename according to specified convention.
            if (AutoDotNetRename == "PascalCase")
                return StringConverter.ToPascalCase(name);

            // That only leaves SnakeKebabToPascalCase.
            var sb = new StringBuilder();
            name.Split(_snakeKebabSeparators, StringSplitOptions.RemoveEmptyEntries).ForEach(part => sb.Append(StringConverter.ToPascalCase(part)));
            return sb.ToString();
        }

        /// <summary>
        /// Warn where the property has been deprecated.
        /// </summary>
        /// <param name="root">The root <see cref="CodeGenConfig"/>.</param>
        /// <param name="config">The <see cref="ConfigBase"/>.</param>
        /// <param name="names">The list of deprecated properties.</param>
        internal static void WarnWhereDeprecated(CodeGenConfig root, ConfigBase config, params string[] names)
        {
            if (config.ExtraProperties == null || config.ExtraProperties.Count == 0 || names.Length == 0)
                return;

            foreach (var xp in config.ExtraProperties)
            {
                if (names.Contains(xp.Key))
                    root.CodeGenArgs?.Logger?.LogWarning("{Deprecated}", $"Warning: Config [{config.BuildFullyQualifiedName(xp.Key)}] has been deprecated and will be ignored.");
            }
        }

        /// <summary>
        /// Format the <paramref name="schema"/> and <paramref name="table"/> for user output.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="table">The table.</param>
        /// <returns>The formatted name.</returns>
        public static string? FormatSchemaTableName(string? schema, string? table) => schema != null && schema.Length > 1 ? $"{schema}.{table}" : table;
    }
}