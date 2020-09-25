// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        #region Key

        /// <summary>
        /// Gets or sets the parameter name (without the `@` prefix).
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The parameter name (without the `@` prefix).", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the corresponding column name; used to infer characteristics.
        /// </summary>
        [JsonProperty("column", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The corresponding column name; used to infer characteristics.",
            Description = "Defaults to `Name`.")]
        public string? Column { get; set; }

        /// <summary>
        /// Gets or sets the SQL type definition (overrides inherited Column definition) including length/precision/scale.
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

        #endregion

        /// <summary>
        /// Indicates whether the parameter is to be used for the where clause only.
        /// </summary>
        public bool WhereOnly { get; set; }

        /// <summary>
        /// Gets or sets the where SQL clause.
        /// </summary>
        public string? WhereSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL operator.
        /// </summary>
        public string? SqlOperator { get; private set; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => "@" + Name;

        /// <summary>
        /// Gets the parameter SQL definition.
        /// </summary>
        public string? ParameterSql { get; private set; }

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
            SqlType = DefaultWhereNull(SqlType, () =>
            {
                var c = Parent!.Parent!.Columns.Where(x => x.Name == Column).SingleOrDefault()?.DbColumn;
                if (c == null)
                    throw new CodeGenException($"Parameter '{Name}' specified Column '{Column}' (Schema.Table '{Parent!.Parent!.Schema}.{Parent!.Parent!.Name}') not found in database.");

                var sb = new StringBuilder($"{c.Type!.ToUpperInvariant()}");
                if (Entities.Column.TypeIsString(c.Type))
                    sb.Append(c.Length.HasValue && c.Length.Value > 0 ? $"({c.Length.Value})" : "(MAX)");

                sb.Append(c.Type.ToUpperInvariant() switch
                {
                    "DECIMAL" => $"({c.Precision}, {c.Scale})",
                    "NUMERIC" => $"({c.Precision}, {c.Scale})",
                    "TIME" => c.Scale.HasValue && c.Scale.Value > 0 ? $"({c.Scale})" : string.Empty,
                    _ => string.Empty
                });

                return sb.ToString();
            });

            SqlOperator = Operator!.ToUpperInvariant() switch
            {
                "EQ" => "=",
                "NE" => "!=",
                "LT" => "<",
                "LE" => "<=",
                "GT" => ">",
                "GE" => ">=",
                "LIKE" => "LIKE",
                _ => Operator
            };

            ParameterSql = $"{ParameterName} AS {SqlType}{(Nullable.HasValue && Nullable.Value ? " NULL" : "")}";
            WhereSql = DefaultWhereNull(WhereSql, () => $"[{Parent!.Parent!.Alias}].[{Column}] {SqlOperator} @{Column}");
        }
    }
}