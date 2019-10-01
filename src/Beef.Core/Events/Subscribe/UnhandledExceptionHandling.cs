﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the unhandled <see cref="Exception"/> options.
    /// </summary>
    public enum UnhandledExceptionHandling
    {
        /// <summary>
        /// Stops and bubbles up the unhandled <see cref="Exception"/> allowing the hosting process to determine the appropriate action.
        /// </summary>
        Stop,

        /// <summary>
        /// Skips and continues effectively swallowing the <see cref="Exception"/>.
        /// </summary>
        Continue,
    }
}