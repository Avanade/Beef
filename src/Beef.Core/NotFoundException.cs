// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an entity not found exception.
    /// </summary>
    public class NotFoundException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
            : base(new LText("Beef.NotFoundException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public NotFoundException(string? message)
            : base(message ?? new LText("Beef.NotFoundException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public NotFoundException(string? message, Exception innerException)
            : base(message ?? new LText("Beef.NotFoundException"), innerException)
        {
        }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.NotFoundError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.NotFoundError; }
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.NotFound; }
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
