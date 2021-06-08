// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents a data <b>Concurrency</b> exception.
    /// </summary>
    /// <remarks>The <see cref="Exception.Message"/> defaults to: <i>A concurrency error occurred; please refresh the data and try again.</i></remarks>
    public class ConcurrencyException : Exception, IBusinessException
    {
        private const string _message = "A concurrency error occurred; please refresh the data and try again.";

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        public ConcurrencyException() : this(null!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public ConcurrencyException(string? message) : base(message ?? new LText("Beef.ConcurrencyException", _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ConcurrencyException(string? message, Exception innerException) : base(message ?? new LText("Beef.ConcurrencyException", _message), innerException) { }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.ConcurrencyError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.ConcurrencyError; 

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.PreconditionFailed; 

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged => ShouldExceptionBeLogged; 
    }
}