// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the standardised <b>Event</b> processing/publishing. 
    /// </summary>
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
        /// Publishes an <see cref="EventData"/> instance (with no <see cref="EventData.Key"/>).
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task PublishAsync(string subject, string? action = null);

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task PublishAsync(string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using the <paramref name="value"/> (infers <see cref="EventData.Key"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task PublishValueAsync<T>(T value, string subject, string? action = null) where T : class;

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using the specified <see cref="EventData.Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task PublishValueAsync<T>(T value, string subject, string? action = null, params IComparable?[] key);

        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task PublishAsync(params EventData[] events);
    }
}