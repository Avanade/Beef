// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Enables support for a <see cref="TenantId"/> column.
    /// </summary>
    public interface IMultiTenant
    {
        /// <summary>
        /// Indicates whether the columns is logically deleted.
        /// </summary>
        Guid? TenantId { get; set; }
    }
}