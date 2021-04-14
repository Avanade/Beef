// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Events
{
    /// <summary>
    /// Details the <see cref="SubjectTemplate"/> and <see cref="Actions"/> for an <see cref="EventSubscriberBase"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EventSubscriberAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberAttribute"/> class.
        /// </summary>
        /// <param name="subjectTemplate">The <see cref="EventMetadata.Subject"/> template for the event required (can contain wildcard).</param>
        /// <param name="actions">The <see cref="EventMetadata.Action"/>(s); where none specified this indicates all.</param>
        public EventSubscriberAttribute(string subjectTemplate, params string[] actions)
        {
            SubjectTemplate = Check.NotEmpty(subjectTemplate, nameof(subjectTemplate));
            Actions = new List<string>(actions);
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata.Subject"/> template for the event required (subscribing to).
        /// </summary>
        public string SubjectTemplate { get; private set; }

        /// <summary>
        /// Gets the <see cref="EventMetadata.Action"/>(s); where none specified this indicates <i>all</i>.
        /// </summary>
        public List<string> Actions { get; private set; }
    }
}