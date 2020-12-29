// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the <see cref="CreateTableValuedParameter"/> capability.
    /// </summary>
    public interface ITrackingTvp
    {
        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The <see cref="CdcTracker"/> list.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        TableValuedParameter CreateTableValuedParameter(IEnumerable<CdcTracker> list);
    }
}