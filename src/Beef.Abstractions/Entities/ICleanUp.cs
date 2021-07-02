// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Provides a means to <see cref="CleanUp"/> the class.
    /// </summary>
    /// <seealso cref="Cleaner"/>.
    public interface ICleanUp
    {
        /// <summary>
        /// Cleans up the properties of the class.
        /// </summary>
        void CleanUp();

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        bool IsInitial { get; }
    }
}
