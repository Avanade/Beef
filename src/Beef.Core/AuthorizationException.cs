// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an <b>Authorization</b> exception.
    /// </summary>
    /// <remarks>The <see cref="Exception.Message"/> defaults to: <i>An authorization error occurred; you are not permitted to perform this action.</i></remarks>
    public class AuthorizationException : Exception, IBusinessException
    {
        private const string _message = "An authorization error occurred; you are not permitted to perform this action.";

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class.
        /// </summary>
        public AuthorizationException() : this(null!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public AuthorizationException(string? message) : base(message ?? new LText("Beef.AuthorizationException", _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public AuthorizationException(string? message, Exception innerException) : base(message ?? new LText("Beef.AuthorizationException", message), innerException) { }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.AuthorizationError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.AuthorizationError;

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged => ShouldExceptionBeLogged;
    }
}