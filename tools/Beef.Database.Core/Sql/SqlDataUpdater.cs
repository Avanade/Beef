// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.CodeGen.Entities;
using Beef.Data.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Provides the capabibilities to update SQL data.
    /// </summary>
    public class SqlDataUpdater
    {
        internal static readonly DateTime DateTimeNow = DateTime.Now;
        internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        private readonly JObject _json;

        /// <summary>
        /// Register the database.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseBase"/>.</param>
        /// <param name="refDataSchema">The reference data schema.</param>
        public static void RegisterDatabase(DatabaseBase db, string refDataSchema)
        {
            if (DbTables == null)
                DbTables = Table.LoadTablesAndColumns(db, refDataSchema);

            RefDataSchema = refDataSchema;
        }

        /// <summary>
        /// Registers the database tables.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="refDataSchema">The reference data schema.</param>
        public static void RegisterDatabase(List<Table> tables, string refDataSchema)
        {
            if (DbTables == null)
                DbTables = tables;

            RefDataSchema = refDataSchema;
        }

        /// <summary>
        /// Gets the reference data schema.
        /// </summary>
        public static string RefDataSchema { get; private set; }

        /// <summary>
        /// Gets the registered database tables.
        /// </summary>
        public static List<Table> DbTables { get; private set; }

        /// <summary>
        /// Reads and parses the YAML <see cref="string"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="string"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(string yaml)
        {
            using (var sr = new StringReader(yaml))
            {
                return ReadYaml(sr);
            }
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return ReadYaml(sr);
            }
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(TextReader tr)
        {
            var yaml = new DeserializerBuilder().Build().Deserialize(tr);
            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yaml);
            return ReadJson(json);
        }

        /// <summary>
        /// Reads and parses the JSON <see cref="string"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="string"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadJson(string json)
        {
            return new SqlDataUpdater(JObject.Parse(json));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataUpdater"/> class.
        /// </summary>
        /// <param name="json">The <see cref="JObject"/> configuration.</param>
        private SqlDataUpdater(JObject json)
        {
            _json = json;
            Parse();
        }

        /// <summary>
        /// Gets <see cref="SqlDataTable"/> list.
        /// </summary>
        public List<SqlDataTable> Tables { get; } = new List<SqlDataTable>();

        /// <summary>
        /// Parses the data operations generating the underlying SQL.
        /// </summary>
        private void Parse()
        {
            if (DbTables == null)
                throw new InvalidOperationException("RegisterDatabase must be invoked before parsing can occur.");

            // Loop through all the schemas.
            foreach (var js in _json.Children<JProperty>())
            {
                // Loop through the collection of tables.
                foreach (var jto in GetChildObjects(js))
                {
                    foreach (var jt in jto.Children<JProperty>())
                    {
                        var sdt = new SqlDataTable(js.Name, jt.Name);
                        if (Tables.Any(t => t.Schema == sdt.Schema && t.Name == sdt.Name))
                            throw new SqlDataUpdaterException($"Table '{sdt.Schema}.{sdt.Name}' has been specified more than once.");

                        // Loop through the collection of rows.
                        foreach (var jro in GetChildObjects(jt))
                        {
                            var row = new SqlDataRow(sdt);

                            foreach (var jr in jro.Children<JProperty>())
                            {
                                if (jr.Value.Type == JTokenType.Object)
                                {
                                    foreach (var jc in jro.Children<JProperty>())
                                    {
                                        var col = sdt.IsRefData ? DatabaseRefDataColumns.CodeColumnName : sdt.DbTable.Columns.Where(x => x.IsPrimaryKey).Select(x => x.Name).SingleOrDefault();
                                        row.AddColumn(col, GetColumnValue(jc.Name));
                                        foreach (var jcv in jc.Values().Where(j => j.Type == JTokenType.Property).Cast<JProperty>())
                                        {
                                            row.AddColumn(jcv.Name, GetColumnValue(jcv.Value));
                                        }
                                    }
                                }
                                else
                                {
                                    if (sdt.IsRefData && jro.Children().Count() == 1)
                                    {
                                        row.AddColumn(DatabaseRefDataColumns.CodeColumnName, GetColumnValue(jr.Name));
                                        row.AddColumn("Text", GetColumnValue(jr.Value));
                                    }
                                    else
                                        row.AddColumn(jr.Name, GetColumnValue(jr.Value));
                                }
                            }

                            sdt.AddRow(row);
                        }

                        if (sdt.Columns.Count > 0)
                        {
                            sdt.Prepare();
                            Tables.Add(sdt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the child objects.
        /// </summary>
        private static IEnumerable<JObject> GetChildObjects(JToken j)
        {
            foreach (var jc in j.Children<JArray>())
            {
                return jc.Children<JObject>();
            }

            return Array.Empty<JObject>();
        }

        /// <summary>
        /// Gets the column value.
        /// </summary>
        private static object GetColumnValue(JToken j)
        {
            switch (j.Type)
            {
                case JTokenType.Boolean: return j.Value<bool>();
                case JTokenType.Date: return j.Value<DateTime>();
                case JTokenType.Float: return j.Value<float>();
                case JTokenType.Guid: return j.Value<Guid>();
                case JTokenType.Integer: return j.Value<int>();
                case JTokenType.TimeSpan: return j.Value<TimeSpan>();
                case JTokenType.Uri: return j.Value<String>();
                case JTokenType.String: return j.Value<String>();
                default: return null;
            }
        }

        /// <summary>
        /// Writes the configuration as XML in preparation for code generation.
        /// </summary>
        /// <returns>The <see cref="XElement"/>.</returns>
        public XElement CreateXml()
        {
            var xgc = new XElement("CodeGeneration");

            foreach (var t in Tables)
            {
                var xt = new XElement("Table",
                    new XAttribute("Name", t.Name),
                    new XAttribute("Schema", t.Schema),
                    new XAttribute("Alias", t.DbTable.Alias),
                    new XAttribute("IsMerge", t.IsMerge),
                    new XAttribute("IsRefData", t.IsRefData));

                // Add the columns configuration (used/referenced).
                foreach (var col in t.Columns)
                {
                    col.Value.CreateXml(xt);
                }

                // Add the actual rows and columns.
                foreach (var r in t.Rows)
                {
                    var xr = new XElement("Row");

                    foreach (var c in r.Columns)
                    {
                        var xc = new XElement("Col",
                            new XAttribute("Name", c.Value.Name),
                            new XAttribute("Value", c.Value.ToSqlValue()),
                            new XAttribute("UseForeignKeyQueryForId", c.Value.UseForeignKeyQueryForId));

                        xr.Add(xc);
                    }

                    xt.Add(xr);
                }

                xgc.Add(xt);
            }

            return xgc;
        }

        /// <summary>
        /// Generates the SQL.
        /// </summary>
        /// <param name="codeGen">The code generation action to execute.</param>
        public void GenerateSql(Action<CodeGeneratorEventArgs> codeGen)
        {
            if (codeGen == null)
                throw new ArgumentNullException(nameof(codeGen));

            var cg = CodeGenerator.Create(CreateXml());
            cg.CodeGenerated += (o, e) => codeGen(e);
            cg.Generate(XElement.Load(typeof(SqlDataUpdater).Assembly.GetManifestResourceStream($"{typeof(DatabaseExecutor).Namespace}.Resources.TableInsertOrMerge_sql.xml")));
        }
    }
}
