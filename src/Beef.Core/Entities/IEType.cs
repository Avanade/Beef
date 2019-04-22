// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Provides the <see cref="EType"/> (entity type name) property.
    /// </summary>
    /// <remarks>Used to provide further context on the underlying entity type where the json serialization is considered ambiquous.</remarks>
    public interface IEType
    {
        /// <summary>
        /// Gets or sets the entity type name.
        /// </summary>
        string EType { get; set; }
    }
}