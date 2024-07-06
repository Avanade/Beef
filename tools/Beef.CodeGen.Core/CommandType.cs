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
        All = Entity | Database | RefData | DataModel,

        /// <summary>
        /// Cleans (removes) all child 'Generated' directories.
        /// </summary>
        Clean = 2048,

        /// <summary>
        /// Counts the files and lines of code for child directories distinguising between 'Generated' and non-generated.
        /// </summary>
        Count = 4096,

        /// <summary>
        /// Reports all the endpoints.
        /// </summary>
        EndPoints = 8192,

        /// <summary>
        /// Parses and imports from an OpenAPI document into a temporary file.
        /// </summary>
        OpenApi = 16384
    }
}