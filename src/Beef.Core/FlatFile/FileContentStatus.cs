// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the file content status.
    /// </summary>
    public enum FileContentStatus
    {
        /// <summary>
        /// Indicates header record.
        /// </summary>
        Header,

        /// <summary>
        /// Indicates a content record(s).
        /// </summary>
        Content,

        /// <summary>
        /// Indicates the trailer record.
        /// </summary>
        Trailer,

        /// <summary>
        /// Indicates the end of file.
        /// </summary>
        EndOfFile
    }
}
