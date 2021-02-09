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
        /// Receives the message and processes when the <paramref name="subject"/> and <paramref name="action"/> has been subscribed.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> or <see cref="EventData{T}"/> only performed where subscribed for processing.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        protected async Task<Result> ReceiveAsync(string? subject, string? action, Func<IEventSubscriber, EventData> getEventData)
        {
            if (Args.AuditWriter == null)
                throw new InvalidOperationException("The Args.AuditWriter must be specified for the ResultHandling.ContinueWithAudit and ResultHandling.ThrowException to function correctly.");

            if (getEventData == null)
                throw new ArgumentNullException(nameof(getEventData));

            if (string.IsNullOrEmpty(subject))
                return CheckResult(Result.InvalidEventData(null, "EventData is invalid; Subject is required."), null, null, null);

            // Match a subscriber to the subject + template supplied.
            var subscriber = Args.CreateEventSubscriber(Args.SubjectTemplateWildcard, Args.SubjectPathSeparator, subject, action);
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
                if (Args.UpdateExecutionContext == null)
                    UpdateExecutionContext(ec, subscriber, @event);
                else
                    Args.UpdateExecutionContext(ec, subscriber, @event);

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
            catch (EventSubscriberUnhandledException) { throw; }
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
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.InvalidEventDataHandling ?? Args.InvalidEventDataHandling);
                    break;

                case SubscriberStatus.NotSubscribed:
                    HandleTheHandling(result, result.ResultHandling ?? Args.NotSubscribedHandling);
                    break;

                case SubscriberStatus.DataNotFound:
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.DataNotFoundHandling ?? Args.DataNotFoundHandling);
                    break;

                case SubscriberStatus.InvalidData:
                    HandleTheHandling(result, result.ResultHandling ?? subscriber?.InvalidDataHandling ?? Args.InvalidDataHandling);
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

                case ResultHandling.ThrowException:
                    throw new EventSubscriberUnhandledException(result);

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
        /// <remarks></remarks>
        private static void UpdateExecutionContext(ExecutionContext executionContext, IEventSubscriber subscriber, EventData @event)
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