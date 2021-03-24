// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) identifier mapping model.
    /// </summary>
    public class CdcIdentifierMapping
    {
        /// <summary>
        /// Gets or sets the table schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the key represented as string.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the global identifier as a string.
        /// </summary>
        public string? GlobalId { get; set; }
    }
}