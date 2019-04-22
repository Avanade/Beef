// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the validation to be performed where the number of columns read does not match the expected; as defined by the underlying <see cref="Type"/>
    /// definition (see <see cref="FileColumnAttribute"/>).
    /// </summary>
    public enum ColumnCountValidation
    {
        /// <summary>
        /// No column count validation is to occur.
        /// </summary>
        None,

        /// <summary>
        /// Where less columns than expected an error should occur.
        /// </summary>
        LessThanError,

        /// <summary>
        /// Where less columns than expected a warning should occur.
        /// </summary>
        LessThanWarning,

        /// <summary>
        /// Where more columns than expected an error should occur.
        /// </summary>
        GreaterThanError,

        /// <summary>
        /// Where more columns than expected a warning should occur.
        /// </summary>
        GreaterThanWarning,

        /// <summary>
        /// Where less or more columns than expected an error should occur.
        /// </summary>
        LessAndGreaterThanError,

        /// <summary>
        /// Where less or more columns than expected a warning should occur.
        /// </summary>
        LessAndGreaterThanWarning
    }
}
