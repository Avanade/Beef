// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using System.Data;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the base <see cref="CdcIdentifierMapping"/> data mapper.
    /// </summary>
    public abstract class CdcIdentifierMappingDbMapperBase : DatabaseMapper<CdcIdentifierMapping>, IIdentifierMappingTvp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdcIdentifierMappingDbMapperBase"/> class.
        /// </summary>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public CdcIdentifierMappingDbMapperBase(string dbTypeName)
        {
            DbTypeName = Check.NotEmpty(dbTypeName, nameof(dbTypeName));

            Property(s => s.Schema);
            Property(s => s.Table);
            Property(s => s.Key);
            Property(s => s.GlobalId);
        }

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The <see cref="CdcIdentifierMapping"/> list.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<CdcIdentifierMapping> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("Schema", typeof(string));
            dt.Columns.Add("Table", typeof(string));
            dt.Columns.Add("Key", typeof(string));
            dt.Columns.Add("GlobalId", typeof(string));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            AddToTableValuedParameter(tvp, list);
            return tvp;
        }
    }
}