// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.Events
{
    /// <summary>
    /// Represents the <b>EventData</b>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EventData : IETag
    {
        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(string subject, string? action = null) 
            => new() { Subject = Check.NotEmpty(subject, nameof(subject)), Action = action };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(Uri source, string subject, string? action = null)
            => new() { Source = Check.NotNull(source, nameof(source)), Subject = Check.NotEmpty(subject, nameof(subject)), Action = action };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(string subject, string? action = null, params IComparable?[] key) 
            => new() { Subject = Check.NotEmpty(subject, nameof(subject)), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(Uri source, string subject, string? action = null, params IComparable?[] key)
            => new() { Source = Check.NotNull(source, nameof(source)), Subject = Check.NotEmpty(subject, nameof(subject)), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null) where T : class
        {
            var ed = new EventData<T> { Value = Check.NotNull(value, nameof(value)) };
            ed.Subject = Check.NotEmpty(subject, nameof(subject));
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
                    ed.Key = uk.UniqueKey.Args.Length == 1 ? uk.UniqueKey.Args[0] : uk.UniqueKey.Args;
                    break;
            }

            return ed;
        }

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(T value, Uri source, string subject, string? action = null) where T : class
        {
            var ed = CreateValueEvent(value, subject, action);
            ed.Source = Check.NotNull(source, nameof(source));
            return ed;
        }

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(T value, string subject, string? action = null, params IComparable?[] key) 
            => new() { Value = value, Subject = Check.NotEmpty(subject, nameof(subject)), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="Key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData<T> CreateValueEvent<T>(T value, Uri source, string subject, string? action = null, params IComparable?[] key)
        {
            var ed = CreateValueEvent(value, subject, action, key);
            ed.Source = Check.NotNull(source, nameof(source));
            return ed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventData"/> class defaulting as applicable using the equivalent <see cref="ExecutionContext"/> <see cref="ExecutionContext.Current"/> values.
        /// </summary>
        public EventData()
        {
            EventId = Guid.NewGuid();

            if (ExecutionContext.HasCurrent)
            {
                TenantId = ExecutionContext.Current.TenantId;
                Timestamp = ExecutionContext.Current.Timestamp;
                Username = ExecutionContext.Current.Username;
                UserId = ExecutionContext.Current.UserId;
                CorrelationId = ExecutionContext.Current.CorrelationId;
                PartitionKey = ExecutionContext.Current.TenantId?.ToString();
            }
            else
                Timestamp = Cleaner.Clean(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets or sets the unique event identifier.
        /// </summary>
        [JsonProperty("eventId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? EventId { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        [JsonProperty("tenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the event source <see cref="Uri"/> (describes the event producer).
        /// </summary>
        [JsonProperty("source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri? Source { get; set; }

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
        /// Gets or sets the unique user identifier that initiated the event.
        /// </summary>
        [JsonProperty("userid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        [JsonProperty("correlationId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string? PartitionKey { get; set; }

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

        /// <summary>
        /// Gets the <see cref="EventData"/> <b>value</b>; will throw <see cref="NotSupportedException"/> where appropriate.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetValue(object? value) => throw new NotSupportedException();
    }

    /// <summary>
    /// Represents the <see cref="EventData"/> with a corresponding <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class EventData<T> : EventData
    {
        private T _value = default!;

        /// <summary>
        /// Gets (same as <see cref="GetValue"/>) or sets (same as <see cref="SetValue"/>) the event value (automatically setting/overriding the <see cref="EventData.ETag"/> and <see cref="EventData.PartitionKey"/>).
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public T Value
        {
            get => _value;

            set
            {
                _value = value;
                if (_value != null && _value is IETag etag)
                    ETag = etag.ETag;

                if (_value != null && _value is IPartitionKey pk)
                    PartitionKey = pk.PartitionKey;
            }
        }

        /// <summary>
        /// Resets the value to the default.
        /// </summary>
        public override void ResetValue() => Value = default!;

        /// <summary>
        /// Indicates whether the <see cref="EventData"/> has a <see cref="Value"/> property; always returns <c>true</c>.
        /// </summary>
        public override bool HasValue => true;

        /// <summary>
        /// Gets the <see cref="EventData"/> <see cref="Value"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/>.</returns>
        public override object? GetValue() => Value;

        /// <summary>
        /// Sets the <see cref="EventData"/> <see cref="Value"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public override void SetValue(object? value) => Value = (T)value!;
    }
}