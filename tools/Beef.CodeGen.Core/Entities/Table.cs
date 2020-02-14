// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using Beef.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beef.CodeGen.Entities
{
    /// <summary>
    /// Represents the SQL Server Database <b>Table</b> schema definition.
    /// </summary>
    public class Table
    {
        private string? _name;

        /// <summary>
        /// Loads the table and column schema details from the database.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseBase"/>.</param>
        /// <param name="refDataSchema">The reference data schema.</param>
        /// <param name="autoSecurity">Indicates whether the UserRole security should be automatically applied.</param>
        /// <param name="skipSqlSpecific">Indicates whether to skip the Microsoft SQL Server specific metadata queries.</param>
        public static async Task<List<Table>> LoadTablesAndColumnsAsync(DatabaseBase db, string? refDataSchema = null, bool autoSecurity = false, bool skipSqlSpecific = false)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            var tables = new List<Table>();
            Table? table = null;

            await db.SqlStatement((await ResourceManager.GetResourceContentAsync("SelectTableAndColumns.sql").ConfigureAwait(false))!).SelectQueryAsync((dr) =>
            {
                var ct = TableMapper.Default.MapFromDb(dr, Mapper.OperationTypes.Get)!;
                if (table == null || table.Schema != ct.Schema || table.Name != ct.Name)
                    tables.Add(table = ct);

                table.Columns.Add(ColumnMapper.Default.MapFromDb(dr, Mapper.OperationTypes.Get)!);
                if (autoSecurity && table.Schema != refDataSchema)
                    table.UserRole = $"{table.Schema}.{table.Name}";
            }).ConfigureAwait(false);

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
                    var col = (from t in tables
                                from c in t.Columns
                                where t.Schema == fk.TableSchema && t.Name == fk.TableName && c.Name == fk.TableColumnName
                                select c).Single();

                    col.ForeignSchema = fk.ForeignSchema;
                    col.ForeignTable = fk.ForeignTable;
                    col.ForeignColumn = fk.ForiegnColumn;
                    col.IsForeignRefData = col.ForeignSchema == refDataSchema;
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

            // Auto-determine reference data relationships even where no foreign key defined.
            foreach (var t in tables)
            {
                foreach (var col in t.Columns.Where(x => !x.IsForeignRefData && x.Name!.Length > 2 && x.Name.EndsWith("Id", StringComparison.InvariantCulture)))
                {
                    var rt = tables.Where(x => x.Name != t.Name && x.Name == col.Name![0..^2] && x.Schema == refDataSchema).SingleOrDefault();
                    if (rt != null)
                    {
                        col.ForeignSchema = rt.Schema;
                        col.ForeignTable = rt.Name;
                        col.ForeignColumn = rt.Columns.Where(x => x.IsPrimaryKey).First().Name;
                        col.IsForeignRefData = col.ForeignSchema == refDataSchema;
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
            return new string(Beef.CodeGen.CodeGenerator.ToSentenceCase(name).Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray());
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
        /// Indicates whether the Table is actually a View.
        /// </summary>
        public bool IsAView { get; set; }

        /// <summary>
        /// Gets or sets the alias (automatically updated when the <see cref="Name"/> is set and the current alias value is <c>null</c>).
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Indicates whether to create a corresponding View for the Table.
        /// </summary>
        public bool View { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Get</b> stored procedure.
        /// </summary>
        public bool Get { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>GetAll</b> stored procedure.
        /// </summary>
        public bool GetAll { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Create</b> stored procedure.
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Update</b> stored procedure.
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Upsert</b> stored procedure.
        /// </summary>
        public bool Upsert { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Delete</b> stored procedure.
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>Merge</b> stored procedure.
        /// </summary>
        public bool Merge { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>User-Defined Table Type</b>.
        /// </summary>
        public bool Udt { get; set; }

        /// <summary>
        /// Gets or sets the default order by for a <see cref="GetAll"/>.
        /// </summary>
        public string? GetAllOrderBy { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        public string? UserRole { get; set; }

        /// <summary>
        /// Indicates whether to create the <b>EfModel</b> class.
        /// </summary>
        public bool EfModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Column"/> list.
        /// </summary>
        public List<Column> Columns { get; private set; } = new List<Column>();

        /// <summary>
        /// Creates (and adds) the <see cref="Table"/> element for code generation.
        /// </summary>
        /// <param name="xml">The <see cref="XElement"/> to add to.</param>
        public void CreateXml(XElement xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            var xt = new XElement("Table",
                new XAttribute("Name", Name),
                new XAttribute("Schema", Schema),
                new XAttribute("Alias", Alias));

            if (View)
                xt.Add(new XAttribute("View", "true"));

            if (Get)
                xt.Add(new XAttribute("Get", "true"));

            if (GetAll)
                xt.Add(new XAttribute("GetAll", "true"));

            if (Create)
                xt.Add(new XAttribute("Create", "true"));

            if (Update)
                xt.Add(new XAttribute("Update", "true"));

            if (Upsert)
                xt.Add(new XAttribute("Upsert", "true"));

            if (Delete)
                xt.Add(new XAttribute("Delete", "true"));

            if (Merge)
                xt.Add(new XAttribute("Merge", "true"));

            if (Udt)
                xt.Add(new XAttribute("Udt", "true"));

            if (EfModel)
                xt.Add(new XAttribute("EfModel", "true"));

            if (!string.IsNullOrEmpty(GetAllOrderBy))
                xt.Add(new XAttribute("OrderBy", GetAllOrderBy));

            if (!string.IsNullOrEmpty(UserRole))
                xt.Add(new XAttribute("UserRole", UserRole));

            xml.Add(xt);

            foreach (var c in Columns)
            {
                c.CreateXml(xt);
            }
        }
    }

    /// <summary>
    /// Represents the <see cref="Table"/> database mapper.
    /// </summary>
#pragma warning disable CA1812 // Apparently never instantiated; by-design - it is!
    internal class TableMapper : DatabaseMapper<Table, TableMapper>
#pragma warning restore CA1812
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableMapper"/> class.
        /// </summary>
        public TableMapper()
        {
            Property(x => x.Name, "TABLE_NAME");
            Property(x => x.Schema, "TABLE_SCHEMA");
            Property(x => x.IsAView).MapFromDb((dr, t, ot) => dr.GetValue<string>("TABLE_TYPE") == "VIEW");
        }

        /// <summary>
        /// Default other properties.
        /// </summary>
        /// <param name="value">The <see cref="Table"/> value.</param>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <param name="operationType">The <see cref="OperationTypes"/>.</param>
        /// <param name="data">Optional data.</param>
        /// <returns>The <see cref="Table"/> value.</returns>
        protected override Table OnMapFromDb(Table value, DatabaseRecord dr, OperationTypes operationType, object? data)
        {
            value.View = !value.IsAView;
            value.Get = true;
            value.GetAll = true;
            value.Create = !value.IsAView;
            value.Update = !value.IsAView;
            value.Upsert = !value.IsAView;
            value.Delete = !value.IsAView;
            value.Merge = !value.IsAView;
            value.Udt = !value.IsAView;
            value.EfModel = true;
            return value;
        }
    }
}
