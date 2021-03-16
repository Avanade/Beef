// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Text;
using AzureServiceBus = Azure.Messaging.ServiceBus;
using MicrosoftServiceBus = Microsoft.Azure.ServiceBus;

namespace Beef.Events
{
    /// <summary>
    /// Provides <see cref="AzureServiceBus.ServiceBusMessage"/> to / from <see cref="EventData"/> mapping (as extension methods). <b>Beef</b> automatically adds the <see cref="EventMetadata"/> properties.
    /// </summary>
    public static class EventDataMapper
    {
        /// <summary>
        /// Converts the <see cref="AzureServiceBus.ServiceBusMessage"/> instance to the corresponding <see cref="EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData ToBeefEventData(this AzureServiceBus.ServiceBusMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var body = Encoding.UTF8.GetString(message.Body);
            return OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData>(body), message);
        }

        /// <summary>
        /// Converts the <see cref="AzureServiceBus.ServiceBusMessage"/> instance to the corresponding <see cref="EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData<T> ToBeefEventData<T>(this AzureServiceBus.ServiceBusMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var body = Encoding.UTF8.GetString(message.Body);
            return (EventData<T>)OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData<T>>(body), message);
        }

        /// <summary>
        /// Converts the <see cref="AzureServiceBus.ServiceBusMessage"/> instance to the corresponding <paramref name="valueType"/> <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData ToBeefEventData(this AzureServiceBus.ServiceBusMessage message, Type valueType)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Beef.Check.NotNull(valueType, nameof(valueType));

            var body = Encoding.UTF8.GetString(message.Body);
            return OverrideKey((EventData)JsonConvert.DeserializeObject(body, typeof(EventData<>).MakeGenericType(new Type[] { valueType }))!, message);
        }

        /// <summary>
        /// Override the key - as the JSON serialized version loses the Type.
        /// </summary>
        private static EventData OverrideKey(EventData ed, AzureServiceBus.ServiceBusMessage msg)
        {
            if (msg.ApplicationProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                ed.Key = key;

            return ed;
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftServiceBus.Message"/> instance to the corresponding <see cref="EventData"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static EventData ToBeefEventData(this MicrosoftServiceBus.Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var body = Encoding.UTF8.GetString(message.Body);
            return OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData>(body), message);
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftServiceBus.Message"/> instance to the corresponding <see cref="EventData{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData<T> ToBeefEventData<T>(this MicrosoftServiceBus.Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var body = Encoding.UTF8.GetString(message.Body);
            return (EventData<T>)OverrideKey(Newtonsoft.Json.JsonConvert.DeserializeObject<EventData<T>>(body), message);
        }

        /// <summary>
        /// Converts the <see cref="MicrosoftServiceBus.Message"/> instance to the corresponding <paramref name="valueType"/> <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <param name="valueType">The value <see cref="Type"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        public static EventData ToBeefEventData(this MicrosoftServiceBus.Message message, Type valueType)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Beef.Check.NotNull(valueType, nameof(valueType));

            var body = Encoding.UTF8.GetString(message.Body);
            return OverrideKey((EventData)JsonConvert.DeserializeObject(body, typeof(EventData<>).MakeGenericType(new Type[] { valueType }))!, message);
        }

        /// <summary>
        /// Override the key - as the JSON serialized version loses the Type.
        /// </summary>
        private static EventData OverrideKey(EventData ed, MicrosoftServiceBus.Message msg)
        {
            if (msg.UserProperties.TryGetValue(EventMetadata.KeyPropertyName, out var key))
                ed.Key = key;

            return ed;
        }

        /// <summary>
        /// Converts the <see cref="EventData"/> to a corresponding <see cref="AzureServiceBus.ServiceBusMessage"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static AzureServiceBus.ServiceBusMessage ToAzureServiceBusMessage(this EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (string.IsNullOrEmpty(eventData.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(eventData));

            var json = JsonConvert.SerializeObject(eventData);
            var bytes = Encoding.UTF8.GetBytes(json);
            var msg = new AzureServiceBus.ServiceBusMessage(bytes)
            {
                Subject = eventData.Subject,
                ContentType = "Beef.Events.EventData"
            };

            if (eventData.EventId != null)
                msg.MessageId = eventData.EventId?.ToString();

            if (eventData.CorrelationId != null)
                msg.CorrelationId = eventData.CorrelationId;

            if (eventData.PartitionKey != null)
                msg.PartitionKey = eventData.PartitionKey;

            msg.ApplicationProperties.Add(EventMetadata.EventIdPropertyName, eventData.EventId);
            msg.ApplicationProperties.Add(EventMetadata.SubjectPropertyName, eventData.Subject);
            msg.ApplicationProperties.Add(EventMetadata.ActionPropertyName, eventData.Action);
            msg.ApplicationProperties.Add(EventMetadata.TenantIdPropertyName, eventData.TenantId);
            msg.ApplicationProperties.Add(EventMetadata.KeyPropertyName, eventData.Key);
            msg.ApplicationProperties.Add(EventMetadata.CorrelationIdPropertyName, eventData.CorrelationId);
            msg.ApplicationProperties.Add(EventMetadata.PartitionKeyPropertyName, eventData.PartitionKey);

            return msg;
        }

        /// <summary>
        /// Converts the <see cref="EventData"/> to a corresponding <see cref="AzureServiceBus.ServiceBusMessage"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        public static MicrosoftServiceBus.Message ToServiceBusMessage(this EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (string.IsNullOrEmpty(eventData.Subject))
                throw new ArgumentException("Subject property is required to be set.", nameof(eventData));

            var json = JsonConvert.SerializeObject(eventData);
            var bytes = Encoding.UTF8.GetBytes(json);
            var msg = new MicrosoftServiceBus.Message(bytes)
            {
                ContentType = "Beef.Events.EventData"
            };

            if (eventData.EventId != null)
                msg.MessageId = eventData.EventId?.ToString();

            if (eventData.CorrelationId != null)
                msg.CorrelationId = eventData.CorrelationId;

            if (eventData.PartitionKey != null)
                msg.PartitionKey = eventData.PartitionKey;

            msg.UserProperties.Add(EventMetadata.EventIdPropertyName, eventData.EventId);
            msg.UserProperties.Add(EventMetadata.SubjectPropertyName, eventData.Subject);
            msg.UserProperties.Add(EventMetadata.ActionPropertyName, eventData.Action);
            msg.UserProperties.Add(EventMetadata.TenantIdPropertyName, eventData.TenantId);
            msg.UserProperties.Add(EventMetadata.KeyPropertyName, eventData.Key);
            msg.UserProperties.Add(EventMetadata.CorrelationIdPropertyName, eventData.CorrelationId);
            msg.UserProperties.Add(EventMetadata.PartitionKeyPropertyName, eventData.PartitionKey);

            return msg;
        }

        /// <summary>
        /// Gets the <i>Beef</i>-related metadata from the <see cref="AzureServiceBus.ServiceBusMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="AzureServiceBus.ServiceBusMessage"/>.</param>
        /// <returns>The values of the following properties: <see cref="EventMetadata.EventIdPropertyName"/>, <see cref="EventMetadata.SubjectPropertyName"/>, <see cref="EventMetadata.ActionPropertyName"/>,
        /// <see cref="EventMetadata.TenantIdPropertyName"/>, <see cref="EventMetadata.CorrelationIdPropertyName"/>, and <see cref="EventMetadata.PartitionKeyPropertyName"/>.</returns>
        public static (Guid? EventId, string? Subject, string? Action, Guid? TenantId, string? CorrelationId, string? PartitionKey) GetBeefMetadata(this AzureServiceBus.ServiceBusMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.ApplicationProperties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);
            message.ApplicationProperties.TryGetValue(EventMetadata.ActionPropertyName, out var action);

            var eventId = (message.ApplicationProperties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null;
            var tenantId = (message.ApplicationProperties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null;
            message.ApplicationProperties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
            message.ApplicationProperties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);

            return (eventId, (string?)subject, (string?)action, tenantId, (string?)correlationId, (string?)partitionKey);
        }

        /// <summary>
        /// Gets the <i>Beef</i>-related metadata from the <see cref="MicrosoftServiceBus.Message"/>.
        /// </summary>
        /// <param name="message">The <see cref="MicrosoftServiceBus.Message"/>.</param>
        /// <returns>The values of the following properties: <see cref="EventMetadata.EventIdPropertyName"/>, <see cref="EventMetadata.SubjectPropertyName"/>, <see cref="EventMetadata.ActionPropertyName"/>,
        /// <see cref="EventMetadata.TenantIdPropertyName"/>, <see cref="EventMetadata.CorrelationIdPropertyName"/>, and <see cref="EventMetadata.PartitionKeyPropertyName"/>.</returns>
        public static (Guid? EventId, string? Subject, string? Action, Guid? TenantId, string? CorrelationId, string? PartitionKey) GetBeefMetadata(this MicrosoftServiceBus.Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.UserProperties.TryGetValue(EventMetadata.SubjectPropertyName, out var subject);
            message.UserProperties.TryGetValue(EventMetadata.ActionPropertyName, out var action);

            var eventId = (message.UserProperties.TryGetValue(EventMetadata.EventIdPropertyName, out var eid) && eid != null && eid is Guid?) ? (Guid?)eid : null;
            var tenantId = (message.UserProperties.TryGetValue(EventMetadata.TenantIdPropertyName, out var tid) && tid != null && tid is Guid?) ? (Guid?)tid : null;
            message.UserProperties.TryGetValue(EventMetadata.CorrelationIdPropertyName, out var correlationId);
            message.UserProperties.TryGetValue(EventMetadata.PartitionKeyPropertyName, out var partitionKey);

            return (eventId, (string?)subject, (string?)action, tenantId, (string?)correlationId, (string?)partitionKey);
        }
    }
}