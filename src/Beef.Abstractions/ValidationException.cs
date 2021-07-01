// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Net;

namespace Beef
{
    /// <summary>
    /// Represents a <b>Validation</b> exception.
    /// </summary>
    /// <remarks>The <see cref="Exception.Message"/> defaults to: <i>A data validation error occurred.</i></remarks>
    public class ValidationException : Exception, IBusinessException
    {
        private const string _key = "Beef.ValidationException";
        private const string _message = "A data validation error occurred.";
        private readonly MessageItemCollection _messages = new();

        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException() : base(null!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public ValidationException(string? message) : base(message ?? new LText(_key, _message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ValidationException(string? message, Exception innerException) : base(message ?? new LText(_key, _message), innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a <see cref="MessageItem"/> list.
        /// </summary>
        /// <param path="messages">The <see cref="MessageItem"/> list.</param>
        public ValidationException(IEnumerable<MessageItem> messages) : base(new LText(_key, _message))
        {
            if (messages != null)
                _messages.AddRange(messages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a <paramref name="message"/> and <see cref="MessageItem"/> list.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="messages">The <see cref="MessageItem"/> list.</param>
        public ValidationException(string? message, IEnumerable<MessageItem> messages) : base(message ?? new LText(_key, _message))
        {
            if (messages != null)
                _messages.AddRange(messages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a single <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="MessageItem"/>.</param>
        public ValidationException(MessageItem item) : base(new LText(_key, _message))
        {
            Check.NotNull(item, nameof(item));
            _messages.Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a <paramref name="message"/>, <see cref="MessageItem"/> list and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="messages">The <see cref="MessageItem"/> list.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ValidationException(string? message, IEnumerable<MessageItem> messages, Exception innerException)
            : base(message ?? new LText(_key), innerException)
        {
            if (messages != null)
                _messages.AddRange(messages);
        }

        /// <summary>
        /// Gets the underlying messages.
        /// </summary>
        public MessageItemCollection Messages => _messages;

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.ValidationError"/>).
        /// </summary>
        public ErrorType ErrorType => ErrorType.ValidationError; 

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