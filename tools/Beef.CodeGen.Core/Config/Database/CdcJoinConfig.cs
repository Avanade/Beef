﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Database;
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
    [CategorySchema("Database", Title = "Provides the _database_ configuration.")]
    [CategorySchema("DotNet", Title = "Provides the _.NET_ configuration.")]
    [CategorySchema("IdentifierMapping", Title = "Provides the _identifier mapping_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CdcJoinConfig : ConfigBase<CodeGenConfig, CdcConfig>, ITableReference, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("CdcJoin", Name);

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        internal Guid UniqueId { get; set; } = Guid.NewGuid();

        #region Key

        /// <summary>
        /// Gets or sets the unqiue name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the table to join.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The schema name of the table to join.",
            Description = "Defaults to `Cdc.Schema`; i.e. same schema.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the table to join.
        /// </summary>
        [JsonProperty("tableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the table to join.",
            Description = "Defaults to `Name`. This is used to specify the actual underlying database table name (where the `Name` has been changed to enable uniqueness).")]
        public string? TableName { get; set; }

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
        [JsonProperty("joinToSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("JoinTo", Title = "The schema name of the table to join to.", IsImportant = true,
            Description = "Defaults to `Parent.Schema`.")]
        public string? JoinToSchema { get; set; }

        /// <summary>
        /// Get or sets the join cardinality.
        /// </summary>
        [JsonProperty("joinCardinality", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("JoinTo", Title = "The join cardinality being whether there is a One-to-Many or One-to-One relationship.", Options = new string[] { "OneToMany", "OneToOne" },
            Description = "Defaults to `OneToMany`. This represents the Parent (`JoinTo`) to child (_this_) relationship.")]
        public string? JoinCardinality { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonProperty("aliasColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the join type option.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The SQL join type.", IsImportant = true, Options = new string[] { "Cdc", "Inner", "Left", "Right", "Full" },
            Description = "Defaults to `Cdc`. The `Cdc` value indicates this is a related secondary table that also has Change Data Capture turned on and equally needs to be monitored for changes.")]
        public string? Type { get; set; }

        #endregion

        #region DotNet

        /// <summary>
        /// Gets or sets the .NET model name.
        /// </summary>
        [JsonProperty("modelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the .NET property name.
        /// </summary>
        [JsonProperty("propertyName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The .NET property name.",
            Description = "Defaults to `TableName` where `JoinCardinality` is `OneToOne`; otherwise, it will be `Name` suffixed by an `s` except when already ending in `s` where it will be suffixed by an `es`.")]
        public string? PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be included (in addition to the primary key) for a logical delete.
        /// </summary>
        [JsonProperty("includeColumnsOnDelete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("DotNet", Title = "The list of `Column` names that should be included (in addition to the primary key) for a logical delete.",
           Description = "Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.")]
        public List<string>? IncludeColumnsOnDelete { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("excludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("DotNet", Title = "The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).",
            Description = "Defaults to `CodeGeneration.CdcExcludeColumnsFromETag`.")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

        #endregion

        #region IdentifierMapping

        /// <summary>
        /// Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.
        /// </summary>
        [JsonProperty("identifierMapping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("IdentifierMapping", Title = "Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.", IsImportant = true,
           Description = "This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s).")]
        public bool? IdentifierMapping { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` with related `Schema`/`Table` values (all split by a `^` lookup character) to enable column one-to-one identifier mapping.
        /// </summary>
        [JsonProperty("identifierMappingColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("IdentifierMapping", Title = "The list of `Column` with related `Schema`/`Table` values (all split by a `^` lookup character) to enable column one-to-one identifier mapping.", IsImportant = true,
            Description = "Each value is formatted as `Column` + `^` + `Schema` + `^` + `Table` where the schema is optional; e.g. `ContactId^dbo^Contact` or `ContactId^Contact`.")]
        public List<string>? IdentifierMappingColumns { get; set; }

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
        /// Gets or sets the corresponding <see cref="CdcJoinOnConfig"/> collection.
        /// </summary>
        [JsonProperty("on", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `JoinOn` collection.")]
        public List<CdcJoinOnConfig>? On { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="On"/> select columns; i.e. those without a specific statement.
        /// </summary>
        public List<CdcJoinOnConfig> OnSelectColumns => On!.Where(x => x.ToStatement == null).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<CdcJoinColumnConfig> Columns { get; } = new List<CdcJoinColumnConfig>();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the <see cref="JoinTo"/> alias.
        /// </summary>
        public string? JoinToAlias { get; private set; }

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<CdcJoinColumnConfig> PrimaryKeyColumns { get; } = new List<CdcJoinColumnConfig>();

        /// <summary>
        /// Gets the join (linked) hierarchy (this and its parent up).
        /// </summary>
        public List<CdcJoinConfig> JoinHierarchy { get; private set; } = new List<CdcJoinConfig>();

        /// <summary>
        /// Gets the join (linked) hierarchy (this and its parent up) in reverse order.
        /// </summary>
        public List<CdcJoinConfig> JoinHierarchyReverse => JoinHierarchy.Reverse<CdcJoinConfig>().ToList();

        /// <summary>
        /// Gets the list of joined "directly related" children.
        /// </summary>
        public List<CdcJoinConfig> JoinCdcChildren => Parent!.Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of non-CDC joined "directly related" children.
        /// </summary>
        public List<CdcJoinConfig> JoinNonCdcChildren => Parent!.Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Inidicates whether it is first in the JoinHierarchy.
        /// </summary>
        public bool IsFirstInJoinHierarchy { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="CdcJoinConfig"/> in the hierarchy.
        /// </summary>
        public CdcJoinConfig? HierarchyParent { get; private set; }

        /// <summary>
        /// Gets the child <see cref="CdcJoinConfig"/> in the hierarchy.
        /// </summary>
        public CdcJoinConfig? HierarchyChild { get; private set; }

        /// <summary>
        /// Gets or sets the indentation index.
        /// </summary>
        public int IndentIndex { get; set; } = 0;

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
        protected override void Prepare()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            TableName = DefaultWhereNull(TableName, () => Name);
            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables.Where(x => x.Name == TableName && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(TableName), $"Specified Schema.Table '{Schema}.{TableName}' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(TableName), $"Specified Schema.Table '{Schema}.{TableName}' cannot be a view.");

            ModelName = DefaultWhereNull(ModelName, () => StringConversion.ToPascalCase(Name));
            JoinTo = DefaultWhereNull(JoinTo, () => Parent!.Name);
            JoinToSchema = DefaultWhereNull(JoinToSchema, () => Parent!.Schema);
            JoinCardinality = DefaultWhereNull(JoinCardinality, () => "OneToMany");
            PropertyName = DefaultWhereNull(PropertyName, () => StringConversion.ToPascalCase(CompareValue(Root.PluralizeCollectionProperties, true) && JoinCardinality == "OneToMany" ? $"{TableName!}{(TableName!.EndsWith("s", StringComparison.InvariantCulture) ? "es" : "s")}" : TableName));
            if (ExcludeColumnsFromETag == null && Root!.CdcExcludeColumnsFromETag != null)
                ExcludeColumnsFromETag = new List<string>(Root!.CdcExcludeColumnsFromETag!);

            // Get the JoinTo CdcJoinConfig.
            CdcJoinConfig? jtc = null;
            if (JoinTo != Parent!.Name || JoinToSchema != Parent!.Schema)
            {
                var tables = Parent!.Joins!.Where(x => x.TableName == JoinTo && x.Schema == JoinToSchema).ToList();
                if (tables.Count == 0 || Parent!.Joins!.IndexOf(this) < Parent!.Joins!.IndexOf(tables[0]))
                    throw new CodeGenException(this, nameof(JoinTo), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' must be previously specified.");
                else if (tables.Count > 1)
                    throw new CodeGenException(this, nameof(JoinTo), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' is ambiguous (more than one found).");

                jtc = tables[0];
                JoinToAlias = tables[0].Alias;
            }
            else
                JoinToAlias = Parent!.Alias;

            // Deal with the columns.
            foreach (var c in DbTable.Columns)
            {
                CdcJoinColumnConfig? cc = null;
                if (c.IsPrimaryKey)
                {
                    cc = new CdcJoinColumnConfig { Name = c.Name, DbColumn = c, IncludeColumnOnDelete = IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!) };
                    cc.IgnoreSerialization = IdentifierMapping == true;
                    cc.Prepare(Root!, this);
                    PrimaryKeyColumns.Add(cc);
                }

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    if (cc == null)
                        cc = new CdcJoinColumnConfig { Name = c.Name, DbColumn = c, IncludeColumnOnDelete = IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!) };

                    cc.IgnoreSerialization = c.IsPrimaryKey && IdentifierMapping == true;
                    var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                    if (ca != null)
                    {
                        var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                            cc.NameAlias = parts[1];
                    }

                    CdcConfig.MapIdentifierMappingColumn(Root!, this, Schema!, IdentifierMappingColumns, cc);
                    cc.Prepare(Root!, this);
                    Columns.Add(cc);

                    if (cc.IdentifierMappingTable != null)
                    {
                        var cc2 = new CdcJoinColumnConfig
                        {
                            Name = "GlobalId",
                            DbColumn = new DbColumn { Name = c.Name, Type = "NVARCHAR", DbTable = cc.DbColumn!.DbTable },
                            NameAlias = "Global" + cc.NameAlias,
                            IdentifierMappingAlias = cc.IdentifierMappingAlias,
                            IdentifierMappingSchema = cc.IdentifierMappingSchema,
                            IdentifierMappingTable = cc.IdentifierMappingTable,
                            IdentifierMappingParent = cc
                        };

                        cc.IdentifierMappingAlias = null;
                        cc.IdentifierMappingParent = null;
                        cc.IgnoreSerialization = true;

                        cc2.Prepare(Root!, this);
                        Columns.Add(cc2);
                    }
                }
            }

            // Update the Join ons.
            if (On == null)
                On = new List<CdcJoinOnConfig>();

            foreach (var on in On)
            {
                on.Prepare(Root!, this);
            }

            // Wire up the hierarchy (parent and child).
            var jhp = PartialClone(true, jtc == null ? 0 : jtc.JoinHierarchy.Count, null);
            JoinHierarchy.Add(jhp);

            if (jtc != null)
            {
                jhp = jtc.PartialClone(false, jtc.JoinHierarchy.Count - 1, jhp);
                JoinHierarchy.Add(jhp);
                for (int i = 1; i < jtc.JoinHierarchy.Count; i++)
                {
                    jhp = jtc.JoinHierarchy[i].PartialClone(false, jtc.JoinHierarchy.Count - i, jhp);
                    JoinHierarchy.Add(jhp);
                }
            }

            jhp = null;
            foreach (var jhr in JoinHierarchyReverse)
            {
                jhr.HierarchyChild = jhp;
                jhp = jhr;
            }
        }

        /// <summary>
        /// Performs a partial clone.
        /// </summary>
        private CdcJoinConfig PartialClone(bool isFirst, int indentIndex, CdcJoinConfig? hierarchyParent)
        {
            var j = new CdcJoinConfig
            {
                Name = Name,
                TableName = TableName,
                Schema = Schema,
                Alias = Alias,
                JoinTo = JoinTo,
                JoinToSchema = JoinToSchema,
                JoinToAlias = JoinToAlias,
                JoinCardinality = JoinCardinality,
                ModelName = ModelName,
                PropertyName = PropertyName,
                Type = Type,
                IsFirstInJoinHierarchy = isFirst,
                On = new List<CdcJoinOnConfig>(),
                DbTable = DbTable,
                IndentIndex = indentIndex,
                HierarchyParent = hierarchyParent,
                IdentifierMapping = IdentifierMapping,
                IdentifierMappingColumns = IdentifierMappingColumns
            };

            j.OverrideRootAndParent(Root!, Parent!);

            foreach (var item in On!)
            {
                var jo = new CdcJoinOnConfig
                {
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    ToColumn = item.ToColumn,
                    ToColumnAlias = item.ToColumnAlias,
                    ToStatement = item.ToStatement,
                    ToDbColumn = item.ToDbColumn
                };

                jo.OverrideRootAndParent(j.Root!, j);
                j.On.Add(jo);
            }

            return j;
        }
    }
}