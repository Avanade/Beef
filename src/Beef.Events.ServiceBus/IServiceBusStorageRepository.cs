// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Poison;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Enables the <see cref="AzureServiceBus.ServiceBusMessage"/> <b>Azure Storage</b> repository.
    /// </summary>
    public interface IServiceBusStorageRepository : IStorageRepository<ServiceBusData> { }
}