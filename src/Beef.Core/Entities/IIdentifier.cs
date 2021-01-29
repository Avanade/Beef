﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Enables the base <c>Id</c> property capability.
    /// </summary>
    public interface IIdentifier { }

    /// <summary>
    /// Provides the <see cref="Id"/> for a class with a <see cref="Type"/> of <see cref="int"/>.
    /// </summary>
    public interface IIntIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        int Id { get; set; }
    }

    /// <summary>
    /// Provides the <see cref="Id"/> for a class with a <see cref="Type"/> of <see cref="Guid"/>.
    /// </summary>
    public interface IGuidIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        Guid Id { get; set; }
    }

    /// <summary>
    /// Provides the <see cref="Id"/> for a class with a <see cref="Type"/> of <see cref="string"/>.
    /// </summary>
    public interface IStringIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        string? Id { get; set; }
    }
}