// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Provides the logging extensions.
    /// </summary>
    [DebuggerStepThrough]
    public static class LoggingExtensions
    {
        /// <summary>
        /// Adds the <see cref="CorrelationIdLogger"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <returns>The <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddCorrelationId(this ILoggingBuilder builder, bool? includeLoggingScopesInOutput = null)
        {
            Check.NotNull(builder, nameof(builder)).Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CorrelationIdLoggerProvider>(_ => new CorrelationIdLoggerProvider(includeLoggingScopesInOutput)));
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TestContextLogger"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <returns>The <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddTestContext(this ILoggingBuilder builder, bool? includeLoggingScopesInOutput = null)
        {
            Check.NotNull(builder, nameof(builder)).Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestContextLoggerProvider>(_ => new TestContextLoggerProvider(includeLoggingScopesInOutput)));
            return builder;
        }
    }
}