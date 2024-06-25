// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Text.Json.Serialization;
using OnRamp.Config;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure where statement configuration.
    /// </summary>
    [CodeGenClass("QueryWhere", Title = "'QueryWhere' object (database-driven)",
        Description = "The `QueryWhere` object defines an additional where `Statement` to be added.")]
    [CodeGenCategory("Key", Title = "Provides the **key** configuration.")]
    public class QueryWhereConfig : ConfigBase<CodeGenConfig, QueryConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the where TSQL statement.
        /// </summary>
        [JsonPropertyName("statement")]
        [CodeGenProperty("Key", Title = "The where TSQL statement.", IsMandatory = true, IsImportant = true)]
        public string? Statement { get; set; }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync() => Task.CompletedTask;
    }
}