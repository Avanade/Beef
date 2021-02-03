// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure where statement configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Where", Title = "'Where' object (database-driven)", 
        Description = "The `Where` object defines an additional where `Statement` to be added. This is in addition to those automatically added based on the `StoredProcedure.Type`.",
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
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    public class WhereConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// Gets or sets the where statement (TSQL).
        /// </summary>
        [JsonProperty("statement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The where statement (TSQL).", IsMandatory = true, IsImportant = true)]
        public string? Statement { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare() 
        {
            CheckOptionsProperties();
        }
    }
}