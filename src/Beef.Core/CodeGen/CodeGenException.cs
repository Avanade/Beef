// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents a Code Generation <see cref="Exception"/>; results should be reported back to consumer.
    /// </summary>
    public class CodeGenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class.
        /// </summary>
        public CodeGenException() : base() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CodeGenException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class with <paramref name="extra"/> context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="extra">Extra context information to append to the message.</param>
        public CodeGenException(string message, string extra) : base($"{message}{Environment.NewLine}{extra}") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public CodeGenException(string message, Exception innerException) : base(message, innerException) { }
    }
}