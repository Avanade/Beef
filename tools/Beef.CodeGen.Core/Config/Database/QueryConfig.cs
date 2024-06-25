// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx.DbSchema;
using System.Text.Json.Serialization;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
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
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The name of the primary table of the query.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the primary table of the view.
        /// </summary>
        [JsonPropertyName("schema")]
        [CodeGenProperty("Key", Title = "The schema name of the primary table of the view.",
            Description = "Defaults to `CodeGeneration.dbo`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonPropertyName("alias")]
        [CodeGenProperty("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonPropertyName("includeColumns")]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonPropertyName("excludeColumns")]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonPropertyName("aliasColumns")]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region View

        /// <summary>
        /// Indicates whether a `View` is to be generated.
        /// </summary>
        [JsonPropertyName("view")]
        [CodeGenProperty("View", Title = "Indicates whether a `View` is to be generated.")]
        public bool? View { get; set; }

        /// <summary>
        /// Gets or sets the `View` name.
        /// </summary>
        [JsonPropertyName("viewName")]
        [CodeGenProperty("View", Title = "The `View` name.",
            Description = "Defaults to `vw` + `Name`; e.g. `vwTableName`.")]
        public string? ViewName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the `View`.
        /// </summary>
        [JsonPropertyName("viewSchema")]
        [CodeGenProperty("View", Title = "The schema name for the `View`.",
            Description = "Defaults to `Schema`.")]
        public string? ViewSchema { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission to be used for security permission checking.
        /// </summary>
        [JsonPropertyName("permission")]
        [CodeGenProperty("Auth", Title = "The permission to be used for security permission checking.", IsImportant = true,
            Description = "The suffix is optional, and where not specified will default to `.READ`.")]
        public string? Permission { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonPropertyName("columnNameIsDeleted")]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `CodeGeneration.IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `TenantId` capability.
        /// </summary>
        [JsonPropertyName("columnNameTenantId")]
        [CodeGenProperty("Infer", Title = "The column name for the `TenantId` capability.",
            Description = "Defaults to `CodeGeneration.TenantId`.")]
        public string? ColumnNameTenantId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `OrgUnitId` capability.
        /// </summary>
        [JsonPropertyName("columnNameOrgUnitId")]
        [CodeGenProperty("Infer", Title = "The column name for the `OrgUnitId` capability.",
            Description = "Defaults to `CodeGeneration.OrgUnitId`.")]
        public string? ColumnNameOrgUnitId { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `RowVersion` capability.
        /// </summary>
        [JsonPropertyName("columnNameRowVersion")]
        [CodeGenProperty("Infer", Title = "The column name for the `RowVersion` capability.",
            Description = "Defaults to `CodeGeneration.RowVersion`.")]
        public string? ColumnNameRowVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameCreatedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedBy` capability.",
            Description = "Defaults to `CodeGeneration.CreatedBy`.")]
        public string? ColumnNameCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `CreatedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameCreatedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `CreatedDate` capability.",
            Description = "Defaults to `CodeGeneration.CreatedDate`.")]
        public string? ColumnNameCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameUpdatedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedBy` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedBy`.")]
        public string? ColumnNameUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `UpdatedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameUpdatedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `UpdatedDate` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedDate`.")]
        public string? ColumnNameUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedBy` capability.
        /// </summary>
        [JsonPropertyName("columnNameDeletedBy")]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedBy` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedBy`.")]
        public string? ColumnNameDeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the column name for the `DeletedDate` capability.
        /// </summary>
        [JsonPropertyName("columnNameDeletedDate")]
        [CodeGenProperty("Infer", Title = "The column name for the `DeletedDate` capability.",
            Description = "Defaults to `CodeGeneration.UpdatedDate`.")]
        public string? ColumnNameDeletedDate { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryJoinConfig"/> collection.
        /// </summary>
        [JsonPropertyName("joins")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Join` collection.", IsImportant = true,
            Markdown = "A `Join` object provides the configuration for a joining table.")]
        public List<QueryJoinConfig>? Joins { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryOrderConfig"/> collection.
        /// </summary>
        [JsonPropertyName("order")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Order` collection.",
            Markdown = "An `Order` object defines the order (sequence).")]
        public List<QueryOrderConfig>? Order { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonPropertyName("where")]
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
        public IColumnConfig? ColumnIsDeleted { get; private set; }

        /// <summary>
        /// Gets the related TenantId column.
        /// </summary>
        public IColumnConfig? ColumnTenantId { get; private set; }

        /// <summary>
        /// Gets the related OrgUnitId column.
        /// </summary>
        public IColumnConfig? ColumnOrgUnitId { get; private set; }

        /// <summary>
        /// Gets the related RowVersion column.
        /// </summary>
        public IColumnConfig? ColumnRowVersion { get; private set; }

        /// <summary>
        /// Gets the related CreatedBy column.
        /// </summary>
        public IColumnConfig? ColumnCreatedBy { get; private set; }

        /// <summary>
        /// Gets the related CreatedDate column.
        /// </summary>
        public IColumnConfig? ColumnCreatedDate { get; private set; }

        /// <summary>
        /// Gets the related UpdatedBy column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedBy { get; private set; }

        /// <summary>
        /// Gets the related UpdatedDate column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedDate { get; private set; }

        /// <summary>
        /// Gets the related DeletedBy column.
        /// </summary>
        public IColumnConfig? ColumnDeletedBy { get; private set; }

        /// <summary>
        /// Gets the related DeletedDate column.
        /// </summary>
        public IColumnConfig? ColumnDeletedDate { get; private set; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTableSchema? DbTable { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
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

            await SetSpecialColumnsAsync().ConfigureAwait(false);
            await PrepareJoinsAsync().ConfigureAwait(false);

            if (Order != null && Order.Count > 0)
            {
                foreach (var order in Order)
                {
                    await order.PrepareAsync(Root!, this).ConfigureAwait(false);
                }
            }

            foreach (var c in DbTable.Columns)
            {
                if (c.IsPrimaryKey)
                {
                    var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c };
                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
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

                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
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
                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
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
                        await cc.PrepareAsync(Root!, j).ConfigureAwait(false);
                        SelectedColumns.Add(cc);
                    }
                }
            }

            // Prepare the where clauses.
            foreach (var where in Where)
            {
                await where.PrepareAsync(Root!, this).ConfigureAwait(false);
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
        private async Task PrepareJoinsAsync()
        {
            if (Joins == null)
                Joins = new List<QueryJoinConfig>();

            // Prepare the Join and also make sure the alias is unique.
            var dict = new Dictionary<string, int> { { Alias!, 1 } };
            foreach (var join in Joins)
            {
                await join.PrepareAsync(Root!, this).ConfigureAwait(false);
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
                await join.PrepareJoinOnAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds or updates the existing view so that the latest information can be referenced by other stored procedures etc.
        /// </summary>
        private void UpdateViewMetadata()
        {
            var dt = new DbTableSchema(Root!.Migrator!, ViewSchema!, ViewName!) { IsAView = true };
            foreach (var c in SelectedColumns)
            {
                var dc = new DbColumnSchema(dt, c.NameAlias!, c.DbColumn!.Type);
                dc.CopyFrom(c.DbColumn);
                dt.Columns.Add(dc);
            }

            // Remove existing view if exists.
            var vwx = Root!.DbTables!.Where(x => x.Name == ViewName && x.Schema == ViewSchema).SingleOrDefault();
            if (vwx != null)
                Root!.DbTables!.Remove(vwx);

            // Add new/updated version.
            Root!.DbTables!.Add(dt);
        }

        /// <summary>
        /// Sets the special columns.
        /// </summary>
        private async Task SetSpecialColumnsAsync()
        {
            ColumnIsDeleted = await GetSpecialColumnAsync(ColumnNameIsDeleted).ConfigureAwait(false);
            ColumnTenantId = await GetSpecialColumnAsync(ColumnNameTenantId).ConfigureAwait(false);
            ColumnOrgUnitId = await GetSpecialColumnAsync(ColumnNameOrgUnitId).ConfigureAwait(false);
            ColumnRowVersion = await GetSpecialColumnAsync(ColumnNameRowVersion).ConfigureAwait(false);
            ColumnCreatedBy = await GetSpecialColumnAsync(ColumnNameCreatedBy).ConfigureAwait(false);
            ColumnCreatedDate = await GetSpecialColumnAsync(ColumnNameCreatedDate).ConfigureAwait(false);
            ColumnUpdatedBy = await GetSpecialColumnAsync(ColumnNameUpdatedBy).ConfigureAwait(false);
            ColumnUpdatedDate = await GetSpecialColumnAsync(ColumnNameUpdatedDate).ConfigureAwait(false);
            ColumnDeletedBy = await GetSpecialColumnAsync(ColumnNameDeletedBy).ConfigureAwait(false);
            ColumnDeletedDate = await GetSpecialColumnAsync(ColumnNameDeletedDate).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the named special column.
        /// </summary>
        private async Task<IColumnConfig?> GetSpecialColumnAsync(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var c = DbTable!.Columns.Where(x => x.Name == name && !x.IsPrimaryKey).SingleOrDefault();
            if (c == null)
                return null;

            var cc = new QueryColumnConfig { Name = c.Name, DbColumn = c };
            await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
            return cc;
        }
    }
}