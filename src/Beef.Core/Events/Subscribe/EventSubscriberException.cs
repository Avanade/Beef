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
        /// Initializes a new instance of the <see cref="EventSubscriberException"/> with a specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public EventSubscriberException(string message) : base(message) { }
    }
}
