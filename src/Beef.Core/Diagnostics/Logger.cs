// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Diagnostics
{
    /// <summary>
    /// Provides the standard mechanism for writing log messages.
    /// </summary>
    public sealed class Logger
    {
        private static bool _overriddenGlobalLogger = false;
        private static Logger _globalLogger = new Logger();

        private readonly Action<LoggerArgs> _binder;

        /// <summary>
        /// Gets or sets the enabled <see cref="LogMessageType"/> values (defaults to <see cref="LogMessageType.Default"/>) that apply to all logger instances.
        /// </summary>
        public static LogMessageType EnabledLogMessageTypes { get; set; } = LogMessageType.Default;

        /// <summary>
        /// Gets or sets the <see cref="LogMessageType"/> for an <see cref="T:Exception"/> (defaults to <see cref="LogMessageType.Critical"/>) that applies to all logger instances.
        /// </summary>
        public static LogMessageType ExceptionLogMessageType { get; set; } = LogMessageType.Critical;

        /// <summary>
        /// Determines whether the <see cref="LogMessageType"/> is enabled (see <see cref="EnabledLogMessageTypes"/>).
        /// </summary>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <returns><c>true</c> if enabled; otherwise, <c>false</c>.</returns>
        public static bool IsEnabled(LogMessageType type)
        {
            return ((EnabledLogMessageTypes & type) == type);
        }

        /// <summary>
        /// Registers the global fallback <see cref="Logger"/> instance (used where the <see cref="ExecutionContext"/> <see cref="ExecutionContext.RegisterLogger(Action{LoggerArgs})"/> has not been performed).
        /// </summary>
        /// <param name="binder">The action that binds the logger to an underlying logging capability.</param>
        /// <remarks>The global logger can only be set once; further requests will be ignored.</remarks>
        public static void RegisterGlobal(Action<LoggerArgs> binder = null)
        {
            if (!_overriddenGlobalLogger)
            {
                _globalLogger = new Logger(binder);
                _overriddenGlobalLogger = true;
            }
        }

        /// <summary>
        /// Gets the default <see cref="Logger"/> (either the <see cref="ExecutionContext"/> <see cref="ExecutionContext.RegisterLogger(Action{LoggerArgs})"/> instance
        /// or the global fallback as per the <see cref="Logger"/> <see cref="RegisterGlobal"/>). 
        /// </summary>
        public static Logger Default => ExecutionContext.Current.Logger ?? _globalLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="binder">The action that binds the logger to an underlying logging capability.</param>
        internal Logger(Action<LoggerArgs> binder = null)
        {
            _binder = binder;
        }

        /// <summary>
        /// Writes a log message with <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="text">The message text.</param>
        private void Write(object data, LogMessageType type, string text)
        {
            if (IsEnabled(type) && _binder != null)
                _binder.Invoke(new LoggerArgs(this, data, type, text));
        }

        /// <summary>
        /// Writes a log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <remarks>Use this instead of performing compositing prior as this will defer this expensive operation for when it is acutally required.</remarks>
        private void Write(object data, LogMessageType type, string format, params object[] values)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            if (_binder != null && IsEnabled(type))
                _binder.Invoke(new LoggerArgs(this, data, type, format, values));
        }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/>.</param>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="type">The <see cref="LogMessageType"/> (defaults to <see cref="Logger.ExceptionLogMessageType"/>).</param>
        /// <param name="format">The message text; or composite format string where <paramref name="values"/> provided.</param>
        /// <param name="values">The values that form part of the message text.</param>
        private void Write(Exception exception, object data, LogMessageType type, string format, params object[] values)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception)); 

            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            var t = type == LogMessageType.None ? Logger.ExceptionLogMessageType : type;
            if (_binder != null && IsEnabled(t))
            {
                var e = (values == null || values.Length == 0) ? new LoggerArgs(this, data, t, format) : new LoggerArgs(this, data, t, format, values);
                e.Exception = exception;
                _binder.Invoke(e);
            }
        }

        #region Shortcuts

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Critical(string text) { Write(null, LogMessageType.Critical, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Critical(string format, params object[] values) { Write(null, LogMessageType.Critical, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Critical2(object data, string text) { Write(data, LogMessageType.Critical, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Critical2(object data, string format, params object[] values) { Write(data, LogMessageType.Critical, format, values); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        public void Exception(Exception exception) { Write(exception, null, ExceptionLogMessageType, exception.Message); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message with a specified <paramref name="text"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="text">The message text.</param>
        public void Exception(Exception exception, string text) { Write(exception, null, ExceptionLogMessageType, text); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message using a composite format string.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Exception(Exception exception, string format, params object[] values) { Write(exception, null, ExceptionLogMessageType, format, values); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        public void Exception2(object data, Exception exception) { Write(exception, data, ExceptionLogMessageType, exception.Message); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message with a specified <paramref name="text"/>.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="text">The message text.</param>
        public void Exception2(object data, Exception exception, string text) { Write(exception, data, ExceptionLogMessageType, text); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Exception2(object data, Exception exception, string format, params object[] values) { Write(exception, data, ExceptionLogMessageType, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Error(string text) { Write(null, LogMessageType.Error, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Error(string format, params object[] values) { Write(null, LogMessageType.Error, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Error2(object data, string text) { Write(data, LogMessageType.Error, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Error2(object data, string format, params object[] values) { Write(data, LogMessageType.Error, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Warning(string text) { Write(null, LogMessageType.Warning, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Warning(string format, params object[] values) { Write(null, LogMessageType.Warning, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Warning2(object data, string text) { Write(data, LogMessageType.Warning, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Warning2(object data, string format, params object[] values) { Write(data, LogMessageType.Warning, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Info(string text) { Write(null, LogMessageType.Info, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Info(string format, params object[] values) { Write(null, LogMessageType.Info, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Info2(object data, string text) { Write(data, LogMessageType.Info, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Info2(object data, string format, params object[] values) { Write(data, LogMessageType.Info, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Debug(string text) { Write(null, LogMessageType.Debug, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Debug(string format, params object[] values) { Write(null, LogMessageType.Debug, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Debug2(object data, string text) { Write(data, LogMessageType.Debug, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Debug2(object data, string format, params object[] values) { Write(data, LogMessageType.Debug, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Trace(string text) { Write(null, LogMessageType.Trace, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Trace(string format, params object[] values) { Write(null, LogMessageType.Trace, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="text">The message text.</param>
        public void Trace2(object data, string text) { Write(data, LogMessageType.Trace, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message using a composite format string.
        /// </summary>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Trace2(object data, string format, params object[] values) { Write(data, LogMessageType.Trace, format, values); }

        #endregion
    }

    /// <summary>
    /// The <see cref="Logger"/> arguments used for the binder (see <see cref="Logger.RegisterGlobal(Action{LoggerArgs})"/> and <see cref="ExecutionContext.RegisterLogger(Action{LoggerArgs})"/>).
    /// </summary>
    /// <remarks>Leverage the <see cref="ExecutionContext"/> to get access to the likes of the correlation identifier and user name.</remarks>
    public class LoggerArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerArgs"/> class for a specified <paramref name="type"/> and <paramref name="text"/>.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> that created this instance.</param>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="text">The message text.</param>
        public LoggerArgs(Logger logger, object data, LogMessageType type, string text)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Data = data;
            Type = type;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerArgs"/> class for a specified <paramref name="type"/> and composite format string.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> that created this instance.</param>
        /// <param name="data">Additional contextual data.</param>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public LoggerArgs(Logger logger, object data, LogMessageType type, string format, params object[] values)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Data = data;
            Type = type;
            Format = format;
            Values = values;
            IsCompositeFormat = true;
        }

        /// <summary>
        /// Gets the <see cref="Logger"/> that created this instance.
        /// </summary>
        public Logger Logger { get; }

        /// <summary>
        /// Gets the <see cref="LogMessageType"/>.
        /// </summary>
        public LogMessageType Type { get; }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the message composite format.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// Gets the composite format values.
        /// </summary>
        public object[] Values { get; private set; }

        /// <summary>
        /// Indicates whether the composite <see cref="Format"/> and <see cref="Values"/> were specified; or simply the <see cref="Text"/>.
        /// </summary>
        public bool IsCompositeFormat { get; private set; } = false;

        /// <summary>
        /// Gets the <see cref="Exception"/>.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Indicates whether the result of an <see cref="Exception"/>.
        /// </summary>
        public bool IsException { get => Exception != null; }

        /// <summary>
        /// Gets the additional contextual data.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Returns the final (composited) output <see cref="string"/> representation (includes <see cref="Exception"/> content if <see cref="IsException"/>).
        /// </summary>
        /// <returns>The output <see cref="string"/>.</returns>
        public override string ToString()
        {
            var str = (IsCompositeFormat) ? string.Format(Format, Values) : Text;
            return IsException ? $"{str} Exception: {Exception.ToString()}" : str;
        }

        /// <summary>
        /// Overrides the <see cref="Text"/>.
        /// </summary>
        /// <param name="text">The new text.</param>
        public void OverrideText(string text)
        {
            Text = text;
            Format = null;
            IsCompositeFormat = false;
        }

        /// <summary>
        /// Overrides the composite format string (<see cref="Format"/> and <see cref="Values"/>).
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void OverrideText(string format, params object[] values)
        {
            Format = format;
            Values = values;
            IsCompositeFormat = true;
            Text = null;
        }
    }

    /// <summary>
    /// Represents the <see cref="Logger"/> message type.
    /// </summary>
    [Flags()]
    public enum LogMessageType
    {
        /// <summary>Indicates no messages (ignore).</summary>
        None = 0,

        /// <summary>Indicates a critical message.</summary>
        Critical = 1,

        /// <summary>Indicates an error message.</summary>
        Error = 2,

        /// <summary>Indicates a warning message.</summary>
        Warning = 4,

        /// <summary>Indicates an informational message.</summary>
        Info = 8,

        /// <summary>Indicates a debug message.</summary>
        Debug = 16,

        /// <summary>Indicates a trace message.</summary>
        Trace = 32,

        /// <remarks>All messages.</remarks>
        All = LogMessageType.Critical | LogMessageType.Error | LogMessageType.Warning | LogMessageType.Info | LogMessageType.Debug | LogMessageType.Trace,

        /// <summary>Default messages (<see cref="LogMessageType.Critical"/>, <see cref="LogMessageType.Error"/>, <see cref="LogMessageType.Warning"/> and <see cref="LogMessageType.Info"/>.</summary>
        Default = LogMessageType.Critical | LogMessageType.Error | LogMessageType.Warning | LogMessageType.Info
    }
}
