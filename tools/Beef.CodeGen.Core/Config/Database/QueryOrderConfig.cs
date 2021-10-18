// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using System;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure order-by configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("QueryOrder", Title = "'QueryOrder' object (database-driven)",
        Description = "The `QueryOrder` object that defines the query order.")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    public class QueryOrderConfig : ConfigBase<CodeGenConfig, QueryConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("QueryOrder", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the column to order by.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the `Column` to order by.", IsMandatory = true, IsImportant = true,
            Description = "See also `Schema` and `Table` as these all relate.")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the order by table schema.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of order by table schema. See also `Name` and `Column` as these all relate.",
            Description = "Defaults to `Query.Schema`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the order by table.
        /// </summary>
        [JsonProperty("table", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the order by table.",
            Description = "Defaults to `Table.Name`; i.e. primary table. See also `Schema` and `Column` as these all relate.")]
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the sort order option.
        /// </summary>
        [JsonProperty("order", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The corresponding sort order.", IsImportant = true, Options = new string[] { "Ascending", "Descending" },
            Description = "Defaults to `Ascending`.")]
        public string? Order { get; set; }

        #endregion

        /// <summary>
        /// Gets the order by SQL.
        /// </summary>
        public string? OrderBySql { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            Table = DefaultWhereNull(Table, () => Parent!.Name);
            Order = DefaultWhereNull(Order, () => "Ascending");

            var c = Root!.DbTables.Where(x => x.Schema == Schema && x.Name == Table).SingleOrDefault()?.Columns.Where(x => x.Name == Name).SingleOrDefault();
            if (c == null)
                throw new CodeGenException(this, nameof(Name), $"OrderBy '{Name}' (Schema.Table '{Schema}.{Table}') not found in database.");

            if (Schema == Parent!.Schema && Table == Parent!.Name)
            {
                if (Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault() == null)
                    throw new CodeGenException(this, nameof(Name), $"OrderBy '{Name}' (Schema.Table '{Schema}.{Table}') not found in Table/Join configuration.");

                OrderBySql = $"[{Parent!.Alias}].[{Name}]";
            }
            else
            {
                var t = Parent!.Joins!.Where(x => Schema == x.Schema && Table == x.Name).SingleOrDefault();
                if (t != null && t.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault() != null)
                    OrderBySql = $"[{t.Alias}].[{Name}]";
                else
                    throw new CodeGenException(this, nameof(Name), $"OrderBy '{Name}' (Schema.Table '{Schema}.{Table}') not found in Table/Join configuration.");
            }

            OrderBySql += $" {(Order!.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";
        }
    }
}