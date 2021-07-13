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
    [CategorySchema("Database", Title = "Provides the _database_ configuration.")]
    [CategorySchema("DotNet", Title = "Provides the _.NET_ configuration.")]
    [CategorySchema("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CategorySchema("IdentifierMapping", Title = "Provides the _identifier mapping_ configuration.")]
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
        /// Gets or sets the default schema name used where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The default schema name used where not otherwise explicitly specified.",
            Description = "Defaults to `CodeGeneration.Schema`.")]
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
        [PropertyCollectionSchema("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the `Cdc` execute outbox stored procedure name.
        /// </summary>
        [JsonProperty("executeStoredProcedureName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The `CDC` _execute_ outbox stored procedure name.",
            Description = "Defaults to `spExecute` (literal) + `Name` + `CdcOutbox` (literal); e.g. `spExecuteTableNameCdcOutbox`.")]
        public string? ExecuteStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the `Cdc` complete outbox stored procedure name.
        /// </summary>
        [JsonProperty("completeStoredProcedureName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The `CDC` _complete_ outbox stored procedure name.",
            Description = "Defaults to `spComplete` (literal) + `Name` + `CdcOutbox` (literal); e.g. `spCompleteTableNameCdcOutbox`.")]
        public string? CompleteStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonProperty("cdcSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The schema name for the generated `CDC`-related database artefacts.",
            Description = "Defaults to `CodeGeneration.CdcSchema`.")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Gets or sets the corresponding `Cdc` Outbox table name.
        /// </summary>
        [JsonProperty("outboxTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Database", Title = "The corresponding `CDC` Outbox table name.",
            Description = "Defaults to `Name` + `Outbox` (literal).")]
        public string? OutboxTableName { get; set; }

        #endregion

        #region DotNet

        /// <summary>
        /// Gets or sets the `Cdc` .NET model name.
        /// </summary>
        [JsonProperty("modelName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the access modifier for the generated CDC `Data` constructor.
        /// </summary>
        [JsonProperty("dataConstructor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The access modifier for the generated CDC `Data` constructor.", Options = new string[] { "Public", "Private", "Protected" },
            Description = "Defaults to `Public`.")]
        public string? DataCtor { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-default) Dependency Injection (DI) parameters for the generated CDC `Data` constructor.
        /// </summary>
        [JsonProperty("dataCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Data", Title = "The list of additional (non-default) Dependency Injection (DI) parameters for the generated CDC `Data` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred.")]
        public List<string>? DataCtorParams { get; set; }

        /// <summary>
        /// Gets or sets the CDC .NET database interface name.
        /// </summary>
        [JsonProperty("databaseName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The .NET database interface name.",
            Description = "Defaults to `IDatabase`.")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonProperty("eventSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CDC", Title = "The Event Source.",
            Description = "Defaults to `ModelName` (as lowercase). Note: when used in code-generation the `CodeGeneration.EventSourceRoot` will be prepended where specified.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Source when an Event is published.
        /// </summary>
        [JsonProperty("eventSourceFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The default formatting for the Source when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndGlobalId" },
            Description = "Defaults to `CodeGeneration.EventSourceFormat`.")]
        public string? EventSourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the event subject.
        /// </summary>
        [JsonProperty("eventSubject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The Event Subject.",
            Description = "Defaults to `ModelName`. Note: when used in code-generation the `CodeGeneration.EventSubjectRoot` will be prepended where specified.")]
        public string? EventSubject { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DataSvc", Title = "The default formatting for the Subject when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey" },
            Description = "Defaults to `CodeGeneration.EventSubjectFormat`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be included (in addition to the primary key) for a logical delete.
        /// </summary>
        [JsonProperty("includeColumnsOnDelete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("DotNet", Title = "The list of `Column` names that should be included (in addition to the primary key) for a logical delete.",
           Description = "Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.")]
        public List<string>? IncludeColumnsOnDelete { get; set; }

        /// <summary>
        /// The option to exclude the generation of the <c>CdcHostedService</c> (background) class (<c>XxxHostedService.cs</c>).
        /// </summary>
        [JsonProperty("excludeHostedService", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("DotNet", Title = "The option to exclude the generation of the `CdcHostedService` (background) class (`XxxHostedService.cs`).", IsImportant = true)]
        public bool? ExcludeHostedService { get; set; }

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

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnNameIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Infer", Title = "The column name for the `IsDeleted` capability.",
            Description = "Defaults to `CodeGeneration.IsDeleted`.")]
        public string? ColumnNameIsDeleted { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="CdcJoinConfig"/> collection.
        /// </summary>
        [JsonProperty("joins", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema("Collections", Title = "The corresponding `Join` collection.", IsImportant = true,
            Markdown = "A `Join` object provides the configuration for a joining table.")]
        public List<CdcJoinConfig>? Joins { get; set; }

        #endregion

        #region ISpecialColumns

        /// <summary>
        /// Gets the related IsDeleted column.
        /// </summary>
        public IColumnConfig? ColumnIsDeleted => GetSpecialColumn(ColumnNameIsDeleted);

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

            var cc = new CdcColumnConfig { Name = c.Name, DbColumn = c };
            cc.Prepare(Root!, this);
            return cc;
        }

        #endregion

        /// <summary>
        /// Gets the SQL formatted selected columns.
        /// </summary>
        public List<CdcColumnConfig> SelectedColumns { get; } = new List<CdcColumnConfig>();

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<CdcColumnConfig> PrimaryKeyColumns { get; } = new List<CdcColumnConfig>();

        /// <summary>
        /// Gets the SQL formatted selected columns excluding the <see cref="PrimaryKeyColumns"/>.
        /// </summary>
        public List<CdcColumnConfig> SelectedColumnsExcludingPrimaryKey => SelectedColumns.Where(x => !(x.DbColumn!.DbTable == DbTable && x.DbColumn.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the SQL formatted selected columns for the .NET Entity (sans IsDeleted).
        /// </summary>
        public List<CdcColumnConfig> SelectedEntityColumns => SelectedColumns.Where(x => !x.IsIsDeletedColumn).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<CdcColumnConfig> Columns { get; } = new List<CdcColumnConfig>();

        /// <summary>
        /// Gets the <see cref="QueryJoinConfig"/> collection for "all" those that are also CDC monitored.
        /// </summary>
        public List<CdcJoinConfig> CdcJoins => Joins!.Where(x => CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the <see cref="QueryJoinConfig"/> collection for "all" those that are not flagged as CDC monitored.
        /// </summary>
        public List<CdcJoinConfig> NonCdcJoins => Joins!.Where(x => !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of CDC joined "directly related" children.
        /// </summary>
        public List<CdcJoinConfig> JoinCdcChildren => Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of non-CDC joined "directly related" children.
        /// </summary>
        public List<CdcJoinConfig> JoinNonCdcChildren => Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the Data constructor parameters.
        /// </summary>
        public List<CtorParameterConfig> DataCtorParameters { get; } = new List<CtorParameterConfig>();

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => DbTable!.QualifiedName;

        /// <summary>
        /// Gets the event source URI.
        /// </summary>
        public string EventSourceUri => Root!.EventSourceRoot + (EventSource!.StartsWith('/') || (Root!.EventSourceRoot != null && Root!.EventSourceRoot.EndsWith('/')) ? EventSource : ("/" + EventSource));

        /// <summary>
        /// Indicates whether there is at least one global identifier being used somewhere.
        /// </summary>
        public bool UsesGlobalIdentifier { get; private set; }

        /// <summary>
        /// Gets the list of properties to exlcude from the ETag.
        /// </summary>
        public List<string> ExcludePropertiesFromETag { get; set; } = new List<string>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            Schema = DefaultWhereNull(Schema, () => Root!.Schema);
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{Schema}.{Name}' cannot be a view.");

            Alias = DefaultWhereNull(Alias, () => DbTable.Alias);

            ExecuteStoredProcedureName = DefaultWhereNull(ExecuteStoredProcedureName, () => $"spExecute{StringConversion.ToPascalCase(Name)}CdcOutbox");
            CompleteStoredProcedureName = DefaultWhereNull(CompleteStoredProcedureName, () => $"spComplete{StringConversion.ToPascalCase(Name)}CdcOutbox");
            CdcSchema = DefaultWhereNull(CdcSchema, () => Root.CdcSchema);
            OutboxTableName = DefaultWhereNull(OutboxTableName, () => Name + "Outbox");
            ModelName = DefaultWhereNull(ModelName, () => Root.RenameForDotNet(Name));
            EventSource = DefaultWhereNull(EventSource, () => ModelName!.ToLowerInvariant());
            EventSourceFormat = DefaultWhereNull(EventSourceFormat, () => Root!.EventSourceFormat);
            EventSubject = DefaultWhereNull(EventSubject, () => ModelName);
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => Root!.EventSubjectFormat);
            DataCtor = DefaultWhereNull(DataCtor, () => "Public");
            DatabaseName = DefaultWhereNull(DatabaseName, () => "IDatabase");
            ExcludeHostedService = DefaultWhereNull(ExcludeHostedService, () => false);
            if (ExcludeColumnsFromETag == null && Root!.CdcExcludeColumnsFromETag != null)
                ExcludeColumnsFromETag = new List<string>(Root!.CdcExcludeColumnsFromETag!);

            ColumnNameIsDeleted = DefaultWhereNull(ColumnNameIsDeleted, () => Root!.ColumnNameIsDeleted);

            foreach (var c in DbTable.Columns)
            {
                var cc = new CdcColumnConfig { Name = c.Name, DbColumn = c };
                var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                if (ca != null)
                {
                    var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        cc.NameAlias = parts[1];
                }

                if (c.IsPrimaryKey)
                {
                    cc.IncludeColumnOnDelete = true;
                    cc.IgnoreSerialization = IdentifierMapping == true;
                    cc.Prepare(Root!, this);
                    PrimaryKeyColumns.Add(cc);
                }
                else if (IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!))
                    cc.IncludeColumnOnDelete = true;

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    if (cc.Name != ColumnIsDeleted?.Name)
                    {
                        MapIdentifierMappingColumn(Root!, this, Schema!, IdentifierMappingColumns, cc);
                        cc.Prepare(Root!, this);
                        Columns.Add(cc);
                    }
                }

                // Always include IsDeleted!
                if (cc.Name == ColumnIsDeleted?.Name)
                {
                    cc.Prepare(Root!, this);
                    Columns.Add(cc);
                }
            }

            // Build up the selected columns list.
            foreach (var c in Columns)
            {
                var cc = new CdcColumnConfig
                {
                    Name = c.Name,
                    DbColumn = c.DbColumn,
                    NameAlias = c.NameAlias,
                    IncludeColumnOnDelete = c.IncludeColumnOnDelete,
                    IgnoreSerialization = c.IgnoreSerialization || c.IdentifierMappingTable != null
                };

                cc.Prepare(Root!, this);
                SelectedColumns.Add(cc);

                if (c.IdentifierMappingTable != null)
                {
                    cc = new CdcColumnConfig
                    {
                        Name = "GlobalId",
                        DbColumn = new DbColumn { Name = c.Name, Type = "NVARCHAR", DbTable = c.DbColumn!.DbTable },
                        NameAlias = "Global" + c.NameAlias,
                        IdentifierMappingAlias = c.IdentifierMappingAlias,
                        IdentifierMappingSchema = c.IdentifierMappingSchema,
                        IdentifierMappingTable = c.IdentifierMappingTable,
                        IdentifierMappingParent = cc
                    };

                    cc.Prepare(Root!, this);
                    SelectedColumns.Add(cc);
                }
            }

            // Data constructors. 
            var tmp = new List<Entity.ParameterConfig>();
            Entity.EntityConfig.AddConfiguredParameters(DataCtorParams, tmp);
            foreach (var t in tmp)
            {
                var ctor = new CtorParameterConfig { Name = t.Name, Type = t.Type };
                ctor.Prepare(Root!, Root!);
                DataCtorParameters.Add(ctor);
            }

            PrepareJoins();

            UsesGlobalIdentifier = IdentifierMapping == true || (IdentifierMappingColumns != null && IdentifierMappingColumns.Count > 1) || Joins.Any(x => x.IdentifierMapping == true || (x.IdentifierMappingColumns != null && x.IdentifierMappingColumns.Count > 1));
            SetUpExcludePropertiesFromETag();
        }

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
                join.Alias = DefaultWhereNull(join.Alias, () => DbTable.CreateAlias(join.Name!));

                if (dict.TryGetValue(join.Alias!, out var val))
                {
                    dict[join.Alias!] = ++val;
                    join.Alias = $"{join.Alias}{val}";
                }
                else
                    dict.Add(join.Alias!, 1);

                join.Prepare(Root!, this);
            }

            // Do some further validation.
            if (Joins.Any(x => x.Name == Name))
                throw new CodeGenException(this, nameof(Name), $"The Name '{Name}' is ambiguous (not unique); please make 'Name' unique and set 'TableName' to the actual table name to correct.");

            foreach (var j in Joins)
            {
                if (Joins.Any(x => x != j && x.Name == j.Name))
                    throw new CodeGenException(this, nameof(Joins), $"The Name '{j.Name}' is ambiguous (not unique); please make 'Name' unique and set 'TableName' to the actual table name to correct.");
            }
        }

        /// <summary>
        /// Check whether column is selected for identity mapping and map accordingly.
        /// </summary>
        internal static void MapIdentifierMappingColumn<T>(CodeGenConfig root, ConfigBase config, string schema, List<string>? identifierMappingColumns, IIdentifierMappingColumn<T> cc) where T : class
        {
            if (identifierMappingColumns == null)
                return;

            var imc = identifierMappingColumns.FirstOrDefault(x => x.StartsWith(cc.Name + "^", StringComparison.Ordinal));
            if (imc == null)
                return;

            if (cc.DbColumn!.IsPrimaryKey)
                throw new CodeGenException(config, nameof(identifierMappingColumns), $"Column '{cc.Name}' cannot be configured using {nameof(IdentifierMappingColumns)} as it is part of the primary key; use the {nameof(IdentifierMapping)} feature instead.");

            var parts = imc.Split("^");
            if (parts.Length < 2 || parts.Length > 3)
                throw new CodeGenException(config, nameof(identifierMappingColumns), $"Column '{cc.Name}' configuration '{imc}' that is not correctly formatted.");

            cc.IdentifierMappingSchema = parts.Length == 3 ? parts[1] : schema;
            cc.IdentifierMappingTable = parts.Length == 2 ? parts[1] : parts[2];
            cc.IdentifierMappingAlias = $"_im{identifierMappingColumns.IndexOf(imc) + 1}";

            var t = root!.DbTables.FirstOrDefault(x => x.Schema == cc.IdentifierMappingSchema && x.Name == cc.IdentifierMappingTable);
            if (t == null)
                throw new CodeGenException(config, nameof(identifierMappingColumns), $"Column '{cc.Name}' references table '{cc.IdentifierMappingSchema}.{cc.IdentifierMappingTable}' that does not exist.");

            if (t.Columns.Count(x => x.IsPrimaryKey) != 1)
                throw new CodeGenException(config, nameof(identifierMappingColumns), $"Column '{cc.Name}' references table '{cc.IdentifierMappingSchema}.{cc.IdentifierMappingTable}' which must only have a single column representing the primary key.");
        }

        /// <summary>
        /// Sets up the <see cref="ExcludePropertiesFromETag"/> list.
        /// </summary>
        private void SetUpExcludePropertiesFromETag()
        {
            if (ExcludeColumnsFromETag != null)
            {
                foreach (var ec in ExcludeColumnsFromETag)
                {
                    var c = Columns.Where(x => x.Name == ec).FirstOrDefault();
                    if (c != null)
                        ExcludePropertiesFromETag.Add(c.NameAlias!);
                }
            }

            if (Joins != null)
            {
                foreach (var j in Joins)
                {
                    if (j.ExcludeColumnsFromETag != null)
                    {
                        var p = string.Join('.', j.JoinHierarchyReverse.Select(x => x.PropertyName));
                        foreach (var ec in j.ExcludeColumnsFromETag)
                        {
                            var c = j.Columns.Where(x => x.Name == ec).FirstOrDefault();
                            if (c != null)
                                ExcludePropertiesFromETag.Add(p + '.' + c.NameAlias!);
                        }
                    }
                }
            }

            if (ExcludePropertiesFromETag != null && ExcludePropertiesFromETag.Count > 0)
                ExcludePropertiesFromETag = ExcludePropertiesFromETag.Distinct().ToList();
        }
    }
}