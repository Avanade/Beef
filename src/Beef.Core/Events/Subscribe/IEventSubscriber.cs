// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Enables the <see cref="EventData"/> subscriber processor capabilities.
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> template for the event required (can contain wildcard).
        /// </summary>
        string SubjectTemplate { get; }

        /// <summary>
        /// Gets the <see cref="EventData.Action"/>(s); where none specified this indicates all.
        /// </summary>
        string[] Actions { get; }

        /// <summary>
        /// Gets the <see cref="RunAsUser"/> option.
        /// </summary>
        RunAsUser RunAsUser { get; }

        /// <summary>
        /// Gets the <see cref="UnhandledExceptionHandling"/> option.
        /// </summary>
        UnhandledExceptionHandling UnhandledExceptionHandling { get; }

        /// <summary>
        /// Gets the value <see cref="Type"/> if any.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Receive and process the subscribed <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task ReceiveAsync(EventData eventData);
    }
}