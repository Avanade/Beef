// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events
{
    /// <summary>
    /// Provides the <i>Beef</i> metadata property names.
    /// </summary>
    public static class EventMetadata
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
    }
}