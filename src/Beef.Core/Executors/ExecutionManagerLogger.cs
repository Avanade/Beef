// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;

namespace Beef.Executors
{
    /// <summary>
    /// Provides a standardised implementation of a <see cref="ExecutionManager"/> logger; designed for attaching the <see cref="LogExecutionRun"/> to the
    /// <see cref="ExecutionManager.PerRunType"/> action. Intercepts the <see cref="IExecutorArgs"/> and writes applicable log message when an <see cref="IExecutorBaseArgs.Exception"/>
    /// is not <c>null</c>. This will not log where there is no <b>exception</b>. The <see cref="IExecutorArgs"/> is passed as the logging <b>data</b> (passed in the
    /// <see cref="Logger"/> shortcut methods ending in <b>2</b>).
    /// </summary>
    public class ExecutionManagerLogger
    {
        /// <summary>
        /// Gets the <b>default</b> instance.
        /// </summary>
        public static ExecutionManagerLogger Default { get; } = new ExecutionManagerLogger();

        /// <summary>
        /// Gets or sets the message where there was an unexpected <see cref="Exception"/> during the <see cref="ExecutorRunType.Run"/> phase.
        /// </summary>
        public string UnexpectedRunExceptionMessage { get; set; } = "An unexpected exception occurred during the 'Run' phase: {0}";

        /// <summary>
        /// Gets or sets the message where there was an unexpected <see cref="Exception"/> during the <see cref="ExecutorRunType.CollectionRun"/> phase.
        /// </summary>
        public string UnexpectedCollectionRunExceptionMessage { get; set; } = "An unexpected exception occurred during the 'Collection Run' phase: {0}";

        /// <summary>
        /// Gets or sets the message where there was an unexpected <see cref="Exception"/> during the <see cref="ExecutorRunType.CollectionIterate"/> phase.
        /// </summary>
        public string UnexpectedCollectionIterateExceptionMessage { get; set; } = "An unexpected exception occurred during the 'Collection Iterate' phase: {0}";

        /// <summary>
        /// Gets or sets the message where there was an unexpected <see cref="Exception"/> during the <see cref="ExecutorRunType.ItemRun"/> phase.
        /// </summary>
        public string UnexpectedItemRunExceptionMessage { get; set; } = "An unexpected exception occurred during the 'Item Run' phase: {0}";

        /// <summary>
        /// Gets or sets the message where there was an unexpected <see cref="Exception"/> during the <see cref="ExecutorRunType.CompletionRun"/> phase.
        /// </summary>
        public string UnexpectedCompletionRunExceptionMessage { get; set; } = "An unexpected exception occurred during the 'Completion Run' phase: {0}";

        /// <summary>
        /// For use with the <see cref="ExecutionManager.PerRunType"/> action to write standardised log messages.
        /// </summary>
        /// <param name="args">The <see cref="IExecutorArgs"/>.</param>
        public void LogExecutionRun(IExecutorArgs args)
        {
            switch (Check.NotNull(args, nameof(args)).RunType)
            {
                case ExecutorRunType.CollectionRun:
                    if (args.Exception != null)
                        Logger.Default.Exception2(args, args.Exception, string.Format(System.Globalization.CultureInfo.InvariantCulture, UnexpectedCollectionRunExceptionMessage ?? "{0}", args.Exception.Message));

                    break;

                case ExecutorRunType.CollectionIterate:
                    if (args.Exception != null)
                        Logger.Default.Exception2(args, args.Exception, string.Format(System.Globalization.CultureInfo.InvariantCulture, UnexpectedCollectionIterateExceptionMessage ?? "{0}", args.Exception.Message));

                    break;

                case ExecutorRunType.ItemRun:
                    if (args.Exception != null)
                        Logger.Default.Exception2(args, args.Exception, string.Format(System.Globalization.CultureInfo.InvariantCulture, UnexpectedItemRunExceptionMessage ?? "{0}", args.Exception.Message));

                    break;

                case ExecutorRunType.CompletionRun:
                    if (args.Exception != null)
                        Logger.Default.Exception2(args, args.Exception, string.Format(System.Globalization.CultureInfo.InvariantCulture, UnexpectedCompletionRunExceptionMessage ?? "{0}", args.Exception.Message));

                    break;

                case ExecutorRunType.Run:
                    if (args.Exception != null)
                        Logger.Default.Exception2(args, args.Exception, string.Format(System.Globalization.CultureInfo.InvariantCulture, UnexpectedRunExceptionMessage ?? "{0}", args.Exception.Message));

                    break;
            }
        }
    }
}