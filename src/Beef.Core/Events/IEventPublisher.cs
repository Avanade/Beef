// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
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
        /// Gets the key seperator <see cref="string"/>.
        /// </summary>
        string KeySeparator => ",";

        /// <summary>
        /// Gets the template wildcard <see cref="string"/>.
        /// </summary>
        string TemplateWildcard { get; }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> format.
        /// </summary>
        EventStringFormat SubjectFormat => EventStringFormat.None;

        /// <summary>
        /// Gets the <see cref="EventData.Action"/> format.
        /// </summary>
        EventStringFormat ActionFormat => EventStringFormat.None;

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
        EventData CreateEvent(string subject, string? action = null) => EventData.CreateEvent(subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData CreateEvent(Uri source, string subject, string? action = null) => EventData.CreateEvent(source, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData CreateEvent(string subject, string? action = null, params IComparable?[] key) => EventData.CreateEvent(subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData CreateEvent(Uri source, string subject, string? action = null, params IComparable?[] key) => EventData.CreateEvent(source, subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventData.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null) where T : class => EventData.CreateValueEvent(value, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventData.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, Uri source, string subject, string? action = null) where T : class => EventData.CreateValueEvent(value, source, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null, params IComparable?[] key) => EventData.CreateValueEvent(value, subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        EventData<T> CreateValueEvent<T>(T value, Uri source, string subject, string? action = null, params IComparable?[] key) => EventData.CreateValueEvent(value, source, subject, action, key);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventData.Key"/>).
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(string subject, string? action = null) => Publish(CreateEvent(subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventData.Key"/>).
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(Uri source, string subject, string? action = null) => Publish(CreateEvent(source, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(string subject, string? action = null, params IComparable?[] key) => Publish(CreateEvent(subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(Uri source, string subject, string? action = null, params IComparable?[] key) => Publish(CreateEvent(source, subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventData.Key"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, string subject, string? action = null) where T : class => Publish(CreateValueEvent(value, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventData.Key"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, Uri source, string subject, string? action = null) where T : class => Publish(CreateValueEvent(value, source, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, string subject, string? action = null, params IComparable?[] key) => Publish(CreateValueEvent(value, subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher PublishValue<T>(T value, Uri source, string subject, string? action = null, params IComparable?[] key) => Publish(CreateValueEvent(value, source, subject, action, key));

        /// <summary>
        /// Publishes (queues) one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(params EventData[] events);

        /// <summary>
        /// Formats a <paramref name="value"/> as its formatted key representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted key.</returns>
        string? FormatKey(object? value)
        {
            if (value == null)
                return null;
            
            if (value is string s)
                return s;

            return value switch
            {
                IIntIdentifier ii => ii.Id.ToString(),
                IGuidIdentifier gi => gi.Id.ToString(),
                IStringIdentifier si => si.Id,
                IUniqueKey uk => uk.UniqueKey.Args.Length == 1 ? FormatKey(uk.UniqueKey.Args[0]) : FormatKey(uk.UniqueKey.Args),
                _ => value.ToString(),
            };
        }

        /// <summary>
        /// Formats one or more <paramref name="values"/> as its formatted key representation.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The formatted key.</returns>
        string? FormatKey(IEnumerable<object?> values) => string.Join(KeySeparator, values);

        /// <summary>
        /// Sends all previously published events.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        Task SendAsync();

        /// <summary>
        /// Resets by clearing the internal cache/store.
        /// </summary>
        void Reset();
    }
}