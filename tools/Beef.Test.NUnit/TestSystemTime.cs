// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides a testing <see cref="ISystemTime"/> where <see cref="UtcNow"/> can be specified.
    /// </summary>
    public class TestSystemTime : ISystemTime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSystemTime"/> class.
        /// </summary>
        /// <param name="overrideUtcNow">The <see cref="UtcNow"/> value.</param>
        public TestSystemTime(DateTime overrideUtcNow) => UtcNow = overrideUtcNow;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime UtcNow { get; private set; }
    }
}