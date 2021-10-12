// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure additional statement configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Execute", Title = "'Execute' object (database-driven)", 
        Description = "The _Execute_ object enables additional TSQL statements to be embedded within the stored procedure.",
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
    public class ExecuteConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the additional TSQL statement.
        /// </summary>
        [JsonProperty("statement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The additional TSQL statement.", IsMandatory = true, IsImportant = true)]
        public string? Statement { get; set; }

        /// <summary>
        /// Gets or sets the location of the statement in relation to the underlying primary stored procedure statement.
        /// </summary>
        [JsonProperty("location", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The location of the statement in relation to the underlying primary stored procedure statement.", IsImportant = true, Options = new string[] { "Before", "After" },
            Description = "Defaults to `After`.")]
        public string? Location { get; set; }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            Location = DefaultWhereNull(Location, () => "After");
        }
    }
}