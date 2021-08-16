// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.Events
{
    /// <summary>
    /// Provides the <i>Beef</i> event-metadata and property names.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EventMetadata : IETag
    {
        #region AttributeNames

        /// <summary>
        /// Gets or sets the <b>EventId</b> attribute name.
        /// </summary>
        public static string EventIdAttributeName { get; set; } = "Beef.EventId";

        /// <summary>
        /// Gets or sets the <b>Subject</b> attribute name.
        /// </summary>
        public static string SubjectAttributeName { get; set; } = "Beef.Subject";

        /// <summary>
        /// Gets or sets the <b>Action</b> attribute name.
        /// </summary>
        public static string ActionAttributeName { get; set; } = "Beef.Action";

        /// <summary>
        /// Gets or sets the <b>Source</b> attribute name.
        /// </summary>
        public static string SourceAttributeName { get; set; } = "Beef.Source";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> attribute name.
        /// </summary>
        public static string TenantIdAttributeName { get; set; } = "Beef.TenantId";

        /// <summary>
        /// Gets or sets the <b>Key</b> attribute name.
        /// </summary>
        public static string KeyPropertyName { get; set; } = "Beef.Key";

        /// <summary>
        /// Gets or sets the <b>ETag</b> attribute name.
        /// </summary>
        public static string ETagAttributeName { get; set; } = "Beef.ETag";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> attribute name.
        /// </summary>
        public static string UsernameAttributeName { get; set; } = "Beef.Username";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> attribute name.
        /// </summary>
        public static string UserIdAttributeName { get; set; } = "Beef.UserId";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> attribute name.
        /// </summary>
        public static string TimestampAttributeName { get; set; } = "Beef.Timestamp";

        /// <summary>
        /// Gets or sets the <b>CorrelationId</b> attribute name.
        /// </summary>
        public static string CorrelationIdAttributeName { get; set; } = "Beef.CorrelationId";

        /// <summary>
        /// Gets or sets the <b>PartitionKey</b> attribute name.
        /// </summary>
        public static string PartitionKeyAttributeName { get; set; } = "Beef.PartitionKey";

        #endregion

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
        /// Gets or sets the event source <see cref="Uri"/> (describes the event published/producer).
        /// </summary>
        [JsonProperty("source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri? Source { get; set; }

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
        [JsonProperty("partitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Gets (converts) the <see cref="Key"/> to a <see cref="Guid"/>. This is achieved by either checking it is a <see cref="Guid"/> and returning as-is; otherwise, the value will be cast to a
        /// <see cref="string"/> and <see cref="Guid.TryParse(string, out Guid)">parsed</see>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> where valid; otherwise, <c>null</c>.</returns>
        public Guid? KeyAsGuid => Key == null ? null : (Key is Guid g ? g : (Guid.TryParse((string)Key, out var gx) ? gx : (Guid?)null));

        /// <summary>
        /// Gets (converts) the <see cref="Key"/> to a <see cref="int"/>. This is achieved by either checking it is a <see cref="int"/> and returning as-is; otherwise, the value will be cast to a
        /// <see cref="string"/> and <see cref="int.TryParse(string, out int)">parsed</see>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> where valid; otherwise, <c>null</c>.</returns>
        public int? KeyAsInt32 => Key == null ? null : (Key is int i ? i : (int.TryParse((string)Key, out var ix) ? ix : (int?)null));

        /// <summary>
        /// Gets (converts) the <see cref="Key"/> to a <see cref="long"/>. This is achieved by either checking it is a <see cref="long"/> and returning as-is; otherwise, the value will be cast to a
        /// <see cref="string"/> and <see cref="long.TryParse(string, out long)">parsed</see>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> where valid; otherwise, <c>null</c>.</returns>
        public long? KeyAsInt64 => Key == null ? null : (Key is long l ? l : (long.TryParse((string)Key, out var lx) ? lx : (long?)null));

        /// <summary>
        /// Gets (converts) the <see cref="Key"/> to a <see cref="string"/>. This is achieved by either checking it is a <see cref="string"/> and returning as-is; otherwise, the value will be cast to a
        /// <see cref="string"/>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> where valid; otherwise, <c>null</c>.</returns>
        public string? KeyAsString => Key == null ? null : (Key is string s ? s : (string)Key);

        /// <summary>
        /// Creates (clones) a new instance copying the existing values.
        /// </summary>
        /// <returns></returns>
        public EventMetadata CopyMetadata() => new()
        {
            EventId = EventId,
            TenantId = TenantId,
            Subject = Subject,
            Action = Action,
            Source = Source,
            Key = Key,
            ETag = ETag,
            Username = Username,
            UserId = UserId,
            Timestamp = Timestamp,
            CorrelationId = CorrelationId,
            PartitionKey = PartitionKey
        };
    }

    /// <summary>
    /// Represents the <b>EventData</b>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EventData : EventMetadata
    {
        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(string subject, string? action = null) 
            => new() { Subject = Check.NotEmpty(subject, nameof(subject)), Action = action };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with no <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(Uri source, string subject, string? action = null)
            => new() { Source = Check.NotNull(source, nameof(source)), Subject = Check.NotEmpty(subject, nameof(subject)), Action = action };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(string subject, string? action = null, params IComparable?[] key) 
            => new() { Subject = Check.NotEmpty(subject, nameof(subject)), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance with the specified <see cref="EventMetadata.Key"/>.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData CreateEvent(Uri source, string subject, string? action = null, params IComparable?[] key)
            => new() { Source = Check.NotNull(source, nameof(source)), Subject = Check.NotEmpty(subject, nameof(subject)), Action = action, Key = key.Length == 1 ? (object?)key[0] : key };

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventMetadata.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
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
                case IInt32Identifier ii:
                    ed.Key = ii.Id;
                    break;

                case IInt64Identifier li:
                    ed.Key = li.Id;
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
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/> (infers the <see cref="EventMetadata.Key"/> from either <see cref="IIdentifier"/> or <see cref="IUniqueKey"/>).
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
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventMetadata.Key"/>.
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
        /// Creates an <see cref="EventData"/> instance with the specified <paramref name="value"/> <see cref="EventMetadata.Key"/>.
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
        /// Initializes a new instance of the <see cref="EventData{T}"/> class from a <see cref="EventMetadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="EventMetadata"/>.</param>
        public EventData(EventMetadata? metadata)
        {
            if (metadata == null)
                return;

            EventId = metadata.EventId;
            TenantId = metadata.TenantId;
            Subject = metadata.Subject;
            Action = metadata.Action;
            Source = metadata.Source;
            Key = metadata.Key;
            ETag = metadata.ETag;
            Username = metadata.Username;
            UserId = metadata.UserId;
            Timestamp = metadata.Timestamp;
            CorrelationId = metadata.CorrelationId;
            PartitionKey = metadata.PartitionKey;
        }

        /// <summary>
        /// Resets the value to the default.
        /// </summary>
        public virtual void ResetValue() { }

        /// <summary>
        /// Gets the <see cref="EventData"/> <b>value</b> <see cref="Type"/>; returns <c>null</c> where <see cref="HasValue"/> is <c>false</c>.
        /// </summary>
        public virtual Type? ValueType => null;

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

        /// <summary>
        /// Merges in the <see cref="EventMetadata"/> updating any existing values where they are currently <c>null</c>.
        /// </summary>
        /// <param name="metadata"></param>
        public void MergeMetadata(EventMetadata metadata)
        {
            InvokeOnNull(EventId, () => EventId = metadata.EventId);
            InvokeOnNull(TenantId, () => TenantId = metadata.TenantId);
            InvokeOnNull(Subject, () => Subject = metadata.Subject);
            InvokeOnNull(Action, () => Action = metadata.Action);
            InvokeOnNull(Source, () => Source = metadata.Source);
            InvokeOnNull(Key, () => Key = metadata.Key);
            InvokeOnNull(ETag, () => ETag = metadata.ETag);
            InvokeOnNull(Username, () => Username = metadata.Username);
            InvokeOnNull(UserId, () => UserId = metadata.UserId);
            InvokeOnNull(Timestamp, () => Timestamp = metadata.Timestamp);
            InvokeOnNull(CorrelationId, () => CorrelationId = metadata.CorrelationId);
            InvokeOnNull(PartitionKey, () => PartitionKey = metadata.PartitionKey);
        }

        /// <summary>
        /// Invokes the action on null value.
        /// </summary>
        private static void InvokeOnNull(object? value, Action action)
        {
            if (value == null)
                action();
        }
    }

    /// <summary>
    /// Represents the <see cref="EventData"/> with a corresponding <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class EventData<T> : EventData
    {
        private T _value = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventData{T}"/> class.
        /// </summary>
        public EventData() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventData{T}"/> class from a <see cref="EventMetadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="EventMetadata"/>.</param>
        public EventData(EventMetadata? metadata) : base(metadata) { }

        /// <summary>
        /// Gets the <see cref="EventData"/> <b>value</b> <see cref="Type"/>; returns <c>null</c> where <see cref="HasValue"/> is <c>false</c>.
        /// </summary>
        public override Type? ValueType => typeof(T);

        /// <summary>
        /// Gets (same as <see cref="GetValue"/>) or sets (same as <see cref="SetValue"/>) the event value (automatically setting/overriding the <see cref="EventMetadata.ETag"/> and <see cref="EventMetadata.PartitionKey"/>).
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