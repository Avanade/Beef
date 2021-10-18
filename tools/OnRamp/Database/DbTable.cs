// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnRamp.Database
{
    /// <summary>
    /// Represents the Database <b>Table</b> schema definition.
    /// </summary>
    public class DbTable
    {
        private string? _name;

        /// <summary>
        /// Loads the table and column schema details from the database.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="exlcudeSqlServerSpecific">Indicates whether to skip the <i>Microsoft SQL Server</i> specific metadata queries.</param>
        /// <param name="additional">Provides an opportunity to perform additional configuration manipulation during the load.</param>
        public static async Task<List<DbTable>> LoadTablesAndColumnsAsync(DbConnection dbConnection, bool exlcudeSqlServerSpecific = false, Action<DbConnection, List<DbTable>>? additional = null)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            bool autoCloseConnection = dbConnection.State != ConnectionState.Open;
            if (autoCloseConnection)
                await dbConnection.OpenAsync().ConfigureAwait(false);

            var tables = new List<DbTable>();
            DbTable? table = null;

            // Get all the tables and their columns.
            using var sr = StreamLocator.GetResourcesStreamReader("SelectTableAndColumns.sql");
            await ExecuteSqlStatementAsync(dbConnection, sr!, (dr) =>
            {
                var dt = new DbTable { Name = dr.GetValue<string>("TABLE_NAME"), Schema = dr.GetValue<string>("TABLE_SCHEMA"), IsAView = dr.GetValue<string>("TABLE_TYPE") == "VIEW" };
                if (table == null || table.Schema != dt.Schema || table.Name != dt.Name)
                    tables.Add(table = dt);

                var dc = new DbColumn
                {
                    Name = dr.GetValue<string>("COLUMN_NAME"),
                    Type = dr.GetValue<string>("DATA_TYPE"),
                    IsNullable = dr.GetValue<string>("IS_NULLABLE").ToUpperInvariant() == "YES",
                    Length = dr.GetValue<int?>("CHARACTER_MAXIMUM_LENGTH"),
                    Precision = dr.GetValue<int?>("NUMERIC_PRECISION") ?? dr.GetValue<int?>("DATETIME_PRECISION"),
                    Scale = dr.GetValue<int?>("NUMERIC_SCALE"),
                    DefaultValue = dr.GetValue<string>("COLUMN_DEFAULT")
                };

                table.Columns.Add(dc);
            }).ConfigureAwait(false);

            // Determine whether a table is considered reference data; has columns: Code, Text and SortOrder
            foreach (var t in tables.Where(x => x.Columns.Any(c => c.Name == "Code") && x.Columns.Any(c => c.Name == "Text") && x.Columns.Any(c => c.Name == "SortOrder")))
            {
                t.IsRefData = true;
            }

            // Configure all the single column primary and unique constraints.
            using var sr2 = StreamLocator.GetResourcesStreamReader("SelectTablePrimaryKey.sql");
            foreach (var pks in ExecuteSqlStatementAsync(dbConnection, sr2!, (dr) =>
            {
                return new
                {
                    ConstraintName = dr.GetValue<string>("CONSTRAINT_NAME"),
                    TableSchema = dr.GetValue<string>("TABLE_SCHEMA"),
                    TableName = dr.GetValue<string>("TABLE_NAME"),
                    TableColumnName = dr.GetValue<string>("COLUMN_NAME"),
                    IsPrimaryKey = dr.GetValue<string>("CONSTRAINT_TYPE").StartsWith("PRIMARY", StringComparison.InvariantCultureIgnoreCase),
                };
            }).ConfigureAwait(false).GetAwaiter().GetResult().GroupBy(x => x.ConstraintName))
            {
                // Only single column unique columns are supported.
                if (pks.Count() > 1 && !pks.First().IsPrimaryKey)
                    continue;

                // Set the column flags as appropriate.
                foreach (var pk in pks)
                {
                    var col = (from t in tables
                                from c in t.Columns
                                where t.Schema == pk.TableSchema && t.Name == pk.TableName && c.Name == pk.TableColumnName
                                select c).Single();

                    if (pk.IsPrimaryKey)
                    {
                        col.IsPrimaryKey = true;
                        col.IsIdentity = col.DefaultValue != null;
                    }
                    else
                        col.IsUnique = true;
                }
            }

            if (!exlcudeSqlServerSpecific)
            {
                // Configure all the single column foreign keys.
                using var sr3 = StreamLocator.GetResourcesStreamReader("SelectTableForeignKeys.sql");
                foreach (var fks in ExecuteSqlStatementAsync(dbConnection, sr3!, (dr) =>
                {
                    return new
                    {
                        ConstraintName = dr.GetValue<string>("FK_CONSTRAINT_NAME"),
                        TableSchema = dr.GetValue<string>("FK_SCHEMA_NAME"),
                        TableName = dr.GetValue<string>("FK_TABLE_NAME"),
                        TableColumnName = dr.GetValue<string>("FK_COLUMN_NAME"),
                        ForeignSchema = dr.GetValue<string>("UQ_SCHEMA_NAME"),
                        ForeignTable = dr.GetValue<string>("UQ_TABLE_NAME"),
                        ForiegnColumn = dr.GetValue<string>("UQ_COLUMN_NAME")
                    };
                }).ConfigureAwait(false).GetAwaiter().GetResult().GroupBy(x => x.ConstraintName).Where(x => x.Count() == 1))
                {
                    var fk = fks.Single();
                    var r = (from t in tables
                               from c in t.Columns
                               where t.Schema == fk.TableSchema && t.Name == fk.TableName && c.Name == fk.TableColumnName
                               select (t, c)).Single();

                    r.c.ForeignSchema = fk.ForeignSchema;
                    r.c.ForeignTable = fk.ForeignTable;
                    r.c.ForeignColumn = fk.ForiegnColumn;
                    r.c.IsForeignRefData = r.t.IsRefData;
                }

                using var sr4 = StreamLocator.GetResourcesStreamReader("SelectTableIdentityColumns.sql");
                await ExecuteSqlStatementAsync(dbConnection, sr4!, (dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    c.IsIdentity = true;
                    c.IdentitySeed = 1;
                    c.IdentityIncrement = 1;
                }).ConfigureAwait(false);

                using var sr5 = StreamLocator.GetResourcesStreamReader("SelectTableAlwaysGeneratedColumns.sql");
                await ExecuteSqlStatementAsync(dbConnection, sr5!, (dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    t.Columns.Remove(c);
                }).ConfigureAwait(false);

                using var sr6 = StreamLocator.GetResourcesStreamReader("SelectTableGeneratedColumns.sql");
                await ExecuteSqlStatementAsync(dbConnection, sr6!, (dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    c.IsComputed = true;
                }).ConfigureAwait(false);
            }

            // Perform any optional additional configuration activities.
            additional?.Invoke(dbConnection, tables);

            // Perform final preparation and auto-determine reference data relationships even where no foreign key defined.
            foreach (var t in tables)
            {
                foreach (var c in t.Columns)
                {
                    c.DbTable = t;

                    if (c.IsPrimaryKey)
                        t.PrimaryKeyColumns.Add(c);

                    if (c.ForeignTable == null)
                    {
                        if (c.Name!.Length > 2 && c.Name!.EndsWith("Id", StringComparison.InvariantCulture))
                        {
                            var rt = tables.Where(x => x.QualifiedName != t.QualifiedName && x.Name == c.Name![0..^2]).FirstOrDefault();
                            if (rt != null)
                            {
                                c.ForeignSchema = rt.Schema;
                                c.ForeignTable = rt.Name;
                                c.ForeignColumn = rt.Columns.Where(x => x.IsPrimaryKey).First().Name;
                                c.IsForeignRefData = rt.IsRefData;
                            }
                        }
                        else if (c.Name!.Length > 4 && c.Name!.EndsWith("Code", StringComparison.InvariantCulture))
                        {
                            var rt = tables.Where(x => x.QualifiedName != t.QualifiedName && x.Name == c.Name![0..^4]).FirstOrDefault();
                            if (rt != null && rt.IsRefData)
                            {
                                c.ForeignSchema = rt.Schema;
                                c.ForeignTable = rt.Name;
                                c.ForeignColumn = rt.Columns.Where(x => x.Name == "Code").First().Name;
                                c.IsForeignRefData = rt.IsRefData;
                            }
                        }
                    }
                }
            }

            if (autoCloseConnection)
                await dbConnection.CloseAsync().ConfigureAwait(false);

            return tables;
        }

        /// <summary>
        /// Executes the SQL statement asynchronously.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="statement">The SQL statement <see cref="StreamReader"/> to execute.</param>
        /// <param name="action">The <see cref="DbDataReader"/> action.</param>
        public static async Task ExecuteSqlStatementAsync(DbConnection dbConnection, StreamReader statement, Action<DbDataReader> action)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            bool autoCloseConnection = dbConnection.State != ConnectionState.Open;
            if (autoCloseConnection)
                await dbConnection.OpenAsync().ConfigureAwait(false);

            var cmd = dbConnection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = await statement.ReadToEndAsync().ConfigureAwait(false);

            using var dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await dr.ReadAsync().ConfigureAwait(false))
            {
                action(dr);
            }

            if (autoCloseConnection)
                await dbConnection.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the SQL statement asynchronously returning a resultant collection.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="IEnumerable{T}"/> <see cref="Type"/>.</typeparam>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="statement">The SQL statement <see cref="StreamReader"/> to execute.</param>
        /// <param name="func">The <see cref="DbDataReader"/> function that returns a value.</param>
        /// <returns>The resultant collection.</returns>
        public static async Task<IEnumerable<T>> ExecuteSqlStatementAsync<T>(DbConnection dbConnection, StreamReader statement, Func<DbDataReader, T> func)
        {
            var list = new List<T>();
            await ExecuteSqlStatementAsync(dbConnection, statement, (dr) => list.Add(func(dr)));
            return list;
        }

        /// <summary>
        /// Create an alias from the name.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <returns>The alias.</returns>
        public static string CreateAlias(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return new string(StringConversion.ToSentenceCase(name.Replace(" ", "").Replace("_", "").Replace("-", ""))!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray());
        }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string? Name
        {
            get { return _name; }

            set
            {
                _name = value;
                if (!string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(Alias))
                    Alias = CreateAlias(_name);
            }
        }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the alias (automatically updated when the <see cref="Name"/> is set and the current alias value is <c>null</c>).
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => $"[{Schema}].[{Name}]";

        /// <summary>
        /// Indicates whether the Table is actually a View.
        /// </summary>
        public bool IsAView { get; set; }

        /// <summary>
        /// Indicates whether the Table is considered reference data.
        /// </summary>
        public bool IsRefData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbColumn"/> list.
        /// </summary>
        public List<DbColumn> Columns { get; private set; } = new List<DbColumn>();

        /// <summary>
        /// Gets the primary key <see cref="DbColumn"/> list.
        /// </summary>
        public List<DbColumn> PrimaryKeyColumns { get; private set; } = new List<DbColumn>();
    }
}