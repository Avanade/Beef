// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Beef.Diagnostics
{
    /// <summary>
    /// Represents the <see cref="ColoredConsoleLogger"/> provider.
    /// </summary>
    public sealed class ColoredConsoleLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ColoredConsoleLogger"/>.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>The <see cref="ColoredConsoleLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new ColoredConsoleLogger(name);

        /// <summary>
        /// Closes and disposes the <see cref="ColoredConsoleLoggerProvider"/>.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents a logger where messages are written to the console.
    /// </summary>
    public sealed class ColoredConsoleLogger : ILogger
    {
        private static readonly LogLevel[] _logLevels = new LogLevel[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        public ColoredConsoleLogger(string name) { _ = name; }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => NullScope.Default;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && _logLevels.Contains(logLevel);

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var text = formatter(state, exception);

            if (exception != null)
            {
                if (text == null)
                    text = exception.ToString();
                else
                    text += Environment.NewLine + exception.ToString();
            }

            ConsoleColor? color = logLevel switch
            {
                LogLevel.Critical => ConsoleColor.Red,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Information => null,
                _ => ConsoleColor.Cyan
            };

            ConsoleWriteLine(text, color);
        }

        /// <summary>
        /// Writes the specified text to the console.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="foregroundColor">The foreground <see cref="ConsoleColor"/>.</param>
        private static void ConsoleWriteLine(string? text = null, ConsoleColor? foregroundColor = null)
        {
            if (string.IsNullOrEmpty(text))
                Console.WriteLine();
            else
            {
                var currColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor ?? currColor;
                Console.WriteLine(text);
                Console.ForegroundColor = currColor;
            }
        }
    }
}