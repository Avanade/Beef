// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Globalization;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Represents the <see cref="TestContextLogger"/> provider.
    /// </summary>
    [ProviderAlias("")]
    public sealed class TestContextLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TestContextLogger"/>.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>The <see cref="TestContextLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new TestContextLogger(name);

        /// <summary>
        /// Closes and disposes the <see cref="TestContextLoggerProvider"/>.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents a logger where all messages are written to an internal (in-memory) list by correlation identifier.
    /// </summary>
    public sealed class TestContextLogger : ILogger
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContextLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        public TestContextLogger(string name) => _name = Check.NotEmpty(name, nameof(name));

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => NullScope.Default;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
                return;

            message = $"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff", DateTimeFormatInfo.InvariantInfo)} {CorrelationIdLogger.GetLogLevel(logLevel)}: {message} [{_name}]";

            if (exception != null)
                message += Environment.NewLine + exception;

            TestContext.Out.WriteLine(message);
        }
    }
}