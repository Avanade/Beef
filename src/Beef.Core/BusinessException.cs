﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents a <b>Business</b> exception.
    /// </summary>
    /// <remarks>This is typically used for a business-oriented error that should be returned to the consumer.</remarks>
    public class BusinessException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        public BusinessException() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public BusinessException(string message)
            : base(message ?? new LText("Beef.BusinessException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public BusinessException(string message, Exception innerException)
            : base(message ?? new LText("Beef.BusinessException"), innerException)
        {
        }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.BusinessError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.BusinessError; }
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.BadRequest; }
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
