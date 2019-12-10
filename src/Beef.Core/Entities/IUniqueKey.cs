﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Provides the <see cref="Entities.UniqueKey"/> for an entity class.
    /// </summary>
    public interface IUniqueKey
    {
        /// <summary>
        /// Indicates whether the <see cref="System.Object"/> has a <see cref="UniqueKey"/> value.
        /// </summary>
        bool HasUniqueKey { get; }

        /// <summary>
        /// Gets the <see cref="UniqueKey"/>.
        /// </summary>
        UniqueKey UniqueKey { get; }

#pragma warning disable CA1819 // Properties should not return arrays; by-design, returns an immutable array.
        /// <summary>
        /// Gets the list of property names that represent the unique key.
        /// </summary>
        string[] UniqueKeyProperties { get; }
#pragma warning restore CA1819
    }
}
