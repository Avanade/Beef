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
        /// <param name="message">The message that describes the error.</param>
        public CodeGenException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class with <paramref name="extra"/> context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="extra">Extra context information to append to the message.</param>
        public CodeGenException(string message, string extra) : base(string.Format("{0}{1}{2}", message, Environment.NewLine, extra)) { }
    }
}

