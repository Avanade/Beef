﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents a data <b>Duplicate</b> exception.
    /// </summary>
    public class DuplicateException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateException"/> class.
        /// </summary>
        public DuplicateException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public DuplicateException(string message)
            : base(message ?? new LText("Beef.DuplicateException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public DuplicateException(string message, Exception innerException)
            : base(message ?? new LText("Beef.DuplicateException"), innerException)
        {
        }

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.DuplicateError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.DuplicateError; }
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
