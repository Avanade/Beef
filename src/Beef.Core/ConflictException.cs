﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents an <b>Conflict</b> exception.
    /// </summary>
    /// <remarks>An example would be where the identifier provided for a Create operation already exists.</remarks>
    public class ConflictException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        public ConflictException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public ConflictException(string message)
            : base(message ?? new LText("Beef.ConflictException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ConflictException(string message, Exception innerException)
            : base(message ?? new LText("Beef.ConflictException"), innerException)
        {
        }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.ConflictError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.ConflictError; }
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.Conflict; }
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
