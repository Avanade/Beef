// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Enables access to the base <see cref="IDatabase"/> instance (see <see cref="BaseDatabase"/>).
    /// </summary>
    public interface IEfDbContext
    {
        /// <summary>
        /// Gets the base <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase BaseDatabase { get; }
    }
}