// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Assembly, IEnumerable<IEventSubscriber>> _subscribers = new ConcurrentDictionary<Assembly, IEnumerable<IEventSubscriber>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> using the <see cref="Assembly.GetCallingAssembly"/> to find the <see cref="IEventSubscriber"/> types.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public EventSubscriberHostArgs(ILogger logger) : this(logger, Assembly.GetCallingAssembly()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> with a specified <paramref name="subscribersAssembly"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="subscribersAssembly">The <see cref="Assembly"/> where the <see cref="IEventSubscriber"/> types are defined.</param>
        public EventSubscriberHostArgs(ILogger logger, Assembly subscribersAssembly)
        {
            Logger = Check.NotNull(logger, nameof(logger));
            Check.NotNull(subscribersAssembly, nameof(subscribersAssembly));

            EventSubscribers = _subscribers.GetOrAdd(subscribersAssembly, (assembly) =>
            {
                var list = new List<IEventSubscriber>();

                foreach (var type in assembly.GetTypes().Where(x => typeof(IEventSubscriber).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    list.Add((IEventSubscriber)Activator.CreateInstance(type));
                }

                if (list.Count == 0)
                    throw new ArgumentException($"No {nameof(IEventSubscriber)} instances were found within Assembly '{assembly.FullName}'; at least one must exist to enable execution.");

                return list;
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHostArgs"/> with a specified <paramref name="eventSubscribers"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="eventSubscribers">One or more <see cref="IEventSubscriber"/> instances.</param>
        public EventSubscriberHostArgs(ILogger logger, params IEventSubscriber[] eventSubscribers)
        {
            Logger = Check.NotNull(logger, nameof(logger));
            Check.NotNull(eventSubscribers, nameof(eventSubscribers));
            Check.IsTrue(eventSubscribers.Any(), nameof(eventSubscribers), $"At least one {nameof(IEventSubscriber)} instance must be specified to enable execution.");
            EventSubscribers = eventSubscribers;
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the list of specific <see cref="IEventSubscriber"/> instances.
        /// </summary>
        public IEnumerable<IEventSubscriber> EventSubscribers { get; private set; }
    }
}