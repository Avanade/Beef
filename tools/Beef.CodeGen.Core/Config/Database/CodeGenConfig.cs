// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("CodeGeneration", Title = "The **CodeGeneration** is used as the global configuration for driving the underlying code-generation.", Description = "", Markdown = "")]
    [CategorySchema("RefData", Title = "Provides the **Reference Data** configuration.")]
    [CategorySchema("Infer", Title = "Provides the **Column Name inference** configuration.")]
    public class CodeGenConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, IRootConfig
    {
        #region RefData

        /// <summary>
        /// Gets or sets the schema name to identify the Reference Data related tables.
        /// </summary>
        [JsonProperty("refDatabaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The schema name to identify the Reference Data related tables.", IsImportant = true,
            Description = "Defaults to `Ref`.")]
        public string? RefDatabaseSchema { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnNameIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `IsDeleted`. To remove capability set to `None`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonProperty("columnNameTenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `TenantId`. To remove capability set to `None`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonProperty("columnNameOrgUnitId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `OrgUnitId`. To remove capability set to `None`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonProperty("columnNameRowVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `RowVersion`. To remove capability set to `None`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CreatedBy`. To remove capability set to `None`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CreatedDate`. To remove capability set to `None`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `UpdatedBy`. To remove capability set to `None`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `UpdatedDate`. To remove capability set to `None`.")]
        public string? ColumnNameUpdatedDate { get; set; }

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

        /// <summary>
        /// Sets the list of tables within the parent database.
        /// </summary>
        /// <param name="dbTables">The list of tables.</param>
        public void SetDbTable(List<Table> dbTables) => DbTables = dbTables;

        /// <summary>
        /// Gets or sets the list of tables that exist within the database.
        /// </summary>
        public List<Table>? DbTables { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            RefDatabaseSchema = DefaultWhereNull(RefDatabaseSchema, () => "Ref");
            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => "IsDeleted");
            ColumnNameTenantId = DefaultWhereNull(ColumnNameTenantId, () => "TenantId");
            ColumnNameOrgUnitId = DefaultWhereNull(ColumnNameOrgUnitId, () => "OrgUnitId");
            ColumnNameRowVersion = DefaultWhereNull(ColumnNameRowVersion, () => "RowVersion");
            ColumnNameCreatedBy = DefaultWhereNull(ColumnNameCreatedBy, () => "CreatedBy");
            ColumnNameCreatedDate = DefaultWhereNull(ColumnNameCreatedDate, () => "CreatedDate");
            ColumnNameUpdatedBy = DefaultWhereNull(ColumnNameUpdatedBy, () => "UpdatedBy");
            ColumnNameUpdatedDate = DefaultWhereNull(ColumnNameUpdatedDate, () => "UpdatedDate");
        }
    }
}
