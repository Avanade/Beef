// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx.DbSchema;
using Newtonsoft.Json;
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
    /// Represents the table configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Table", Title = "'Table' object (entity-driven)", 
        Description = "The `Table` object identifies an existing database `Table` (or `View`) and defines its code-generation characteristics.", 
        Markdown = @"The columns for the table (or view) are inferred from the database schema definition. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns from all the `StoredProcedure` children. A table can be defined more that once to enable different column configurations as required.

In addition to the primary [`Stored Procedures`](#Collections) generation, the following types of artefacts can also be generated:
- [Entity Framework](#EntityFramework) - Enables the generation of C# model code for [Entity Framework](https://docs.microsoft.com/en-us/ef/) data access.
- [UDT and TVP](#UDT) - Enables the [User-Defined Tables (UDT)](https://docs.microsoft.com/en-us/sql/relational-databases/server-management-objects-smo/tasks/using-user-defined-tables) and [Table-Valued Parameters (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) to enable collections of data to be passed bewteen SQL Server and .NET.",
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
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Columns", Title = "Provides the _Columns_ configuration.")]
    [CodeGenCategory("CodeGen", Title = "Provides the _Code Generation_ configuration.", Description = "These primarily provide a shorthand to create the standard `Get`, `GetAll`, `Create`, `Update`, `Upsert`, `Delete` and `Merge`.")]
    [CodeGenCategory("EntityFramework", Title = "Provides the _Entity Framework (EF) model_ configuration.")]
    [CodeGenCategory("UDT", Title = "Provides the _User Defined Table_ and _Table-Valued Parameter_ configuration.")]
    [CodeGenCategory("Auth", Title = "Provides the _Authorization_ configuration.")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class TableConfig : ConfigBase<CodeGenConfig, CodeGenConfig>, ITableReference, ISpecialColumnNames, ISpecialColumns
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Table", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the `Table` (or `View`) in the database.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the `Table` in the database.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the `Schema` where the `Table` is defined in the database.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the `Schema` where the `Table` is defined in the database.", IsImportant = true,
            Description = "Defaults to `CodeGeneration.Schema`.")]
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
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region CodeGen

        /// <summary>
        /// Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("get", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Get` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Get { get; set; }

        /// <summary>
        /// Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("getAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `GetAll` stored procedure is to be automatically generated where not otherwise explicitly specified.",
            Description = "The `GetAllOrderBy` is used to specify the `GetAll` query sort order.")]
        public bool? GetAll { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names (including sort order `ASC`/`DESC` literal) to be used as the `GetAll` query sort order.
        /// </summary>
        [JsonProperty("getAllOrderBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("CodeGen", Title = "The list of `Column` names (including sort order `ASC`/`DESC` literal) to be used as the `GetAll` query sort order.",
            Description = "This relates to the `GetAll` selection.")]
        public List<string>? GetAllOrderBy { get; set; }

        /// <summary>
        /// Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("create", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Create` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Create { get; set; }

        /// <summary>
        /// Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("update", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Update` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Update { get; set; }

        /// <summary>
        /// Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("upsert", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Upsert` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Upsert { get; set; }

        /// <summary>
        /// Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("delete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Delete` stored procedure is to be automatically generated where not otherwise explicitly specified.")]
        public bool? Delete { get; set; }

        /// <summary>
        /// Indicates whether a `Merge` (insert/update/delete of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("merge", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CodeGen", Title = "Indicates whether a `Merge` (insert/update/delete of `Udt` list) stored procedure is to be automatically generated where not otherwise explicitly specified.",
            Description = "This will also require a `Udt` (SQL User Defined Table) and `Tvp` (.NET Table-Valued Parameter) to function.")]
        public bool? Merge { get; set; }

        #endregion

        #region EntityFramework

        /// <summary>
        /// Indicates whether an `Entity Framework` .NET (C#) model is to be generated.
        /// </summary>
        [JsonProperty("efModel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("EntityFramework", Title = "Indicates whether an `Entity Framework` .NET (C#) model is to be generated.")]
        public bool? EfModel { get; set; }

        /// <summary>
        /// Gets or sets the .NET (C#) EntityFramework (EF) model name.
        /// </summary>
        [JsonProperty("efModelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("EntityFramework", Title = "The .NET (C#) EntityFramework (EF) model name.",
            Description = "Defaults to `Name` applying the `CodeGeneration.AutoDotNetRename`.")]
        public string? EfModelName { get; set; }

        #endregion

        #region UDT

        /// <summary>
        /// Indicates whether a `User Defined Table (UDT)` type should be created.
        /// </summary>
        [JsonProperty("udt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("UDT", Title = "Indicates whether a `User Defined Table (UDT)` type should be created.", IsImportant = true)]
        public bool? Udt { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the `User Defined Table (UDT)`.
        /// </summary>
        [JsonProperty("udtExcludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("UDT", Title = "The list of `Column` names to be excluded from the `User Defined Table (UDT)`.",
            Description = "Where not specified this indicates that no `Columns` are to be excluded.")]
        public List<string>? UdtExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.
        /// </summary>
        [JsonProperty("tvp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("UDT", Title = "The name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.", IsImportant = true)]
        public string? Tvp { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "The permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).", IsImportant = true)]
        public string? Permission { get; set; }

        /// <summary>
        /// Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.
        /// </summary>
        [JsonProperty("orgUnitImmutable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Auth", Title = "Indicates whether the `OrgUnitId` column is considered immutable, in that it can not be changed once set.", IsImportant = true,
            Description = "Defaults to `CodeGeneration.OrgUnitImmutable`. This is only applicable for stored procedures.")]
        public bool? OrgUnitImmutable { get; set; }

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
        /// Gets or sets the corresponding <see cref="StoredProcedureConfig"/> collection.
        /// </summary>
        [JsonProperty("storedProcedures", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `StoredProcedure` collection.",
            Markdown = "A `StoredProcedure` object defines the stored procedure code-generation characteristics.")]
        public List<StoredProcedureConfig>? StoredProcedures { get; set; }

        #endregion

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<TableColumnConfig> Columns { get; } = new List<TableColumnConfig>();

        /// <summary>
        /// Gets the related IsDeleted column.
        /// </summary>
        public IColumnConfig? ColumnIsDeleted { get; set; }

        /// <summary>
        /// Gets the related TenantId column.
        /// </summary>
        public IColumnConfig? ColumnTenantId { get; set; }

        /// <summary>
        /// Gets the related OrgUnitId column.
        /// </summary>
        public IColumnConfig? ColumnOrgUnitId { get; set; }

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
        private TableColumnConfig? GetSpecialColumn(string? name) => Columns?.Where(x => x.Name == name && !x.DbColumn!.IsPrimaryKey).SingleOrDefault();

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
        /// Gets the identity columns considered part of the primary key.
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
        public DbTableSchema? DbTable { get; private set; }

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => DbTable!.QualifiedName;

        /// <summary>
        /// Gets or sets the view where statements.
        /// </summary>
        public List<string>? ViewWhere { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConverter.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));
            EfModelName = DefaultWhereNull(EfModelName, () => Root.RenameForDotNet(Name));
            OrgUnitImmutable = DefaultWhereNull(OrgUnitImmutable, () => Parent!.OrgUnitImmutable);

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
                var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                if (ca != null)
                {
                    var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && parts[1].Length > 0)
                        cc.NameAlias = parts[1];
                }

                cc.NameAlias ??= Root.RenameForDotNet(cc.Name);

                await cc.PrepareAsync(Root!, this).ConfigureAwait(false);

                // Certain special columns have to always be included.
                if (cc.Name == ColumnNameTenantId)
                {
                    ColumnTenantId = cc;
                    Columns.Add(cc);
                }
                else if (cc.Name == ColumnNameOrgUnitId)
                {
                    ColumnOrgUnitId = cc;
                    Columns.Add(cc);
                }
                else if (cc.Name == ColumnNameIsDeleted)
                {
                    ColumnIsDeleted = cc;
                    Columns.Add(cc);
                }
                else if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                    Columns.Add(cc);
                else if (cc.IsAudit && StoredProcedures!.Any(x => x.Type == "Create" || x.Type == "Update" || x.Type == "Upsert" || x.Type == "Delete" || x.Type == "Merge"))
                    Columns.Add(cc);
            }

            foreach (var storedProcedure in StoredProcedures!)
            {
                await storedProcedure.PrepareAsync(Root!, this).ConfigureAwait(false);
            }

            if (CompareValue(DbTable.IsAView, true))
                PrepareView();
        }

        /// <summary>
        /// Prepares the stored procedures.
        /// </summary>
        private void PrepareStoredProcedures()
        {
            StoredProcedures ??= new List<StoredProcedureConfig>();

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
                    foreach (var ob in GetAllOrderBy)
                    {
                        var parts = ob.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 1)
                            spc.OrderBy.Add(new OrderByConfig { Name = parts[0] }); 
                        else if (parts.Length > 1)
                            spc.OrderBy.Add(new OrderByConfig { Name = parts[0], Order = parts[1].StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending" });
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