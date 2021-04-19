// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events
{
    /// <summary>
    /// Represents the <see cref="EventData"/> <see cref="EventSubscriberData{TOriginating}"/>.
    /// </summary>
    public class EventDataSubscriberData : EventSubscriberData<EventData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDataSubscriberData"/> class.
        /// </summary>
        /// <param name="originating">The originating event/message.</param>
        public EventDataSubscriberData(EventData originating) : base(originating) { }
    }
}