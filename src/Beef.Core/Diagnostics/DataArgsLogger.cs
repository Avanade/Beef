// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Diagnostics
{
    /// <summary>
    /// Utility for invoking the <see cref="Logger.Default"/> <see cref="Logger"/> invoking the corresponding ends-in-<b>2</b> methods passing the configured <see cref="DataArgs"/>.
    /// </summary>
    public class DataArgsLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataArgsLogger"/> class.
        /// </summary>
        /// <param name="dataArgs">The data arguments.</param>
        internal DataArgsLogger(object dataArgs) { DataArgs = dataArgs ?? throw new ArgumentNullException(nameof(dataArgs)); }

        /// <summary>
        /// Gets the data arguments.
        /// </summary>
        public object DataArgs { get; private set; }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Critical(string text) { Logger.Default.Critical2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Critical"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Critical(string format, params object[] values) { Logger.Default.Critical2(DataArgs, format, values); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        public void Exception(Exception exception) { Logger.Default.Exception2(DataArgs, exception); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message with a specified <paramref name="text"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="text">The message text.</param>
        public void Exception(Exception exception, string text) { Logger.Default.Exception2(DataArgs, exception, text); }

        /// <summary>
        /// Writes an <see cref="System.Exception"/> log message using a composite format string.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/></param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Exception(Exception exception, string format, params object[] values) { Logger.Default.Exception2(DataArgs, exception, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Error(string text) { Logger.Default.Error2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Error"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Error(string format, params object[] values) { Logger.Default.Error2(DataArgs, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Warning(string text) { Logger.Default.Warning2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Warning"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Warning(string format, params object[] values) { Logger.Default.Warning2(DataArgs, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Info(string text) { Logger.Default.Info2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Info"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Info(string format, params object[] values) { Logger.Default.Info2(DataArgs, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Debug(string text) { Logger.Default.Debug2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Debug"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Debug(string format, params object[] values) { Logger.Default.Debug2(DataArgs, format, values); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void Trace(string text) { Logger.Default.Trace2(DataArgs, text); }

        /// <summary>
        /// Writes an <see cref="LogMessageType.Trace"/> log message using a composite format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        public void Trace(string format, params object[] values) { Logger.Default.Trace2(DataArgs, format, values); }
    }
}
