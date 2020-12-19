﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
    [CategorySchema("DotNet", Title = "Provides the _.NET_ configuration.")]
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
            Description = "Defaults to `Table.Schema`; i.e. same schema.")]
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

        #region DotNet

        /// <summary>
        /// Gets or sets the .NET model name.
        /// </summary>
        [JsonProperty("modelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the .NET property name.
        /// </summary>
        [JsonProperty("propertyName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The .NET property name.",
            Description = "Defaults to `ModelName` where `JoinCardinality` is `OneToOne`; otherwise, it will be `ModelName` suffixed by an `s` except when already ending in `s` where it will be suffixed by an `es`.")]
        public string? PropertyName { get; set; }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<CdcJoinOnConfig>? On { get; set; }

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
        /// Gets the list of joined children.
        /// </summary>
        public List<CdcJoinConfig> JoinChildren => Parent!.Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema).ToList();

        /// <summary>
        /// Inidicates whether it is first in the JoinHierarchy.
        /// </summary>
        public bool IsFirstInJoinHierarchy { get; private set; }

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

            TableName = DefaultWhereNull(TableName, () => Name);
            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' cannot be a view.");

            ModelName = DefaultWhereNull(ModelName, () => StringConversion.ToPascalCase(Name));
            JoinTo = DefaultWhereNull(JoinTo, () => Parent!.Name);
            JoinToSchema = DefaultWhereNull(JoinToSchema, () => Parent!.Schema);
            JoinCardinality = DefaultWhereNull(JoinCardinality, () => "OneToMany");
            PropertyName = DefaultWhereNull(PropertyName, () => JoinCardinality == "OneToMany" ? $"{ModelName}{(Name!.EndsWith("s", StringComparison.InvariantCulture) ? "s" : "es")}" : ModelName);

            // Get the JoinTo CdcJoinConfig.
            CdcJoinConfig? jtc = null;
            if (JoinTo != Parent!.Name || JoinToSchema != Parent!.Schema)
            {
                var tables = Parent!.Joins!.Where(x => x.Name == JoinTo && x.Schema == JoinToSchema).ToList();
                if (tables.Count == 0 || Parent!.Joins!.IndexOf(this) < Parent!.Joins!.IndexOf(tables[0]))
                    throw new CodeGenException(this, nameof(Name), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' must be previously specified.");
                else if (tables.Count > 1)
                    throw new CodeGenException(this, nameof(Name), $"Specified JoinTo Schema.Table '{JoinToSchema}.{JoinTo}' is ambiguous (more than one found).");

                jtc = tables[0];
                JoinToAlias = tables[0].Alias;
            }
            else
                JoinToAlias = Parent!.Alias;

            // Update the Join ons.
            if (On == null)
                On = new List<CdcJoinOnConfig>();

            foreach (var on in On)
            {
                on.Prepare(Root!, this);
            }

            // Wire up the hierarchy.
            JoinHierarchy.Add(this.PartialClone(true));

            if (jtc != null)
            {
                JoinHierarchy.Add(jtc);
                for (int i = 1; i < jtc.JoinHierarchy.Count; i++)
                {
                    JoinHierarchy.Add(jtc.JoinHierarchy[i].PartialClone(false));
                }
            }

            foreach (var c in DbTable.Columns)
            {
                if (c.IsPrimaryKey)
                {
                    var cc = new CdcJoinColumnConfig { Name = c.Name, DbColumn = c };
                    cc.Prepare(Root!, this);
                    PrimaryKeyColumns.Add(cc);
                }

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    var cc = new CdcJoinColumnConfig { Name = c.Name, DbColumn = c };
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
        /// Performs a partial clone.
        /// </summary>
        private CdcJoinConfig PartialClone(bool isFirst)
        {
            var j = new CdcJoinConfig
            {
                Name = Name,
                Schema = Schema,
                Alias = Alias,
                JoinTo = JoinTo,
                JoinToSchema = JoinToSchema,
                JoinToAlias = JoinToAlias,
                NonCdc = NonCdc,
                IsFirstInJoinHierarchy = isFirst,
                On = new List<CdcJoinOnConfig>(),
                DbTable = DbTable,
                Root = Root,
                Parent = Parent
            };

            foreach (var item in On!)
            {
                var jo = new CdcJoinOnConfig
                {
                    Name = item.Name,
                    ToColumn = item.ToColumn,
                    ToStatement = item.ToStatement,
                    Root = j.Root,
                    Parent = j
                };

                j.On.Add(jo);
            }

            return j;
        }
    }
}