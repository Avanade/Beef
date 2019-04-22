// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef
{
    /// <summary>
    /// Represents the <see cref="DataContextScope"/> options.
    /// </summary>
    public enum DataContextScopeOption
    {
        /// <summary>Indicates that the existing <see cref="DataContextScope"/> should be used where available; otherwise, create new.</summary>
        UseExisting = 0,

        /// <summary>Indicates that a new <see cref="DataContextScope"/> should be used; in addition to any pre-existing.</summary>
        RequiresNew = 1
    }
}
