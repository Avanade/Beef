// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the shared/base capabilities for an <see cref="EventData"/> subscriber with no corresponding value.
    /// </summary>
    public abstract class EventSubscriberBase : IEventSubscriber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberBase"/> class.
        /// </summary>
        /// <param name="subjectTemplate">The <see cref="EventData.Subject"/> template for the event required (can contain wildcard).</param>
        /// <param name="action">The <see cref="EventData.Action"/>; where <b>null</b> this indicates all.</param>
        protected EventSubscriberBase(string subjectTemplate, string action = null)
        {
            SubjectTemplate = Check.NotEmpty(subjectTemplate, nameof(subjectTemplate));
            Action = action;
        }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> template for the event required (subscribing to).
        /// </summary>
        public string SubjectTemplate { get; private set; }

        /// <summary>
        /// Gets the <see cref="EventData.Action"/>; where <b>null</b> this indicates all.
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// Determines the <see cref="Subscribe.RunAsUser"/>; defaults to <see cref="RunAsUser.Originating"/>.
        /// </summary>
        public RunAsUser RunAsUser { get; protected set; } = RunAsUser.Originating;

        /// <summary>
        /// Determines the <see cref="Subscribe.UnhandledExceptionHandling"/> option; defaults to <see cref="UnhandledExceptionHandling.Stop"/>.
        /// </summary>
        public UnhandledExceptionHandling UnhandledExceptionHandling { get; protected set; } = UnhandledExceptionHandling.Stop;

        /// <summary>
        /// Gets the value <see cref="Type"/>; always <c>null</c> as there will not be one.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Receive and process the <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public abstract Task ReceiveAsync(EventData eventData);
    }

    /// <summary>
    /// Provides the base capabilities for an <see cref="EventData"/> subscriber with no corresponding value.
    /// </summary>
    public abstract class EventSubscriber : EventSubscriberBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriber"/> class.
        /// </summary>
        /// <param name="subjectTemplate">The <see cref="EventData.Subject"/> template for the event required (can contain wildcard).</param>
        /// <param name="action">The <see cref="EventData.Action"/>; where <b>null</b> this indicates all.</param>
        public EventSubscriber(string subjectTemplate, string action = null) : base(subjectTemplate, action) { }

        /// <summary>
        /// Gets the value <see cref="Type"/>; always <c>null</c> as there will not be one.
        /// </summary>
        public override Type ValueType { get => null; }

        /// <summary>
        /// Receive and process the <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public override abstract Task ReceiveAsync(EventData eventData);
    }

    /// <summary>
    /// Provides the base capabilities for an <see cref="EventData"/> subscriber with a typed <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">The <see cref="EventData{T}"/> <see cref="Type"/>.</typeparam>
    public abstract class EventSubscriber<T> : EventSubscriberBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriber"/> class.
        /// </summary>
        /// <param name="subjectTemplate">The <see cref="EventData.Subject"/> template for the event required (can contain wildcard).</param>
        /// <param name="action">The <see cref="EventData.Action"/>; where <b>null</b> this indicates all.</param>
        public EventSubscriber(string subjectTemplate, string action = null) : base(subjectTemplate, action) { }

        /// <summary>
        /// Gets the value <see cref="Type"/>.
        /// </summary>
        public override Type ValueType { get => typeof(T); }

        /// <summary>
        /// Receive and process the <see cref="EventData"/> (internally casts the <paramref name="eventData"/> to <see cref="EventData{T}"/> and invokes <see cref="ReceiveAsync(EventData{T})"/>).
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public override Task ReceiveAsync(EventData eventData) => ReceiveAsync((EventData<T>)eventData);

        /// <summary>
        /// Receive and process the <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData{T}"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public abstract Task ReceiveAsync(EventData<T> eventData);
    }
}