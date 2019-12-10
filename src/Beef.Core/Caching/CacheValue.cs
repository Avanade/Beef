// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;

namespace Beef.Caching
{
    /// <summary>
    /// Represents the internal cached <see cref="Value"/> and corresponding <see cref="Policy"/>.
    /// </summary>
    internal struct CacheValue<TValue>
    {
        /// <summary>
        /// Gets the <see cref="ICachePolicy"/>.
        /// </summary>
        public ICachePolicy Policy { get; internal set; }

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        public TValue Value { get; internal set; }
    }
}
