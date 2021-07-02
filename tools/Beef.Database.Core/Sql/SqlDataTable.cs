// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using Beef.Data.Database;
using System;
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
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name.StartsWith('$'))
            {
                IsMerge = true;
                name = name[1..];
            }

            if (name.StartsWith('^'))
            {
                UseIdentifierGenerator = true;
                name = name[1..];
            }

            DbTable = SqlDataUpdater.DbTables.Where(t => t.Schema == schema && t.Name == name).SingleOrDefault();
            if (DbTable == null)
                throw new SqlDataUpdaterException($"Table '{schema}.{name}' does not exist within the specified database.");

            Schema = schema;
            Name = name;

            // Will attempt to infer by checking for specified columns.
            IsRefData = DbTable.Columns.Any(x => x.Name == "Code") && DbTable.Columns.Any(x => x.Name == "Text") && DbTable.Columns.Any(x => x.Name == "IsActive") && DbTable.Columns.Any(x => x.Name == "SortOrder");

            // Check that an identifier generator can be used.
            if (UseIdentifierGenerator)
            {
                if (DbTable.PrimaryKeyColumns.Count > 0 && Enum.TryParse<SqlDataTableIdentifierType>(DbTable.PrimaryKeyColumns[0].DotNetType, true, out var igType))
                    IdentifierType = igType;
                else
                    throw new SqlDataUpdaterException($"Table '{schema}.{name}' specifies usage of IIdentifierGenerator; either there is more than one column representing the primary key or the underlying type is not supported.");
            }
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
        /// Gets or sets the database <see cref="CodeGen.DbModels.DbTable"/>.
        /// </summary>
        public DbTable DbTable { get; private set; }

        /// <summary>
        /// Indicates whether the table is reference data.
        /// </summary>
        public bool IsRefData { get; private set; }

        /// <summary>
        /// Indicates whether the table data is to be merged.
        /// </summary>
        public bool IsMerge { get; private set; }

        /// <summary>
        /// Indicates whether to use the identifier generator for the primary key (single column) on create (where not specified).
        /// </summary>
        public bool UseIdentifierGenerator { get; private set; }

        /// <summary>
        /// Gets the identifier generator (see <see cref="UseIdentifierGenerator"/>) <see cref="Type"/>.
        /// </summary>
        public SqlDataTableIdentifierType? IdentifierType { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public List<DbColumn> Columns { get; } = new List<DbColumn>();

        /// <summary>
        /// Gets the merge match columns.
        /// </summary>
        public List<DbColumn> MergeMatchColumns => Columns.Where(x => !x.IsAudit && !(UseIdentifierGenerator && x.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the merge update columns.
        /// </summary>
        public List<DbColumn> MergeUpdateColumns => Columns.Where(x => !(UseIdentifierGenerator && x.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the primary key columns.
        /// </summary>
        public List<DbColumn> PrimaryKeyColumns => Columns.Where(x => x.IsPrimaryKey).ToList();

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
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            foreach (var c in row.Columns)
            {
                AddColumn(c.Name!);
            }

            Rows.Add(row);
        }

        /// <summary>
        /// Add to specified columns.
        /// </summary>
        private void AddColumn(string name)
        {
            var column = DbTable.Columns.Where(x => x.Name == name).SingleOrDefault();
            if (column == null)
                return;

            if (!Columns.Any(x => x.Name == name))
                Columns.Add(column);
        }

        /// <summary>
        /// Prepares the data.
        /// </summary>
        public void Prepare(IIdentifierGenerators? ig)
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
                AddColumnWhereNotSpecified(row, hasCreatedBy, DatabaseColumns.CreatedByName, ExecutionContext.EnvironmentUsername);
                AddColumnWhereNotSpecified(row, hasUpdatedDate, DatabaseColumns.UpdatedDateName, SqlDataUpdater.DateTimeNow);
                AddColumnWhereNotSpecified(row, hasUpdatedBy, DatabaseColumns.UpdatedByName, ExecutionContext.EnvironmentUsername);
                AddColumnWhereNotSpecified(row, hasIsActive, DatabaseRefDataColumns.IsActiveColumnName, true);
                AddColumnWhereNotSpecified(row, hasSortOrder, DatabaseRefDataColumns.SortOrderColumnName, i + 1);

                if (ig != null && UseIdentifierGenerator)
                {
                    var pkc = DbTable.PrimaryKeyColumns[0];
                    var val = row.Columns.SingleOrDefault(x => x.Name == pkc.Name!);
                    if (val == null)
                    {
                        switch (IdentifierType)
                        {
                            case SqlDataTableIdentifierType.Guid:
                                if (ig!.GuidGenerator != null)
                                    AddColumnWhereNotSpecified(row, true, pkc.Name!, ig.GuidGenerator.GenerateIdentifierAsync().GetAwaiter().GetResult());

                                break;

                            case SqlDataTableIdentifierType.String:
                                if (ig!.StringGenerator != null)
                                    AddColumnWhereNotSpecified(row, true, pkc.Name!, ig.StringGenerator.GenerateIdentifierAsync().GetAwaiter().GetResult());

                                break;

                            case SqlDataTableIdentifierType.Int:
                                if (ig!.IntGenerator != null)
                                    AddColumnWhereNotSpecified(row, true, pkc.Name!, ig.IntGenerator.GenerateIdentifierAsync().GetAwaiter().GetResult());

                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the column where not already specified.
        /// </summary>
        private void AddColumnWhereNotSpecified(SqlDataRow row, bool when, string name, object value)
        {
            if (when && !row.Columns.Any(x => x.Name == name))
            {
                AddColumn(name);
                row.AddColumn(name, value);
            }
        }
    }

    /// <summary>
    /// Defines the identifier generator <see cref="Type"/>.
    /// </summary>
    public enum SqlDataTableIdentifierType
    {
        /// <summary>
        /// Represents an invalid <see cref="Type"/>.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Represents a <see cref="string"/> <see cref="Type"/>.
        /// </summary>
        String = 1,

        /// <summary>
        /// Represents a <see cref="System.Guid"/> <see cref="Type"/>.
        /// </summary>
        Guid = 2,

        /// <summary>
        /// Represents an <see cref="int"/> <see cref="Type"/>.
        /// </summary>
        Int = 3
    }
}