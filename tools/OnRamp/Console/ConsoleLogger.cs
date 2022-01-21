// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;

namespace OnRamp.Console
{
    /// <summary>
    /// Represents an <see cref="ILogger"/> that writes to an <see cref="IConsole"/>.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly IConsole _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/>.
        /// </summary>
        /// <param name="console"></param>
        public ConsoleLogger(IConsole console) => _console = console ?? throw new ArgumentNullException(nameof(console));

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => NullScope.Default;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (exception != null)
                message += Environment.NewLine + exception;

            var foregroundColor = _console.ForegroundColor;

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    _console.ForegroundColor = ConsoleColor.Red;
                    _console.Error.WriteLine(message);
                    break;

                case LogLevel.Warning:
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Out.WriteLine(message);
                    break;

                default:
                    _console.Out.WriteLine(message);
                    break;
            }

            _console.ForegroundColor = foregroundColor;
        }

        /// <summary>
        /// Represents a null scope for loggers.
        /// </summary>
        private sealed class NullScope : IDisposable
        {
            /// <summary>
            /// Gets the default instance.
            /// </summary>
            public static NullScope Default { get; } = new NullScope();

            /// <summary>
            /// Initializes a new instance of the <see cref="NullScope"/> class.
            /// </summary>
            private NullScope() { }

            /// <summary>
            /// Closes and disposes the <see cref="NullScope"/>.
            /// </summary>
            public void Dispose() { }
        }
    }
}