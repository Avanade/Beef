// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the column value.
        /// </summary>
        public object Value { get; set; }

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

            if (Value is string)
                return $"'{((string)Value).Replace("'", "''")}'";
            else if (Value is bool)
                return ((bool)Value) ? "1" : "0";
            else if (Value is Guid)
                return $"'{Value}'";
            else if (Value is DateTime)
                return $"'{((DateTime)Value).ToString(SqlDataUpdater.DateTimeFormat)}'";
            else
                return Value.ToString();
        }
    }
}