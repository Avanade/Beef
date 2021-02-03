// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) tracking model.
    /// </summary>
    public class CdcTracker
    {
        /// <summary>
        /// Gets or sets the key represented as string.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the hash code represented as a string.
        /// </summary>
        public string? Hash { get; set; }
    }
}