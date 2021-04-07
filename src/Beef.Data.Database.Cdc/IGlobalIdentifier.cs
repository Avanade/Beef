// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the <see cref="GlobalId"/> for a class.
    /// </summary>
    public interface IGlobalIdentifier
    {
        /// <summary>
        /// Gets or sets the global identifier.
        /// </summary>
        string? GlobalId { get; set; }
    }
}