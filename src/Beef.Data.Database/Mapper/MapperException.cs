// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Data.Database.Mapper
{
    /// <summary>
    /// Represents a <b>Mapper</b> exception.
    /// </summary>
    public class MapperException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapperException"/> class.
        /// </summary>
        public MapperException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public MapperException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public MapperException(string message, Exception innerException) : base(message, innerException) { }
    }
}