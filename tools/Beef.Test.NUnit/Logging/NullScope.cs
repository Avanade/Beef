// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Represents the null scope for the loggers.
    /// </summary>
    internal class NullScope : IDisposable
    {
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static NullScope Default { get; } = new NullScope();

        /// <summary>
        /// Initializes a new instance of the <see cref="NullScope"/> class.
        /// </summary>
        private NullScope() { }

        /// <summary>
        /// Closes and disposes the <see cref="NullScope"/>.
        /// </summary>
        public void Dispose() { }
    }
}