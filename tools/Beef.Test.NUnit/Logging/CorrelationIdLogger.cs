// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Represents the <see cref="CorrelationIdLogger"/> provider.
    /// </summary>
    [ProviderAlias("")]
    public sealed class CorrelationIdLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CorrelationIdLogger"/>.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>The <see cref="CorrelationIdLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new CorrelationIdLogger(name);

        /// <summary>
        /// Closes and disposes the <see cref="CorrelationIdLoggerProvider"/>.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents a logger where all messages are written to an internal (in-memory) list by correlation identifier.
    /// </summary>
    public sealed class CorrelationIdLogger : ILogger
    {
        private static readonly ConcurrentDictionary<string, List<(DateTime, string)>> _messageDict = new ConcurrentDictionary<string, List<(DateTime, string)>>();
        private readonly string _name;

        /// <summary>
        /// Gets the messages for the specified <paramref name="correlationId"/> whilst also removing.
        /// </summary>
        /// <param name="correlationId">The correlation identifier (defaults to <see cref="ExecutionContext.CorrelationId"/>).</param>
        /// <param name="includeDefaultIdInGetMessages">Indicates whether to include <see cref="DefaultId"/> messages in the response. Including can result in messages related to one request being reported
        /// in another as there is no way to appropriately correlate. These will be messages that occur before the <see cref="ExecutionContext"/> has been set; therefore should be a limited to internal 
        /// infrastructure messages that are likely less important in the test output log.</param>
        /// <returns>A messages <see cref="string"/> array.</returns>
        public static List<string> GetMessages(string? correlationId = null, bool includeDefaultIdInGetMessages = false)
        {
            var list = new List<(DateTime, string)>();

            if (_messageDict.TryRemove(correlationId ?? ExecutionContext.Current.CorrelationId ?? throw new ArgumentNullException(nameof(correlationId)), out var msgs))
            {
                foreach (var m in msgs)
                {
                    list.Add(m);
                }
            }

            if (_messageDict.TryRemove(DefaultId, out msgs))
            {
                if (includeDefaultIdInGetMessages)
                {
                    foreach (var m in msgs)
                    {
                        list.Add(m);
                    }
                }
            }

            return list.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        }

        /// <summary>
        /// Gets the default identifier used where the <see cref="ExecutionContext.CorrelationId"/> is not defined.
        /// </summary>
        public static string DefaultId { get; } = Guid.Empty.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        public CorrelationIdLogger(string name) => _name = Check.NotEmpty(name, nameof(name));

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

            var id = ExecutionContext.HasCurrent && ExecutionContext.Current.CorrelationId != null ? ExecutionContext.Current.CorrelationId : DefaultId;

            var timestamp = DateTime.Now;
            message = $"{timestamp.ToString("yyyy-MM-ddTHH:mm:ss.ffff", DateTimeFormatInfo.InvariantInfo)} {GetLogLevel(logLevel)}: {message} [{_name}]{(id == DefaultId ? "*" : "")}";

            if (exception != null)
                message += Environment.NewLine + exception;

            var msgs = _messageDict.GetOrAdd(id, new List<(DateTime, string)>());
            msgs.Add((timestamp, message));
        }

        /// <summary>
        /// Gets the shortened log level.
        /// </summary>
        internal static string GetLogLevel(LogLevel level) =>
            level switch
            {
                LogLevel.Critical => "Cri",
                LogLevel.Error => "Err",
                LogLevel.Warning => "Wrn",
                LogLevel.Information => "Inf",
                LogLevel.Trace => "Trc",
                LogLevel.Debug => "Dbg",
                _ => "?",
            };
    }
}