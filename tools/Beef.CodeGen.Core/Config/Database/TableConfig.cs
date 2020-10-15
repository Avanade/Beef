// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Table", Title = "The **Table** is used to identify a database `Table` and define its code-generation characteristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("CodeGen", Title = "Provides the **CodeGen** configuration to select via shorthand the code-gen artefacts.")]
    [CategorySchema("UDT", Title = "Provides the **UDT (user defined type)** configuration.")]
    [CategorySchema("Auth", Title = "Provides the **Authorization** configuration.")]
    public class TableConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, ITableReference, ISpecialColumnNames, ISpecialColumns
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the `Table` in the database.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `Table` in the database.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the `Schema` where the `Table` is defined in the database.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `Schema` where the `Table` is defined in the database.", IsMandatory = true, IsImportant = true)]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

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

        #endregion

        #region CodeGen

        /// <summary>
        /// Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("get", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Get { get; set; }

        /// <summary>
        /// Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("getAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? GetAll { get; set; }

        /// <summary>
        /// Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("create", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Create { get; set; }

        /// <summary>
        /// Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("update", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Update { get; set; }

        /// <summary>
        /// Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("upsert", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Upsert { get; set; }

        /// <summary>
        /// Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("delete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Delete { get; set; }

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

        /// <summary>
        /// Indicates whether an `Entity Framework` .NET (C#) model is to be generated.
        /// </summary>
        [JsonProperty("efModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates whether an `Entity Framework` .NET (C#) model is to be generated.")]
        public bool? EfModel { get; set; }

        /// <summary>
        /// Gets or sets the .NET (C#) EntityFramework (EF) model name.
        /// </summary>
        [JsonProperty("efModelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "The .NET (C#) EntityFramework (EF) model name.",
            Description = "Defaults to `Name`.")]
        public string? EfModelName { get; set; }

        #endregion

        #region Udt

        /// <summary>
        /// Indicates whether a `User Defined Table (UDT)` type should be created.
        /// </summary>
        [JsonProperty("udt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "Indicates whether a `User Defined Table (UDT)` type should be created.", IsImportant = true)]
        public bool? Udt { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the `User Defined Table (UDT)`.
        /// </summary>
        [JsonProperty("udtExcludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Udt", Title = "The list of `Column` names to be excluded from the `User Defined Table (UDT)`.",
            Description = "Where not specified this indicates that no `Columns` are to be excluded.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? UdtExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.
        /// </summary>
        [JsonProperty("tvp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "The name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.", IsImportant = true)]
        public string? Tvp { get; set; }

        /// <summary>
        /// Indicates whether a `Merge` (upsert of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("merge", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "Indicates whether a `Merge` (upsert of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Merge { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).", IsImportant = true)]
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
        /// Gets or sets the corresponding <see cref="StoredProcedureConfig"/> collection.
        /// </summary>
        [JsonProperty("storedProcedures", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `StoredProcedure` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<StoredProcedureConfig>? StoredProcedures { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="GetAllOrderByConfig"/> collection).
        /// </summary>
        [JsonProperty("getAllOrderBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `GetAll` stored procedure `OrderBy` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<GetAllOrderByConfig>? GetAllOrderBy { get; set; }

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<TableColumnConfig> Columns { get; } = new List<TableColumnConfig>();

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
        /// Gets the named special colum.
        /// </summary>
        private TableColumnConfig? GetSpecialColumn(string? name) => Columns?.Where(x => x.Name == name && name != "None" && !x.DbColumn!.IsPrimaryKey).SingleOrDefault();

        /// <summary>
        /// Indicates whether there any audit columns.
        /// </summary>
        public bool HasAuditColumns => ColumnCreatedBy != null || ColumnCreatedDate != null || ColumnUpdatedBy != null || ColumnUpdatedDate != null || ColumnDeletedBy != null || ColumnDeletedDate != null;

        /// <summary>
        /// Indicates whether there are any audit "By" columns.
        /// </summary>
        public bool HasAuditByColumns => ColumnCreatedBy != null || ColumnUpdatedBy != null || ColumnDeletedBy != null;

        /// <summary>
        /// Indicates whether there are any audit "Date" columns.
        /// </summary>
        public bool HasAuditDateColumns => ColumnCreatedDate != null || ColumnUpdatedDate != null || ColumnDeletedDate != null;

        /// <summary>
        /// Indicates whether there are any audit "Created" columns
        /// </summary>
        public bool HasAuditCreated => ColumnCreatedBy != null || ColumnCreatedDate != null;

        /// <summary>
        /// Indicates whether there are any audit "Updated" columns
        /// </summary>
        public bool HasAuditUpdated => ColumnUpdatedBy != null || ColumnUpdatedDate != null;

        /// <summary>
        /// Indicates whether there are any audit "Deleted" columns
        /// </summary>
        public bool HasAuditDeleted => ColumnDeletedBy != null || ColumnDeletedDate != null;

        /// <summary>
        /// Gets the columns considered part of the primary key.
        /// </summary>
        public List<TableColumnConfig> PrimaryKeyColumns => Columns.Where(x => x.DbColumn!.IsPrimaryKey).ToList();

        /// <summary>
        /// Gets the columns considered part of the primary key.
        /// </summary>
        public List<TableColumnConfig> PrimaryKeyIdentityColumns => Columns.Where(x => x.DbColumn!.IsPrimaryKey && x.DbColumn!.IsIdentity).ToList();

        /// <summary>
        /// Gets the core columns (excludes special internal IsDeleted and TenantId columns).
        /// </summary>
        public List<TableColumnConfig> CoreColumns => Columns.Where(x => x.DbColumn!.IsPrimaryKey || !(x.Name == ColumnIsDeleted?.Name || x.Name == ColumnTenantId?.Name)).ToList();

        /// <summary>
        /// Gets the UDT columns (excludes special columns).
        /// </summary>
        public List<TableColumnConfig> UdtColumns => Columns.Where(x => !x.IsAudit && !x.IsIsDeletedColumn && !x.IsTenantIdColumn && (UdtExcludeColumns == null || !UdtExcludeColumns.Contains(x.Name!))).ToList();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => DbTable!.QualifiedName;

        /// <summary>
        /// Inidicates whether the source of the EfModel is a database View.
        /// </summary>
        public bool EfIsAView => CompareValue(View, true) || DbTable!.IsAView;

        /// <summary>
        /// Gets the Ef database object name.
        /// </summary>
        public string EfDbName => CompareValue(View, true) ? "vw" + Name! : Name!;

        /// <summary>
        /// Gets or sets the view where statements.
        /// </summary>
        public List<string>? ViewWhere { get; private set; }

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
            EfModelName = DefaultWhereNull(EfModelName, () => Name);
            ViewName = DefaultWhereNull(ViewName, () => "vw" + Name);
            ViewSchema = DefaultWhereNull(ViewSchema, () => Schema);

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

            PrepareStoredProcedures();

            foreach (var c in DbTable.Columns)
            {
                var cc = new TableColumnConfig { Name = c.Name, DbColumn = c };
                cc.Prepare(Root!, this);

                // Certain special columns have to always be included.
                if (cc.IsTenantIdColumn || cc.IsOrgUnitIdColumn || cc.IsIsDeletedColumn)
                    Columns.Add(cc);
                else if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                    Columns.Add(cc);
                else if (cc.IsAudit && StoredProcedures!.Any(x => x.Type == "Create" || x.Type == "Update" || x.Type == "Upsert" || x.Type == "Delete" || x.Type == "Merge"))
                    Columns.Add(cc);
            }

            foreach (var storedProcedure in StoredProcedures!)
            {
                storedProcedure.Prepare(Root!, this);
            }

            if (CompareValue(View, true))
                PrepareView();
        }

        /// <summary>
        /// Prepares the stored procedures.
        /// </summary>
        private void PrepareStoredProcedures()
        {
            if (StoredProcedures == null)
                StoredProcedures = new List<StoredProcedureConfig>();

            // Add in selected operations where applicable (in reverse order in which output).
            if (CompareValue(Delete, true) && !StoredProcedures.Any(x => x.Name == "Delete"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Delete", Type = "Delete" });

            if (CompareValue(Merge, true) && !StoredProcedures.Any(x => x.Name == "Merge"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Merge", Type = "Merge" });

            if (CompareValue(Upsert, true) && !StoredProcedures.Any(x => x.Name == "Upsert"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Upsert", Type = "Upsert" });

            if (CompareValue(Update, true) && !StoredProcedures.Any(x => x.Name == "Update"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Update", Type = "Update" });

            if (CompareValue(Create, true) && !StoredProcedures.Any(x => x.Name == "Create"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Create", Type = "Create" });

            if (CompareValue(GetAll, true) && !StoredProcedures.Any(x => x.Name == "GetAll"))
            {
                var spc = new StoredProcedureConfig { Name = "GetAll", Type = "GetColl" };
                if (GetAllOrderBy != null)
                {
                    spc.OrderBy = new List<OrderByConfig>();
                    foreach (var gaob in GetAllOrderBy)
                    {
                        spc.OrderBy.Add(new OrderByConfig { Name = gaob.Name, Order = gaob.Order });
                    }
                }
                else if (DbTable!.IsRefData)
                {
                    spc.OrderBy = new List<OrderByConfig>(new OrderByConfig[] { new OrderByConfig { Name = "SortOrder" }, new OrderByConfig { Name = "Code" } });
                }

                StoredProcedures.Add(spc);
            }

            if (CompareValue(Get, true) && !StoredProcedures.Any(x => x.Name == "Get"))
                StoredProcedures.Add(new StoredProcedureConfig { Name = "Get", Type = "Get" });
        }

        /// <summary>
        /// Prepares the view.
        /// </summary>
        private void PrepareView()
        {
            ViewWhere = new List<string>();
            if (ColumnTenantId != null)
                ViewWhere.Add($"[{Alias}].[{ColumnTenantId.Name}] = dbo.fnGetTenantId(NULL)");

            if (!string.IsNullOrEmpty(Permission))
                ViewWhere.Add($"{Root!.GetUserPermissionSql}(NULL, NULL, '{Permission.ToUpperInvariant()}.READ', NULL) = 1");
        }
    }
}