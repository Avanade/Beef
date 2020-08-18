// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Provides the logging extensions.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Adds the <see cref="CorrelationIdLogger"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddCorrelationId(this ILoggingBuilder builder)
        {
            Check.NotNull(builder, nameof(builder)).Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CorrelationIdLoggerProvider>());
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TestContextLogger"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddTestContext(this ILoggingBuilder builder)
        {
            Check.NotNull(builder, nameof(builder)).Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestContextLoggerProvider>());
            return builder;
        }
    }
}