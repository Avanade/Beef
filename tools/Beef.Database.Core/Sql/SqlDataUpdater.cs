// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using Beef.CodeGen.Database;
using Beef.CodeGen.Utility;
using Beef.Data.Database;
using HandlebarsDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Provides the capabibilities to update SQL data.
    /// </summary>
    public class SqlDataUpdater
    {
        internal static readonly DateTime DateTimeNow = Entities.Cleaner.Clean(DateTime.Now);
        internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        private readonly JObject _json;

        /// <summary>
        /// Register the database.
        /// </summary>
        /// <param name="dbConn">The <see cref="DbConnection"/>.</param>
        public static async Task RegisterDatabaseAsync(DbConnection dbConn)
        {
            if (DbTables == null)
                DbTables = await DbTable.LoadTablesAndColumnsAsync(dbConn, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the registered database tables.
        /// </summary>
        public static List<DbTable>? DbTables { get; private set; }

        /// <summary>
        /// Reads and parses the YAML <see cref="string"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="string"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(string yaml)
        {
            using var sr = new StringReader(yaml);
            return ReadYaml(sr);
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(Stream s)
        {
            using var sr = new StreamReader(s);
            return ReadYaml(sr);
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadYaml(TextReader tr)
        {
            var yaml = new DeserializerBuilder().Build().Deserialize(tr)!;
            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yaml);
            return ReadJson(json);
        }

        /// <summary>
        /// Reads and parses the JSON <see cref="string"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="string"/>.</param>
        /// <returns>The <see cref="SqlDataUpdater"/>.</returns>
        public static SqlDataUpdater ReadJson(string json) => new SqlDataUpdater(JObject.Parse(json));

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
        /// Gets the configured <see cref="IIdentifierGenerators"/>.
        /// </summary>
        public IIdentifierGenerators? IdentifierGenerators { get; set; }

        /// <summary>
        /// Parses the data operations generating the underlying SQL.
        /// </summary>
        private void Parse()
        {
            if (DbTables == null)
                throw new InvalidOperationException("RegisterDatabase must be invoked before parsing can occur.");

            // Get the identifier generator configuration where applicable.
            var idjson = _json["^Type"];
            if (idjson != null)
            {
                var typeName = idjson.ToObject<string>();
                if (string.IsNullOrEmpty(typeName))
                    throw new SqlDataUpdaterException($"Identifier generators property '^Type' is not a valid string.");

                var type = Type.GetType(typeName, false);
                if (type == null || type.GetConstructor(Array.Empty<Type>()) == null)
                    throw new SqlDataUpdaterException($"Identifier generators Type '{typeName}' does not exist or have a default (parameter-lesss) constructor.");

                var idgen = Activator.CreateInstance(type)!;
                IdentifierGenerators = idgen as IIdentifierGenerators;
                if (IdentifierGenerators == null)
                    throw new SqlDataUpdaterException($"Identifier generators Type '{typeName}' does not implement IIdentifierGenerators.");
            }

            // Loop through all the schemas.
            foreach (var js in _json.Children<JProperty>())
            {
                // Reserved; ignore.
                if (js.Name == "^Type")
                    continue;

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
                                        if (!string.IsNullOrEmpty(col))
                                        {
                                            row.AddColumn(col, GetColumnValue(jc.Name));
                                            foreach (var jcv in jc.Values().Where(j => j.Type == JTokenType.Property).Cast<JProperty>())
                                            {
                                                row.AddColumn(jcv.Name, GetColumnValue(jcv.Value));
                                            }
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
                            sdt.Prepare(IdentifierGenerators);
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
        private static object? GetColumnValue(JToken j)
        {
            return j.Type switch
            {
                JTokenType.Boolean => j.Value<bool>(),
                JTokenType.Date => j.Value<DateTime>(),
                JTokenType.Float => j.Value<float>(),
                JTokenType.Guid => j.Value<Guid>(),
                JTokenType.Integer => j.Value<int>(),
                JTokenType.TimeSpan => j.Value<TimeSpan>(),
                JTokenType.Uri => j.Value<String>(),
                JTokenType.String => GetRuntimeParameterValue(j.Value<String>()),
                _ => null
            };
        }

        /// <summary>
        /// Get the runtime parameter value.
        /// </summary>
        private static object? GetRuntimeParameterValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Get runtime value when formatted like: ^(DateTime.UtcNow)
            if (value.StartsWith("^(") && value.EndsWith(")"))
            {
                var (val, msg) = GetSystemRuntimeValue(value[2..^1]);
                if (msg == null)
                    return val;

                // Try again adding the System namespace.
                (val, msg) = GetSystemRuntimeValue("System." + value[2..^1]);
                if (msg == null)
                    return val;

                throw new SqlDataUpdaterException(msg);
            }
            else
                return value;
        }

        /// <summary>
        /// Get the system runtime value.
        /// </summary>
        private static (object? value, string? message) GetSystemRuntimeValue(string param)
        {
            var ns = param.Split(",");
            if (ns.Length > 2)
                return (null, $"Runtime value parameter '{param}' is invalid; incorrect format.");

            var parts = ns[0].Split(".");
            if (parts.Length <= 1)
                return (null, $"Runtime value parameter '{param}' is invalid; incorrect format.");

            Type? type = null;
            int i = parts.Length;
            for (; i >= 0; i--)
            {
                if (ns.Length == 1)
                    type = Type.GetType(string.Join('.', parts[0..^(parts.Length - i)]));
                else
                    type = Type.GetType(string.Join('.', parts[0..^(parts.Length - i)]) + "," + ns[1]);

                if (type != null)
                    break;
            }

            if (type == null)
                return (null, $"Runtime value parameter '{param}' is invalid; no Type can be found.");

            return GetSystemPropertyValue(param, type, null, parts[i..]);
        }

        /// <summary>
        /// Recursively navigates the properties and values to discern the value.
        /// </summary>
        private static (object? value, string? message) GetSystemPropertyValue(string param, Type type, object? obj, string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return (obj, null);

            var part = parts[0];
            if (part.EndsWith("()"))
            {
                var mi = type.GetMethod(part[0..^2], Array.Empty<Type>());
                if (mi == null || mi.GetParameters().Length != 0)
                    return (null, $"Runtime value parameter '{param}' is invalid; specified method '{part}' is invalid.");

                return GetSystemPropertyValue(param, mi.ReturnType, mi.Invoke(obj, null), parts[1..]);
            }
            else
            {
                var pi = type.GetProperty(part);
                if (pi == null || !pi.CanRead)
                    return (null, $"Runtime value parameter '{param}' is invalid; specified property '{part}' is invalid.");

                return GetSystemPropertyValue(param, pi.PropertyType, pi.GetValue(obj, null), parts[1..]);
            }
        }

        /// <summary>
        /// Generates the SQL.
        /// </summary>
        /// <param name="codeGen">The code generation action to execute.</param>
        public Task GenerateSqlAsync(Action<CodeGenOutputArgs> codeGen)
        {
            if (codeGen == null)
                throw new ArgumentNullException(nameof(codeGen));

            using var st = typeof(SqlDataUpdater).Assembly.GetManifestResourceStream($"{typeof(DatabaseExecutor).Namespace}.Resources.TableInsertOrMerge_sql.hbs");
            using var tr = new StreamReader(st!);

            var cg = new HandlebarsCodeGenerator(tr);
            foreach (var t in Tables)
            {
                codeGen(new CodeGenOutputArgs(new CodeGen.Scripts.CodeGenScript(), null, $"{t.Schema}.{t.Name} SQL", cg.Generate(t)));
            }

            return Task.CompletedTask;
        }
    }
}