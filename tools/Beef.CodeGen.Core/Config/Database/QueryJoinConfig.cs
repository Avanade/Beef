// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx.DbSchema;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the table join configuration.
    /// </summary>
    [CodeGenClass("QueryJoin", Title = "'QueryJoin' object (database-driven)", 
        Description = "The `QueryJoin` object defines a join to another (or same) table within a query. The `Type` defines the join type, such as inner join, etc."
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required (for example duplicate name).",
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
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    public class QueryJoinConfig : ConfigBase<CodeGenConfig, QueryConfig>, ITableReference, ISpecialColumnNames, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("QueryJoin", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the table to join.
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The name of the table to join.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the table to join.
        /// </summary>
        [JsonPropertyName("schema")]
        [CodeGenProperty("Key", Title = "The schema name of the table to join.",
            Description = "Defaults to `Table.Schema`; i.e. same schema.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonPropertyName("alias")]
        [CodeGenProperty("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the join type option.
        /// </summary>
        [JsonPropertyName("type")]
        [CodeGenProperty("Key", Title = "The SQL join type.", IsImportant = true, Options = new string[] { "Inner", "Left", "Right", "Full" },
            Description = "Defaults to `Inner`.")]
        public string? Type { get; set; }

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
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        public List<string>? AliasColumns { get; set; }

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

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryJoinOnConfig"/> collection.
        /// </summary>
        [JsonPropertyName("on")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `JoinOn` collection.")]
        public List<QueryJoinOnConfig>? On { get; set; }

        /// <summary>
        /// Gets all the other joins excluding this.
        /// </summary>
        public List<QueryJoinConfig> OtherJoins => Parent!.Joins!.Where(x => x != this).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<QueryJoinColumnConfig> Columns { get; } = new List<QueryJoinColumnConfig>();

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
        /// Gets the list of primary key columns.
        /// </summary>
        public List<QueryJoinColumnConfig> PrimaryKeyColumns { get; } = new List<QueryJoinColumnConfig>();

        /// <summary>
        /// Gets the Join table qualified name.
        /// </summary>
        public string QualifiedName => $"[{Schema}].[{Name}]";

        /// <summary>
        /// Gets the join <see cref="Type"/> as SQL.
        /// </summary>
        public string JoinTypeSql => Type?.ToUpperInvariant() switch
        {
            "LEFT" => "LEFT OUTER JOIN",
            "RIGHT" => "RIGHT OUTER JOIN",
            "FULL" => "FULL OUTER JOIN",
            _ => "INNER JOIN"
        };

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConverter.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));

            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => Parent!.ColumnNameIsDeleted);
            ColumnNameTenantId = DefaultWhereNull(ColumnNameTenantId, () => Parent!.ColumnNameTenantId);
            ColumnNameOrgUnitId = DefaultWhereNull(ColumnNameOrgUnitId, () => Parent!.ColumnNameOrgUnitId);
            ColumnNameRowVersion = DefaultWhereNull(ColumnNameRowVersion, () => Parent!.ColumnNameRowVersion);
            ColumnNameCreatedBy = DefaultWhereNull(ColumnNameCreatedBy, () => Parent!.ColumnNameCreatedBy);
            ColumnNameCreatedDate = DefaultWhereNull(ColumnNameCreatedDate, () => Parent!.ColumnNameCreatedDate);
            ColumnNameUpdatedBy = DefaultWhereNull(ColumnNameUpdatedBy, () => Parent!.ColumnNameUpdatedBy);
            ColumnNameUpdatedDate = DefaultWhereNull(ColumnNameUpdatedDate, () => Parent!.ColumnNameUpdatedDate);
            ColumnNameDeletedBy = DefaultWhereNull(ColumnNameDeletedBy, () => Parent!.ColumnNameDeletedBy);
            ColumnNameDeletedDate = DefaultWhereNull(ColumnNameDeletedDate, () => Parent!.ColumnNameDeletedDate);

            await PrepareSpecialColumnsAsync().ConfigureAwait(false);

            foreach (var c in DbTable.Columns)
            {
                if (c.IsPrimaryKey)
                {
                    var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c };
                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                    PrimaryKeyColumns.Add(cc);
                }

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c };
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
        }

        /// <summary>
        /// Perform the JoinOn preparation.
        /// </summary>
        public async Task PrepareJoinOnAsync()
        {
            On ??= new List<QueryJoinOnConfig>();

            foreach (var on in On)
            {
                await on.PrepareAsync(Root!, this).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Prepare the special columns.
        /// </summary>
        /// <returns></returns>
        private async Task PrepareSpecialColumnsAsync()
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

            var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c };
            await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
            return cc;
        }
    }
}