// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Enables an entity to identify whether it is logically deleted.
    /// </summary>
    public interface ILogicallyDeleted
    {
        /// <summary>
        /// Indicates whether the entity is logically deleted.
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// Clears all the non-key (i.e non <see cref="Beef.Entities.UniqueKey"/>) properties where <see cref="IsDeleted"/> as the data is technically non-existing.
        /// </summary>
        void ClearWhereDeleted();
    }
}