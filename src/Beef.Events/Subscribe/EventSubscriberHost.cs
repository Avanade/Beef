// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.ContinueSilent"/>.
        /// </summary>
        public ResultHandling NotSubscribedHandling { get; set; } = ResultHandling.ContinueSilent;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.Stop"/>.
        /// </summary>
        public ResultHandling DataNotFoundHandling { get; set; } = ResultHandling.Stop;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidEventData"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.Stop"/>.
        /// </summary>
        public ResultHandling InvalidEventDataHandling { get; set; } = ResultHandling.Stop;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidData"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.Stop"/>.
        /// </summary>
        public ResultHandling InvalidDataHandling { get; set; } = ResultHandling.Stop;

        /// <summary>
        /// Gets or sets the subject path seperator <see cref="string"/> (see <see cref="IEventPublisher.PathSeparator"/>).
        /// </summary>
        public string SubjectPathSeparator { get; set; } = ".";

        /// <summary>
        /// Gets or sets the subject template wildcard <see cref="string"/> (see <see cref="IEventPublisher.TemplateWildcard"/>).
        /// </summary>
        public string SubjectTemplateWildcard { get; set; } = "*";

        /// <summary>
        /// Receives the message and processes when the <paramref name="subject"/> and <paramref name="action"/> has been subscribed.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> or <see cref="EventData{T}"/> only performed where subscribed for processing.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        protected async Task<Result> ReceiveAsync(string? subject, string? action, Func<IEventSubscriber, EventData> getEventData)
        {
            if (Args.AuditWriter == null)
                throw new InvalidOperationException("The Args.AuditWriter must be specified; otherwise, ResultHandling.ContinueWithAudit cannot be actioned.");

            if (getEventData == null)
                throw new ArgumentNullException(nameof(getEventData));

            if (string.IsNullOrEmpty(subject))
                return CheckResult(Result.InvalidEventData(null, "EventData is invalid; Subject is required."), null, null, null);

            // Match a subscriber to the subject + template supplied.
            var subscriber = Args.CreateEventSubscriber(SubjectTemplateWildcard, SubjectPathSeparator, subject, action);
            if (subscriber == null)
                return CheckResult(Result.NotSubscribed(), subject, action, null);

            // Where matched get the EventData and execute the subscriber receive.
            EventData @event;
            try
            {
                @event = getEventData(subscriber);
                if (@event == null)
                    return CheckResult(Result.InvalidEventData(null, $"EventData is invalid; is required."), subject, action, subscriber);
            }
#pragma warning disable CA1031 // Do not catch general exception types; by design, need this to be a catch-all.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                // Handle the exception as per the subscriber configuration.
                if (subscriber.UnhandledExceptionHandling == UnhandledExceptionHandling.Continue)
                    return CheckResult(Result.ExceptionContinue(ex, $"An unhandled exception was encountered and ignored as the EventSubscriberHost is configured to continue: {ex.Message}"), subject, action, subscriber);

                return CheckResult(Result.InvalidEventData(ex), subject, action, subscriber);
            }

            try
            {
                // Create and set the execution context for the event.
                ExecutionContext.Reset();
                var ec = Args.ServiceProvider.GetService<ExecutionContext>();
                UpdateExecutionContext(ec, subscriber, @event);
                ec.ServiceProvider = Args.ServiceProvider;
                ec.CorrelationId = @event.CorrelationId;
                ExecutionContext.SetCurrent(ec);

                // Process the event.
                return CheckResult(await subscriber.ReceiveAsync(@event).ConfigureAwait(false), subject, action, subscriber);
            }
            catch (InvalidEventDataException iedex) { return CheckResult(Result.InvalidEventData(iedex), subject, action, subscriber); }
            catch (ValidationException vex) { return CheckResult(Result.InvalidData(vex), subject, action, subscriber); }
            catch (BusinessException bex) { return CheckResult(Result.InvalidData(bex), subject, action, subscriber); }
            catch (NotFoundException) { return CheckResult(Result.DataNotFound(), subject, action, subscriber); }
            catch (EventSubscriberStopException) { throw; }
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
        private Result CheckResult(Result result, string? subject, string? action, IEventSubscriber? subscriber = null)
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

                case SubscriberStatus.InvalidEventData:
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.InvalidEventDataHandling ?? InvalidEventDataHandling);
                    break;

                case SubscriberStatus.NotSubscribed:
                    HandleTheHandling(result, result.ResultHandling ?? NotSubscribedHandling);
                    break;

                case SubscriberStatus.DataNotFound:
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.DataNotFoundHandling ?? DataNotFoundHandling);
                    break;

                case SubscriberStatus.InvalidData:
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.InvalidDataHandling ?? InvalidDataHandling);
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
                    Logger.Create<EventSubscriberHost>().LogWarning(result.ToString());
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
        /// Updates the <see cref="ExecutionContext"/> for processing the <paramref name="event"/> setting the username based on the <see cref="IEventSubscriber"/> <see cref="IEventSubscriber.RunAsUser"/>.
        /// </summary>
        /// <param name="executionContext">The <see cref="ExecutionContext"/> to update.</param>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <remarks>When overridding it is the responsibility of the overridder to honour the <see cref="IEventSubscriber.RunAsUser"/> selection.</remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords; by-design, is the best name.
        protected virtual void UpdateExecutionContext(ExecutionContext executionContext, IEventSubscriber subscriber, EventData @event)
#pragma warning restore CA1716 
        {
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));

            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            executionContext.Username = subscriber.RunAsUser == RunAsUser.Originating ? @event.Username ?? SystemUsername : SystemUsername;
            executionContext.UserId = @event.UserId;
            executionContext.TenantId = @event.TenantId;
        }
    }
}