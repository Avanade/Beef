// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events
{
    /// <summary>
    /// Thrown where the <see cref="EventData"/> instantiation fails or is invalid; will result in a <see cref="SubscriberStatus.InvalidEventData"/> <see cref="Result"/>.
    /// </summary>
    public class InvalidEventDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEventDataException"/> with an <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        internal InvalidEventDataException(Exception exception) : base(exception.Message, exception) { }
    }
}