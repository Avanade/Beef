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
        /// Initializes a new instance of the <see cref="EventSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        protected EventSubscriberHost(EventSubscriberHostArgs args = null) => Args = Check.NotNull(args, nameof(args));

        /// <summary>
        /// Gets the <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        public EventSubscriberHostArgs Args { get; private set; }

        /// <summary>
        /// Indicates whether multiple messages (<see cref="EventData"/>) can be processed; default is <c>false</c>.
        /// </summary>
        public bool AreMultipleMessagesSupported { get; set; }

        /// <summary>
        /// Receives the message and processes when the <paramref name="subject"/> and <paramref name="action"/> has been subscribed.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> or <see cref="EventData{T}"/> only performed where subscribed for processing.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected async Task ReceiveAsync(string subject, string action, Func<IEventSubscriber, EventData> getEventData)
        {
            Check.NotEmpty(subject, nameof(subject));
            if (getEventData == null)
                throw new ArgumentNullException(nameof(getEventData));

            // Match a subscriber to the subject + template supplied.
            var subscribers = Args.EventSubscribers.Where(r => Event.Match(r.SubjectTemplate, subject) && (r.Actions == null || r.Actions.Count == 0 || r.Actions.Contains(action, StringComparer.InvariantCultureIgnoreCase))).ToArray();
            var subscriber = subscribers.Length == 1 ? subscribers[0] : subscribers.Length == 0 ? (IEventSubscriber)null : throw new EventSubscriberException($"There are {subscribers.Length} {nameof(IEventSubscriber)} instances subscribing to Subject '{subject}' and Action '{action}'; there must be only a single subscriber.");
            if (subscriber == null)
                return;

            // Where matched get the EventData and execute the subscriber receive.
            var @event = getEventData(subscriber) ?? throw new EventSubscriberException($"An EventData instance must not be null (Subject `{subject}` and Action '{action}').");

            try
            {
                ExecutionContext.Reset(false);
                ExecutionContext.SetCurrent(BindLogger(CreateExecutionContext(subscriber, @event)));
                await subscriber.ReceiveAsync(@event);
            }
            catch (Exception ex)
            {
                // Handle the exception as per the subscriber configuration.
                if (subscriber.UnhandledExceptionHandling == UnhandledExceptionHandling.Continue)
                {
                    Logger.Default.Warning($"An unhandled exception was encountered and ignored as the EventSubscriberHost is configured to continue: {ex.ToString()}");
                    return;
                }

                throw;
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

            return new ExecutionContext { Username = subscriber.RunAsUser == RunAsUser.Originating ? @event.Username : SystemUsername, TenantId = @event.TenantId };
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