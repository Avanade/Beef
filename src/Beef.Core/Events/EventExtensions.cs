using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Events
{
    /// <summary>
    /// Event specific extension methods.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public EventData CreateEvent(this string subject, string? action = null) => EventData.CreateEvent(subject, action);

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
    }
}
