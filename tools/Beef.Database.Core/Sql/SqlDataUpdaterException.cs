// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Represents a <see cref="SqlDataUpdater"/> exception.
    /// </summary>
    public class SqlDataUpdaterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataUpdaterException"/> class.
        /// </summary>
        public SqlDataUpdaterException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataUpdaterException"/> class with a specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public SqlDataUpdaterException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataUpdaterException"/> class with a specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public SqlDataUpdaterException(string message, Exception innerException) : base(message, innerException) { }
    }
}