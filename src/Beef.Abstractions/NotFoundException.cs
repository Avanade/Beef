// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an entity not found exception.
    /// </summary>
    /// <remarks>The <see cref="Exception.Message"/> defaults to: <i>Requested data was not found.</i></remarks>
    public class NotFoundException : Exception, IBusinessException
    {
        private const string _message = "Requested data was not found.";

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException() : base(new LText("Beef.NotFoundException")) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public NotFoundException(string? message) : base(message ?? new LText("Beef.NotFoundException", _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public NotFoundException(string? message, Exception innerException) : base(message ?? new LText("Beef.NotFoundException", _message), innerException) { }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.NotFoundError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.NotFoundError; 

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.NotFound; 

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged => ShouldExceptionBeLogged; 
    }
}