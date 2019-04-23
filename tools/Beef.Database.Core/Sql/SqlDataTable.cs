// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Beef.Data.Database;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Represents a SQL data table.
    /// </summary>
    public class SqlDataTable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataTable"/> class.
        /// </summary>
        /// <param name="schema">The schema name.</param>
        /// <param name="name">The table name.</param>
        public SqlDataTable(string schema, string name)
        {
            if (name.StartsWith('$'))
            {
                IsMerge = true;
                name = name.Substring(1);
            }

            DbTable = SqlDataUpdater.DbTables.Where(t => t.Schema == schema && t.Name == name).SingleOrDefault();
            if (DbTable == null)
                throw new SqlDataUpdaterException($"Table '{schema}.{name}' does not exist within the specified database.");

            Schema = schema;
            Name = name;
            IsRefData = Schema == SqlDataUpdater.RefDataSchema;
        }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string Schema { get; private set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the database <see cref="Table"/>.
        /// </summary>
        public Table DbTable { get; private set; }

        /// <summary>
        /// Indicates whether the table is reference data.
        /// </summary>
        public bool IsRefData { get; private set; }

        /// <summary>
        /// Indicates whether the table data is to be merged.
        /// </summary>
        public bool IsMerge { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public Dictionary<string, Column> Columns { get; } = new Dictionary<string, Column>();

        /// <summary>
        /// Gets the rows.
        /// </summary>
        public List<SqlDataRow> Rows { get; } = new List<SqlDataRow>();

        /// <summary>
        /// Adds a row (key value pairs of column name and corresponding value).
        /// </summary>
        /// <param name="row">The row.</param>
        public void AddRow(SqlDataRow row)
        {
            foreach (var c in row.Columns.Where(x => x.Value != null))
            {
                AddColumn(c.Value.Name);
            }

            Rows.Add(row);
        }

        /// <summary>
        /// Add to specified columns.
        /// </summary>
        private void AddColumn(string name)
        {
            if (!Columns.ContainsKey(name))
                Columns.Add(name, DbTable.Columns.Where(x => x.Name == name).SingleOrDefault());
        }

        /// <summary>
        /// Prepares the data.
        /// </summary>
        public void Prepare()
        {
            var hasCreatedDate = DbTable.Columns.Any(x => x.Name == DatabaseColumns.CreatedDateName);
            var hasCreatedBy = DbTable.Columns.Any(x => x.Name == DatabaseColumns.CreatedByName);
            var hasUpdatedDate = DbTable.Columns.Any(x => x.Name == DatabaseColumns.UpdatedDateName);
            var hasUpdatedBy = DbTable.Columns.Any(x => x.Name == DatabaseColumns.UpdatedByName);
            var hasIsActive = IsRefData && DbTable.Columns.Any(x => x.Name == DatabaseRefDataColumns.IsActiveColumnName);
            var hasSortOrder = IsRefData && DbTable.Columns.Any(x => x.Name == DatabaseRefDataColumns.SortOrderColumnName);

            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                AddColumnWhereNotSpecified(row, hasCreatedDate, DatabaseColumns.CreatedDateName, SqlDataUpdater.DateTimeNow);
                AddColumnWhereNotSpecified(row, hasCreatedBy, DatabaseColumns.CreatedByName, SqlDataUpdater.EnvironmentUsername);
                AddColumnWhereNotSpecified(row, hasUpdatedDate, DatabaseColumns.UpdatedDateName, SqlDataUpdater.DateTimeNow);
                AddColumnWhereNotSpecified(row, hasUpdatedBy, DatabaseColumns.UpdatedByName, SqlDataUpdater.EnvironmentUsername);
                AddColumnWhereNotSpecified(row, hasIsActive, DatabaseRefDataColumns.IsActiveColumnName, true);
                AddColumnWhereNotSpecified(row, hasSortOrder, DatabaseRefDataColumns.SortOrderColumnName, i + 1);
            }
        }

        /// <summary>
        /// Adds the column where not already specified.
        /// </summary>
        private void AddColumnWhereNotSpecified(SqlDataRow row, bool when, string name, object value)
        {
            if (when && !row.Columns.ContainsKey(name))
            {
                AddColumn(name);
                row.AddColumn(name, value);
            }
        }
    }
}
