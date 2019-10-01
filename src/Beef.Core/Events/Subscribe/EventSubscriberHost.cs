// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Reflection;
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
        /// <param name="args">The optional <see cref="EventSubscriberHostArgs"/>.</param>
        public EventSubscriberHost(EventSubscriberHostArgs args = null) => Args = args ?? new EventSubscriberHostArgs(Assembly.GetCallingAssembly());

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
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> / <see cref="EventData{T}"/> where subscribed for processing.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected async Task ReceiveAsync(string subject, string action, Func<EventData> getEventData)
        {
            // Match a subscriber to the subject + template.
            var subscribers = Args.EventSubscribers.Where(r => Event.Match(r.SubjectTemplate, subject) && (action == null || StringComparer.InvariantCultureIgnoreCase.Compare(r.Action, action) == 0)).ToArray();
            var subscriber = subscribers.Length == 1 ? subscribers[0] : subscribers.Length == 0 ? (IEventSubscriber)null : throw new EventSubscriberException($"There are {subscribers.Length} {nameof(IEventSubscriber)} instances subscribing to Subject `{subject}` and Action '{action}'; there can be no more than 1 subscriber.");
            if (subscriber == null)
                return;

            // Where matched get the EventData and execute the subscriber receive.
            var @event = getEventData() ?? throw new EventSubscriberException($"An EventData instance must not be null (Subject `{subject}` and Action '{action}').");

            try
            {
                ExecutionContext.Reset(false);
                ExecutionContext.SetCurrent(CreateExecutionContext(subscriber, @event));

                await subscriber.ReceiveAsync(@event);
            }
            catch (Exception)
            {
                // Handle the exception as per the subscriber configuration.
                if (subscriber.UnhandledExceptionHandling == UnhandledExceptionHandling.Continue)
                    return;

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
        protected virtual ExecutionContext CreateExecutionContext(IEventSubscriber subscriber, EventData @event)
        {
            var ec = new ExecutionContext { TenantId = @event.TenantId };
            if (subscriber.RunAsUser == RunAsUser.Originating)
                ec.Username = @event.Username;

            return new ExecutionContext { Username = subscriber.RunAsUser == RunAsUser.Originating ? @event.Username : SystemUsername, TenantId = @event.TenantId };
        }
    }
}