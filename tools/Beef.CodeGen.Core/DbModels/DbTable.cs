// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using Beef.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beef.CodeGen.DbModels
{
    /// <summary>
    /// Represents the SQL Server Database <b>Table</b> schema definition.
    /// </summary>
    public class DbTable
    {
        private string? _name;

        /// <summary>
        /// Loads the table and column schema details from the database.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseBase"/>.</param>
        /// <param name="skipSqlSpecific">Indicates whether to skip the Microsoft SQL Server specific metadata queries.</param>
        public static async Task<List<DbTable>> LoadTablesAndColumnsAsync(DatabaseBase db, bool skipSqlSpecific = false)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            var tables = new List<DbTable>();
            DbTable? table = null;

            // Get all the tables and their columns.
            await db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableAndColumns.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
            {
                var ct = DbTableMapper.Default.MapFromDb(dr, Mapper.OperationTypes.Get)!;
                if (table == null || table.Schema != ct.Schema || table.Name != ct.Name)
                    tables.Add(table = ct);

                table.Columns.Add(DbColumnMapper.Default.MapFromDb(dr, Mapper.OperationTypes.Get)!);
            }).ConfigureAwait(false);

            // Determine whether a table is considered reference data; has columns: Code, Text and SortOrder
            foreach (var t in tables.Where(x => x.Columns.Any(c => c.Name == "Code") && x.Columns.Any(c => c.Name == "Text") && x.Columns.Any(c => c.Name == "SortOrder")))
            {
                t.IsRefData = true;
            }

            // Configure all the single column primary and unique constraints.
            foreach (var pks in db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTablePrimaryKey.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
            {
                return new
                {
                    ConstraintName = dr.GetValue<string>("CONSTRAINT_NAME"),
                    TableSchema = dr.GetValue<string>("TABLE_SCHEMA"),
                    TableName = dr.GetValue<string>("TABLE_NAME"),
                    TableColumnName = dr.GetValue<string>("COLUMN_NAME"),
                    IsPrimaryKey = dr.GetValue<string>("CONSTRAINT_TYPE").StartsWith("PRIMARY", StringComparison.InvariantCultureIgnoreCase),
                };
            }).GetAwaiter().GetResult().GroupBy(x => x.ConstraintName))
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

            if (!skipSqlSpecific)
            {
                // Configure all the single column foreign keys.
                foreach (var fks in db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableForeignKeys.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
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

                await db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableIdentityColumns.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    c.IsIdentity = true;
                    c.IdentitySeed = 1;
                    c.IdentityIncrement = 1;
                }).ConfigureAwait(false);

                await db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableAlwaysGeneratedColumns.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    t.Columns.Remove(c);
                }).ConfigureAwait(false);

                await db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableGeneratedColumns.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
                {
                    var t = tables.Single(x => x.Schema == dr.GetValue<string>("TABLE_SCHEMA") && x.Name == dr.GetValue<string>("TABLE_NAME"));
                    var c = t.Columns.Single(x => x.Name == dr.GetValue<string>("COLUMN_NAME"));
                    c.IsComputed = true;
                }).ConfigureAwait(false);
            }

            // Perform final preparation and auto-determine reference data relationships even where no foreign key defined.
            foreach (var t in tables)
            {
                foreach (var c in t.Columns)
                {
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

            return tables;
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

#pragma warning disable CA1308 // Normalize strings to uppercase; by-design, a lowercase is required.
            return new string(StringConversion.ToSentenceCase(name)!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray());
#pragma warning restore CA1308 
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
    }

    /// <summary>
    /// Represents the <see cref="DbTable"/> database mapper.
    /// </summary>
#pragma warning disable CA1812 // Apparently never instantiated; by-design - it is!
    internal class DbTableMapper : DatabaseMapper<DbTable, DbTableMapper>
#pragma warning restore CA1812
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbTableMapper"/> class.
        /// </summary>
        public DbTableMapper()
        {
            Property(x => x.Name, "TABLE_NAME");
            Property(x => x.Schema, "TABLE_SCHEMA");
            Property(x => x.IsAView).MapFromDb((dr, t, ot) => dr.GetValue<string>("TABLE_TYPE") == "VIEW");
        }
    }
}
