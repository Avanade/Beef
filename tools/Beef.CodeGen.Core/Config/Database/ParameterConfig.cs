// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx.DbSchema;
using OnRamp;
using OnRamp.Config;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure parameter configuration.
    /// </summary>
    [CodeGenClass("Parameter", Title = "'Parameter' object (database-driven)", 
        Description = "The `Parameter` is used to define a stored procedure parameter and its charateristics. These are in addition to those that are automatically inferred (added) by the selected `StoredProcedure.Type`.",
        ExampleMarkdown = @"A YAML example is as follows:
``` yaml
tables:
- { name: Table, schema: Test, create: true, update: true, upsert: true, delete: true, merge: true, udt: true, getAll: true, getAllOrderBy: [ Name Des ], excludeColumns: [ Other ], permission: TestSec,
    storedProcedures: [
      { name: GetByArgs, type: GetColl, excludeColumns: [ Count ],
        parameters: [
          { name: Name, nullable: true, operator: LIKE },
          { name: MinCount, operator: GE, column: Count },
          { name: MaxCount, operator: LE, column: Count, nullable: true }
        ]
      },
      { name: Get, type: Get, withHints: NOLOCK,
        execute: [
          { statement: EXEC Demo.Before, location: Before },
          { statement: EXEC Demo.After }
        ]
      },
      { name: Update, type: Update }
    ]
  }
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    public class ParameterConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Parameter", Name);

        #region Key

        /// <summary>
        /// Gets or sets the parameter name (without the `@` prefix).
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The parameter name (without the `@` prefix).", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the corresponding column name; used to infer characteristics.
        /// </summary>
        [JsonPropertyName("column")]
        [CodeGenProperty("Key", Title = "The corresponding column name; used to infer characteristics.",
            Description = "Defaults to `Name`.")]
        public string? Column { get; set; }

        /// <summary>
        /// Gets or sets the SQL type definition (overrides inherited Column definition) including length/precision/scale.
        /// </summary>
        [JsonPropertyName("sqlType")]
        [CodeGenProperty("Key", Title = "The SQL type definition (overrides inherited Column definition) including length/precision/scale.")]
        public string? SqlType { get; set; }

        /// <summary>
        /// Indicates whether the parameter is nullable.
        /// </summary>
        [JsonPropertyName("nullable")]
        [CodeGenProperty("Key", Title = "Indicates whether the parameter is nullable.",
            Description = "Note that when the parameter value is `NULL` it will not be included in the query.")]
        public bool? Nullable { get; set; }

        /// <summary>
        /// Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.
        /// </summary>
        [JsonPropertyName("treatColumnNullAs")]
        [CodeGenProperty("Key", Title = "Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.")]
        public bool? TreatColumnNullAs { get; set; }

        /// <summary>
        /// Indicates whether the parameter is a collection (one or more values to be included `IN` the query).
        /// </summary>
        [JsonPropertyName("collection")]
        [CodeGenProperty("Key", Title = "Indicates whether the parameter is a collection (one or more values to be included `IN` the query).")]
        public bool? Collection { get; set; }

        /// <summary>
        /// Gets or sets the collection type.
        /// </summary>
        [JsonPropertyName("collectionType")]
        [CodeGenProperty("Key", Title = "The collection type.", IsImportant = true, Options = ["JSON", "UDT"],
            Description = "Values are `JSON` being a JSON array (preferred) or `UDT` for a User-Defined Type (legacy). Defaults to `StoredProcedure.CollectionType`.")]
        public string? CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the where clause equality operator.
        /// </summary>
        [JsonPropertyName("operator")]
        [CodeGenProperty("Key", Title = "The where clause equality operator", IsImportant = true, Options = ["EQ", "NE", "LT", "LE", "GT", "GE", "LIKE"],
            Description = "Defaults to `EQ`.")]
        public string? Operator { get; set; }

        #endregion

        /// <summary>
        /// Indicates whether the parameter is to be used for the where.
        /// </summary>
        public bool IsWhere { get; set; }

        /// <summary>
        /// Indicates whether the parameter is to be used for the where clause only.
        /// </summary>
        public bool WhereOnly { get; set; }

        /// <summary>
        /// Gets or sets the where SQL clause.
        /// </summary>
        public string? WhereSql { get; set; }

        /// <summary>
        /// Gets or sets the SQL operator.
        /// </summary>
        public string? SqlOperator { get; private set; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => "@" + Name;

        /// <summary>
        /// Gets the parameter SQL definition.
        /// </summary>
        public string? ParameterSql { get; private set; }

        /// <summary>
        /// Inidicates whether the parameter is OUTPUT versus input.
        /// </summary>
        public bool Output { get; set; }

        /// <summary>
        /// Gets the corresponding <see cref="DbColumnSchema"/>.
        /// </summary>
        public DbColumnSchema? DbColumn { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Column = DefaultWhereNull(Column, () => Name);
            DbColumn = (Parent!.Parent!.Columns.Where(x => x.Name == Column).SingleOrDefault()?.DbColumn);

            Operator = DefaultWhereNull(Operator, () => "EQ");
            CollectionType = DefaultWhereNull(CollectionType, () => Parent!.CollectionType);
            if (CompareValue(Collection, true))
                Nullable = CollectionType == "JSON";

            SqlType = DefaultWhereNull(SqlType, () =>
            {
                if (DbColumn is null)
                    throw new CodeGenException(this, nameof(Column), $"Column '{Column}' (Schema.Table '{Parent!.Parent!.Schema}.{Parent!.Parent!.Name}') not found in database.");

                if (CompareValue(Collection, true))
                {
                    if (CollectionType == "JSON")
                        return $"NVARCHAR(MAX)";

                    var sb = new StringBuilder();
                    var udt = DbColumn!.Type!.ToUpperInvariant() switch
                    {
                        "UNIQUEIDENTIFIER" => "UniqueIdentifier",
                        "NVARCHAR" => "NVarChar",
                        "INT" => "Int",
                        "BIGINT" => "BigInt",
                        "DATETIME2" => "DateTime2",
                        _ => DbColumn.Type
                    };

                    sb.Append($"[dbo].[udt{udt}List] READONLY");
                    return sb.ToString();
                }
                else
                {
                    return DbColumn.DbTable.Migration.SchemaConfig.ToFormattedSqlType(DbColumn, false);
                }
            });

            if (CompareValue(Collection, true))
                Name += "s";

            SqlOperator = Operator!.ToUpperInvariant() switch
            {
                "EQ" => "=",
                "NE" => "!=",
                "LT" => "<",
                "LE" => "<=",
                "GT" => ">",
                "GE" => ">=",
                "LIKE" => "LIKE",
                _ => Operator
            };

            ParameterSql = $"{ParameterName} AS {SqlType}{(CompareValue(Nullable, true) ? " NULL = NULL" : "")}{(Output ? " = NULL OUTPUT" : "")}";
            WhereSql = DefaultWhereNull(WhereSql, () =>
            {
                if (CompareValue(Collection, true))
                {
                    if (CollectionType == "JSON")
                        return $"({ParameterName}Count = 0 OR [{Parent!.Parent!.Alias}].[{Column}] IN (SELECT VALUE FROM OPENJSON({ParameterName})))";
                    else
                        return $"({ParameterName}Count = 0 OR [{Parent!.Parent!.Alias}].[{Column}] IN (SELECT [Value] FROM {ParameterName}))";
                }
                else
                {
                    var sql = TreatColumnNullAs != null ? $"ISNULL([{Parent!.Parent!.Alias}].[{Column}], {TreatColumnNullAs})" : $"[{Parent!.Parent!.Alias}].[{Column}]";
                    sql += $" {SqlOperator} @{Name}";
                    return TreatColumnNullAs == null && CompareValue(Nullable, true) ? $"(@{Name} IS NULL OR {sql})" : sql;
                }
            });

            return Task.CompletedTask;
        }
    }
}