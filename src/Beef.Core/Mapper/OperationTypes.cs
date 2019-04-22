using System;

namespace Beef.Mapper
{
    /// <summary>
    /// Represents the mapping operation types (CRUD - Create/Read/Update/Delete).
    /// </summary>
    [Flags]
    public enum OperationTypes
    {
        /// <summary>
        /// Unspecified operation.
        /// </summary>
        Unspecified = 1,

        /// <summary>
        /// A <b>Get</b> (Read) operation.
        /// </summary>
        Get = 2,

        /// <summary>
        /// A <b>Create</b> operation.
        /// </summary>
        Create = 4,

        /// <summary>
        /// An <b>update</b> operation.
        /// </summary>
        Update = 8,

        /// <summary>
        /// A <b>delete</b> operation.
        /// </summary>
        Delete = 16,

        /// <summary>
        /// Any operation.
        /// </summary>
        Any = Unspecified | Get | Create | Update | Delete,

        /// <summary>
        /// Any operation except <see cref="Get"/>.
        /// </summary>
        AnyExceptGet = Unspecified | Create | Update | Delete,

        /// <summary>
        /// Any operation except <see cref="Create"/>.
        /// </summary>
        AnyExceptCreate = Unspecified | Get | Update | Delete,

        /// <summary>
        /// Any operation except <see cref="Update"/>.
        /// </summary>
        AnyExceptUpdate = Unspecified | Get | Create | Delete
    }
}
