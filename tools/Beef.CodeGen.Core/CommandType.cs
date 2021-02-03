// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents the command type.
    /// </summary>
    [Flags]
    public enum CommandType
    {
        /// <summary>
        /// The primary entity configuration.
        /// </summary>
        Entity = 1,

        /// <summary>
        /// The database configuration.
        /// </summary>
        Database = 2,

        /// <summary>
        /// The reference data configuration.
        /// </summary>
        RefData = 4,

        /// <summary>
        /// The data model configuration.
        /// </summary>
        DataModel = 8,

        /// <summary>
        /// All command types selected.
        /// </summary>
        All = Entity | Database | RefData | DataModel
    }
}