// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events
{
    /// <summary>
    /// Provides the <i>Beef</i> metadata property names.
    /// </summary>
    public class EventMetadata
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
        /// Gets or sets the <b>CorrelationId</b> property name.
        /// </summary>
        public static string CorrelationIdPropertyName { get; set; } = "Beef.CorrelationId";

        /// <summary>
        /// Gets or sets the <b>PartitionKey</b> property name.
        /// </summary>
        public static string PartitionKeyPropertyName { get; set; } = "Beef.PartitionKey";

        /// <summary>
        /// Gets or sets the unique event identifier.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the event subject (the name should use the '.' character to denote paths).
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the entity key (could be single value or an array of values).
        /// </summary>
        public object? Key { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string? PartitionKey { get; set; }
    }
}