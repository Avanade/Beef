// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("View", Title = "The **View** is used to define a database view across one or more joined tables", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class QueryConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, ITableReference, ISpecialColumnNames, ISpecialColumns
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the primary table of the view.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the table to join.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the primary table of the view.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The schema name of the primary table of the view.",
            Description = "Defaults to `dbo`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        /// <summary>
        /// Indicates whether a `View` is to be generated.
        /// </summary>
        [JsonProperty("view", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `View` is to be generated.")]
        public bool? View { get; set; }

        /// <summary>
        /// Gets or sets the `View` name.
        /// </summary>
        [JsonProperty("viewName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `View` name.",
            Description = "Defaults to `vw` + `Name`; e.g. `vwTableName`.")]
        public string? ViewName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the `View`.
        /// </summary>
        [JsonProperty("viewSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The schema name for the `View`.",
            Description = "Defaults to `Schema`.")]
        public string? ViewSchema { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Key", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this Indicates whether all `Columns` are to be included.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Key", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonProperty("aliasColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Key", Title = "The list of `Column` and `Alias` pairs to enables column renaming.", IsImportant = true,
            Description = "A value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission to be used for security permission checking.
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The permission to be used for security permission checking ().", IsImportant = true,
            Description = "The suffix is optional and where not specified will default to `READ`.")]
        public string? Permission { get; set; }

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

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `UpdatedBy`. To remove capability set to `None`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `UpdatedDate`. To remove capability set to `None`.")]
        public string? ColumnNameDeletedDate { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryJoinConfig"/> collection.
        /// </summary>
        [JsonProperty("joins", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Join` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<QueryJoinConfig>? Joins { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryOrderConfig"/> collection.
        /// </summary>
        [JsonProperty("order", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Order` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<QueryOrderConfig>? Order { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonProperty("where", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Where` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<QueryWhereConfig>? Where { get; set; }

        /// <summary>
        /// Gets the SQL formatted selected columns.
        /// </summary>
        public List<string> SelectedColumns { get; } = new List<string>();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<QueryColumnConfig> Columns { get; } = new List<QueryColumnConfig>();

        /// <summary>
        /// Gets the related IsDeleted column.
        /// </summary>
        public IColumnConfig? ColumnIsDeleted => GetSpecialColumn(ColumnNameIsDeleted);

        /// <summary>
        /// Gets the related TenantId column.
        /// </summary>
        public IColumnConfig? ColumnTenantId => GetSpecialColumn(ColumnNameTenantId);

        /// <summary>
        /// Gets the related OrgUnitId column.
        /// </summary>
        public IColumnConfig? ColumnOrgUnitId => GetSpecialColumn(ColumnNameOrgUnitId);

        /// <summary>
        /// Gets the related RowVersion column.
        /// </summary>
        public IColumnConfig? ColumnRowVersion => GetSpecialColumn(ColumnNameRowVersion);

        /// <summary>
        /// Gets the related CreatedBy column.
        /// </summary>
        public IColumnConfig? ColumnCreatedBy => GetSpecialColumn(ColumnNameCreatedBy);

        /// <summary>
        /// Gets the related CreatedDate column.
        /// </summary>
        public IColumnConfig? ColumnCreatedDate => GetSpecialColumn(ColumnNameCreatedDate);

        /// <summary>
        /// Gets the related UpdatedBy column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedBy => GetSpecialColumn(ColumnNameUpdatedBy);

        /// <summary>
        /// Gets the related UpdatedDate column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedDate => GetSpecialColumn(ColumnNameUpdatedDate);

        /// <summary>
        /// Gets the related DeletedBy column.
        /// </summary>
        public IColumnConfig? ColumnDeletedBy => GetSpecialColumn(ColumnNameDeletedBy);

        /// <summary>
        /// Gets the related DeletedDate column.
        /// </summary>
        public IColumnConfig? ColumnDeletedDate => GetSpecialColumn(ColumnNameDeletedDate);

        /// <summary>
        /// Gets the named special column.
        /// </summary>
        private IColumnConfig? GetSpecialColumn(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var c = DbTable!.Columns.Where(x => x.Name == name && name != "None" && !x.IsPrimaryKey).SingleOrDefault();
            if (c == null)
                return null;

            var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c };
            cc.Prepare(Root!, this);
            return cc;
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            Schema = DefaultWhereNull(Schema, () => "dbo");
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException($"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConversion.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));
            ViewName = DefaultWhereNull(ViewName, () => "vw" + Name);
            ViewSchema = DefaultWhereNull(ViewSchema, () => Schema);
            if (!string.IsNullOrEmpty(Permission) && Permission.Split(".", StringSplitOptions.RemoveEmptyEntries).Length == 1)
                Permission += ".Read";

            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => Root!.ColumnNameIsDeleted);
            ColumnNameTenantId = DefaultWhereNull(ColumnNameTenantId, () => Root!.ColumnNameTenantId);
            ColumnNameOrgUnitId = DefaultWhereNull(ColumnNameOrgUnitId, () => Root!.ColumnNameOrgUnitId);
            ColumnNameRowVersion = DefaultWhereNull(ColumnNameRowVersion, () => Root!.ColumnNameRowVersion);
            ColumnNameCreatedBy = DefaultWhereNull(ColumnNameCreatedBy, () => Root!.ColumnNameCreatedBy);
            ColumnNameCreatedDate = DefaultWhereNull(ColumnNameCreatedDate, () => Root!.ColumnNameCreatedDate);
            ColumnNameUpdatedBy = DefaultWhereNull(ColumnNameUpdatedBy, () => Root!.ColumnNameUpdatedBy);
            ColumnNameUpdatedDate = DefaultWhereNull(ColumnNameUpdatedDate, () => Root!.ColumnNameUpdatedDate);
            ColumnNameDeletedBy = DefaultWhereNull(ColumnNameDeletedBy, () => Root!.ColumnNameDeletedBy);
            ColumnNameDeletedDate = DefaultWhereNull(ColumnNameDeletedDate, () => Root!.ColumnNameDeletedDate);

            PrepareJoins();

            if (Order != null && Order.Count > 0)
            {
                foreach (var order in Order)
                {
                    order.Prepare(Root!, this);
                }
            }

            foreach (var c in DbTable.Columns)
            {
                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c };
                    var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                    if (ca != null)
                    {
                        var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                            cc.NameAlias = parts[1];
                    }

                    cc.Prepare(Root!, this);
                    Columns.Add(cc);
                }
            }

            if (Where == null)
                Where = new List<QueryWhereConfig>();

            if (ColumnTenantId != null)
                Where.Add(new QueryWhereConfig { Statement = $"[{Alias}].[{ColumnTenantId.Name}] = dbo.fnGetTenantId(NULL)" });

            if (ColumnIsDeleted != null)
                Where.Add(new QueryWhereConfig { Statement = $"([{Alias}].[{ColumnIsDeleted.Name}] IS NULL OR [{Alias}].[{ColumnIsDeleted.Name}] = 0)" });

            if (!string.IsNullOrEmpty(Permission))
                Where.Add(new QueryWhereConfig { Statement = $"{Root!.GetUserPermissionSql}(NULL, NULL, '{Permission.ToUpperInvariant()}', NULL) = 1" });

            // Build up the selected columns list.
            foreach (var c in Columns)
            {
                if (!c.IsIsDeletedColumn && !c.IsTenantIdColumn)
                    SelectedColumns.Add(CreateSelectedColumnSql(c)!);
            }

            foreach (var j in Joins!)
            {
                if (j.ColumnTenantId != null)
                    Where.Add(new QueryWhereConfig { Statement = $"[{j.Alias}].[{j.ColumnTenantId.Name}] = dbo.fnGetTenantId(NULL)" });

                if (j.ColumnIsDeleted != null)
                    Where.Add(new QueryWhereConfig { Statement = $"([{j.Alias}].[{j.ColumnIsDeleted.Name}] IS NULL OR [{j.Alias}].[{j.ColumnIsDeleted.Name}] = 0)" });

                foreach (var c in j.Columns)
                {
                    if (!c.IsIsDeletedColumn && !c.IsTenantIdColumn)
                        SelectedColumns.Add(CreateSelectedColumnSql(c)!);
                }
            }

            // Prepare the where clauses.
            foreach (var where in Where)
            {
                where.Prepare(Root!, this);
            }
        }

        /// <summary>
        /// Create the selected column SQL.
        /// </summary>
        private static string? CreateSelectedColumnSql(IColumnConfig c) => string.IsNullOrEmpty(c?.NameAlias) ? c?.QualifiedName : $"{c?.QualifiedName} AS [{c?.NameAlias}]";

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => DbTable!.QualifiedName;

        /// <summary>
        /// Prepares the joins.
        /// </summary>
        private void PrepareJoins()
        {
            if (Joins == null)
                Joins = new List<QueryJoinConfig>();

            // Prepare the Join and also make sure the alias is unique.
            var dict = new Dictionary<string, int> { { Alias!, 1 } };
            foreach (var join in Joins)
            {
                join.Prepare(Root!, this);
                if (dict.TryGetValue(join.Alias!, out var val))
                {
                    dict[join.Alias!] = val++;
                    join.Alias = $"{join.Alias}{val}";
                }
                else
                    dict.Add(join.Alias!, 1);
            }

            // Now that the alias has been updated we can prepare accordingly.
            foreach (var join in Joins)
            {
                join.PrepareJoinOn();
            }
        }
    }
}