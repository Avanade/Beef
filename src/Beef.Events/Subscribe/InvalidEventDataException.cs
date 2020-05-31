// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Thrown where the <see cref="EventData"/> instantiation fails or is invalid; will result in a <see cref="SubscriberStatus.InvalidEventData"/> <see cref="Result"/>.
    /// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors; this is for internal use only and only needs the one constructor.
    public class InvalidEventDataException : Exception
#pragma warning restore CA1032
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEventDataException"/> with an <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        internal InvalidEventDataException(Exception exception) : base(exception.Message, exception) { }
    }
}