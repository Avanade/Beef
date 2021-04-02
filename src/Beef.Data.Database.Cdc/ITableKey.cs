// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the <see cref="TableKey"/> and the <see cref="IUniqueKey"/>.
    /// </summary>
    public interface ITableKey : IUniqueKey
    {
        /// <summary>
        /// Gets the <i>table's primary key</i> (represented as an <see cref="UniqueKey"/>) from the actual database table; not from the change-data-capture source.
        /// </summary>
        /// <remarks>The underlying <see cref="UniqueKey.IsInitial">IsInitial</see> will be <c>true</c> when the record no longer exists within the database (i.e. has been physically deleted).</remarks>
        UniqueKey TableKey { get; }
    }
}