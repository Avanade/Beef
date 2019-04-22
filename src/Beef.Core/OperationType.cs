// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef
{
    /// <summary>
    /// Represents the possible operations types.
    /// </summary>
    /// <remarks>Based on the standard CRUD operations: <see cref="Create"/>, <see cref="Read"/>, <see cref="Update"/> and <see cref="Delete"/>.</remarks>
    public enum OperationType
    {
        /// <summary>
        /// An unknown/unspecified operation type.
        /// </summary>
        Unspecified,

        /// <summary>
        /// A <b>create</b> operation.
        /// </summary>
        Create,

        /// <summary>
        /// A <b>read</b> operation.
        /// </summary>
        Read,

        /// <summary>
        /// An <b>update</b> operation.
        /// </summary>
        Update,

        /// <summary>
        /// A <b>delete</b> operation.
        /// </summary>
        Delete
    }
}
