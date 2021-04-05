// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using System.Data;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the base <see cref="CdcTracker"/> data mapper.
    /// </summary>
    public abstract class CdcTrackingDbMapperBase : DatabaseMapper<CdcTracker>, ITrackingTvp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdcIdentifierMappingDbMapperBase"/> class.
        /// </summary>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public CdcTrackingDbMapperBase(string dbTypeName)
        {
            DbTypeName = Check.NotEmpty(dbTypeName, nameof(dbTypeName));

            Property(s => s.Key);
            Property(s => s.Hash);
        }

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The <see cref="CdcTracker"/> list.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<CdcTracker> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("Key", typeof(string));
            dt.Columns.Add("Hash", typeof(string));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            AddToTableValuedParameter(tvp, list);
            return tvp;
        }
    }
}