// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Beef.Events
{
    /// <summary>
    /// Provides <see cref="EventHubs.EventData"/> to / from <see cref="Beef.Events.EventData"/> mapping (as extension methods). <b>Beef</b> automatically adds the following <see cref="EventHubs.EventData.Properties"/>
    /// to an <see cref="EventHubs.EventData"/> for usage: <see cref="Beef.Events.EventData.Subject"/> (named <see cref="SubjectPropertyName"/>), <see cref="Beef.Events.EventData.Action"/> (named
    /// <see cref="ActionPropertyName"/>) and <see cref="Beef.Events.EventData.TenantId"/> (named <see cref="TenantIdPropertyName"/>).
    /// </summary>
    public static class EventDataMapper
    {
        /// <summary>
        /// Gets the <b>Subject</b> property name.
        /// </summary>
        public const string SubjectPropertyName = "Beef.Subject";

        /// <summary>
        /// Gets the <b>Action</b> property name.
        /// </summary>
        public const string ActionPropertyName = "Beef.Action";

        /// <summary>
        /// Gets the <b>TenantId</b> property name.
        /// </summary>
        public const string TenantIdPropertyName = "Beef.TenantId";

        /// <summary>
        /// Converts the <see cref="EventHubs.EventData"/> instance to the corresponding <see cref="Beef.Events.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <returns>The <see cref="Beef.Events.EventData"/>.</returns>
        public static Beef.Events.EventData ToBeefEventData(this EventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Beef.Events.EventData>(body);
        }

        /// <summary>
        /// Converts the <see cref="EventHubs.EventData"/> instance to the corresponding <see cref="Beef.Events.EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <returns>The <see cref="Beef.Events.EventData{T}"/>.</returns>
        public static Beef.Events.EventData<T> ToBeefEventData<T>(this EventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Beef.Events.EventData<T>>(body);
        }

        /// <summary>
        /// Converts the <see cref="EventHubs.EventData"/> instance to the corresponding <paramref name="valueType"/> <see cref="Beef.Events.EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        /// <returns>The <see cref="Beef.Events.EventData{T}"/>.</returns>
        public static Beef.Events.EventData ToBeefEventData(this EventHubs.EventData eventData, Type valueType)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            Beef.Check.NotNull(valueType, nameof(valueType));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return (Beef.Events.EventData)Newtonsoft.Json.JsonConvert.DeserializeObject(body, typeof(Beef.Events.EventData<>).MakeGenericType(new Type[] { valueType }));
        }

        /// <summary>
        /// Converts the <see cref="Beef.Events.EventData"/> to a corresponding <see cref="EventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <returns>The <see cref="Beef.Events.EventData"/>.</returns>
        public static EventHubs.EventData ToEventHubsEventData(this Beef.Events.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (string.IsNullOrEmpty(eventData.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(eventData));

            var json = JsonConvert.SerializeObject(eventData);
            var bytes = Encoding.UTF8.GetBytes(json);
            var ed = new EventHubs.EventData(bytes);

            ed.Properties.Add(SubjectPropertyName, eventData.Subject);
            ed.Properties.Add(ActionPropertyName, eventData.Action);
            ed.Properties.Add(TenantIdPropertyName, eventData.TenantId);

            return ed;
        }

        /// <summary>
        /// Gets the <i>Beef</i>-related metadata from the <see cref="EventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <returns>The values of the following properties: <see cref="SubjectPropertyName"/>, <see cref="ActionPropertyName"/> and <see cref="TenantIdPropertyName"/>.</returns>
        public static (string subject, string action, Guid? tenantId) GetBeefMetadata(this EventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (!eventData.Properties.TryGetValue(SubjectPropertyName, out var subject) || string.IsNullOrEmpty((string)subject))
                throw new ArgumentException($"EventData does not contain required property '{SubjectPropertyName}'.");

            eventData.Properties.TryGetValue(ActionPropertyName, out var action);

            if (eventData.Properties.TryGetValue(TenantIdPropertyName, out var tenantId) && tenantId != null && tenantId is Guid?)
                return ((string)subject, (string)action, (Guid?)tenantId);
            else
                return ((string)subject, (string)action, (Guid?)null);
        }
    }
}