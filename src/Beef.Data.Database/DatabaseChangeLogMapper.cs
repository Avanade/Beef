// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents a database <see cref="ChangeLog"/> mapper.
    /// </summary>
    public class DatabaseChangeLogMapper : DatabaseMapper<ChangeLog, DatabaseChangeLogMapper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseChangeLogMapper"/> mapper.
        /// </summary>
        public DatabaseChangeLogMapper()
        {
            Property(x => x.CreatedBy, DatabaseColumns.CreatedByName);
            Property(x => x.CreatedDate, DatabaseColumns.CreatedDateName);
            Property(x => x.UpdatedBy, DatabaseColumns.UpdatedByName);
            Property(x => x.UpdatedDate, DatabaseColumns.UpdatedDateName);
        }
    }
}