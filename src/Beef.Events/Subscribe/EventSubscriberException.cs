// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Represents an event subscriber <see cref="Exception"/>.
    /// </summary>
    public class EventSubscriberException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberException"/>.
        /// </summary>
        public EventSubscriberException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberException"/> with a specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public EventSubscriberException(string message) : base(message) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberException"/> with a specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public EventSubscriberException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}