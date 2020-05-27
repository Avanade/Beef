// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;
using System;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Enables <see cref="PoisonMessage"/> persistence. There will be a single instance (created using the <see cref="PoisonMessagePersistence.Create"/>) per <c>ConsumerGroup/PartitionId</c> whose
    /// lifetime is managed within the <see cref="Beef.Events.Triggers.Listener.ResilientEventHubProcessor"/> to orchestrate the poison message persistence.
    /// </summary>
    public interface IPoisonMessagePersistence
    {
#pragma warning disable CA1716 // Identifiers should not match keywords; by-design, best name.
        /// <summary>
        /// A <i>potential</i> poisoned <see cref="EventData"/> has been identified and needs to be orchestrated. <i>Note:</i> only a single event per <c>ConsumerGroup/PartitionId</c> can be set to this state.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <param name="exception">The corresponding <see cref="Exception"/>.</param>
        Task SetAsync(EventHubs.EventData @event, Exception exception);

        /// <summary>
        /// A previously identified poisoned <see cref="EventData"/> has successfully processed and should be removed.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <param name="action">The corresponding reason (<see cref="PoisonMessageAction"/>) for removal.</param>
        Task RemoveAsync(EventHubs.EventData @event, PoisonMessageAction action);

        /// <summary>
        /// Checks whether the <see cref="EventData"/> is in a <b>Poison</b> state and determines the corresponding <see cref="PoisonMessageAction"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> CheckAsync(EventHubs.EventData @event);

        /// <summary>
        /// Audits (persist) the skipped <see cref="EventHubs.EventData"/>.
        /// </summary>
        /// <param name="event">The corresponding <see cref="EventData"/>.</param>
        /// <param name="exceptionText">The exception/reason text for skipping.</param>
        Task SkipAuditAsync(EventHubs.EventData @event, string exceptionText);
#pragma warning restore CA1716
    }
}