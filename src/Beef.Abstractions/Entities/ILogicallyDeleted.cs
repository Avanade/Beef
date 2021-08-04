// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Enables support for logical deletes via an <see cref="IsDeleted"/> property.
    /// </summary>
    public interface ILogicallyDeleted
    {
        /// <summary>
        /// Indicates whether the entity is considered logically deleted.
        /// </summary>
        bool? IsDeleted { get; set; }
    }
}