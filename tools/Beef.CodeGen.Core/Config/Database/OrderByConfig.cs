// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure order-by configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("OrderBy", Title = "The **OrderBy** is used to define the query order", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class OrderByConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the column to order by.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the column to order by.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the corresponding column name; used to infer characteristics.
        /// </summary>
        [JsonProperty("order", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The corresponding column name; used to infer characteristics.", IsImportant = true, Options = new string[] { "Ascending", "Descending" },
            Description = "Defaults to `Ascending`.")]
        public string? Order { get; set; }

        #endregion

        /// <summary>
        /// Gets the order by SQL.
        /// </summary>
        public string OrderBySql => $"[{Parent!.Parent!.Alias}].[{Name}] {(Order!.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name.Substring(1);

            Order = DefaultWhereNull(Order, () => "Ascending");
        }
    }
}