// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the overall file validation(s) required. 
    /// </summary>
    [Flags]
#pragma warning disable CA1714 // Flags enums should have plural names; not required.
    public enum FileValidation
#pragma warning restore CA1714
    {
        /// <summary>
        /// Indicates that the validation is unspecified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Indicates that the file must have rows; i.e. cannot be empty.
        /// </summary>
        MustHaveRows = 1,

        /// <summary>
        /// Indicates that the file where it has rows, must have a header row.
        /// </summary>
        MustHaveHeaderRow = 2,

        /// <summary>
        /// Indicates that the file where it has rows, must have a trailer row.
        /// </summary>
        MustHaveTrailerRow = 4,

        /// <summary>
        /// Indicates that the file where it has rows, must have at least one content row.
        /// </summary>
        MustHaveAtLeastOneContentRow = 8
    }
}
