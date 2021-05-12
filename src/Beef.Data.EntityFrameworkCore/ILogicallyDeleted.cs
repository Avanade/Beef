// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Enables support for an <see cref="IsDeleted"/> column.
    /// </summary>
    public interface ILogicallyDeleted
    {
        /// <summary>
        /// Indicates whether the columns is logically deleted.
        /// </summary>
        bool? IsDeleted { get; set; }
    }
}