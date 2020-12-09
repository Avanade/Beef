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
        ExampleMarkdown = "Under construction.")]
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