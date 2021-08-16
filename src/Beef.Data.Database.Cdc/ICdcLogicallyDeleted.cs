// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Enables an entity to identify whether it is logically deleted.
    /// </summary>
    public interface ICdcLogicallyDeleted : ILogicallyDeleted
    {
        /// <summary>
        /// Clears all the non-key (i.e non <see cref="Beef.Entities.UniqueKey"/>) properties where <see cref="ILogicallyDeleted.IsDeleted"/> as the data is technically non-existing.
        /// </summary>
        void ClearWhereDeleted();
    }
}