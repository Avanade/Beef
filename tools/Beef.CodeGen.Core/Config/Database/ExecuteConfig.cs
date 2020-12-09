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
        Markdown = "")]
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
            CheckOptionsProperties();

            Location = DefaultWhereNull(Location, () => "After");
        }
    }
}