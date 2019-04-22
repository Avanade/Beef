// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.RefData
{
    /// <summary>
    /// Provides the sort order for the reference data.
    /// </summary>
    public enum ReferenceDataSortOrder
    {
        /// <summary>
        /// Ordered by <see cref="ReferenceDataBase.SortOrder"/> (default).
        /// </summary>
        SortOrder,

        /// <summary>
        /// Ordered by <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        Id,

        /// <summary>
        /// Ordered by <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        Code,

        /// <summary>
        /// Ordered by <see cref="ReferenceDataBase.Text"/>.
        /// </summary>
        Text
    }
}
