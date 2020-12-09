// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure where statement configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("QueryWhere", Title = "'QueryWhere' object (database-driven)",
        Description = "The `QueryWhere` object is used to define additional filtering.",
        Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class QueryWhereConfig : ConfigBase<CodeGenConfig, QueryConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the where TSQL statement.
        /// </summary>
        [JsonProperty("statement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The where TSQL statement.", IsMandatory = true, IsImportant = true)]
        public string? Statement { get; set; }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare() 
        {
            CheckOptionsProperties();
        }
    }
}