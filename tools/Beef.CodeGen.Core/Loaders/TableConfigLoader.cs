// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Beef.Data.Database;
using Beef.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Beef.CodeGen.Loaders
{
    /// <summary>
    /// Represents an <see cref="Table"/> configuration loader.
    /// </summary>
    public class TableConfigLoader : ICodeGenConfigLoader, ICodeGenConfigGetLoaders
    {
        private List<Table> _tables;

        /// <summary>
        /// Gets the corresponding loaders.
        /// </summary>
        /// <returns>An <see cref="ICodeGenConfigLoader"/> array.</returns>
        public ICodeGenConfigLoader[] GetLoaders()
        {
            return new ICodeGenConfigLoader[]
            {
                new TableConfigLoader()
            };
        }

        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name { get { return "Table"; } }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> before the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadBeforeChildren(CodeGenConfig config)
        {
            if (_tables == null)
                LoadDatabase(config.Root.GetAttributeValue<string>("ConnectionString") ?? throw new CodeGenException("Config.ConnectionString has not been specified."), config.Root.GetAttributeValue<string>("RefDatabaseSchema"));

            var name = config.GetAttributeValue<string>("Name") ?? throw new CodeGenException("Table has no Name attribute specified.");
            config.AttributeAdd("Schema", "dbo");
            var schema = config.GetAttributeValue<string>("Schema");
            var table = _tables.Where(x => x.Name == name && x.Schema == schema).SingleOrDefault();
            if (table == null)
                throw new CodeGenException($"Specified Schema.Table '{schema}.{name}' not found in database.");

            config.AttributeAdd("Alias", table.Alias);
            config.AttributeAdd("IsAView", XmlConvert.ToString(table.IsAView));
        }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> after the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadAfterChildren(CodeGenConfig config)
        {
            var schema = config.GetAttributeValue<string>("Schema");
            var name = config.GetAttributeValue<string>("Name");
            var table = _tables.Where(x => x.Name == name && x.Schema == schema).SingleOrDefault();

            AddTableColumns(config, table);
            ExcludeUdtColumns(config);

            var autoGet = config.GetAttributeValue<bool>("Get");
            var autoGetAll = config.GetAttributeValue<bool>("GetAll");
            var autoCreate = config.GetAttributeValue<bool>("Create");
            var autoUpdate = config.GetAttributeValue<bool>("Update");
            var autoUpsert = config.GetAttributeValue<bool>("Upsert");
            var autoDelete = config.GetAttributeValue<bool>("Delete");
            var autoMerge = config.GetAttributeValue<bool>("Merge");

            // Check quick Get/Create/Update/Delete configurations.
            var spsConfig = CodeGenConfig.FindConfigList(config, "StoredProcedure");
            if (spsConfig != null)
            {
                foreach (CodeGenConfig spConfig in spsConfig)
                {
                    switch (spConfig.GetAttributeValue<string>("Name"))
                    {
                        case "Get": autoGet = false; break;
                        case "GetAll": autoGetAll = false; break;
                        case "Create": autoCreate = false; break;
                        case "Update": autoUpdate = false; break;
                        case "Upsert": autoUpsert = false; break;
                        case "Delete": autoDelete = false; break;
                        case "Merge": autoMerge = false; break;
                    }
                }
            }

            // Stop if no quick configs.
            if (!autoGet && !autoGetAll && !autoCreate && !autoUpdate && !autoUpsert && !autoDelete && !autoMerge)
                return;

            // Where no stored procedures already defined, need to add in the placeholder.
            if (spsConfig == null)
            {
                spsConfig = new List<CodeGenConfig>();
                config.Children.Add("StoredProcedure", spsConfig);
            }

            // Add each stored proecedure.
            if (autoMerge)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Merge");
                sp.AttributeAdd("Type", "Merge");
                spsConfig.Insert(0, sp);
            }

            if (autoDelete)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Delete");
                sp.AttributeAdd("Type", "Delete");
                spsConfig.Insert(0, sp);
            }

            if (autoUpsert)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Upsert");
                sp.AttributeAdd("Type", "Upsert");
                spsConfig.Insert(0, sp);
            }

            if (autoUpdate)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Update");
                sp.AttributeAdd("Type", "Update");
                spsConfig.Insert(0, sp);
            }

            if (autoCreate)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Create");
                sp.AttributeAdd("Type", "Create");
                spsConfig.Insert(0, sp);
            }

            if (autoGetAll)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "GetAll");
                sp.AttributeAdd("Type", "GetAll");

                if (config.Root.Attributes.ContainsKey("RefDatabaseSchema") && table.Schema == config.Root.GetAttributeValue<string>("RefDatabaseSchema"))
                {
                    var obList = new List<CodeGenConfig>();
                    CodeGenConfig ob = new CodeGenConfig("OrderBy", config);
                    ob.AttributeAdd("Name", "SortOrder");
                    ob.AttributeAdd("Order", "ASC");
                    obList.Add(ob);

                    ob = new CodeGenConfig("OrderBy", config);
                    ob.AttributeAdd("Name", "Code");
                    ob.AttributeAdd("Order", "ASC");
                    obList.Add(ob);

                    sp.Children.Add("OrderBy", obList);
                }

                spsConfig.Insert(0, sp);
            }

            if (autoGet)
            {
                CodeGenConfig sp = new CodeGenConfig("StoredProcedure", config);
                sp.AttributeAdd("Name", "Get");
                sp.AttributeAdd("Type", "Get");
                spsConfig.Insert(0, sp);
            }
        }

        private void AddTableColumns(CodeGenConfig config, Table table)
        {
            if (CodeGenConfig.FindConfigList(config, "Column") != null)
                return;

            var eci = string.IsNullOrEmpty(config.GetAttributeValue<string>("IncludeColumns")) ? new List<string>() : config.GetAttributeValue<string>("IncludeColumns").Split(',').Select(x => x.Trim()).ToList();
            var ecx = string.IsNullOrEmpty(config.GetAttributeValue<string>("ExcludeColumns")) ? new List<string>() : config.GetAttributeValue<string>("ExcludeColumns").Split(',').Select(x => x.Trim()).ToList();
            var colList = new List<CodeGenConfig>();
            foreach (var col in table.Columns)
            {
                if (eci.Count > 0 && !eci.Contains(col.Name))
                    continue;

                if (ecx.Contains(col.Name))
                    continue;

                var c = new CodeGenConfig("Column", config);
                c.AttributeAdd("Name", col.Name);
                c.AttributeAdd("Type", col.Type);
                c.AttributeAdd("IsNullable", XmlConvert.ToString(col.IsNullable));
                c.AttributeAdd("IsIdentity", XmlConvert.ToString(col.IsIdentity));
                c.AttributeAdd("IsPrimaryKey", XmlConvert.ToString(col.IsPrimaryKey));
                c.AttributeAdd("IsComputed", XmlConvert.ToString(col.IsComputed));
                if (col.Length.HasValue)
                    c.AttributeAdd("Length", XmlConvert.ToString(col.Length.Value));

                if (col.Precision.HasValue)
                    c.AttributeAdd("Precision", XmlConvert.ToString(col.Precision.Value));

                if (col.Scale.HasValue)
                    c.AttributeAdd("Scale", XmlConvert.ToString(col.Scale.Value));

                c.AttributeAdd("DotNetType", col.DotNetType);

                colList.Add(c);
            }

            if (colList.Count > 0)
                config.Children.Add("Column", colList);
        }

        private void ExcludeUdtColumns(CodeGenConfig config)
        {
            var ecx = string.IsNullOrEmpty(config.GetAttributeValue<string>("UdtExcludeColumns")) ? new List<string>() : config.GetAttributeValue<string>("UdtExcludeColumns").Split(',').Select(x => x.Trim()).ToList();
            if (ecx.Count == 0)
                return;

            var csConfig = CodeGenConfig.FindConfigList(config, "Column");
            if (csConfig != null)
            {
                foreach (CodeGenConfig cConfig in csConfig)
                {
                    if (ecx.Contains(cConfig.GetAttributeValue<string>("Name")))
                        cConfig.AttributeUpdate("UdtExclude", "true");
                }
            }
        }

        private void LoadDatabase(string connString, string refDataSchema)
        {
            Logger.Default.Info($"   Querying database: {connString}");

            var db = new SqlServerDb(connString);
            using (db.SetBypassDataContextScopeDbConnection())
            {
                _tables = Table.LoadTablesAndColumns(db, refDataSchema, false, false);
            }
        }

        private class SqlServerDb : DatabaseBase
        {
            public SqlServerDb(string connectionString) : base(connectionString, System.Data.SqlClient.SqlClientFactory.Instance) { }
        }
    }
}
