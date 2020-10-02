// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("StoredProcedure", Title = "The **StoredProcedure** is used to identify a database `Stored Procedure` and define its code-generation characteristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("Auth", Title = "Provides the **Authorization** configuration.")]
    public class StoredProcedureConfig : ConfigBase<CodeGenConfig, TableConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the `StoredProcedure` in the database.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `StoredProcedure` in the database.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure operation type.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The stored procedure operation type.", IsImportant = true,
            Options = new string[] { "Get", "GetColl", "Create", "Update", "Upsert", "Delete", "Merge" },
            Description = "Defaults to `GetColl`.")]
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether standardized paging support should be added.
        /// </summary>
        [JsonProperty("paging", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether standardized paging support should be added.", IsImportant = true,
            Description = "This only applies where the stored procedure operation `Type` is `GetColl`.")]
        public bool? Paging { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement to perform the reselect after a `Create`, `Update` or `Upsert` stored procedure operation `Type`.
        /// </summary>
        [JsonProperty("reselectStatement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The SQL statement to perform the reselect after a `Create`, `Update` or `Upsert` stored procedure operation `Type`.",
            Description = "Defaults to `[{{Table.Schema}}].[sp{{Table.Name}}Get]` passing the primary key column(s).")]
        public string? ReselectStatement { get; set; }

        /// <summary>
        /// Indicates whether to select into a `#TempTable` to allow other statements to get access to the selected data. 
        /// </summary>
        [JsonProperty("intoTempTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether to select into a `#TempTable` to allow other statements to get access to the selected data.",
            Description = "A `Select * from #TempTable` is also performed (code-generated) where the stored procedure operation `Type` is `GetColl`.")]
        public bool? IntoTempTable { get; set; }

        /// <summary>
        /// Gets or sets the column names to be used in the `Merge` statement to determine whether to insert, update or delete.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        [JsonProperty("mergeOverrideIdentityColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Key", Title = "The list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_.",
            Description = "This is used to override the default behaviour of using the primary key column(s).")]
        public List<string>? MergeOverrideIdentityColumns { get; set; }

        /// <summary>
        /// Gets or sets the table hints using the SQL Server `WITH()` statement; the value specified will be used as-is; e.g. `NOLOCK` will result in `WITH(NOLOCK)`.
        /// </summary>
        [JsonProperty("withHints", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "the table hints using the SQL Server `WITH()` statement; the value specified will be used as-is; e.g. `NOLOCK` will result in `WITH(NOLOCK)`.")]
        public string? WithHints { get; set; }

        /// <summary>
        /// Gets or sets the permission (full name being `name.action`) override to be used for security permission checking.
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The name of the `StoredProcedure` in the database.")]
        public string? Permission { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="ParameterConfig"/> collection.
        /// </summary>
        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Parameter` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<ParameterConfig>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonProperty("where", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Where` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<WhereConfig>? Where { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="OrderByConfig"/> collection.
        /// </summary>
        [JsonProperty("orderby", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `OrderBy` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<OrderByConfig>? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="ExecuteConfig"/> collection.
        /// </summary>
        [JsonProperty("execute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Execute` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<ExecuteConfig>? Execute { get; set; }

        /// <summary>
        /// Gets the parameters to be used as the arguments parameters.
        /// </summary>
        public List<ParameterConfig> ArgumentParameters => Parameters!.Where(x => !x.WhereOnly).ToList();

        /// <summary>
        /// Gets the parameters defined as a collection.
        /// </summary>
        public List<ParameterConfig> CollectionParameters => Parameters!.Where(x => CompareValue(x.Collection, true)).ToList();

        /// <summary>
        /// Gets the "Before" <see cref="ExecuteConfig"/> collection.
        /// </summary>
        public List<ExecuteConfig>? ExecuteBefore => Execute!.Where(x => x.Location == "Before").ToList();

        /// <summary>
        /// Gets the "After" <see cref="ExecuteConfig"/> collection.
        /// </summary>
        public List<ExecuteConfig>? ExecuteAfter => Execute!.Where(x => x.Location == "After").ToList();

        /// <summary>
        /// Gets the settable columns.
        /// </summary>
        public List<SettableColumnConfig> SettableColumns { get; } = new List<SettableColumnConfig>();

        /// <summary>
        /// Gets the settable columns for an insert.
        /// </summary>
        public List<SettableColumnConfig> SettableColumnsInsert => SettableColumns.Where(x => (!(x.DbColumn!.IsPrimaryKey && x.DbColumn.IsIdentity) && !x.IsRowVersionColumn && !x.IsAudit) || (x.IsAudit && x.IsCreated)).ToList();

        /// <summary>
        /// Gets the settable columns for an update.
        /// </summary>
        public List<SettableColumnConfig> SettableColumnsUpdate => SettableColumns.Where(x => (!(x.DbColumn!.IsPrimaryKey && x.DbColumn.IsIdentity) && !x.IsTenantIdColumn && !x.IsAudit) || (x.IsAudit && x.IsUpdated)).ToList();

        /// <summary>
        /// Gets the settable columns for a delete.
        /// </summary>
        public List<SettableColumnConfig> SettableColumnsDelete => SettableColumns.Where(x => x.IsAudit && x.IsDeleted).ToList();

        /// <summary>
        /// Gets the settable columns for an upsert-insert.
        /// </summary>
        public List<SettableColumnConfig> SettableColumnsUpsertInsert => SettableColumns.Where(x => !x.IsAudit || (x.IsAudit && x.IsCreated)).ToList();

        /// <summary>
        /// Gets the settable columns for an upsert-update.
        /// </summary>
        public List<SettableColumnConfig> SettableColumnsUpsertUpdate => SettableColumns.Where(x => (!(x.DbColumn!.IsPrimaryKey && x.DbColumn.IsIdentity) && !x.IsTenantIdColumn && !x.IsAudit) || (x.IsAudit && x.IsUpdated)).ToList();

        /// <summary>
        /// Gets the primary merge on statements.
        /// </summary>
        public List<string>? MergeOn { get; private set; }

        /// <summary>
        /// Gets the merge matching source columns
        /// </summary>
        public List<string>? MergeMatchSourceColumns { get; private set; }

        /// <summary>
        /// Gets the merge matching target columns
        /// </summary>
        public List<string>? MergeMatchTargetColumns { get; private set; }

        /// <summary>
        /// Gets the merge list jon on statements.
        /// </summary>
        public List<string>? MergeListJoinOn { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            Type = DefaultWhereNull(Type, () => "GetColl");
            Permission = DefaultWhereNull(Permission?.ToUpperInvariant(), () => Parent!.Permission == null ? null : Parent!.Permission!.ToUpperInvariant() + "." + Type switch
            {
                "Delete" => "DELETE",
                "Get" => "READ",
                "GetColl" => "READ",
                _ => "WRITE"
            });

            if (Parameters == null)
                Parameters = new List<ParameterConfig>();

            if (Where == null)
                Where = new List<WhereConfig>();

            if (OrderBy == null)
                OrderBy = new List<OrderByConfig>();

            if (Execute == null)
                Execute = new List<ExecuteConfig>();

            foreach (var parameter in Parameters)
            {
                parameter.IsWhere = Parent!.Columns.Any(x => x.Name == (parameter.Column ?? parameter.Name));
            }

            AddColumnsAsParameters();

            foreach (var parameter in Parameters.AsQueryable().Reverse())
            {
                parameter.Prepare(Root!, this);
                if (parameter.IsWhere)
                    Where.Insert(0, new WhereConfig { Statement = parameter.WhereSql });
            }

            foreach (var where in Where)
            {
                where.Prepare(Root!, this);
            }

            foreach (var orderby in OrderBy)
            {
                orderby.Prepare(Root!, this);
            }

            foreach (var execute in Execute)
            {
                execute.Prepare(Root!, this);
            }

            foreach (var settable in SettableColumns)
            {
                settable.Prepare(Root!, this);
            }
        }

        /// <summary>
        /// Insert the special TenantId and IsDeleted where only parameters (i.e. not used as arguments).
        /// </summary>
        private void AddWhereOnlyParameters(bool bookEnd)
        {
            if (Parent!.DbTable!.IsAView)
                return;

            var tenantId = Parent.ColumnTenantId == null ? null : new ParameterConfig { Name = Parent.ColumnTenantId.Name, IsWhere = true, WhereOnly = true };
            var isDeleted = Parent.ColumnIsDeleted == null ? null : new ParameterConfig { Name = Parent.ColumnIsDeleted.Name, IsWhere = true, WhereOnly = true, WhereSql = $"ISNULL({Parent.ColumnIsDeleted.QualifiedName}, 0) = 0" };

            if (bookEnd)
            {
                if (tenantId != null)
                    Parameters!.Insert(0, tenantId);

                if (isDeleted != null)
                    Parameters!.Add(isDeleted);
            }
            else
            {
                if (tenantId != null)
                    Parameters!.Add(tenantId);

                if (isDeleted != null)
                    Parameters!.Add(isDeleted);
            }
        }

        /// <summary>
        /// Add columns as parameters depending on type.
        /// </summary>
        private void AddColumnsAsParameters()
        {
            switch (Type)
            {
                case "Get":
                    foreach (var c in Parent!.PrimaryKeyColumns.AsEnumerable().Reverse())
                    {
                        Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.DbColumn!.IsNullable, IsWhere = true });
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "GetColl":
                    AddWhereOnlyParameters(bookEnd: true);
                    break;

                case "Create":
                    foreach (var c in Parent!.Columns.Where(x => x.IsCreateColumn && !x.IsIsDeletedColumn && !x.IsRowVersionColumn).Reverse())
                    {
                        if (!c.IsTenantIdColumn)
                        {
                            if (c.DbColumn!.IsPrimaryKey)
                                Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = !c.DbColumn!.IsIdentity && c.DbColumn.IsNullable, Output = c.DbColumn.IsIdentity });
                            else
                                Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn.IsNullable });
                        }

                        if (!c.IsRowVersionColumn)
                            SettableColumns.Insert(0, new SettableColumnConfig { Name = c.Name, DbColumn = c.DbColumn });
                    }

                    break;

                case "Update":
                    foreach (var c in Parent!.Columns.Where(x => x.IsUpdateColumn && !x.IsIsDeletedColumn && !x.IsTenantIdColumn).Reverse())
                    {
                        if (!c.IsTenantIdColumn)
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn!.IsNullable, IsWhere = c.DbColumn!.IsPrimaryKey });

                        if (!c.IsRowVersionColumn)
                            SettableColumns.Insert(0, new SettableColumnConfig { Name = c.Name, DbColumn = c.DbColumn });
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "Upsert":
                    foreach (var c in Parent!.Columns.Where(x => (x.IsCreateColumn || x.IsUpdateColumn) && !x.IsIsDeletedColumn).Reverse())
                    {
                        if (c.IsRowVersionColumn)
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = true, IsWhere = false });
                        else if (!c.IsTenantIdColumn)
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn!.IsNullable, IsWhere = c.DbColumn!.IsPrimaryKey, Output = Type == "Create" && c.DbColumn.IsIdentity });

                        if (!c.IsRowVersionColumn)
                            SettableColumns.Insert(0, new SettableColumnConfig { Name = c.Name, DbColumn = c.DbColumn });
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "Delete":
                    foreach (var c in Parent!.Columns.Where(x => x.DbColumn!.IsPrimaryKey || x == Parent.ColumnDeletedBy || x == Parent.ColumnDeletedDate).Reverse())
                    {
                        var audit = c == Parent.ColumnDeletedBy || c == Parent.ColumnDeletedDate;
                        if (audit && Parent!.ColumnIsDeleted == null)
                            continue;

                        Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = audit || c.DbColumn!.IsNullable, IsWhere = c.DbColumn!.IsPrimaryKey });

                        if (audit)
                            SettableColumns.Insert(0, new SettableColumnConfig { Name = c.Name, DbColumn = c.DbColumn });
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "Merge":
                    foreach (var c in Parent!.Columns.Where(x => !(x == Parent.ColumnRowVersion || x == Parent.ColumnIsDeleted || x.DbColumn!.IsComputed)).Reverse())
                    {
                        var p = Parameters?.Where(x => (x.Column != null && c.Name == x.Column) || (x.Column == null && c.Name == x.Name)).SingleOrDefault();
                        SettableColumns.Insert(0, new SettableColumnConfig { Name = c.Name, DbColumn = c.DbColumn, MergeValueSql = p?.ParameterName });
                    }

                    if (MergeOverrideIdentityColumns == null)
                        MergeOverrideIdentityColumns = new List<string>();

                    MergeOn = new List<string>();
                    MergeMatchSourceColumns = new List<string>();
                    MergeMatchTargetColumns = new List<string>();
                    MergeListJoinOn = new List<string>();
                    foreach (var c in Parent!.Columns.Where(x => !x.DbColumn!.IsComputed))
                    {
                        if (c.DbColumn!.IsPrimaryKey)
                        {
                            if (MergeOverrideIdentityColumns.Count == 0)
                                MergeOn.Add($"[{Parent.Alias}].[{c.Name}] = [List].[{c.Name}]");

                            MergeListJoinOn.Add($"[{Parent.Alias}].[{c.Name}] = [List].[{c.Name}]");
                        }
                        else if (c.IsRowVersionColumn)
                            MergeListJoinOn.Add($"[{Parent.Alias}].[{c.Name}] = [List].[{c.Name}]");
                        else if (c.IsTenantIdColumn)
                        {
                            MergeOn.Add($"[{Parent.Alias}].[{c.Name}] = @{Parent.ColumnTenantId.Name}");
                            MergeListJoinOn.Add($"[{Parent.Alias}].[{c.Name}] = @{Parent.ColumnTenantId.Name}");
                        }
                        else if (c.IsIsDeletedColumn)
                            MergeListJoinOn.Add($"ISNULL([{Parent.Alias}].[{c.Name}], 0) = 0");
                        else if (!c.IsAudit && (Parent.UdtExcludeColumns == null || !Parent.UdtExcludeColumns!.Contains(c.Name!)))
                        {
                            MergeMatchSourceColumns.Add($"[list].[{c.Name}]");
                            MergeMatchTargetColumns.Add($"[{Parent!.Alias}].[{c.Name}]");
                        }
                    }

                    if (MergeOverrideIdentityColumns != null && MergeOverrideIdentityColumns.Count > 0)
                    {
                        foreach (var name in MergeOverrideIdentityColumns)
                        {
                            if (!MergeOn.Contains(name))
                                MergeOn.Add($"[{Parent.Alias}].[{name}] = [List].[{name}]");
                        }

                        foreach (var parameter in Parameters!)
                        {
                            var name = parameter.Column ?? parameter.Name;
                            MergeOn.Add($"[{Parent.Alias}].[{name}] = {parameter.ParameterName}");
                        }
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;
            }
        }
    }
}