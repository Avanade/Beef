// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an <b>Authentication</b> exception.
    /// </summary>
    /// <remarks>The <see cref="Exception.Message"/> defaults to: <i>An authentication error occured; the credentials you provided are not valid.</i></remarks>
    public class AuthenticationException : Exception, IBusinessException
    {
        private const string _message = "An authentication error occured; the credentials you provided are not valid.";

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
        /// </summary>
        public AuthenticationException() : this(null!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public AuthenticationException(string? message) : base(message ?? new LText("Beef.AuthenticationException", _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public AuthenticationException(string? message, Exception innerException) : base(message ?? new LText("Beef.AuthenticationException", _message), innerException) { }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.AuthenticationError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.AuthenticationError;

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged => ShouldExceptionBeLogged; 
    }
}