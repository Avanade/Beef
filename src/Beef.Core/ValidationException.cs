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
    public class ValidationException : Exception, IBusinessException
    {
        /// <summary>
        /// Get or sets the <see cref="ShouldBeLogged"/> value.
        /// </summary>
        public static bool ShouldExceptionBeLogged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException()
            : base(new LText("Beef.ValidationException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified messsage.
        /// </summary>
        /// <param name="message">The message text.</param>
        public ValidationException(string? message)
            : base(message ?? new LText("Beef.ValidationException"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ValidationException(string? message, Exception innerException)
            : base(message ?? new LText("Beef.ValidationException"), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a <see cref="MessageItem"/> list.
        /// </summary>
        /// <param path="messages">The <see cref="MessageItem"/> list.</param>
        public ValidationException(IEnumerable<MessageItem> messages)
            : base(new LText("Beef.ValidationException"))
        {
            if (messages != null)
                Messages.AddRange(messages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a <paramref name="message"/> and <see cref="MessageItem"/> list.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="messages">The <see cref="MessageItem"/> list.</param>
        public ValidationException(string? message, IEnumerable<MessageItem> messages)
            : base(message ?? new LText("Beef.ValidationException"))
        {
            if (messages != null)
                Messages.AddRange(messages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a single <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="MessageItem"/>.</param>
        public ValidationException(MessageItem item)
            : base(new LText("Beef.ValidationException"))
        {
            Check.NotNull(item, nameof(item));
            Messages.Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> with a <paramref name="message"/>, <see cref="MessageItem"/> list and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="messages">The <see cref="MessageItem"/> list.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public ValidationException(string? message, IEnumerable<MessageItem> messages, Exception innerException)
            : base(message ?? new LText("Beef.ValidationException"), innerException)
        {
            if (messages != null)
                Messages.AddRange(messages);
        }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection Messages { get; } = new MessageItemCollection();

        /// <summary>
        /// Gets the <see cref="ErrorType"/> (see <see cref="ErrorType.ValidationError"/>).
        /// </summary>
        public ErrorType ErrorType
        {
            get { return ErrorType.ValidationError; }
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