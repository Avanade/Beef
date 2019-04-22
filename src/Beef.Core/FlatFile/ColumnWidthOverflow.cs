// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef


namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the action to perform when the width of the data exceeds the defined column width.
    /// </summary>
    public enum ColumnWidthOverflow
    {
        /// <summary>
        /// Indicates to truncate the excess and warn.
        /// </summary>
        Truncate,

        /// <summary>
        /// Indicates that an error should occur.
        /// </summary>
        Error
    }
}
