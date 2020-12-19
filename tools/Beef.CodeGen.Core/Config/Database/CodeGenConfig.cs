// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Beef.Data.Database;
using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("CodeGeneration", Title = "'CodeGeneration' object (database-driven)",
        Description = "The `CodeGeneration` object defines global properties that are used to drive the underlying database-driven code generation.",
        Markdown = "")]
    [CategorySchema("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CategorySchema("CDC", Title = "Provides the _Change Data Capture (CDC)_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, IRootConfig, ISpecialColumnNames
    {
        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnNameIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonProperty("columnNameTenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `TenantId`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonProperty("columnNameOrgUnitId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `OrgUnitId`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonProperty("columnNameRowVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `RowVersion`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CreatedBy`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CreatedDate`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `UpdatedBy`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `UpdatedDate`.")]
        public string? ColumnNameDeletedDate { get; set; }

        /// <summary>
        /// Gets or sets the SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.
        /// </summary>
        [JsonProperty("orgUnitJoinSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The SQL table or function that is to be used to join against for security-based `OrgUnitId` verification.",
            Description = "Defaults to `[Sec].[fnGetUserOrgUnits]()`.")]
        public string? OrgUnitJoinSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL stored procedure that is to be used for `Permission` verification.
        /// </summary>
        [JsonProperty("checkUserPermissionSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The SQL stored procedure that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[spCheckUserHasPermission]`.")]
        public string? CheckUserPermissionSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL function that is to be used for `Permission` verification.
        /// </summary>
        [JsonProperty("getUserPermissionSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The SQL function that is to be used for `Permission` verification.",
            Description = "Defaults to `[Sec].[fnGetUserHasPermission]`.")]
        public string? GetUserPermissionSql { get; set; }

        #endregion

        #region CDC

        /// <summary>
        /// Gets or sets the root for the event name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The root for the event name by prepending to all event subject names.",
            Description = "Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overidden within the `Entity`(s).", IsImportant = true)]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The formatting for the Action when an Event is published.", Options = new string[] { "None", "UpperCase", "PastTense", "PastTenseUpperCase" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required)`.")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "None", "Newtonsoft" },
            Description = "Defaults to `Newtonsoft`. This can be overridden within the `Entity`(s).")]
        public string? JsonSerializer { get; set; }

        #endregion

        #region RuntimeParameters

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        public Dictionary<string, string> RuntimeParameters { get; internal set; } = new Dictionary<string, string>();

        /// <summary>
        /// Replaces the <see cref="RuntimeParameters"/> with the specified <paramref name="parameters"/> (copies values).
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public void ReplaceRuntimeParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return;

            foreach (var p in parameters)
            {
                if (RuntimeParameters.ContainsKey(p.Key))
                    RuntimeParameters[p.Key] = p.Value;
                else
                    RuntimeParameters.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Resets the runtime parameters.
        /// </summary>
        public void ResetRuntimeParameters() => RuntimeParameters.Clear();

        /// <summary>
        /// Gets the specified runtime parameter value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="isRequired">Indicates whether the parameter is mandatory and therefore must exist and have non-<c>null</c> value.</param>
        /// <returns>The runtime parameter value.</returns>
        internal string? GetRuntimeParameter(string name, bool isRequired = false)
        {
            if ((!RuntimeParameters.TryGetValue(name, out var value) && isRequired) || (isRequired && string.IsNullOrEmpty(value)))
                throw new CodeGenException($"Runtime parameter '{name}' was not found or had no value; this is required to function.");
            else
                return value;
        }

        /// <summary>
        /// Gets the specified runtime parameter value as a <see cref="bool"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The runtime parameter value.</returns>
        internal bool GetRuntimeBoolParameter(string name)
        {
            var val = GetRuntimeParameter(name);
            if (string.IsNullOrEmpty(val))
                return false;

            if (bool.TryParse(val, out var value))
                return value;

            throw new CodeGenException($"Runtime parameter '{name}' must be a boolean; value '{val}' is invalid.");
        }

        #endregion

        #region KeyValue

        /// <summary>
        /// Gets or sets Key1 value. 
        /// </summary>
        public string? KV1 { get; set; }

        /// <summary>
        /// Gets or sets Key1 value. 
        /// </summary>
        public string? KV2 { get; set; }

        /// <summary>
        /// Gets or sets Key1 value. 
        /// </summary>
        public string? KV3 { get; set; }

        /// <summary>
        /// Gets or sets Key1 value. 
        /// </summary>
        public string? KV4 { get; set; }

        /// <summary>
        /// Gets or sets Key1 value. 
        /// </summary>
        public string? KV5 { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableConfig"/> collection.
        /// </summary>
        [JsonProperty("tables", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Table` collection.", IsImportant = true,
            Markdown = "A `Table` object provides the relationship to an existing table within the database.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<TableConfig>? Tables { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryConfig"/> collection.
        /// </summary>
        [JsonProperty("queries", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Query` collection.", IsImportant = true,
            Markdown = "A `Query` object provides the primary configuration for a query, including multiple table joins.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<QueryConfig>? Queries { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="CdcConfig"/> collection.
        /// </summary>
        [JsonProperty("cdc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Cdc` collection.", IsImportant = true,
            Markdown = "A `Cdc` object provides the primary configuration for Change Data Capture (CDC), including multiple table joins to form a composite entity.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
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
        /// Gets the company name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string Company => GetRuntimeParameter("Company", true)!;

        /// <summary>
        /// Gets the application name from the <see cref="RuntimeParameters"/>.
        /// </summary>
        public string AppName => GetRuntimeParameter("AppName", true)!;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckOptionsProperties();
            LoadDbTablesConfig();

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
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "None");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "Newtonsoft");

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
            Logger.Default.Log(LogLevel.Information, string.Empty);
            Logger.Default.Log(LogLevel.Information, $"  Querying database to infer table(s)/column(s) configuration...");

            if (!RuntimeParameters.TryGetValue("ConnectionString", out var cs))
                throw new CodeGenException("ConnectionString must be specified as a RuntimeParameter.");

            var sw = Stopwatch.StartNew();
            using var db = new SqlServerDb(cs);
            DbTables = DbTable.LoadTablesAndColumnsAsync(db, false).GetAwaiter().GetResult();

            sw.Stop();
            Logger.Default.Log(LogLevel.Information, $"    Database query complete [{sw.ElapsedMilliseconds}ms]");
            Logger.Default.Log(LogLevel.Information, string.Empty);
        }

        /// <summary>
        /// SQL Server DB.
        /// </summary>
        private class SqlServerDb : DatabaseBase
        {
            public SqlServerDb(string connectionString) : base(connectionString, Microsoft.Data.SqlClient.SqlClientFactory.Instance) { }
        }
    }
}