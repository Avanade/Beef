// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.WebJobs.Description;
using System;

namespace Beef.Events.Triggers
{
    /// <summary>
    /// Attribute used to mark a job function that should be invoked using a resilient (transient-fault-handling) <b>Azure EventHub</b> trigger. Given the required resiliency retry requirements within this
    /// trigger it is unable to be executed on an Azure consumption plan; i.e. must be always-on. Finally, the invoked function can only accept a single <see cref="EventData"/> parameter; i.e. arrays are not
    /// supported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class ResilientEventHubTriggerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubTriggerAttribute"/> class.
        /// </summary>)
        /// <param name="eventHubName">The Event hub name to listen on for messages.</param>
        public ResilientEventHubTriggerAttribute(string eventHubName) => EventHubName = Check.NotNull(eventHubName, nameof(eventHubName));

        /// <summary>
        /// Gets the name of the event hub (defaults to "%EventHubName%").
        /// </summary>
        public string EventHubName { get; private set; }

        /// <summary>
        /// Gets or sets connection string (defaults to "EventHubConnectionString").
        /// </summary>
        public string? Connection { get; set; } = "EventHubConnectionString";

        /// <summary>
        /// Get or sets the optional name of the consumer group (defauts to "$Default" where not specified).
        /// </summary>
        public string? ConsumerGroup { get; set; }
    }
}