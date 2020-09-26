// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the column configuration.
    /// </summary>
    public class SettableColumnConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the qualified name.
        /// </summary>
        public string QualifiedName => $"[{Parent!.Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => $"@{Name}";

        /// <summary>
        /// Gets or sets the database column configuration.
        /// </summary>
        public Column? DbColumn { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare() { }
    }
}