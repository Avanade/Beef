// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

#pragma warning disable CA1716 // Identifiers should not match keywords; best name so it stays!

using System.Threading.Tasks;
using AzureEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Enables the <see cref="AzureEventHubs.EventData"/> <b>Azure Storage</b> repository.
    /// </summary>
    public interface IEventHubStorageRepository : IAuditWriter<EventHubData>
    {
        /// <summary>
        /// Checks whether the <paramref name="event"/> is considered poison.
        /// </summary>
        /// <param name="event">The <see cref="EventHubData"/>.</param>
        /// <returns>The <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> CheckPoisonedAsync(EventHubData @event);

        /// <summary>
        /// Marks the <paramref name="event"/> with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventHubData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task MarkAsPoisonedAsync(EventHubData @event, Result result);

        /// <summary>
        /// Marks the previously poisoned <paramref name="event"/> to skip.
        /// </summary>
        /// <param name="event">The <see cref="EventHubData"/>.</param>
        Task SkipPoisonedAsync(EventHubData @event);

        /// <summary>
        /// Removes the poisoned <paramref name="event"/>.
        /// </summary>
        /// <param name="event">The <see cref="EventHubData"/>.</param>
        Task RemovePoisonedAsync(EventHubData @event);
    }
}

#pragma warning restore CA1716