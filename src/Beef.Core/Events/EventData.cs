// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Events
{
    /// <summary>
    /// Represents the <b>EventData</b>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EventData : IETag
    {
        /// <summary>
        /// Creates an <see cref="EventData"/> instance (with no <see cref="Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The intended <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(IEventPublisher eventPublisher, string subject, string? action = null) 
            => new EventData { Subject = PrependPrefix(eventPublisher, Check.NotEmpty(subject, nameof(subject))), Action = action };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="Key"/>.
        /// </summary>
        /// <param name="eventPublisher">The intended <see cref="IEventPublisher"/>.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(IEventPublisher eventPublisher, string subject, string? action = null, params IComparable?[] key) 
            => new EventData { Subject = PrependPrefix(eventPublisher, Check.NotEmpty(subject, nameof(subject))), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The intended <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(IEventPublisher eventPublisher, T value, string subject, string? action = null) where T : class
        {
            var ed = new EventData<T> { Value = Check.NotNull(value, nameof(value)) };
            ed.Subject = PrependPrefix(eventPublisher, Check.NotEmpty(subject, nameof(subject)));
            ed.Action = action;

            switch (value)
            {
                case IIntIdentifier ii:
                    ed.Key = ii.Id;
                    break;

                case IGuidIdentifier gi:
                    ed.Key = gi.Id;
                    break;

                case IStringIdentifier si:
                    ed.Key = si.Id;
                    break;

                case IUniqueKey uk:
                    if (uk.HasUniqueKey)
                        ed.Key = uk.UniqueKey.Args.Length == 1 ? uk.UniqueKey.Args[0] : uk.UniqueKey.Args;
                    else
                        throw new InvalidOperationException("A Value that implements IUniqueKey must have one; i.e. HasUniqueKey = true.");

                    break;
            }

            return ed;
        }

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The intended <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(IEventPublisher eventPublisher, T value, string subject, string? action = null, params IComparable?[] key) 
            => new EventData<T> { Value = value, Subject = PrependPrefix(eventPublisher, Check.NotEmpty(subject, nameof(subject))), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Prepend the <see cref="IEventPublisher.EventSubjectPrefix"/> to the <paramref name="subject"/> where specified.
        /// </summary>
        private static string PrependPrefix(IEventPublisher eventPublisher, string subject) => string.IsNullOrEmpty(eventPublisher?.EventSubjectPrefix) ? subject : eventPublisher?.EventSubjectPrefix + eventPublisher?.PathSeparator + subject;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventData"/> class defaulting the <see cref="TenantId"/> and <see cref="Timestamp"/> as applicable 
        /// (will use <see cref="ExecutionContext"/> where <see cref="ExecutionContext.HasCurrent"/>).
        /// </summary>
        public EventData()
        {
            if (ExecutionContext.HasCurrent)
            {
                TenantId = ExecutionContext.Current.TenantId;
                Timestamp = ExecutionContext.Current.Timestamp;
                Username = ExecutionContext.Current.Username;
            }
            else
                Timestamp = Cleaner.Clean(DateTime.Now);
        }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        [JsonProperty("tenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the event subject (the name should use the '.' character to denote paths).
        /// </summary>
        [JsonProperty("subject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        [JsonProperty("action", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the entity key (could be single value or an array of values).
        /// </summary>
        [JsonProperty("key", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object? Key { get; set; }

        /// <summary>
        /// Gets or sets the username that initiated the event.
        /// </summary>
        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <summary>
        /// Resets the value to the default.
        /// </summary>
        public virtual void ResetValue() { }

        /// <summary>
        /// Indicates whether the <see cref="EventData"/> has a <b>value</b> property; i.e. is an <see cref="EventData{T}"/>.
        /// </summary>
        public virtual bool HasValue => false;

        /// <summary>
        /// Gets the <see cref="EventData"/> <b>value</b>; returns <c>null</c> where <see cref="HasValue"/> is <c>false</c>.
        /// </summary>
        /// <returns>The <see cref="EventData{T}.Value"/> or <c>null</c>.</returns>
        public virtual object? GetValue() => null;
    }

    /// <summary>
    /// Represents the <see cref="EventData"/> with a corresponding <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class EventData<T> : EventData
    {
        private T _value = default!;

        /// <summary>
        /// Gets (same as <see cref="GetValue"/>) or sets the event value (automatically setting the <see cref="EventData.ETag"/> where not already set).
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
#pragma warning disable CA1721 // Property names should not match get methods; by-design, most meaningful name (are same-same).
        public T Value
#pragma warning restore CA1721 
        {
            get => _value;

            set
            {
                _value = value;
                if (ETag == null && _value != null && _value is IETag etag)
                    ETag = etag.ETag;
            }
        }

        /// <summary>
        /// Resets the value to the default.
        /// </summary>
        public override void ResetValue()
        {
            Value = default!;
        }

        /// <summary>
        /// Indicates whether the <see cref="EventData"/> has a <see cref="Value"/> property; always returns <c>true</c>.
        /// </summary>
        public override bool HasValue => true;

        /// <summary>
        /// Gets the <see cref="EventData"/> <see cref="Value"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/>.</returns>
        public override object? GetValue() => Value;
    }
}