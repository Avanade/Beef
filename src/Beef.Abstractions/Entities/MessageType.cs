// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Represents the type of <see cref="MessageItem"/>.
    /// </summary>
    public enum MessageType
    {
        /// <summary>Indicates an informational message.</summary>
        Info = 0,
        /// <summary>Indicates a warning message.</summary>
        Warning = 1,
        /// <summary>Indicates an error message.</summary>
        Error = 2,
    }
}