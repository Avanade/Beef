// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.Events
{
    /// <summary>
    /// Arguments required for creating an <see cref="EventSubscriberHost"/> instance.
    /// </summary>
    public class EventSubscriberHostArgs
    {
        private readonly List<EventSubscriberConfig> _subscribers;
        private IAuditWriter? _auditWriter;

        /// <summary>
        /// Creates an <see cref="EventSubscriberHostArgs"/> using the specified <typeparamref name="TStartup"/> (to infer the underlying subscribers <see cref="Assembly"/>).
        /// </summary>
        /// <typeparam name="TStartup">The startup class whereby all the subscribers reside.</typeparam>
        /// <returns>A <see cref="EventSubscriberHostArgs"/>.</returns>
        public static EventSubscriberHostArgs Create<TStartup>() where TStartup : class => Create(typeof(TStartup).Assembly);

        /// <summary>
        /// Creates an <see cref="EventSubscriberHostArgs"/> using the specified  <paramref name="subscribersAssembly"/>.
        /// </summary>
        /// <param name="subscribersAssembly">The <see cref="Assembly"/> where the <see cref="IEventSubscriber"/> types are defined.</param>
        /// <returns>A <see cref="EventSubscriberHostArgs"/>.</returns>
        public static EventSubscriberHostArgs Create(Assembly subscribersAssembly) => new EventSubscriberHostArgs(subscribersAssembly);

        /// <summary>
        /// Creates an <see cref="EventSubscriberHostArgs"/> using the specified <paramref name="eventSubscriberTypes"/>.
        /// </summary>
        /// <param name="eventSubscriberTypes">One or more <see cref="IEventSubscriber"/> types.</param>
        /// <returns>A <see cref="EventSubscriberHostArgs"/>.</returns>
        public static EventSubscriberHostArgs Create(params Type[] eventSubscriberTypes) => new EventSubscriberHostArgs(eventSubscriberTypes);

        /// <summary>
        /// Gets all the subscriber configuration from the specified assembly.
        /// </summary>
        private static List<EventSubscriberConfig> GetSubscriberConfig(Assembly subscribersAssembly)
        {
            var subscribers = new List<EventSubscriberConfig>();

            foreach (var type in (subscribersAssembly ?? throw new ArgumentNullException(nameof(subscribersAssembly))).GetTypes().Where(x => typeof(IEventSubscriber).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
            {
                var esa = type.GetCustomAttribute<EventSubscriberAttribute>();
                if (esa == null)
                    throw new ArgumentException($"Assembly contains Type '{type.Name}' that implements IEventSubscriber but is not decorated with the required EventSubscriberAttribute.", nameof(subscribersAssembly));

                subscribers.Add(new EventSubscriberConfig(esa.SubjectTemplate, esa.Actions, type));
            }

            return subscribers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> with a specified <paramref name="subscribersAssembly"/>.
        /// </summary>
        private EventSubscriberHostArgs(Assembly subscribersAssembly)
        {
            _subscribers = GetSubscriberConfig(subscribersAssembly ?? throw new ArgumentNullException(nameof(subscribersAssembly)));

            if (_subscribers.Count == 0)
                throw new ArgumentException($"No {nameof(IEventSubscriber)} instances were found within Assembly '{subscribersAssembly.FullName}'; at least one must exist to enable execution.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> with a specified <paramref name="eventSubscriberTypes"/>.
        /// </summary>
        private EventSubscriberHostArgs(params Type[] eventSubscriberTypes)
        {
            if (eventSubscriberTypes == null || eventSubscriberTypes.Length == 0)
                throw new ArgumentException($"At least one event {nameof(IEventSubscriber)} must be specified to enable execution.", nameof(eventSubscriberTypes));

            _subscribers = new List<EventSubscriberConfig>();
            foreach (var type in eventSubscriberTypes)
            {
                if (typeof(IEventSubscriber).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var esa = type.GetCustomAttribute<EventSubscriberAttribute>();
                    if (esa == null)
                        throw new ArgumentException($"Type '{type.Name}' implements IEventSubscriber but is not decorated with the required EventSubscriberAttribute.", nameof(eventSubscriberTypes));

                    _subscribers.Add(new EventSubscriberConfig(esa.SubjectTemplate, esa.Actions, type));
                }
                else
                    throw new ArgumentException($"Type 'type.name' must implement IEventSubscriber and be decorated with the required EventSubscriberAttribute.");
            }
        }

        /// <summary>
        /// Uses (sets) the <see cref="ServiceProvider"/> (this will be automatically set by <i>Beef</i>).
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <returns>he <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public EventSubscriberHostArgs UseServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
            return this;
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        /// Indicates whether multiple messages (<see cref="EventData"/>) can be processed (where supported); default is <c>false</c>.
        /// </summary>
        public bool AreMultipleMessagesSupported { get; set; } = false;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.NotSubscribed"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.ContinueSilent"/>.
        /// </summary>
        public ResultHandling NotSubscribedHandling { get; set; } = ResultHandling.ContinueSilent;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.ThrowException"/>.
        /// </summary>
        public ResultHandling DataNotFoundHandling { get; set; } = ResultHandling.ThrowException;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidEventData"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.ThrowException"/>.
        /// </summary>
        public ResultHandling InvalidEventDataHandling { get; set; } = ResultHandling.ThrowException;

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidData"/> status (can be overriden by an <see cref="IEventSubscriber"/>). Defaults to <see cref="ResultHandling.ThrowException"/>.
        /// </summary>
        public ResultHandling InvalidDataHandling { get; set; } = ResultHandling.ThrowException;

        /// <summary>
        /// Gets or sets the subject path seperator <see cref="string"/> (see <see cref="IEventPublisher.PathSeparator"/>).
        /// </summary>
        public string SubjectPathSeparator { get; set; } = ".";

        /// <summary>
        /// Gets or sets the subject template wildcard <see cref="string"/> (see <see cref="IEventPublisher.TemplateWildcard"/>).
        /// </summary>
        public string SubjectTemplateWildcard { get; set; } = "*";

        /// <summary>
        /// Gets the action that overrides the update of the <see cref="Beef.ExecutionContext"/>.
        /// </summary>
        internal Action<ExecutionContext, IEventSubscriber, EventData>? UpdateExecutionContext { get; private set; }

        /// <summary>
        /// Gets the <see cref="IAuditWriter"/>. Defaults to <see cref="LoggerAuditWriter"/>.
        /// </summary>
        public IAuditWriter AuditWriter { get => _auditWriter ??= new LoggerAuditWriter(); set => _auditWriter = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.AuditWriter"/> to write the audit information.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public EventSubscriberHostArgs UseAuditWriter(IAuditWriter auditWriter)
        {
            AuditWriter = auditWriter;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="AreMultipleMessagesSupported"/> value to <b>true</b>.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (to support fluent-style method chaining).</returns>
        public EventSubscriberHostArgs AllowMultipleMessages()
        {
            AreMultipleMessagesSupported = true;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="InvalidEventDataHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (to support fluent-style method chaining).</returns>
        public EventSubscriberHostArgs InvalidEventData(ResultHandling handling)
        {
            InvalidEventDataHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="NotSubscribedHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (to support fluent-style method chaining).</returns>
        public EventSubscriberHostArgs NotSubscribed(ResultHandling handling)
        {
            NotSubscribedHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="DataNotFoundHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (to support fluent-style method chaining).</returns>
        public EventSubscriberHostArgs DataNotFound(ResultHandling handling)
        {
            DataNotFoundHandling = handling;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="InvalidDataHandling"/> value.
        /// </summary>
        /// <param name="handling">The <see cref="ResultHandling"/> value.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (to support fluent-style method chaining).</returns>
        public EventSubscriberHostArgs InvalidData(ResultHandling handling)
        {
            InvalidDataHandling = handling;
            return this;
        }

        /// <summary>
        /// Provides a means to override the update of the <see cref="Beef.ExecutionContext"/> (is created by using the <see cref="ExecutionContext.ServiceProvider"/> as instantiated by 
        /// <see cref="Beef.ServiceExtensions.AddBeefExecutionContext(IServiceCollection, Func{IServiceProvider, ExecutionContext}?)"/>). The default will update the <see cref="ExecutionContext.Username"/>,
        /// <see cref="ExecutionContext.UserId"/> and <see cref="ExecutionContext.TenantId"/>.
        /// </summary>
        /// <param name="updateExecutionContext">The action to update the <see cref="Beef.ExecutionContext"/>.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance to support fluent-style method chaining.</returns>
        /// <remarks><b>Note:</b> when overriding it is the responsibility of the overridder to honour the <see cref="IEventSubscriber.RunAsUser"/> selection.
        /// <para><b>Note:</b> that the <see cref="ExecutionContext.ServiceProvider"/> and <see cref="ExecutionContext.CorrelationId"/> are set directly by <i>Beef</i>.</para></remarks>
        public EventSubscriberHostArgs ExecutionContext(Action<ExecutionContext, IEventSubscriber, EventData> updateExecutionContext)
        {
            UpdateExecutionContext = Check.NotNull(updateExecutionContext, nameof(updateExecutionContext));
            return this;
        }

        /// <summary>
        /// Gets or sets the maximum number of attempts before the event is automatically skipped.
        /// </summary>
        /// <remarks>This functionality is dependent on the <see cref="EventSubscriberHost"/> providing the functionality to check and action.</remarks>
        public int? MaxAttempts { get; set; }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.MaxAttempts"/>.
        /// </summary>
        /// <param name="maxAttempts">The maxiumum attempts.</param>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public EventSubscriberHostArgs UseMaxAttempts(int? maxAttempts)
        {
            MaxAttempts = maxAttempts;
            return this;
        }

        /// <summary>
        /// Gets a list (array) of all the subscriber <see cref="Type"/>s.
        /// </summary>
        /// <returns>A list (array) of all the subscriber <see cref="Type"/>s.</returns>
        public Type[] GetSubscriberTypes() => _subscribers.Select(x => x.EventSubscriberType).ToArray();

        /// <summary>
        /// Finds and creates the <see cref="IEventSubscriber"/> where found for the <paramref name="subject"/> and <paramref name="action"/>.
        /// </summary>
        /// <param name="subjectTemplateWildcard">The template wildcard (see <see cref="IEventPublisher.TemplateWildcard"/>).</param>
        /// <param name="subjectPathSeparator">The path separator (see <see cref="IEventPublisher.PathSeparator"/>.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="action">The action.</param>
        /// <returns>An instantiated <see cref="IEventSubscriber"/> where found; otherwise, <c>null</c>.</returns>
        internal IEventSubscriber? CreateEventSubscriber(string subjectTemplateWildcard, string subjectPathSeparator, string subject, string? action)
        {
            var subscribers = _subscribers.Where(
                x => EventSubjectMatcher.Match(subjectTemplateWildcard, subjectPathSeparator, x.SubjectTemplate, subject) 
                  && (x.Actions == null || x.Actions.Count == 0 || x.Actions.Contains(action, StringComparer.InvariantCultureIgnoreCase))).Select(x => x.EventSubscriberType).ToArray();

            var type = subscribers.Length == 1 
                ? subscribers[0] 
                : subscribers.Length == 0 ? (Type?)null : throw new EventSubscriberException($"There are {subscribers.Length} {nameof(IEventSubscriber)} instances subscribing to Subject '{subject}' and Action '{action}'; there must be only a single subscriber.");

            if (type == null)
                return null;

            return (IEventSubscriber)(ServiceProvider?.GetService(type) 
                ?? throw new InvalidOperationException($"Subscriber {type.Name} was unable to be instantiated through the ServiceProvider; please ensure correctly configured."));
        }

        /// <summary>
        /// Event subscriber configuration.
        /// </summary>
        private class EventSubscriberConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EventSubscriberConfig"/> class.
            /// </summary>
            public EventSubscriberConfig(string subjectTemplate, List<string> actions, Type eventSubscriberType)
            {
                SubjectTemplate = Check.NotNull(subjectTemplate, nameof(subjectTemplate));
                Actions = Check.NotNull(actions, nameof(actions));
                EventSubscriberType = Check.NotNull(eventSubscriberType, nameof(eventSubscriberType));
            }

            /// <summary>
            /// Gets the <see cref="EventData.Subject"/> template for the event required (subscribing to).
            /// </summary>
            public string SubjectTemplate { get; private set; }

            /// <summary>
            /// Gets the <see cref="EventData.Action"/>(s); where none specified this indicates <i>all</i>.
            /// </summary>
            public List<string> Actions { get; private set; } = new List<string>();

            /// <summary>
            /// Gets or sets the corresponding <see cref="EventSubscriberBase"/> <see cref="Type"/>.
            /// </summary>
            public Type EventSubscriberType { get; private set; }
        }
    }
}