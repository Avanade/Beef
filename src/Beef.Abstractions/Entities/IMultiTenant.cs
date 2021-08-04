// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Enables support for a <see cref="TenantId"/> column.
    /// </summary>
    public interface IMultiTenant
    {
        /// <summary>
        /// Gets or sets the <b>Tenant</b> identifier.
        /// </summary>
        Guid? TenantId { get; set; }
    }
}