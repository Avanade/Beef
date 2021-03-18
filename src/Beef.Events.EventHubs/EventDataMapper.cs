// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Text;
using AzureEventHubs = Azure.Messaging.EventHubs;
using MicrosoftEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events
{
    /// <summary>
    /// Provides <see cref="AzureEventHubs.EventData"/> to / from <see cref="EventData"/> mapping (as extension methods). <b>Beef</b> automatically adds automatically adds the <see cref="EventMetadata"/> properties.
    /// </summary>
    public static class EventDataMapper
    {
        /// <summary>
        /// Converts the <see cref="AzureEventHubs.EventData"/> instance to the corresponding <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData ToBeefEventData(this AzureEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.EventBody);
            return OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData>(body), eventData);
        }

        /// <summary>
        /// Converts the <see cref="AzureEventHubs.EventData"/> instance to the corresponding <see cref="EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData<T> ToBeefEventData<T>(this AzureEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.EventBody);
            return (EventData<T>)OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData<T>>(body), eventData);
        }

        /// <summary>
        /// Converts the <see cref="AzureEventHubs.EventData"/> instance to the corresponding <paramref name="valueType"/> <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData ToBeefEventData(this AzureEventHubs.EventData eventData, Type valueType)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            Beef.Check.NotNull(valueType, nameof(valueType));

            var body = Encoding.UTF8.GetString(eventData.EventBody);
            return OverrideKey((EventData)JsonConvert.DeserializeObject(body, typeof(EventData<>).MakeGenericType(new Type[] { valueType }))!, eventData);
        }

        /// <summary>
        /// Override the key - as the JSON serialized version loses the Type.
        /// </summary>
        private static EventData OverrideKey(EventData ed, AzureEventHubs.EventData eh)
        {
            if (eh.Properties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                ed.Key = key;

            return ed;
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftEventHubs.EventData"/> instance to the corresponding <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData ToBeefEventData(this MicrosoftEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData>(body), eventData);
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftEventHubs.EventData"/> instance to the corresponding <see cref="EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData<T> ToBeefEventData<T>(this MicrosoftEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return (EventData<T>)OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData<T>>(body), eventData);
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftEventHubs.EventData"/> instance to the corresponding <paramref name="valueType"/> <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData ToBeefEventData(this MicrosoftEventHubs.EventData eventData, Type valueType)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            Beef.Check.NotNull(valueType, nameof(valueType));

            var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            return OverrideKey((EventData)JsonConvert.DeserializeObject(body, typeof(EventData<>).MakeGenericType(new Type[] { valueType }))!, eventData);
        }

        /// <summary>
        /// Override the key - as the JSON serialized version loses the Type.
        /// </summary>
        private static EventData OverrideKey(EventData ed, MicrosoftEventHubs.EventData eh)
        {
            if (eh.Properties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                ed.Key = key;

            return ed;
        }

        /// <summary>
        /// Converts the <see cref="EventData"/> to a corresponding <see cref="MicrosoftEventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static MicrosoftEventHubs.EventData ToEventHubsEventData(this EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (string.IsNullOrEmpty(eventData.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(eventData));

            var json = JsonConvert.SerializeObject(eventData);
            var bytes = Encoding.UTF8.GetBytes(json);
            var ed = new MicrosoftEventHubs.EventData(bytes);

            ed.Properties.Add(EventMetadata.EventIdPropertyName, eventData.EventId);
            ed.Properties.Add(EventMetadata.SubjectPropertyName, eventData.Subject);
            ed.Properties.Add(EventMetadata.ActionPropertyName, eventData.Action);
            ed.Properties.Add(EventMetadata.TenantIdPropertyName, eventData.TenantId);
            ed.Properties.Add(EventMetadata.KeyPropertyName, eventData.Key);
            ed.Properties.Add(EventMetadata.CorrelationIdPropertyName, eventData.CorrelationId);
            ed.Properties.Add(EventMetadata.PartitionKeyPropertyName, eventData.PartitionKey);

            return ed;
        }

        /// <summary>
        /// Converts the <see cref="EventData"/> to a corresponding <see cref="AzureEventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static AzureEventHubs.EventData ToAzureEventHubsEventData(this EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (string.IsNullOrEmpty(eventData.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(eventData));

            var json = JsonConvert.SerializeObject(eventData);
            var bytes = Encoding.UTF8.GetBytes(json);
            var ed = new AzureEventHubs.EventData(bytes);

            ed.Properties.Add(EventMetadata.EventIdPropertyName, eventData.EventId);
            ed.Properties.Add(EventMetadata.SubjectPropertyName, eventData.Subject);
            ed.Properties.Add(EventMetadata.ActionPropertyName, eventData.Action);
            ed.Properties.Add(EventMetadata.TenantIdPropertyName, eventData.TenantId);
            ed.Properties.Add(EventMetadata.KeyPropertyName, eventData.Key);
            ed.Properties.Add(EventMetadata.CorrelationIdPropertyName, eventData.CorrelationId);
            ed.Properties.Add(EventMetadata.PartitionKeyPropertyName, eventData.PartitionKey);

            return ed;
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="AzureEventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The values of the following properties: <see cref="EventMetadata.EventIdPropertyName"/>, <see cref="EventMetadata.SubjectPropertyName"/>, <see cref="EventMetadata.ActionPropertyName"/>,
        /// <see cref="EventMetadata.TenantIdPropertyName"/>, <see cref="EventMetadata.CorrelationIdPropertyName"/>, and <see cref="EventMetadata.PartitionKeyPropertyName"/>.</returns>
        public static (Guid? EventId, string? Subject, string? Action, Guid? TenantId, string? CorrelationId, string? PartitionKey) GetBeefMetadata(this AzureEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            eventData.Properties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);
            eventData.Properties.TryGetValue(EventMetadata.ActionPropertyName, out var action);

            var eventId = (eventData.Properties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null;
            var tenantId = (eventData.Properties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null;
            eventData.Properties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
            eventData.Properties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);

            return (eventId, (string?)subject, (string?)action, tenantId, (string?)correlationId, (string?)partitionKey);
        }

        /// <summary>
        /// Gets the <i>Beef</i>-related metadata from the <see cref="MicrosoftEventHubs.EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="MicrosoftEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        public static EventMetadata GetEventMetadata(this MicrosoftEventHubs.EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            eventData.Properties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);
            eventData.Properties.TryGetValue(EventMetadata.ActionPropertyName, out var action);
            eventData.Properties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
            eventData.Properties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);

            return new EventMetadata
            {
                EventId = (eventData.Properties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null,
                TenantId = (eventData.Properties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null,
                Subject = (string?)subject,
                Action = (string?)action,
                CorrelationId = (string?)correlationId,
                PartitionKey = (string?)partitionKey
            };
        }
    }
}