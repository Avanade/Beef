// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Provides the required table reference properties.
    /// </summary>
    public interface ITableReference
    {
        /// <summary>
        /// Gets the table schema.
        /// </summary>
        string? Schema { get; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        string? Table { get; }

        /// <summary>
        /// Gets the table alias.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Best name.")]
        string? Alias { get; }

        /// <summary>
        /// Gets the corresponding <see cref="DbTable"/>.
        /// </summary>
        DbTable? DbTable { get; }
    }
}