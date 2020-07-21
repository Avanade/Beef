// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Arguments required for creating an <see cref="EventSubscriberHost"/> instance.
    /// </summary>
    public class EventSubscriberHostArgs
    {
        private readonly List<EventSubscriberConfig> _subscribers;
        private ILogger? _logger;

        /// <summary>
        /// Creates an <see cref="EventSubscriberHostArgs"/> using the specified <paramref name="serviceProvider"/> and <paramref name="subscribersAssembly"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="subscribersAssembly">The <see cref="Assembly"/> where the <see cref="IEventSubscriber"/> types are defined.</param>
        /// <returns>A <see cref="EventSubscriberHostArgs"/>.</returns>
        public static EventSubscriberHostArgs Create(IServiceProvider serviceProvider, Assembly subscribersAssembly) => new EventSubscriberHostArgs(serviceProvider, subscribersAssembly);

        /// <summary>
        /// Creates an <see cref="EventSubscriberHostArgs"/> using the specified <paramref name="serviceProvider"/> and <paramref name="eventSubscriberTypes"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="eventSubscriberTypes">One or more <see cref="IEventSubscriber"/> types.</param>
        /// <returns>A <see cref="EventSubscriberHostArgs"/>.</returns>
        public static EventSubscriberHostArgs Create(IServiceProvider serviceProvider, params Type[] eventSubscriberTypes) => new EventSubscriberHostArgs(serviceProvider, eventSubscriberTypes);

        /// <summary>
        /// Gets all the subscriber types from the specified <paramref name="subscribersAssembly"/>.
        /// </summary>
        /// <param name="subscribersAssembly">The <see cref="Assembly"/> where the <see cref="IEventSubscriber"/> types are defined.</param>
        /// <returns>An array of subscriber types.</returns>
        public static Type[] GetSubscriberTypes(Assembly subscribersAssembly) => GetSubscriberConfig(subscribersAssembly ?? throw new ArgumentNullException(nameof(subscribersAssembly))).Select(x => x.EventSubscriberType).ToArray();

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
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="subscribersAssembly">The <see cref="Assembly"/> where the <see cref="IEventSubscriber"/> types are defined.</param>
        private EventSubscriberHostArgs(IServiceProvider serviceProvider, Assembly subscribersAssembly)
        {
            ServiceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
            _subscribers = GetSubscriberConfig(subscribersAssembly ?? throw new ArgumentNullException(nameof(subscribersAssembly)));

            if (_subscribers.Count == 0)
                throw new ArgumentException($"No {nameof(IEventSubscriber)} instances were found within Assembly '{subscribersAssembly.FullName}'; at least one must exist to enable execution.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> with a specified <paramref name="eventSubscriberTypes"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="eventSubscriberTypes">One or more <see cref="IEventSubscriber"/> types.</param>
        private EventSubscriberHostArgs(IServiceProvider serviceProvider, params Type[] eventSubscriberTypes)
        {
            ServiceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
            if (eventSubscriberTypes.Length == 0)
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
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger => _logger ??= ServiceProvider.GetService<ILogger>();

        /// <summary>
        /// Gets or sets the audit writer. This is invoked where the <see cref="Result"/> has a corresponding <see cref="ResultHandling"/> of <see cref="ResultHandling.ContinueWithAudit"/>.
        /// </summary>
        public Action<Result>? AuditWriter { get; set; }

        /// <summary>
        /// Uses (sets) the <see cref="AuditWriter"/> to write the audit information to the <see cref="ILogger"/>; this should only be used in testing situations.
        /// </summary>
        /// <returns>The <see cref="EventSubscriberHostArgs"/> instance (for fluent-style method chaining).</returns>
        public EventSubscriberHostArgs UseLoggerForAuditing()
        {
            AuditWriter = (result) => Logger.LogWarning($"Subscriber '{result.Subscriber?.GetType()?.Name}' unsuccessful; Event skipped. Status: {result}'");
            return this;
        }

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

            return (IEventSubscriber)ServiceProvider.GetService(type) 
                ?? throw new InvalidOperationException($"Subscriber {type.Name} was unable to be instantiated through the ServiceProvider; please ensure correctly configured.");
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