// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

#pragma warning disable IDE0060 // Remove unused parameter; for backwards compatibility.

using Beef.Entities;
using System;

namespace Beef.Events
{
    /// <summary>
    /// Event specific extension methods.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(this IEventPublisher eventPublisher, string subject, string? action = null) => EventData.CreateEvent(subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(this IEventPublisher eventPublisher, Uri source, string subject, string? action = null) => EventData.CreateEvent(source, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(this IEventPublisher eventPublisher, string subject, string? action = null, params IComparable?[] key) => EventData.CreateEvent(subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(this IEventPublisher eventPublisher, Uri source, string subject, string? action = null, params IComparable?[] key) => EventData.CreateEvent(source, subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventMetadata.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(this IEventPublisher eventPublisher, T value, string subject, string? action = null) where T : class => EventData.CreateValueEvent(value, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventMetadata.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(this IEventPublisher eventPublisher, T value, Uri source, string subject, string? action = null) where T : class => EventData.CreateValueEvent(value, source, subject, action);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(this IEventPublisher eventPublisher, T value, string subject, string? action = null, params IComparable?[] key) => EventData.CreateValueEvent(value, subject, action, key);

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(this IEventPublisher eventPublisher, T value, Uri source, string subject, string? action = null, params IComparable?[] key) => EventData.CreateValueEvent(value, source, subject, action, key);

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventMetadata.Key"/>).
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher Publish(this IEventPublisher eventPublisher, string subject, string? action = null) => eventPublisher.Publish(eventPublisher.CreateEvent(subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance (with no <see cref="EventMetadata.Key"/>).
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher Publish(this IEventPublisher eventPublisher, Uri source, string subject, string? action = null) => eventPublisher.Publish(eventPublisher.CreateEvent(source, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher Publish(this IEventPublisher eventPublisher, string subject, string? action = null, params IComparable?[] key) => eventPublisher.Publish(eventPublisher.CreateEvent(subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher Publish(this IEventPublisher eventPublisher, Uri source, string subject, string? action = null, params IComparable?[] key) => eventPublisher.Publish(eventPublisher.CreateEvent(source, subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventMetadata.Key"/>).
        /// </summary>
        /// <typeventPublisheraram name="T">The value <see cref="Type"/>.</typeventPublisheraram>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher PublishValue<T>(this IEventPublisher eventPublisher, T value, string subject, string? action = null) where T : class => eventPublisher.Publish(eventPublisher.CreateValueEvent(value, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventMetadata.Key"/>).
        /// </summary>
        /// <typeventPublisheraram name="T">The value <see cref="Type"/>.</typeventPublisheraram>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher PublishValue<T>(this IEventPublisher eventPublisher, T value, Uri source, string subject, string? action = null) where T : class => eventPublisher.Publish(eventPublisher.CreateValueEvent(value, source, subject, action));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <typeventPublisheraram name="T">The value <see cref="Type"/>.</typeventPublisheraram>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher PublishValue<T>(this IEventPublisher eventPublisher, T value, string subject, string? action = null, params IComparable?[] key) => eventPublisher.Publish(eventPublisher.CreateValueEvent(value, subject, action, key));

        /// <summary>
        /// Publishes (queues) an <see cref="EventData"/> instance using the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <typeventPublisheraram name="T">The value <see cref="Type"/>.</typeventPublisheraram>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        public static IEventPublisher PublishValue<T>(this IEventPublisher eventPublisher, T value, Uri source, string subject, string? action = null, params IComparable?[] key) => eventPublisher.Publish(eventPublisher.CreateValueEvent(value, source, subject, action, key));
    }
}

#pragma warning restore IDE0060