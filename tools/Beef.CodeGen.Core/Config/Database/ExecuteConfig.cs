// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure additional statement configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Parameter", Title = "The **Execute** statement to enable additional functionality to be added before or after the primary statement.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class ExecuteConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// Gets or sets the additiona statement (TSQL).
        /// </summary>
        [JsonProperty("statement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The additional statement (TSQL).", IsMandatory = true, IsImportant = true)]
        public string? Statement { get; set; }

        /// <summary>
        /// Gets or sets the location of the statement in relation to the primary statement.
        /// </summary>
        [JsonProperty("location", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The location of the statement in relation to the primary statement.", IsImportant = true, Options = new string[] { "Before", "After" },
            Description = "Defaults to `After`.")]
        public string? Location { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Requirement is for lowercase.")]
        protected override void Prepare()
        {
            Location = DefaultWhereNull(Location, () => "After");
        }
    }
}