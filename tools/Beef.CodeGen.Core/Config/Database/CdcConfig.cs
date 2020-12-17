// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Cdc", Title = "'Cdc' object (database-driven)",
        Description = "The `Cdc` object enables the definition of the primary table, one-or-more child tables and their relationships, to enable Change Data Capture (CDC) event publishing."
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("Columns", Title = "Provides the _Columns_ configuration.")]
    [CategorySchema("CDC", Title = "Provides the _Change Data Capture (CDC)_ configuration.")]
    [CategorySchema("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CdcConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, ITableReference, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Cdc", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the primary table.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the primary table.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the primary table of the view.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The schema name of the primary table.",
            Description = "Defaults to `dbo`.")]
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
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Cdc

        /// <summary>
        /// Gets or sets the `Cdc` get outbox stored procedure name.
        /// </summary>
        [JsonProperty("cdcStoredProcedureName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The `CDC` get outbox stored procedure name.",
            Description = "Defaults to `spGet` (literal) + `Name` + `OutboxData` (literal); e.g. `spGetTableNameOutboxData`.")]
        public string? StoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonProperty("cdcSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The schema name for the `CDC`-related database artefacts.",
            Description = "Defaults to `Schema` + `Cdc` (literal).")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Gets or sets the corresponding `Cdc` Outbox Envelope table name.
        /// </summary>
        [JsonProperty("cdcEnvelopeTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The corresponding `CDC` Outbox Envelope table name.",
            Description = "Defaults to `Name` + `OutboxEnvelope` (literal).")]
        public string? EnvelopeTableName { get; set; }

        /// <summary>
        /// Gets or sets the `Cdc` .NET model name.
        /// </summary>
        [JsonProperty("cdcModelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated CDC `Data` constructor.
        /// </summary>
        [JsonProperty("cdcDataConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The access modifier for the generated CDC `Data` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataConstructor { get; set; }

        /// <summary>
        /// Gets or sets the CDC .NET database interface name.
        /// </summary>
        [JsonProperty("cdcDatabaseName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The .NET database interface name.",
            Description = "Defaults to `IDatabase`.")]
        public string? DatabaseName { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="CdcJoinConfig"/> collection.
        /// </summary>
        [JsonProperty("joins", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Join` collection.", IsImportant = true,
            Markdown = "A `Join` object provides the configuration for a joining table.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<CdcJoinConfig>? Joins { get; set; }

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
        /// Gets the SQL formatted selected columns.
        /// </summary>
        public List<IColumnConfig> SelectedColumns { get; } = new List<IColumnConfig>();

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<CdcColumnConfig> PrimaryKeyColumns { get; } = new List<CdcColumnConfig>();

        /// <summary>
        /// Gets the SQL formatted selected columns excluding the <see cref="PrimaryKeyColumns"/>.
        /// </summary>
        public List<IColumnConfig> SelectedColumnsExcludingPrimaryKey => SelectedColumns.Where(x => !(x.DbColumn!.DbTable == DbTable && x.DbColumn.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<CdcColumnConfig> Columns { get; } = new List<CdcColumnConfig>();

        /// <summary>
        /// Gets the  <see cref="QueryJoinConfig"/> collection for those that are also CDC monitored.
        /// </summary>
        public List<CdcJoinConfig> CdcJoins => Joins!.Where(x => CompareNullOrValue(x.NonCdc, true)).ToList();

        /// <summary>
        /// Gets the  <see cref="QueryJoinConfig"/> collection for those that are not flagged as CDC monitored.
        /// </summary>
        public List<CdcJoinConfig> NonCdcJoins => Joins!.Where(x => CompareValue(x.NonCdc, false)).ToList();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets or sets the event subject format.
        /// </summary>
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            Schema = DefaultWhereNull(Schema, () => "dbo");
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' cannot be a view.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConversion.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));

            StoredProcedureName = DefaultWhereNull(StoredProcedureName, () => $"spGet{StringConversion.ToPascalCase(Name)}OutboxData");
            CdcSchema = DefaultWhereNull(CdcSchema, () => Schema + "Cdc");
            EnvelopeTableName = DefaultWhereNull(EnvelopeTableName, () => Name + "OutboxEnvelope");
            ModelName = DefaultWhereNull(ModelName, () => StringConversion.ToPascalCase(Name));
            DataConstructor = DefaultWhereNull(DataConstructor, () => "Public");
            DatabaseName = DefaultWhereNull(DatabaseName, () => "IDatabase");

            PrepareJoins();

            foreach (var c in DbTable.Columns)
            {
                if (c.IsPrimaryKey)
                {
                    var cc = new CdcColumnConfig { Name = c.Name, DbColumn = c };
                    cc.Prepare(Root!, this);
                    PrimaryKeyColumns.Add(cc);
                }

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    var cc = new CdcColumnConfig { Name = c.Name, DbColumn = c };
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

            // Build up the selected columns list.
            foreach (var c in Columns)
            {
                var cc = new CdcColumnConfig { Name = c.Name, DbColumn = c.DbColumn, NameAlias = c.NameAlias };
                cc.Prepare(Root!, this);
                SelectedColumns.Add(cc);
            }

            //foreach (var j in Joins!)
            //{
            //    if (j.ColumnTenantId != null)
            //        Where.Add(new QueryWhereConfig { Statement = $"[{j.Alias}].[{j.ColumnTenantId.Name}] = dbo.fnGetTenantId(NULL)" });

            //    if (j.ColumnIsDeleted != null)
            //        Where.Add(new QueryWhereConfig { Statement = $"([{j.Alias}].[{j.ColumnIsDeleted.Name}] IS NULL OR [{j.Alias}].[{j.ColumnIsDeleted.Name}] = 0)" });

            //    foreach (var c in j.Columns)
            //    {
            //        if (!c.IsIsDeletedColumn && !c.IsTenantIdColumn)
            //        {
            //            var cc = new QueryJoinColumnConfig { Name = c.Name, DbColumn = c.DbColumn, NameAlias = c.NameAlias };
            //            cc.Prepare(Root!, j);
            //            SelectedColumns.Add(cc);
            //        }
            //    }
            //}

            //// Prepare the where clauses.
            //foreach (var where in Where)
            //{
            //    where.Prepare(Root!, this);
            //}

            //CdcEventSubject = DefaultWhereNull(CdcEventSubject, () =>
            //{
            //    var sb = new StringBuilder();

            //    foreach (var pkc in PrimaryKeyColumns)
            //    {
            //        if (sb.Length == 0)
            //            sb.Append($"{CdcModelName}.");
            //        else
            //            sb.Append(",");

            //        sb.Append("{model.");
            //        sb.Append(StringConversion.ToPascalCase(pkc.NameAlias));
            //        sb.Append("}");
            //    }

            //    return sb.ToString();
            //});

            //CdcEventSubjectFormat = string.IsNullOrEmpty(Root.EventSubjectRoot) ? CdcEventSubject : $"{Root.EventSubjectRoot}.{CdcEventSubject}";
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
                Joins = new List<CdcJoinConfig>();

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