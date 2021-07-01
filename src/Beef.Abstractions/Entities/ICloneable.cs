// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Provides a means to <see cref="Clone"/> a copy of a class instance.
    /// </summary>
    public interface ICloneable
    {
        /// <summary>
        /// Creates a new object that is a deep copy of the current instance.
        /// </summary>
        /// <returns>A cloned instance.</returns>
        object Clone();
    }
}
