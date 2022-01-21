// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Represents the <see cref="CorrelationIdLogger"/> provider.
    /// </summary>
    [ProviderAlias("CorrelationIdLogger")]
    //[DebuggerStepThrough]
    public sealed class CorrelationIdLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider? _scopeProvider;
        private readonly bool? _includeLoggingScopesInOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdLoggerProvider"/> class.
        /// </summary>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        public CorrelationIdLoggerProvider(bool? includeLoggingScopesInOutput) => _includeLoggingScopesInOutput = includeLoggingScopesInOutput;

        /// <summary>
        /// Creates a new instance of the <see cref="CorrelationIdLogger"/>.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>The <see cref="CorrelationIdLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new CorrelationIdLogger(name, _includeLoggingScopesInOutput, _scopeProvider);

        /// <inheritdoc/>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        /// <summary>
        /// Closes and disposes the <see cref="CorrelationIdLoggerProvider"/>.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents a logger where all messages are written to an internal (in-memory) list by correlation identifier.
    /// </summary>
    //[DebuggerStepThrough]
    public sealed class CorrelationIdLogger : ILogger
    {
        private static readonly ConcurrentDictionary<string, List<(DateTime, string)>> _messageDict = new ConcurrentDictionary<string, List<(DateTime, string)>>();
        private readonly string _name;
        private readonly bool? _includeLoggingScopesInOutput;
        private readonly IExternalScopeProvider _scopeProvider;

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
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <param name="scopeProvider">The <see cref="IExternalScopeProvider"/>.</param>
        public CorrelationIdLogger(string name, bool? includeLoggingScopesInOutput, IExternalScopeProvider? scopeProvider = null)
        {
            _name = Check.NotEmpty(name, nameof(name));
            _includeLoggingScopesInOutput = includeLoggingScopesInOutput;
            _scopeProvider = scopeProvider ?? new LoggerExternalScopeProvider();
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);

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
            var sb = new StringBuilder();
            sb.Append($"{timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffff", DateTimeFormatInfo.InvariantInfo)} {GetLogLevel(logLevel)}: {message} [{_name}]{(id == DefaultId ? "*" : "")}");

            if ((_includeLoggingScopesInOutput.HasValue && _includeLoggingScopesInOutput.Value) || (!_includeLoggingScopesInOutput.HasValue && TestSetUp.IncludeLoggingScopesInOutput))
                _scopeProvider?.ForEachScope<object>((scope, _) => ScopeWriter(sb, scope), null!);

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append(exception);
            }

            var msgs = _messageDict.GetOrAdd(id, new List<(DateTime, string)>());
            msgs.Add((timestamp, sb.ToString()));
        }

        /// <summary>
        /// Write out the scope content.
        /// </summary>
        internal static void ScopeWriter(StringBuilder sb, object? scope)
        {
            if (scope == null)
                return;

            if (scope is IEnumerable<KeyValuePair<string, object>> dict && dict.Any())
            {
                if (dict.Count() == 1 && dict.First().Key == "{OriginalFormat}")
                    return;

                bool first = true;
                sb.Append(" >");
                foreach (var kv in dict)
                {
                    if (kv.Key != "{OriginalFormat}")
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(',');

                        sb.Append($" {kv.Key ?? "<null>"}=\"{kv.Value ?? "<null>"}\"");
                    }
                }
            }
            else
                sb.Append($" > {scope}");
        }

        /// <summary>
        /// Gets the shortened log level.
        /// </summary>
        internal static string GetLogLevel(LogLevel level) => level switch
        {
            LogLevel.Critical => "crit",
            LogLevel.Error => "fail",
            LogLevel.Warning => "warn",
            LogLevel.Information => "info",
            LogLevel.Debug => "dbug",
            LogLevel.Trace => "trce",
            _ => "???"
        };
    }
}