// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents a <b>Business</b> exception.
    /// </summary>
    /// <remarks>This is typically used for a business-oriented error that should be returned to the consumer.
    /// <para>The <see cref="Exception.Message"/> defaults to: <i>A business error occurred.</i></para></remarks>
    public class BusinessException : Exception, IBusinessException
    {
        private const string _message = "A business error occurred.";

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        public BusinessException() : this(null!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public BusinessException(string? message) : base(message ?? new LText("Beef.BusinessException", _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public BusinessException(string? message, Exception innerException) : base(message ?? new LText("Beef.BusinessException", _message), innerException) { }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.BusinessError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.BusinessError; 

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.BadRequest; 

        /// <summary>
        /// Indicates whether the <see cref="Exception"/> should be logged (returns the <see cref="ShouldExceptionBeLogged"/> value).
        /// </summary>
        public bool ShouldBeLogged => ShouldExceptionBeLogged; 
    }
}