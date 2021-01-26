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
    /// <see cref="ActionPropertyName"/>), <see cref="Beef.Events.EventData.EventId"/> (named <see cref="EventIdPropertyName"/>) and <see cref="Beef.Events.EventData.TenantId"/> 
    /// (named <see cref="TenantIdPropertyName"/>).
    /// </summary>
    public static class EventDataMapper
    {
        /// <summary>
        /// Gets or sets the <b>EventId</b> property name.
        /// </summary>
        public static string EventIdPropertyName { get; set; } = "Beef.EventId";

        /// <summary>
        /// Gets or sets the <b>Subject</b> property name.
        /// </summary>
        public static string SubjectPropertyName { get; set; } = "Beef.Subject";

        /// <summary>
        /// Gets or sets the <b>Action</b> property name.
        /// </summary>
        public static string ActionPropertyName { get; set; } = "Beef.Action";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> property name.
        /// </summary>
        public static string TenantIdPropertyName { get; set; } = "Beef.TenantId";

        /// <summary>
        /// Gets or sets the <b>Key</b> property name.
        /// </summary>
        public static string KeyPropertyName { get; set; } = "Beef.Key";

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
            return OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<Beef.Events.EventData>(body), eventData);
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
            return (Beef.Events.EventData<T>)OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<Beef.Events.EventData<T>>(body), eventData);
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
            return OverrideKey((Beef.Events.EventData)JsonConvert.DeserializeObject(body, typeof(Beef.Events.EventData<>).MakeGenericType(new Type[] { valueType }))!, eventData);
        }
        
        /// <summary>
        /// Override the key - as the JSON serialized version loses the Type.
        /// </summary>
        private static Beef.Events.EventData OverrideKey(Beef.Events.EventData ed, EventHubs.EventData eh)
        {
            if (eh.Properties.TryGetValue(KeyPropertyName, out var key))
                ed.Key = key;

            return ed;
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

            ed.Properties.Add(EventIdPropertyName, eventData.EventId);
            ed.Properties.Add(SubjectPropertyName, eventData.Subject);
            ed.Properties.Add(ActionPropertyName, eventData.Action);
            ed.Properties.Add(TenantIdPropertyName, eventData.TenantId);
            ed.Properties.Add(KeyPropertyName, eventData.Key);

            return ed;
        }

        /// <summary>
        /// Gets the <i>Beef</i>-related metadata from the <see cref="EventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubs.EventData"/>.</param>
        /// <returns>The values of the following properties: <see cref="SubjectPropertyName"/>, <see cref="ActionPropertyName"/> and <see cref="TenantIdPropertyName"/>.</returns>
        public static (Guid? eventId, string? subject, string? action, Guid? tenantId) GetBeefMetadata(this EventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            eventData.Properties.TryGetValue(SubjectPropertyName, out var subject);
            eventData.Properties.TryGetValue(ActionPropertyName, out var action);

            var eventId = (eventData.Properties.TryGetValue(EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null;
            var tenantId = (eventData.Properties.TryGetValue(TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null;

            return (eventId, (string?)subject, (string?)action, tenantId);
        }
    }
}