// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp.Config;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure order-by configuration.
    /// </summary>
    [CodeGenClass("OrderBy", Title = "'OrderBy' object (database-driven)", 
        Description = "The `OrderBy` object defines the query order. Only valid for `StoredProcedure.Type` of `GetAll`.",
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
    public class OrderByConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("OrderBy", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the column to order by.
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The name of the `Column` to order by.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the sort order option.
        /// </summary>
        [JsonPropertyName("order")]
        [CodeGenProperty("Key", Title = "The corresponding sort order.", IsImportant = true, Options = new string[] { "Ascending", "Descending" },
            Description = "Defaults to `Ascending`.")]
        public string? Order { get; set; }

        #endregion

        /// <summary>
        /// Gets the order by SQL.
        /// </summary>
        public string OrderBySql => $"[{Parent!.Parent!.Alias}].[{Name}] {(Order!.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Order = DefaultWhereNull(Order, () => "Ascending");
            return Task.CompletedTask;
        }
    }
}