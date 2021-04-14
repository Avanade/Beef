// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events
{
    /// <summary>
    /// Enables the base event/message data.
    /// </summary>
    public interface IEventSubscriberData
    {
        /// <summary>
        /// Gets the originating event/message.
        /// </summary>
        object Originating { get; }

        /// <summary>
        /// Gets the invocation attempt counter.
        /// </summary>
        /// <remarks>A value of zero indicates that the attempt count is currently unknown.</remarks>
        int Attempt { get; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata"/>.
        /// </summary>
        EventMetadata Metadata { get; }

        /// <summary>
        /// Sets the <see cref="EventMetadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="EventMetadata"/>.</param>
        void SetMetadata(EventMetadata metadata);
    }

    /// <summary>
    /// Provides for the base event/message data.
    /// </summary>
    public abstract class EventSubscriberData<TOriginating> : IEventSubscriberData where TOriginating : class
    {
        private EventMetadata? _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberData{TOriginating}"/> class.
        /// </summary>
        /// <param name="originating">The originating event/message.</param>
        public EventSubscriberData(TOriginating originating) => Originating = Check.NotNull(originating, nameof(originating));

        /// <summary>
        /// Gets the originating event/message.
        /// </summary>
        object IEventSubscriberData.Originating => Originating;

        /// <summary>
        /// Gets the originating event/message.
        /// </summary>
        public TOriginating Originating { get; }

        /// <summary>
        /// Gets or sets the invocation attempt counter.
        /// </summary>
        /// <remarks>This is managed (updated) internally and should be assumed is read only. A value of zero indicates that the attempt count is currently unknown.</remarks>
        public int Attempt { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata"/>.
        /// </summary>
        public EventMetadata Metadata => _metadata ?? throw new InvalidOperationException("Metadata property must not be accessed prior to being set.");

        /// <summary>
        /// Sets the <see cref="EventMetadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="EventMetadata"/>.</param>
        void IEventSubscriberData.SetMetadata(EventMetadata metadata) => _metadata = Check.NotNull(metadata, nameof(metadata));
    }
}