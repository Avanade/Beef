// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the table join on condition configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("JoinOn", Title = "The **JoinOn** is used to define a table join on configuration", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class QueryJoinOnConfig : ConfigBase<CodeGenConfig, QueryJoinConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the join column to join on.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the join table column to join on.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the join to table schema.
        /// </summary>
        [JsonProperty("toSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the table join to table schema.",
            Description = "Defaults to `Table.Schema`; i.e. same schema. See also `ToTable` and `ToColumn` as these all relate.")]
        public string? ToSchema { get; set; }

        /// <summary>
        /// Gets or sets the name of the join to table.
        /// </summary>
        [JsonProperty("toTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the join to table.",
            Description = "Defaults to `Table.Name`; i.e. primary table. See also `ToSchema` and `ToColumn` as these all relate.")]
        public string? ToTable { get; set; }

        /// <summary>
        /// Gets or sets the name of the join to column.
        /// </summary>
        [JsonProperty("toColumn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name name of the join to column.", IsImportant = true,
            Description = "Defaults to `Name`; i.e. assumes same name. See also `ToSchema` and `ToTable` as these all relate.")]
        public string? ToColumn { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name (`Alias.Name`)` of the column being joined to or other valid SQL (e.g. function) bypassing (overridding) the `Schema`, `Table` and `Column` logic.
        /// </summary>
        [JsonProperty("toSql", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The fully qualified name (`Alias.Name`) of the column being joined to or other valid SQL (e.g. function) bypassing the `Schema`, `Table` and `Column` logic.",
            Description = "")]
        public string? ToSql { get; set; }

        #endregion

        /// <summary>
        /// Gets the formatted Join On SQL statement.
        /// </summary>
        public string JoinOnSql => $"[{Parent!.Alias}].[{Name}] = {ToSql}";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name.Substring(1);

            var c = Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault();
            if (c == null)
                throw new CodeGenException($"JoinOn '{Name}' (Schema.Table '{Parent!.Schema}.{Parent!.Name}') not found in database.");

            if (string.IsNullOrEmpty(ToSql))
            {
                ToSchema = DefaultWhereNull(ToSchema, () => Parent!.Parent!.Schema);
                ToTable = DefaultWhereNull(ToTable, () => Parent!.Parent!.Name);
                ToColumn = DefaultWhereNull(ToColumn, () => Name);

                c = Root!.DbTables.Where(x => x.Schema == ToSchema && x.Name == ToTable).SingleOrDefault()?.Columns.Where(x => x.Name == ToColumn).SingleOrDefault();
                if (c == null)
                    throw new CodeGenException($"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in database.");

                if (ToSchema == Parent!.Parent!.Schema && ToTable == Parent!.Parent!.Name)
                {
                    if (Parent!.Parent!.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException($"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in Table/Join configuration.");

                    ToSql = $"[{Parent!.Parent!.Alias}].[{ToColumn}]";
                }
                else
                {
                    var t = Parent!.Parent!.Joins!.Where(x => ToSchema == x.Schema && ToTable == x.Name).SingleOrDefault();
                    if (t != null && t.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() != null)
                    {
                        ToSql = $"[{t.Alias}].[{ToColumn}]";
                    }
                    else
                        throw new CodeGenException($"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in Table/Join configuration.");
                }
            }
        }
    }
}