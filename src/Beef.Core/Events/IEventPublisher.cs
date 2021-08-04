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
        /// Gets the default source <see cref="Uri"/> to be used where not otherwise specified (see <see cref="EventMetadata.Source"/>).
        /// </summary>
        Uri? DefaultSource { get; }

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
        /// Gets the <see cref="EventMetadata.Subject"/> format.
        /// </summary>
        EventStringFormat SubjectFormat => EventStringFormat.None;

        /// <summary>
        /// Gets the <see cref="EventMetadata.Action"/> format.
        /// </summary>
        EventStringFormat ActionFormat => EventStringFormat.None;

        /// <summary>
        /// Gets the published/queued events (dequeues).
        /// </summary>
        /// <returns>An <see cref="EventData"/> array.</returns>
        /// <remarks>Note to implementers: this should act as a dequeue; in that this method should not be considered idempotent.</remarks>
        EventData[] GetEvents();

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
                IInt32Identifier ii => ii.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                IInt64Identifier li => li.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
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