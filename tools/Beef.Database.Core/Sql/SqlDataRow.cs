// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Represents the SQL data row.
    /// </summary>
    public class SqlDataRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataRow"/> class.
        /// </summary>
        /// <param name="table">The parent <see cref="SqlDataTable"/>.</param>
        public SqlDataRow(SqlDataTable table)
        {
            Table = table;
        }

        /// <summary>
        /// Gets the <see cref="SqlDataTable"/>.
        /// </summary>
        public SqlDataTable Table { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public Dictionary<string, SqlDataColumn> Columns { get; } = new Dictionary<string, SqlDataColumn>();

        /// <summary>
        /// Adds a <see cref="SqlDataColumn"/> to the row using the specified name and value.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <param name="value">The column value.</param>
        public void AddColumn(string name, object value)
        {
            AddColumn(new SqlDataColumn { Name = name, Value = value });
        }

        /// <summary>
        /// Adds a <see cref="SqlDataColumn"/> to the row.
        /// </summary>
        /// <param name="column">The <see cref="SqlDataColumn"/>.</param>
        public void AddColumn(SqlDataColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var col = Table.DbTable.Columns.Where(c => c.Name == column.Name).SingleOrDefault();
            if (col == null)
            {
                // Check and see if it is a reference data id.
                col = Table.DbTable.Columns.Where(c => c.Name == column.Name + "Id").SingleOrDefault();
                if (col == null || !col.IsForeignRefData)
                    throw new SqlDataUpdaterException($"Table '{Table.Schema}.{Table.Name}' does not have a column named '{column.Name}'.");

                column.Name += "Id";
            }

            if (!Columns.TryAdd(column.Name, column))
                throw new SqlDataUpdaterException($"Table '{Table.Schema}.{Table.Name}' column '{column.Name}' has been specified more than once.");

            if (column.Value == null)
                return;

            string str = null;
            try
            {
                str = column.Value is DateTime ? ((DateTime)column.Value).ToString(SqlDataUpdater.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture) : column.Value.ToString();
                switch (col.DotNetType)
                {
                    case "string": column.Value = str; break;
                    case "decimal": column.Value = decimal.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "DateTime": column.Value = DateTime.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "bool": column.Value = bool.Parse(str); break;
                    case "DateTimeOffset": column.Value = DateTimeOffset.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "double": column.Value = double.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "int": column.Value = int.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "long": column.Value = long.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "short": column.Value = short.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "byte": column.Value = byte.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "float": column.Value = float.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "TimeSpan": column.Value = TimeSpan.Parse(str, System.Globalization.CultureInfo.InvariantCulture); break;

                    case "Guid":
                        if (int.TryParse(str, out int a))
                            column.Value = new Guid(a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        else
                        {
                            if (Guid.TryParse(str, out Guid g))
                                column.Value = g;
                            else if (col.IsForeignRefData)
                            {
                                column.Value = str;
                                column.UseForeignKeyQueryForId = true;
                            }
                        }

                        break;

                    default:
                        throw new SqlDataUpdaterException($"Table '{Table.Schema}.{Table.Name}' column '{column.Name}' type '{col.Type}' is not supported.");
                }
            }
            catch (FormatException fex)
            {
                if (col.IsForeignRefData)
                {
                    column.Value = str;
                    column.UseForeignKeyQueryForId = true;
                }
                else
                    throw new SqlDataUpdaterException($"'{Table.Schema}.{Table.Name}' column '{column.Name}' type '{col.Type}' cannot parse value '{column.Value.ToString()}': {fex.Message}");
            }
        }
    }
}
