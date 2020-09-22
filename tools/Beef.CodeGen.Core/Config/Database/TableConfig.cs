// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Table", Title = "The **Table** is used to identify a database `Table` and define its code-generation characteristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("CodeGen", Title = "Provides the **CodeGen** configuration to select via shorthand the code-gen artefacts.")]
    [CategorySchema("UDT", Title = "Provides the **UDT (user defined type)** configuration.")]
    [CategorySchema("Auth", Title = "Provides the **Authorization** configuration.")]
    public class TableConfig : ConfigBase<CodeGenConfig, CodeGenConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the `Table` in the database.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `Table` in the database.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the `Schema` where the `Table` is defined in the database.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `Schema` where the `Table` is defined in the database.", IsMandatory = true, IsImportant = true)]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the `Schema` where the `Table` is defined in the database.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the `Schema` where the `Table` is defined in the database.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that no `Columns` are to be excluded.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? ExcludeColumns { get; set; }

        #endregion

        #region CodeGen

        /// <summary>
        /// Indicates that a `Get` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("get", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `Get` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Get { get; set; }

        /// <summary>
        /// Indicates that a `GetColl` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("getColl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `GetAll` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? GetColl { get; set; }

        /// <summary>
        /// Gets or sets the list of columns names (including sort order ASC/DESC) to be used as the GetAll query sort order (will automatically add `alias` where not specified for column).
        /// </summary>
        [JsonProperty("getAll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "The list of columns names (including sort order ASC/DESC) to be used as the GetAll query sort order (will automatically add `alias` where not specified for column).")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? GetCollOrderBy { get; set; }

        /// <summary>
        /// Indicates that a `Create` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("create", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `Create` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Create { get; set; }

        /// <summary>
        /// Indicates that a `Update` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("update", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `Update` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Update { get; set; }

        /// <summary>
        /// Indicates that a `Upsert` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("upsert", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `Upsert` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Upsert { get; set; }

        /// <summary>
        /// Indicates that a `Delete` stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("delete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `Delete` stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Delete { get; set; }

        /// <summary>
        /// Indicates that a `View` will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("view", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("CodeGen", Title = "Indicates that a `View` will be automatically generated where not otherwise explicitly specified (only applies for a `Table`).")]
        public bool? View { get; set; }

        #endregion

        #region Udt

        /// <summary>
        /// Indicates that a `User Defined Table (UDT)` type should be created.
        /// </summary>
        [JsonProperty("udt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "Indicates that a `User Defined Table (UDT)` type should be created.", IsImportant = true)]
        public bool? Udt { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the `User Defined Table (UDT)`.
        /// </summary>
        [JsonProperty("udtExcludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "The list of `Column` names to be excluded from the `User Defined Table (UDT)`.",
            Description = "Where not specified this indicates that no `Columns` are to be excluded.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO.")]
        public List<string>? UdtExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.
        /// </summary>
        [JsonProperty("tvp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "The name of the .NET entity associated with the `Udt` so that it can be expressed (created) as a Table-Valued Parameter for usage within the corresponding `DbMapper`.", IsImportant = true)]
        public string? Tvp { get; set; }

        /// <summary>
        /// Indicates that a `Merge` (upsert of `Udt` list) stored procedure will be automatically generated where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("merge", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Udt", Title = "Indicates that a `Merge` (upsert of `Udt` list) stored procedure will be automatically generated where not otherwise explicitly specified.")]
        public bool? Merge { get; set; }

        #endregion

        #region Auth

        /// <summary>
        /// Gets or sets the permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).
        /// </summary>
        [JsonProperty("permission", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Auth", Title = "The permission (prefix) to be used for security permission checking (suffix defaults to `Read`, `Write` or `Delete` and can be overridden in the underlying stored procedure).", IsImportant = true)]
        public string? Permission { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the corresponding <see cref="StoredProcedureConfig"/> collection.
        /// </summary>
        [JsonProperty("storedProcedures", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertyCollectionSchema(Title = "The corresponding `StoredProcedure` collection.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is appropriate for what is obstensibly a DTO.")]
        public List<StoredProcedureConfig>? StoredProcedures { get; set; }

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public Table? DbTable { get; private set; }

        /// <summary>
        /// Gets the selected (actual) database column configurations.
        /// </summary>
        public List<Column> DbColumns { get; } = new List<Column>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            Schema = DefaultWhereNull(Schema, () => "dbo");
            DbTable = Root!.DbTables.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException($"Specified Schema.Table '{Schema}.{Name}' not found in database.");

            Alias = DefaultWhereNull(Alias, () => new string(StringConversion.ToSentenceCase(Name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray()));

            foreach (var c in DbTable.Columns)
            {
                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                    DbColumns.Add(c);
            }

            if (StoredProcedures != null && StoredProcedures.Count > 0)
            {
                foreach (var storedProcedure in StoredProcedures)
                {
                    storedProcedure.Prepare(Root!, this);
                }
            }
        }
    }
}