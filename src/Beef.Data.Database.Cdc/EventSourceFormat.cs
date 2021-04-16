// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Defines the event source format.
    /// </summary>
    public enum EventSourceFormat
    {
        /// <summary>
        /// The source name appended with the <see cref="Entities.IUniqueKey"/>.
        /// </summary>
        NameAndKey,

        /// <summary>
        /// The source name appended with the <see cref="IGlobalIdentifier"/> where exists; otherwise, will fall back to <see cref="NameAndKey"/>.
        /// </summary>
        NameAndGlobalId,

        /// <summary>
        /// The source name only.
        /// </summary>
        NameOnly
    }
}