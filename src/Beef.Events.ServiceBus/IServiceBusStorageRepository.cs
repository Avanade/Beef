// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Enables the <see cref="AzureServiceBus.ServiceBusMessage"/> <b>Azure Storage</b> repository.
    /// </summary>
    public interface IServiceBusStorageRepository : IAuditWriter<ServiceBusData>
    {
        /// <summary>
        /// Checks whether the <paramref name="message"/> is considered poison.
        /// </summary>
        /// <param name="message">The <see cref="ServiceBusData"/>.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAsync(ServiceBusData message);

        /// <summary>
        /// Marks the <paramref name="message"/> with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="message">The <see cref="ServiceBusData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <param name="maxAttempts">The maximum number of attempts; a <c>null</c> or any non-positive number indicates infinite.</param>
        /// <returns>The resulting <see cref="UnhandledExceptionHandling"/>.</returns>
        Task<UnhandledExceptionHandling> MarkAsPoisonedAsync(ServiceBusData message, Result result, int? maxAttempts);

        /// <summary>
        /// Marks the previously poisoned <paramref name="message"/> to skip and updates the internal attempts counter.
        /// </summary>
        /// <param name="message">The <see cref="ServiceBusData"/>.</param>
        Task SkipPoisonedAsync(ServiceBusData message);

        /// <summary>
        /// Removes the poisoned <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The <see cref="ServiceBusData"/>.</param>
        Task RemovePoisonedAsync(ServiceBusData message);
    }
}