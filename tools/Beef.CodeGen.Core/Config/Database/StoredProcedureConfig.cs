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
    [ClassSchema("StoredProcedure", Title = "'StoredProcedure' object (database-driven)",
        Description = "The code generation for an `StoredProcedure` is primarily driven by the `Type` property. This encourages (enforces) a consistent implementation for the standardised **CRUD** (Create, Read, Update and Delete) actions, as well as supporting `Upsert`, `Merge` and ad-hoc queries as required.",
        Markdown = @"The valid `Type` values are as follows:

- **`Get`** - indicates a get (read) returning a single row value. The primary key is automatically added as a `Parameter`.
- **`GetAll`** - indicates an ad-hoc query/get (read) returning one or more rows (collection). No `Parameter`s are automatically added.
- **`Create`** - indicates the creation of a row. All columns are added as `Parameter`s.
- **`Update`** - indicates the updating of a row. All columns are added as `Parameter`s.
- **`Upsert`** - indicates the upserting (create or update) of a row. All columns are added as `Parameter`s.
- **`Delete`** - indicates the deleting of a row. The primary key is automatically added as a `Parameter`.
- **`Merge`** - indicates the merging (create, update or delete) of one or more rows (collection) through the use of a [Table-Valued Parameter (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) Type parameter.",
        ExampleMarkdown = @"A YAML example is as follows:
``` yaml
tables:
- { name: Table, schema: Test, create: true, update: true, upsert: true, delete: true, merge: true, udt: true, getAll: true, getAllOrderBy: [ Name Des ], excludeColumns: [ Other ], permission: TestSec,
    storedProcedures: [
      { name: GetByArgs, type: GetColl, excludeColumns: [ Count ],
        parameters: [
          { name: Name, nullable: true, operator: LIKE },
          { name: MinCount, operator: GE, column: Count },
          { name: MaxCount, operator: LE, column: Count, nullable: true }
        ]
      },
      { name: Get, type: Get, withHints: NOLOCK,
        execute: [
          { statement: EXEC Demo.Before, location: Before },
          { statement: EXEC Demo.After }
        ]
      },
      { name: Update, type: Update }
    ]
  }
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("Merge", Title = "Provides _Merge_ configuration (where `Type` is `Merge`).")]
    [CategorySchema("Additional", Title = "Provides _additional ad-hoc_ configuration.")]
    [CategorySchema("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CategorySchema("Columns", Title = "Provides the _Columns_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class StoredProcedureConfig : ConfigBase<CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("StoredProcedure", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the `StoredProcedure`.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `StoredProcedure`; generally the verb/action, i.e. `Get`, `Update`, etc.", IsMandatory = true, IsImportant = true,
            Description = "See `StoredProcedureName` for the actual name used in the database.")]
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
        /// Gets or sets the `StoredProcedure` name in the database.
        /// </summary>
        [JsonProperty("storedProcedureName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `StoredProcedure` name in the database.",
            Description = "Defaults to `sp` + `Table.Name` + `Name`; e.g. `spTableName` or `spPersonGet`.")]
        public string? StoredProcedureName { get; set; }

        #endregion

        #region Additional

        /// <summary>
        /// Gets or sets the SQL statement to perform the reselect after a `Create`, `Update` or `Upsert` stored procedure operation `Type`.
        /// </summary>
        [JsonProperty("reselectStatement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Additional", Title = "The SQL statement to perform the reselect after a `Create`, `Update` or `Upsert` stored procedure operation `Type`.",
            Description = "Defaults to `[{{Table.Schema}}].[sp{{Table.Name}}Get]` passing the primary key column(s).")]
        public string? ReselectStatement { get; set; }

        /// <summary>
        /// Indicates whether to select into a `#TempTable` to allow other statements access to the selected data. 
        /// </summary>
        [JsonProperty("intoTempTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Additional", Title = "Indicates whether to select into a `#TempTable` to allow other statements access to the selected data.",
            Description = "A `Select * from #TempTable` is also performed (code-generated) where the stored procedure operation `Type` is `GetColl`.")]
        public bool? IntoTempTable { get; set; }

        /// <summary>
        /// Gets or sets the table hints using the SQL Server `WITH()` statement; the value specified will be used as-is; e.g. `NOLOCK` will result in `WITH(NOLOCK)`.
        /// </summary>
        [JsonProperty("withHints", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Additional", Title = "the table hints using the SQL Server `WITH()` statement; the value specified will be used as-is; e.g. `NOLOCK` will result in `WITH(NOLOCK)`.")]
        public string? WithHints { get; set; }

        #endregion

        #region Merge

        /// <summary>
        /// Gets or sets the column names to be used in the `Merge` statement to determine whether to insert, update or delete.
        /// </summary>
        [JsonProperty("mergeOverrideIdentityColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Merge", Title = "The list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_.",
            Description = "This is used to override the default behaviour of using the primary key column(s).")]
        public List<string>? MergeOverrideIdentityColumns { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission (full name being `name.action`) override to be used for security permission checking.
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The name of the `StoredProcedure` in the database.")]
        public string? Permission { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output (further filters `Table.IncludeColumns`).
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be included in the underlying generated _settable_ output (further filters `Table.IncludeColumns`).", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included. Only filters the columns where `Type` is `Get`, `GetColl`, `Create`, `Update` or `Upsert`.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output (further filters `Table.ExcludeColumns`).
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be excluded from the underlying generated _settable_ output (further filters `Table.ExcludeColumns`).", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded. Only filters the columns where `Type` is `Get`, `GetColl`, `Create`, `Update` or `Upsert`.")]
        public List<string>? ExcludeColumns { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="ParameterConfig"/> collection.
        /// </summary>
        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Parameter` collection.")]
        public List<ParameterConfig>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonProperty("where", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Where` collection.")]
        public List<WhereConfig>? Where { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="OrderByConfig"/> collection.
        /// </summary>
        [JsonProperty("orderby", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `OrderBy` collection.")]
        public List<OrderByConfig>? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="ExecuteConfig"/> collection.
        /// </summary>
        [JsonProperty("execute", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Execute` collection.")]
        public List<ExecuteConfig>? Execute { get; set; }

        #endregion

        /// <summary>
        /// Gets the parameters to be used as the arguments parameters.
        /// </summary>
        public List<ParameterConfig> ArgumentParameters => Parameters!.Where(x => !x.WhereOnly).ToList();

        /// <summary>
        /// Gets the selected columns.
        /// </summary>
        public List<StoredProcedureColumnConfig> SelectedColumns => Parent!.CoreColumns.Where(x => IsSelectedColumn(x, false)).Select(c => { var cc = new StoredProcedureColumnConfig { Name = c.Name, DbColumn = c.DbColumn }; cc.Prepare(Root!, this); return cc; }).ToList();

        /// <summary>
        /// Indicates whether the OrgUnitId column is included as a parameter.
        /// </summary>
        public bool HasOrgUnitIdParameter => ArgumentParameters.Any(x => x.Name == Parent!.ColumnNameOrgUnitId);

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
        public List<StoredProcedureColumnConfig> SettableColumns { get; } = new List<StoredProcedureColumnConfig>();

        /// <summary>
        /// Gets the settable columns for an insert.
        /// </summary>
        public List<StoredProcedureColumnConfig> SettableColumnsInsert => SettableColumns.Where(x => (!(x.DbColumn!.IsPrimaryKey && x.DbColumn.IsIdentity) && !x.IsRowVersionColumn && !x.IsAudit) || (x.IsAudit && x.IsCreated)).ToList();

        /// <summary>
        /// Gets the settable columns for an update.
        /// </summary>
        public List<StoredProcedureColumnConfig> SettableColumnsUpdate => SettableColumns.Where(x => (!x.DbColumn!.IsPrimaryKey && !x.IsTenantIdColumn && !x.IsAudit) || (x.IsAudit && x.IsUpdated)).ToList();

        /// <summary>
        /// Gets the settable columns for a delete.
        /// </summary>
        public List<StoredProcedureColumnConfig> SettableColumnsDelete => SettableColumns.Where(x => x.IsAudit && x.IsDeleted).ToList();

        /// <summary>
        /// Gets the settable columns for an upsert-insert.
        /// </summary>
        public List<StoredProcedureColumnConfig> SettableColumnsUpsertInsert => SettableColumns.Where(x => !x.IsAudit || (x.IsAudit && x.IsCreated)).ToList();

        /// <summary>
        /// Gets the settable columns for an upsert-update.
        /// </summary>
        public List<StoredProcedureColumnConfig> SettableColumnsUpsertUpdate => SettableColumns.Where(x => (!x.DbColumn!.IsPrimaryKey && !x.IsTenantIdColumn && !x.IsAudit) || (x.IsAudit && x.IsUpdated)).ToList();

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
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            StoredProcedureName = DefaultWhereNull(StoredProcedureName, () => $"sp{Parent!.Name}{Name}");
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
            var isDeleted = Parent.ColumnIsDeleted == null ? null : new ParameterConfig { Name = Parent.ColumnIsDeleted.Name, IsWhere = true, WhereOnly = true, WhereSql = $"({Parent.ColumnIsDeleted.QualifiedName} IS NULL OR {Parent.ColumnIsDeleted.QualifiedName} = 0)" };

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
                        if (IsSelectedColumn(c, false))
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
                            else if (IsSelectedColumn(c, true))
                                Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn.IsNullable });
                        }

                        InsertSettableColumn(c);
                    }

                    break;

                case "Update":
                    foreach (var c in Parent!.Columns.Where(x => x.IsUpdateColumn && !x.IsIsDeletedColumn && !x.IsTenantIdColumn).Reverse())
                    {
                        if (!c.IsTenantIdColumn && IsSelectedColumn(c, true))
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn!.IsNullable, IsWhere = c.DbColumn!.IsPrimaryKey });

                        InsertSettableColumn(c);
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "Upsert":
                    foreach (var c in Parent!.Columns.Where(x => (x.IsCreateColumn || x.IsUpdateColumn) && !x.IsIsDeletedColumn).Reverse())
                    {
                        if (c.IsRowVersionColumn)
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = true, IsWhere = false });
                        else if (!c.IsTenantIdColumn && IsSelectedColumn(c, true))
                            Parameters!.Insert(0, new ParameterConfig { Name = c.Name, Nullable = c.IsAudit || c.DbColumn!.IsNullable, IsWhere = c.DbColumn!.IsPrimaryKey, Output = Type == "Create" && c.DbColumn.IsIdentity });

                        InsertSettableColumn(c);
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
                            InsertSettableColumn(c);
                    }

                    AddWhereOnlyParameters(bookEnd: false);
                    break;

                case "Merge":
                    foreach (var c in Parent!.Columns.Where(x => !(x == Parent.ColumnRowVersion || x == Parent.ColumnIsDeleted || x.DbColumn!.IsComputed)).Reverse())
                    {
                        var p = Parameters?.Where(x => (x.Column != null && c.Name == x.Column) || (x.Column == null && c.Name == x.Name)).SingleOrDefault();
                        SettableColumns.Insert(0, new StoredProcedureColumnConfig { Name = c.Name, DbColumn = c.DbColumn, MergeValueSql = p?.ParameterName });
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
                            MergeOn.Add($"[{Parent.Alias}].[{c.Name}] = @{Parent.ColumnTenantId?.Name}");
                            MergeListJoinOn.Add($"[{Parent.Alias}].[{c.Name}] = @{Parent.ColumnTenantId?.Name}");
                        }
                        else if (c.IsIsDeletedColumn)
                            MergeListJoinOn.Add($"([{Parent.Alias}].[{c.Name}] IS NULL OR [{Parent.Alias}].[{c.Name}] = 0)");
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

        /// <summary>
        /// Indicates whether the column is selcted (further column filtering applied).
        /// </summary>
        private bool IsSelectedColumn(TableColumnConfig c, bool mustIncludeAudit) => (mustIncludeAudit && c.IsAudit) || ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)));

        /// <summary>
        /// Insert as a settable column and further filter columns where appropriate.
        /// </summary>
        private void InsertSettableColumn(TableColumnConfig c)
        {
            if (!c.IsRowVersionColumn && IsSelectedColumn(c, true))
                SettableColumns.Insert(0, new StoredProcedureColumnConfig { Name = c.Name, DbColumn = c.DbColumn });
        }
    }
}