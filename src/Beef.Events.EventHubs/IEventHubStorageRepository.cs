// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Poison;
using AzureEventHubs = Azure.Messaging.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Enables the <see cref="AzureEventHubs.EventData"/> <b>Azure Storage</b> repository.
    /// </summary>
    public interface IEventHubStorageRepository : IStorageRepository<EventHubData> { }
}