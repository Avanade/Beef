// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the base <see cref="EventData"/> subscriber capabilities.
    /// </summary>
    public abstract class EventSubscriberHost
    {
        /// <summary>
        /// Gets or sets the <see cref="RunAsUser.System"/> username (this defaults to <see cref="ExecutionContext"/> <see cref="ExecutionContext.EnvironmentUsername"/>).
        /// </summary>
        public static string SystemUsername { get; set; } = ExecutionContext.EnvironmentUsername;

        /// <summary>
        /// Gets the username based on the <see cref="IEventSubscriber.RunAsUser"/> (see also <see cref="SystemUsername"/>).
        /// </summary>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The username.</returns>
        protected static string GetUsername(IEventSubscriber subscriber, EventData @event)
            => Check.NotNull(subscriber, nameof(subscriber)).RunAsUser == RunAsUser.Originating ? Check.NotNull(@event, nameof(@event)).Username ?? SystemUsername : SystemUsername;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        protected EventSubscriberHost(EventSubscriberHostArgs args) => Args = Check.NotNull(args, nameof(args));

        /// <summary>
        /// Gets the <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        public EventSubscriberHostArgs Args { get; private set; }

        /// <summary>
        /// Indicates whether multiple messages (<see cref="EventData"/>) can be processed; default is <c>false</c>.
        /// </summary>
        public bool AreMultipleMessagesSupported { get; set; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (can be overriden by an <see cref="IEventSubscriber"/>).
        /// </summary>
        public ResultHandling NotSubscribedHandling { get; set; } = ResultHandling.ContinueSilent;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (can be overriden by an <see cref="IEventSubscriber"/>).
        /// </summary>
        public ResultHandling DataNotFoundHandling { get; set; } = ResultHandling.Stop;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidData"/> status (can be overriden by an <see cref="IEventSubscriber"/>).
        /// </summary>
        public ResultHandling InvalidDataHandling { get; set; } = ResultHandling.Stop;

        /// <summary>
        /// Receives the message and processes when the <paramref name="subject"/> and <paramref name="action"/> has been subscribed.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> or <see cref="EventData{T}"/> only performed where subscribed for processing.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        protected async Task<Result> ReceiveAsync(string subject, string? action, Func<IEventSubscriber, EventData> getEventData)
        {
            if (Args.AuditWriter == null)
                throw new InvalidOperationException("The Args.AuditWriter must be specified; otherwise, ResultHandling.ContinueWithAudit cannot be actioned.");

            if (string.IsNullOrEmpty(subject))
                throw new EventSubscriberException("The Event Subject is required.");

            if (getEventData == null)
                throw new ArgumentNullException(nameof(getEventData));

            // Match a subscriber to the subject + template supplied.
            var subscribers = Args.EventSubscribers.Where(r => Event.Match(r.SubjectTemplate, subject) && (r.Actions == null || r.Actions.Count == 0 || r.Actions.Contains(action, StringComparer.InvariantCultureIgnoreCase))).ToArray();
            var subscriber = subscribers.Length == 1 ? subscribers[0] : subscribers.Length == 0 ? (IEventSubscriber?)null : throw new EventSubscriberException($"There are {subscribers.Length} {nameof(IEventSubscriber)} instances subscribing to Subject '{subject}' and Action '{action}'; there must be only a single subscriber.");
            if (subscriber == null)
                return CheckResult(Result.NotSubscribed(), subject, action, null);

            // Where matched get the EventData and execute the subscriber receive.
            var @event = getEventData(subscriber) ?? throw new EventSubscriberException($"An EventData instance must not be null (Subject `{subject}` and Action '{action}').");

            try
            {
                ExecutionContext.Reset(false);
                ExecutionContext.SetCurrent(BindLogger(CreateExecutionContext(subscriber, @event)));
                return CheckResult(await subscriber.ReceiveAsync(@event).ConfigureAwait(false), subject, action, subscriber);
            }
            catch (ValidationException vex) { return CheckResult(Result.InvalidData(vex), subject, action, subscriber); }
            catch (BusinessException bex) { return CheckResult(Result.InvalidData(bex), subject, action, subscriber); }
            catch (NotFoundException) { return CheckResult(Result.DataNotFound(), subject, action, subscriber); }
            catch (Exception ex)
            {
                // Handle the exception as per the subscriber configuration.
                if (subscriber.UnhandledExceptionHandling == UnhandledExceptionHandling.Continue)
                    return CheckResult(Result.ExceptionContinue(ex, $"An unhandled exception was encountered and ignored as the EventSubscriberHost is configured to continue: {ex.Message}"), subject, action, subscriber);

                throw;
            }
        }

        /// <summary>
        /// Checks the <see cref="Result"/> and handles accordingly.
        /// </summary>
        private Result CheckResult(Result result, string subject, string? action, IEventSubscriber? subscriber = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.Subject = subject;
            result.Action = action;
            result.Subscriber = subscriber;

            switch (result.Status)
            {
                case SubscriberStatus.Success: 
                    break;

                case SubscriberStatus.NotSubscribed:
                    HandleTheHandling(result, NotSubscribedHandling);
                    break;

                case SubscriberStatus.DataNotFound:
                    HandleTheHandling(result, subscriber?.DataNotFoundHandling ?? DataNotFoundHandling);
                    break;

                case SubscriberStatus.InvalidData:
                    HandleTheHandling(result, subscriber?.InvalidDataHandling ?? InvalidDataHandling);
                    break;

                case SubscriberStatus.ExceptionContinue:
                    Args.AuditWriter?.Invoke(result);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Handle the result as required.
        /// </summary>
        private void HandleTheHandling(Result result, ResultHandling handling)
        {
            switch (result.ResultHandling ?? handling)
            {
                case ResultHandling.ContinueWithLogging:
                    Logger.Default.Warning(result.ToString());
                    break;

                case ResultHandling.ContinueWithAudit:
                    Args.AuditWriter?.Invoke(result);
                    break;

                case ResultHandling.Stop:
                    throw new EventSubscriberStopException(result);

                case ResultHandling.ContinueSilent:
                default:
                    break;
            }
        }

        /// <summary>
        /// Creates the <see cref="ExecutionContext"/> for processing the <paramref name="event"/> setting the username based on the <see cref="IEventSubscriber"/> <see cref="IEventSubscriber.RunAsUser"/>.
        /// </summary>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="ExecutionContext"/>.</returns>
        /// <remarks>When overridding it is the responsibility of the overridder to honour the <see cref="IEventSubscriber.RunAsUser"/> selection.</remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords; by-design, is the best name.
        protected virtual ExecutionContext CreateExecutionContext(IEventSubscriber subscriber, EventData @event)
#pragma warning restore CA1716 
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return new ExecutionContext { Username = subscriber.RunAsUser == RunAsUser.Originating ? @event.Username ?? SystemUsername : SystemUsername, TenantId = @event.TenantId };
        }

        /// <summary>
        /// Bind the logger to the execution context.
        /// </summary>
        private ExecutionContext BindLogger(ExecutionContext ec)
        {
            if (ec == null)
                throw new EventSubscriberException("An ExecutionContext instance must be returned from CreateExecutionContext.");

            ec.RegisterLogger((largs) => BindLogger(Args.Logger, largs));
            return ec;
        }

        /// <summary>
        /// Binds (redirects) Beef <see cref="Beef.Diagnostics.Logger"/> to the ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.
        /// </summary>
        /// <param name="logger">The ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.</param>
        /// <param name="args">The Beef <see cref="LoggerArgs"/>.</param>
        /// <remarks>Redirects (binds) the Beef logger to the ASP.NET logger.</remarks>
        private static void BindLogger(ILogger logger, LoggerArgs args)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(args, nameof(args));

            switch (args.Type)
            {
                case LogMessageType.Critical:
                    logger.LogCritical(args.ToString());
                    break;

                case LogMessageType.Info:
                    logger.LogInformation(args.ToString());
                    break;

                case LogMessageType.Warning:
                    logger.LogWarning(args.ToString());
                    break;

                case LogMessageType.Error:
                    logger.LogError(args.ToString());
                    break;

                case LogMessageType.Debug:
                    logger.LogDebug(args.ToString());
                    break;

                case LogMessageType.Trace:
                    logger.LogTrace(args.ToString());
                    break;
            }
        }
    }
}