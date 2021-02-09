// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the standardised <b>Event</b> publishing and sending. 
    /// </summary>
    /// <remarks>Note to implementers: The <i>Publish*</i> methods should only cache/store the events queue (order must be maintained) to be sent; they should only be sent where <see cref="SendAsync"/> is explicitly requested.
    /// <para>The key reason for queuing the published events it to promote a single atomic send operation; i.e. all events should be sent together, and either succeed or fail together.</para></remarks>
    public interface IEventPublisher
    {
        /// <summary>
        /// Gets the prefix for an <see cref="EventData.Subject"/> when creating an <see cref="EventData"/> or <see cref="EventData{T}"/>. <i>Note:</i> the <see cref="PathSeparator"/> will automatically be applied.
        /// </summary>
        string? EventSubjectPrefix { get; }

        /// <summary>
        /// Gets the path seperator <see cref="string"/>.
        /// </summary>
        string PathSeparator { get; }

        /// <summary>
        /// Gets the template wildcard <see cref="string"/>.
        /// </summary>
        string TemplateWildcard { get; }

        /// <summary>
        /// Indicates whether the published (queued) events have been sent.
        /// </summary>
        bool HasBeenSent { get; }

        /// <summary>
        /// Gets the published/queued events.
        /// </summary>
        /// <returns>An <see cref="EventData"/> array.</returns>
        EventData[] GetEvents();

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData CreateEvent(string subject, string? action = null);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData CreateEvent(string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventData.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null) where T : class;

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventData.Key"/>).
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(string subject, string? action = null);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventData.Key"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, string subject, string? action = null) where T : class;

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Publishes (queues) one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(params EventData[] events);

        /// <summary>
        /// Sends all previously published events.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        Task SendAsync();

        /// <summary>
        /// Resets by clearing the internal cache/store and setting <see cref="HasBeenSent"/> back to <c>false</c>.
        /// </summary>
        void Reset();
    }
}