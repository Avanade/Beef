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
        /// Creates an <see cref="EventData"/> instance using a <see cref="Subject"/> <paramref name="template"/>.
        /// </summary>
        /// <param name="template">The <see cref="Subject"/> template (see <see cref="Event.CreateSubjectFromTemplate(string, KeyValuePair{string, object}[])"/>).</param>
        /// <param name="action">The event action.</param>
        /// <param name="keyValuePairs">The key/value pairs.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData Create(string template, string action, params KeyValuePair<string, object>[] keyValuePairs)
        {
            return ApplySubjectTemplate(new EventData(), Check.NotEmpty(template, nameof(template)), action, keyValuePairs);
        }

        /// <summary>
        /// Creates an <see cref="EventData{T}"/> instance using a <paramref name="value"/> and <see cref="Subject"/> <paramref name="template"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="template">The <see cref="Subject"/> template (see <see cref="Event.CreateSubjectFromTemplate(string, KeyValuePair{string, object}[])"/>).</param>
        /// <param name="action">The event action.</param>
        /// <param name="keyValuePairs">The key/value pairs.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData<T> Create<T>(T value, string template, string action, params KeyValuePair<string, object>[] keyValuePairs)
        {
            return (EventData<T>)ApplySubjectTemplate(new EventData<T> { Value = Check.NotNull(value, nameof(value)) }, Check.NotEmpty(template, nameof(template)), action, keyValuePairs);
        }

        /// <summary>
        /// Applies the subject template.
        /// </summary>
        private static EventData ApplySubjectTemplate(EventData ed, string template, string action, params KeyValuePair<string, object>[] keyValuePairs)
        {
            ed.Subject = Event.CreateSubjectFromTemplate(template, keyValuePairs);
            ed.Action = action;

            switch (keyValuePairs.Length)
            {
                case 0:
                    return ed;

                case 1:
                    ed.Key = keyValuePairs[0].Value;
                    return ed;

                default:
                    ed.Key = keyValuePairs.Select(x => x.Value).ToArray();
                    return ed;
            }
        }

        /// <summary>
        /// Creates an <see cref="EventData"/> instance using the <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData Create<T>(T value, string subject, string action = null)
        {
            Check.NotNull(value, nameof(value));

            var ed = new EventData<T> { Value = value };
            ed.Subject = Check.NotEmpty(subject, nameof(subject));
            ed.Action = action;

            switch (value)
            {
                case IIntIdentifier ii:
                    ed.Key = ii;
                    break;

                case IGuidIdentifier gi:
                    ed.Key = gi;
                    break;

                case IUniqueKey uk:
                    if (uk.HasUniqueKey)
                        ed.Key = uk.UniqueKey.Args.Length == 1 ? uk.UniqueKey.Args[0] : uk.UniqueKey.Args;

                    break;
            }

            return ed;
        }

        /// <summary>
        /// Creates an <see cref="EventData"/> instance. 
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData Create(string subject, string action = null)
        {
            var ed = new EventData { Action = action };
            ed.Subject = Check.NotEmpty(subject, nameof(subject)); ;
            return ed;
        }

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
                Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        [JsonProperty("tenantId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the event subject (the name should use the '/' character to denote paths).
        /// </summary>
        [JsonProperty("subject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        [JsonProperty("action", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the entity key (could be single value or an array of values).
        /// </summary>
        [JsonProperty("key", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Key { get; set; }

        /// <summary>
        /// Gets or sets the username that initiated the event.
        /// </summary>
        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ETag { get; set; } 

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
        public virtual object GetValue() => null;
    }

    /// <summary>
    /// Represents the <see cref="EventData"/> with a corresponding <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class EventData<T> : EventData
    {
        private T _value;

        /// <summary>
        /// Gets or sets the event value (automatically setting the <see cref="EventData.ETag"/> where not already set).
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public T Value
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
            Value = default;
        }

        /// <summary>
        /// Indicates whether the <see cref="EventData"/> has a <see cref="Value"/> property; always returns <c>true</c>.
        /// </summary>
        public override bool HasValue => true;

        /// <summary>
        /// Gets the <see cref="EventData"/> <see cref="Value"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/>.</returns>
        public override object GetValue() => Value;
    }
}