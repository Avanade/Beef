// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure parameter configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Parameter", Title = "The **Parameter** is used to define a Stored Procedure's Parameter and its charateristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class ParameterConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// Gets or sets the parameter name (without the `@`).
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The parameter name (without the `@`).", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the corresponding column name; used to infer characteristics.
        /// </summary>
        [JsonProperty("column", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The corresponding column name; used to infer characteristics.",
            Description = "Defaults to `Name`.")]
        public string? Column { get; set; }

        /// <summary>
        /// Gets or sets the SQL type definition (overrides inerhited Column definition) including length/precision/scale.
        /// </summary>
        [JsonProperty("sqlType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The SQL type definition (overrides inerhited Column definition) including length/precision/scale.")]
        public string? SqlType { get; set; }

        /// <summary>
        /// Indicates whether the parameter is nullable.
        /// </summary>
        [JsonProperty("nullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the parameter is nullable.",
            Description = "Note that When the parameter value is `NULL` it will not be included in the query.")]
        public bool? Nullable { get; set; }

        /// <summary>
        /// Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.
        /// </summary>
        [JsonProperty("treatColumnNullAs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.")]
        public bool? TreatColumnNullAs { get; set; }

        /// <summary>
        /// Indicates whether the parameter is a collection (one or more values to be included `IN` the query).
        /// </summary>
        [JsonProperty("collection", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the parameter is a collection (one or more values to be included `IN` the query).")]
        public bool? Collection { get; set; }

        /// <summary>
        /// Gets or sets the where clause equality operator.
        /// </summary>
        [JsonProperty("operator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The where clause equality operator", IsImportant = true, Options = new string[] { "EQ", "NE", "LT", "LE", "GT", "GE", "LIKE" },
            Description = "Defaults to `EQ`.")]
        public string? Operator { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name.Substring(1);

            Column = DefaultWhereNull(Column, () => Name);
            Operator = DefaultWhereNull(Operator, () => "EQ");
        }
    }
}