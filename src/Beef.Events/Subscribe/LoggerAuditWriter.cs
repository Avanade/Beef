// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Represents an <see cref="IAuditWriter"/> that writes the audits to the <see cref="Logger"/>.
    /// </summary>
    public class LoggerAuditWriter : IAuditWriter, IUseLogger
    {
        private ILogger? _logger;

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get => _logger ?? throw new InvalidOperationException("The UseLogger method must be invoked to set the Logger before it can be used."); }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.AuditWriter"/> to write the audit information.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public LoggerAuditWriter UseLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logger"><inheritdoc/></param>
        void IUseLogger.UseLogger(ILogger logger) => UseLogger(logger);

        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> using the <see cref="Logger"/>.
        /// </summary>
        /// <param name="originatingEvent">The originating event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        public Task WriteAuditAsync(object originatingEvent, Result result) => WriteFormattedAuditAsync(Logger, originatingEvent, result);

        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> using the <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="originatingEvent">The originating event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <remarks>This provides a reusable/standardized approach to the logging.</remarks>
#pragma warning disable IDE0060, CA1801 // Review unused parameters; want same contract as Interface to avoid possible future breaking change.
        public static Task WriteFormattedAuditAsync(ILogger logger, object originatingEvent, Result result)
#pragma warning restore IDE0060, CA1801
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Exception != null)
            {
                if (result.Status == SubscriberStatus.ExceptionContinue)
                    logger.LogWarning(result.Exception, FormatReasonMessage(result));
                else
                    logger.LogError(result.Exception, FormatReasonMessage(result));
            }
            else
            {
                switch (result.Status)
                {
                    case SubscriberStatus.Success:
                        logger.LogInformation(FormatReasonMessage(result));
                        break;

                    case SubscriberStatus.PoisonMismatch:
                        logger.LogError(FormatReasonMessage(result));
                        break;

                    default:
                        logger.LogWarning(FormatReasonMessage(result));
                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Formats the reason message.
        /// </summary>
        private static string FormatReasonMessage(Result result) => $"Event Subscriber - {result}";
    }
}