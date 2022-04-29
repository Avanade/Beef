// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Beef.Test.NUnit.Logging
{
    /// <summary>
    /// Represents the <see cref="TestContextLogger"/> provider.
    /// </summary>
    [ProviderAlias("TestContextLogger")]
    [DebuggerStepThrough]
    public sealed class TestContextLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly bool? _includeLoggingScopesInOutput;
        private IExternalScopeProvider? _scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContextLoggerProvider"/> class.
        /// </summary>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        public TestContextLoggerProvider(bool? includeLoggingScopesInOutput) => _includeLoggingScopesInOutput = includeLoggingScopesInOutput;

        /// <summary>
        /// Creates a new instance of the <see cref="TestContextLogger"/>.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>The <see cref="TestContextLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new TestContextLogger(name, _includeLoggingScopesInOutput, _scopeProvider);

        /// <inheritdoc/>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        /// <summary>
        /// Closes and disposes the <see cref="TestContextLoggerProvider"/>.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents a logger where all messages are written directly to <see cref="TestContext.Out"/>.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class TestContextLogger : ILogger
    {
        private readonly string _name;
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly bool? _includeLoggingScopesInOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContextLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <param name="scopeProvider">The <see cref="IExternalScopeProvider"/>.</param>
        public TestContextLogger(string name, bool? includeLoggingScopesInOutput = null, IExternalScopeProvider ? scopeProvider = null)
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
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
                return;

            var sb = new StringBuilder();
            sb.Append($"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffff", DateTimeFormatInfo.InvariantInfo)} {CorrelationIdLogger.GetLogLevel(logLevel)}: {message} [{_name}]");

            if ((_includeLoggingScopesInOutput.HasValue && _includeLoggingScopesInOutput.Value) || (!_includeLoggingScopesInOutput.HasValue && TestSetUp.IncludeLoggingScopesInOutput))
                _scopeProvider?.ForEachScope<object>((scope, _) => CorrelationIdLogger.ScopeWriter(sb, scope), null!);

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append(exception);
            }

            TestContext.Out.WriteLine(sb.ToString());
        }
    }
}