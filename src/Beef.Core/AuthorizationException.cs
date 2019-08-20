// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an <b>Authorization</b> exception.
    /// </summary>
    public class AuthorizationException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public AuthorizationException(string message = null)
            : base(message ?? new LText("Beef.AuthorizationException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public AuthorizationException(string message, Exception innerException)
            : base(message ?? new LText("Beef.AuthorizationException"), innerException)
        {
        }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.AuthorizationError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.AuthorizationError; }
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.Forbidden; }
        }

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged
        {
            get { return ShouldExceptionBeLogged; }
        }
    }
}
