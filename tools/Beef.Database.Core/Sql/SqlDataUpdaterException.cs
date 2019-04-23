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
        /// <param name="message"></param>
        public SqlDataUpdaterException(string message) : base(message) { }
    }
}
