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
    [ClassSchema("QueryJoinOn", Title = "'QueryJoinOn' object (database-driven)",
        Description = "The `QueryJoinOn` object defines the join on characteristics for a join within a query.",
        Markdown = "")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    public class QueryJoinOnConfig : ConfigBase<CodeGenConfig, QueryJoinConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("QueryJoinOn", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the join column (from the `Join` table).
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the join column (from the `Join` table).", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the other join to table schema.
        /// </summary>
        [JsonProperty("toSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the other join to table schema.",
            Description = "Defaults to `Table.Schema`; i.e. same schema. See also `ToTable` and `ToColumn` as these all relate.")]
        public string? ToSchema { get; set; }

        /// <summary>
        /// Gets or sets the name of the other join to table.
        /// </summary>
        [JsonProperty("toTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the other join to table.",
            Description = "Defaults to `Table.Name`; i.e. primary table. See also `ToSchema` and `ToColumn` as these all relate.")]
        public string? ToTable { get; set; }

        /// <summary>
        /// Gets or sets the name of the other join to column.
        /// </summary>
        [JsonProperty("toColumn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the other join to column.", IsImportant = true,
            Description = "Defaults to `Name`; i.e. assumes same name. See also `ToSchema` and `ToTable` as these all relate.")]
        public string? ToColumn { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name (`Alias.Name`) of the other column being joined to or other valid SQL (e.g. function) bypassing the corresponding `Schema`, `Table` and `Column` logic.
        /// </summary>
        [JsonProperty("toStatement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The fully qualified name (`Alias.Name`) of the other column being joined to or other valid SQL (e.g. function) bypassing the corresponding `Schema`, `Table` and `Column` logic.")]
        public string? ToStatement { get; set; }

        #endregion

        /// <summary>
        /// Gets the formatted Join On SQL statement.
        /// </summary>
        public string JoinOnSql => $"[{Parent!.Alias}].[{Name}] = {ToStatement}";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            var c = Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault();
            if (c == null)
                throw new CodeGenException(this, nameof(Name), $"JoinOn '{Name}' (Schema.Table '{Parent!.Schema}.{Parent!.Name}') not found in database.");

            if (string.IsNullOrEmpty(ToStatement))
            {
                ToSchema = DefaultWhereNull(ToSchema, () => Parent!.Parent!.Schema);
                ToTable = DefaultWhereNull(ToTable, () => Parent!.Parent!.Name);
                ToColumn = DefaultWhereNull(ToColumn, () => Name);

                c = Root!.DbTables.Where(x => x.Schema == ToSchema && x.Name == ToTable).SingleOrDefault()?.Columns.Where(x => x.Name == ToColumn).SingleOrDefault();
                if (c == null)
                    throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in database.");

                if (ToSchema == Parent!.Parent!.Schema && ToTable == Parent!.Parent!.Name)
                {
                    if (Parent!.Parent!.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in Table/Join configuration.");

                    ToStatement = $"[{Parent!.Parent!.Alias}].[{ToColumn}]";
                }
                else
                {
                    var t = Parent!.Parent!.Joins!.Where(x => ToSchema == x.Schema && ToTable == x.Name).SingleOrDefault();
                    if (t != null && t.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() != null)
                    {
                        ToStatement = $"[{t.Alias}].[{ToColumn}]";
                    }
                    else
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (Schema.Table '{ToSchema}.{ToTable}') not found in Table/Join configuration.");
                }
            }
        }
    }
}