// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the column configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Parameter", Title = "The **Where** statement is used to define additional filtering.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class ColumnConfig : ConfigBase<CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="Column"/> configuration.
        /// </summary>
        public Column? DbColumn { get; set; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        public string QualifiedName => $"[{Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => "@" + Name;

        /// <summary>
        /// Gets the parameter SQL definition.
        /// </summary>
        public string? ParameterSql { get; private set; }

        /// <summary>
        /// Gets the where equality clause.
        /// </summary>
        public string WhereEquals => Name == Parent?.ColumnIsDeleted?.Name ? $"ISNULL({QualifiedName}, 0) = 0" : $"{QualifiedName} = {ParameterName}";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            ParameterSql = CreateParameterSql();
        }

        private string CreateParameterSql()
        {
            var sb = new StringBuilder($"{ParameterName} AS {DbColumn!.Type!.ToUpperInvariant()}");
            if (Column.TypeIsString(DbColumn!.Type))
                sb.Append(DbColumn!.Length.HasValue && DbColumn!.Length.Value > 0 ? $"({DbColumn!.Length.Value})" : "(MAX)");

            sb.Append(DbColumn!.Type.ToUpperInvariant() switch
            {
                "DECIMAL" => $"({DbColumn!.Precision}, {DbColumn!.Scale})",
                "NUMERIC" => $"({DbColumn!.Precision}, {DbColumn!.Scale})",
                "TIME" => DbColumn!.Scale.HasValue && DbColumn!.Scale.Value > 0 ? $"({DbColumn!.Scale})" : string.Empty,
                _ => string.Empty
            });

            if (DbColumn!.IsNullable)
                sb.Append(" NULL");

            return sb.ToString();
        }
    }
}