// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Specialized;
using System.ComponentModel;

namespace Beef.Entities
{
    /// <summary>
    /// Extends <see cref="IChangeTracking"/> to add tracking (logging) of the property changes.
    /// </summary>
    public interface IChangeTrackingLogging : IChangeTracking
    {
        /// <summary>
        /// Determines that until <see cref="IChangeTracking.AcceptChanges"/> is invoked all property changes are to be logged (see <see cref="ChangeTracking"/>).
        /// </summary>
        void TrackChanges();

        /// <summary>
        /// Lists the properties that have had changes tracked.
        /// </summary>
        StringCollection? ChangeTracking { get; }

        /// <summary>
        /// Indicates whether entity is currently <see cref="ChangeTracking"/>; <see cref="TrackChanges"/> and <see cref="IChangeTracking.AcceptChanges"/>.
        /// </summary>
        bool IsChangeTracking { get; }
    }
}
