// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
    /// <summary>
    /// Enables the specification of the system time.
    /// </summary>
    public interface ISystemTime
    {
        /// <summary>
        /// Gets the current system time in UTC.
        /// </summary>
        DateTime UtcNow { get; }
    }
}