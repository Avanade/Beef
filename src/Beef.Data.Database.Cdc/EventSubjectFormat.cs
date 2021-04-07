// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Defines the event subject format.
    /// </summary>
    public enum EventSubjectFormat
    {
        /// <summary>
        /// The subject name appended with the entity key.
        /// </summary>
        NameAndKey,

        /// <summary>
        /// The subject name only.
        /// </summary>
        NameOnly
    }
}