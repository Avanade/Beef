// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the table join configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("CdcJoin", Title = "'CdcJoin' object (database-driven)", 
        Description = "The `CdcJoin` object defines a join to another (or same) table within a CDC entity. "
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("JoinTo", Title = "Provides the _join to_ configuration.")]
    [CategorySchema("Columns", Title = "Provides the _Columns_ configuration.")]
    [CategorySchema("CDC", Title = "Provides the _Change Data Capture (CDC)_ configuration.")]
    public class CdcJoinConfig : ConfigBase<CodeGenConfig, CdcConfig>, ITableReference, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("CdcJoin", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the table to join.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the table to join.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the table to join.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The schema name of the table to join.",
            Description = "Defaults to `Table.Schema`; i.e. same schema.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        #endregion

        #region JoinTo

        /// <summary>
        /// Gets or sets the name of the parent table to join to.
        /// </summary>
        [JsonProperty("joinTo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("JoinTo", Title = "The name of the table to join to (must be previously specified).", IsImportant = true,
            Description = "Defaults to `Parent.Name`.")]
        public string? JoinTo { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the parent table to join to.
        /// </summary>
        [JsonProperty("joinTo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("JoinTo", Title = "The schema name of the table to join to.", IsImportant = true,
            Description = "Defaults to `Parent.Schema`.")]
        public string? JoinToSchema { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonProperty("aliasColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Cdc

        /// <summary>
        /// Indicates whether the joined table is not being monitored for CDC and will include the selected columns.
        /// </summary>
        [JsonProperty("nonCdc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "Indicates whether the joined table is *not* being monitored for Change Data Capture (CDC) and will include the selected columns with the `Parent` columns.",
            Description = "Can only join against the `Parent` table. This is primarily provided to enable key/identifier mapping.")]
        public bool? NonCdc { get; set; }

        #endregion

        #region ISpecialColumns

        /// <summary>
        /// Gets the related IsDeleted column.
        /// </summary>
        public IColumnConfig? ColumnIsDeleted => null;

        /// <summary>
        /// Gets the related TenantId column.
        /// </summary>
        public IColumnConfig? ColumnTenantId => null;

        /// <summary>
        /// Gets the related OrgUnitId column.
        /// </summary>
        public IColumnConfig? ColumnOrgUnitId => null;

        /// <summary>
        /// Gets the related RowVersion column.
        /// </summary>
        public IColumnConfig? ColumnRowVersion => null;

        /// <summary>
        /// Gets the related CreatedBy column.
        /// </summary>
        public IColumnConfig? ColumnCreatedBy => null;

        /// <summary>
        /// Gets the related CreatedDate column.
        /// </summary>
        public IColumnConfig? ColumnCreatedDate => null;

        /// <summary>
        /// Gets the related UpdatedBy column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedBy => null;

        /// <summary>
        /// Gets the related UpdatedDate column.
        /// </summary>
        public IColumnConfig? ColumnUpdatedDate => null;

        /// <summary>
        /// Gets the related DeletedBy column.
        /// </summary>
        public IColumnConfig? ColumnDeletedBy => null;

        /// <summary>
        /// Gets the related DeletedDate column.
        /// </summary>
        public IColumnConfig? ColumnDeletedDate => null;

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="QueryJoinOnConfig"/> collection.
        /// </summary>
        [JsonProperty("on", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `JoinOn` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<QueryJoinOnConfig>? On { get; set; }

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<QueryJoinColumnConfig> Columns { get; } = new List<QueryJoinColumnConfig>();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<QueryJoinColumnConfig> PrimaryKeyColumns { get; } = new List<QueryJoinColumnConfig>();

        /// <summary>
        /// Gets the Join table qualified name.
        /// </summary>
        public string QualifiedName => $"[{Schema}].[{Name}]";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' cannot be a view.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConversion.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));
            JoinTo = DefaultWhereNull(JoinTo, () => Parent!.Name);
            JoinToSchema = DefaultWhereNull(JoinTo, () => Parent!.Schema);

            if (JoinTo != Parent!.Name || JoinToSchema != Parent!.Schema)
            {
                var tables = Parent!.Joins!.Where(x => x.Name == JoinTo && x.Schema == JoinToSchema).ToList();
                if (tables.Count == 0 || Parent!.Joins!.IndexOf(this) < Parent!.Joins!.IndexOf(tables[0]))
                    throw new CodeGenException(this, nameof(Name), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' must be previously specified.");
                else if (tables.Count > 1)
                    throw new CodeGenException(this, nameof(Name), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' is ambiguous (more than one found).");
            }

            foreach (var c in DbTable.Columns)
            {
                if (c.IsPrimaryKey)
                {
                    var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c };
                    cc.Prepare(Root!, this);
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

                    cc.Prepare(Root!, this);
                    Columns.Add(cc);
                }
            }
        }

        /// <summary>
        /// Perform the JoinOn preparation.
        /// </summary>
        public void PrepareJoinOn()
        {
            if (On == null)
                On = new List<QueryJoinOnConfig>();

            foreach (var on in On)
            {
                on.Prepare(Root!, this);
            }
        }
    }
}