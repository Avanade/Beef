// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp
{
    /// <summary>
    /// Represents a Code Generation <see cref="Exception"/> where changes to a generated artefact were found.
    /// </summary>
    /// <remarks>Raised where the code-generation would result in changes to an underlying artefact. This is managed by setting <see cref="CodeGeneratorArgsBase.ExpectNoChanges"/> to <c>true</c>.</remarks>
    public class CodeGenChangesFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenChangesFoundException"/> class.
        /// </summary>
        public CodeGenChangesFoundException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenChangesFoundException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CodeGenChangesFoundException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenChangesFoundException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public CodeGenChangesFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}