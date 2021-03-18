// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events.Repository
{
    /// <summary>
    /// Enables the <b>storage repository</b>.
    /// </summary>
    /// <typeparam name="TData">The event data <see cref="Type"/>.</typeparam>
    public interface IStorageRepository<TData> : IAuditWriter where TData : class, IEventSubscriberData
    {
        /// <summary>
        /// Checks whether the event is considered poison.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAsync(TData data);

        /// <summary>
        /// Marks the event with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <param name="maxAttempts">The maximum number of attempts; a <c>null</c> or any non-positive number indicates infinite.</param>
        /// <returns>The resulting <see cref="UnhandledExceptionHandling"/>.</returns>
        Task<UnhandledExceptionHandling> MarkAsPoisonedAsync(TData data, Result result, int? maxAttempts);

        /// <summary>
        /// Marks the previously poisoned event to skip and updates the internal attempts counter.
        /// </summary>
        /// <param name="data">The event data.</param>
        Task SkipPoisonedAsync(TData data);

        /// <summary>
        /// Removes the poisoned event.
        /// </summary>
        /// <param name="data">The event data.</param>
        Task RemovePoisonedAsync(TData data);
    }
}