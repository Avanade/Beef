// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Provides the <see cref="Id"/> for a class with a <see cref="Type"/> of <see cref="int"/>.
    /// </summary>
    public interface IIntIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        int Id { get; set; }
    }

    /// <summary>
    /// Provides the <see cref="Id"/> for a class with a <see cref="Type"/> of <see cref="Guid"/>.
    /// </summary>
    public interface IGuidIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        Guid Id { get; set; }
    }
}
