// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using OnRamp.Database;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Query", Title = "'Query' object (database-driven)",
        Description = "The `Query` object enables the definition of more complex multi-table queries (`Joins`) that would primarily result in a database _View_. The primary table `Name` for the query is required to be specified. Multiple queries can be specified for the same table(s)."
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name)."
            + " Additional `Where` and `Order` configuration can also be added as required.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
queries:
- { name: Table, schema: Test, view: true, viewName: vwTestQuery, excludeColumns: [CreatedBy, UpdatedBy], permission: TestSec,
    joins: [
      { name: Person, schema: Demo, excludeColumns: [CreatedDate, UpdatedDate], aliasColumns: [RowVersion ^ RowVersionP],
        on: [
          { name: PersonId, toColumn: TableId }
        ]
      }
    ]
  }
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Columns", Title = "Provides the _Columns_ configuration.")]
    [CodeGenCategory("View", Title = "Provides the _View_ configuration.")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class QueryConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, ITableReference, ISpecialColumnNames, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Query", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the primary table of the query.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the primary table of the query.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the primary table of the view.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The schema name of the primary table of the view.",
            Description = "Defaults to `CodeGeneration.dbo`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonProperty("aliasColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region View

        /// <summary>
        /// Indicates whether a `View` is to be generated.
        /// </summary>
        [JsonProperty("view", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("View", Title = "Indicates whether a `View` is to be generated.")]
        public bool? View { get; set; }

        /// <summary>
        /// Gets or sets the `View` name.
        /// </summary>
        [JsonProperty("viewName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("View", Title = "The `View` name.",
            Description = "Defaults to `vw` + `Name`; e.g. `vwTableName`.")]
        public string? ViewName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the `View`.
        /// </summary>
        [JsonProperty("viewSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("View", Title = "The schema name for the `View`.",
            Description = "Defaults to `Schema`.")]
        public string? ViewSchema { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission to be used for security permission checking.
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "The permission to be used for security permission checking.", IsImportant = true,
            Description = "The suffix is optional, and where not specified will default to `.READ`.")]
        public string? Permission { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnNameIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `CodeGeneration.IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonProperty("columnNameTenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `CodeGeneration.TenantId`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonProperty("columnNameOrgUnitId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `CodeGeneration.OrgUnitId`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonProperty("columnNameRowVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `CodeGeneration.RowVersion`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CodeGeneration.CreatedBy`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameCreatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CodeGeneration.CreatedDate`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedBy`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonProperty("columnNameUpdatedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedDate`.")]
        public string? ColumnNameUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedBy`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonProperty("columnNameDeletedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedDate`.")]
        public string? ColumnNameDeletedDate { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryJoinConfig"/> collection.
        /// </summary>
        [JsonProperty("joins", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Join` collection.", IsImportant = true,
            Markdown = "A `Join` object provides the configuration for a joining table.")]
        public List<QueryJoinConfig>? Joins { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryOrderConfig"/> collection.
        /// </summary>
        [JsonProperty("order", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Order` collection.",
            Markdown = "An `Order` object defines the order (sequence).")]
        public List<QueryOrderConfig>? Order { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonProperty("where", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Where` collection.",
            Markdown = "A `Where` object defines the selection/filtering.")]
        public List<QueryWhereConfig>? Where { get; set; }

        #endregion

        /// <summary>
        /// Gets the SQL formatted selected columns.
        /// </summary>
        public List<IColumnConfig> SelectedColumns { get; } = new List<IColumnConfig>();

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<QueryColumnConfig> PrimaryKeyColumns { get; } = new List<QueryColumnConfig>();

        /// <summary>
        /// Gets the SQL formatted selected columns excluding the <see cref="PrimaryKeyColumns"/>.
        /// </summary>
        public List<IColumnConfig> SelectedColumnsExcludingPrimaryKey => SelectedColumns.Where(x => !(x.DbColumn!.DbTable == DbTable && x.DbColumn.IsPrimaryKey)).ToList();

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

            var c = DbTable!.Columns.Where(x => x.Name == name && !x.IsPrimaryKey).SingleOrDefault();
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
        protected override void Prepare()
        {
            Schema = DefaultWhereNull(Schema, () => "dbo");
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConverter.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));
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
                if (c.IsPrimaryKey)
                {
                    var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c };
                    cc.Prepare(Root!, this);
                    PrimaryKeyColumns.Add(cc);
                }

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
                {
                    var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c.DbColumn, NameAlias = c.NameAlias };
                    cc.Prepare(Root!, this);
                    SelectedColumns.Add(cc);
                }
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
                    {
                        var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c.DbColumn, NameAlias = c.NameAlias };
                        cc.Prepare(Root!, j);
                        SelectedColumns.Add(cc);
                    }
                }
            }

            // Prepare the where clauses.
            foreach (var where in Where)
            {
                where.Prepare(Root!, this);
            }

            // Update the centralised view metadata.
            if (CompareValue(View, true))
                UpdateViewMetadata();
        }

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

        /// <summary>
        /// Adds or updates the existing view so that the latest information can be referenced by other stored procedures etc.
        /// </summary>
        private void UpdateViewMetadata()
        {
            var dt = new DbTable { Schema = ViewSchema, Name = ViewName, IsAView = true };
            foreach (var c in SelectedColumns)
            {
                var dc = c.DbColumn!.Clone();
                dc.DbTable = dt;
                dc.Name = c.NameAlias;
                dt.Columns.Add(dc);
            }

            // Remove existing view if exists.
            var vwx = Root!.DbTables!.Where(x => x.Name == ViewName && x.Schema == ViewSchema).SingleOrDefault();
            if (vwx != null)
                Root!.DbTables!.Remove(vwx);

            // Add new/updated version.
            Root!.DbTables!.Add(dt);
        }
    }
}