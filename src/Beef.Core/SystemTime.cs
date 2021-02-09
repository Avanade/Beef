// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef
{
    /// <summary>
    /// Provides the system time in UTC.
    /// </summary>
    public class SystemTime : ISystemTime
    {
        /// <summary>
        /// Gets the <see cref="ISystemTime"/> instance from the <paramref name="serviceProvider"/> where configured; otherwise, returns a new instance of <see cref="SystemTime"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <returns>The <see cref="ISystemTime"/> using the <paramref name="serviceProvider"/> where configured; otherwise, a new instance of <see cref="SystemTime"/>.</returns>
        public static ISystemTime Get(IServiceProvider serviceProvider) => Check.NotNull(serviceProvider, nameof(serviceProvider)).GetService<ISystemTime>() ?? new SystemTime();

        /// <summary>
        /// Gets the current system time in UTC.
        /// </summary>
        /// <remarks>The time is set during instantiation and remains constant for lifetime.</remarks>
        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}