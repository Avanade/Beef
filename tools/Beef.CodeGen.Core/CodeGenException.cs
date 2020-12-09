// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
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
        public CodeGenException(string message, string? extra) : base($"{message}{Environment.NewLine}{extra}") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public CodeGenException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenException"/> class for a specified <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The <see cref="ConfigBase"/> that the excpetion is related to.</param>
        /// <param name="propertyName">The corresponding property name that is in error.</param>
        /// <param name="message">The message that describes the error.</param>
        public CodeGenException(ConfigBase config, string propertyName, string message) : base($"{Check.NotNull(config, nameof(config)).BuildFullyQualifiedName(propertyName)}: {message}") { }
    }
}