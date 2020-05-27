// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Represents an <see cref="EventSubscriberHost"/> <see cref="Exception"/> that is is thrown when the <see cref="ResultHandling"/> is <see cref="ResultHandling.Stop"/>.
    /// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors; this is for internal use only and only needs the one constructor.
    public class EventSubscriberStopException : Exception
#pragma warning restore CA1032
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberStopException"/>.
        /// </summary>
        /// <param name="result">The <see cref="Result"/>.</param>
        internal EventSubscriberStopException(Result result) : base(result.ToString()) => Result = result;

        /// <summary>
        /// Gets the <see cref="Result"/>.
        /// </summary>
        public Result Result { get; private set; }
    }
}