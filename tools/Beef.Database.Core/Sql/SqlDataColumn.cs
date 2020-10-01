// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using System;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Represents the SQL data column.
    /// </summary>
    public class SqlDataColumn
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the column value.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="CodeGen.DbModels.DbColumn"/> configuration.
        /// </summary>
        public DbColumn? DbColumn { get; set; }

        /// <summary>
        /// Gets the SQL formatted value.
        /// </summary>
        public string SqlValue => ToSqlValue();

        /// <summary>
        /// Indicates whether to use a foreign key query for the identifier.
        /// </summary>
        public bool UseForeignKeyQueryForId { get; set; }

        /// <summary>
        /// Gets the value formatted for use in a SQL statement.
        /// </summary>
        /// <returns>The value formatted for use in a SQL statement.</returns>
        public string ToSqlValue()
        {
            if (Value == null)
                return "NULL";
            else if (Value is string str)
                return $"'{str.Replace("'", "''", StringComparison.Ordinal)}'";
            else if (Value is bool b)
                return b ? "1" : "0";
            else if (Value is Guid)
                return $"'{Value}'";
            else if (Value is DateTime time)
                return $"'{time.ToString(SqlDataUpdater.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture)}'";
            else
                return Value.ToString()!;
        }
    }
}