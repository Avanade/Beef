// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

#pragma warning disable CA1716 // Identifiers should not match keywords; as 'event' is the best name it stays!

using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Enables event/message repository management.
    /// </summary>
    /// <remarks>Manages audit persistance (see <see cref="WriteAuditAsync"/>) and poison message handling where <see cref="IsPoisonSupportEnabled"/>.</remarks>
    public interface IEventRepository
    {
        /// <summary>
        /// Writes the event/message to the audit repository.
        /// </summary>
        /// <param name="event">The event/message.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(object @event, Result result);

        /// <summary>
        /// Indicates whether poison event/message support is enabled.
        /// </summary>
        bool IsPoisonSupportEnabled { get; }

        /// <summary>
        /// Checks whether the event/message is in a poison state and determines the corresponding <see cref="PoisonMessageAction"/>.
        /// </summary>
        /// <param name="event">The event/message.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> CheckPoisonedAsync(object @event);

        /// <summary>
        /// Mark an event/message as being poisoned and that it needs to be managed by either successfully retrying or being explictly skipped.
        /// </summary>
        /// <remarks>This needs to support insert and update depending on whether the first time identified.</remarks>
        /// <param name="event">The event/message.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> MarkAsPoisonedAsync(object @event, Result result);

        /// <summary>
        /// Remove the poisoned event/message.
        /// </summary>
        /// <remarks>This could be the result of a subsequent success or requested skip.</remarks>
        /// <param name="event">The event/message.</param>
        Task RemovePoisonedAsync(object @event);
    }

    /// <summary>
    /// Enables repository management of event/message <see cref="Type"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>Manages audit persistance (see <see cref="WriteAuditAsync"/>) and poison message handling where <see cref="IEventRepository.IsPoisonSupportEnabled"/>.</remarks>
    public interface IEventRepository<T> : IEventRepository where T : class
    {
        /// <summary>
        /// Writes the event/message to the audit repository.
        /// </summary>
        /// <param name="event">The event/message.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(T @event, Result result);

        /// <summary>
        /// Checks whether the event/message is in a <b>Poison</b> state and determines the corresponding <see cref="PoisonMessageAction"/>.
        /// </summary>
        /// <param name="event">The event/message.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> CheckPoisonedAsync(T @event);

        /// <summary>
        /// Mark a event/message as being Poisoned and that it needs to be managed by either successfully retrying or being explictly skipped.
        /// </summary>
        /// <remarks>This needs to support insert and update depending on whether the first time identified.</remarks>
        /// <param name="event">The event/message.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        Task<PoisonMessageAction> MarkAsPoisonedAsync(T @event, Result result);

        /// <summary>
        /// Remove the previously identified poisoned event/message.
        /// </summary>
        /// <remarks>This could be the result of a subsequent success or requested skip.</remarks>
        /// <param name="event">The event/message.</param>
        Task RemovePoisonedAsync(T @event);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        Task<PoisonMessageAction> IEventRepository.CheckPoisonedAsync(object @event) => CheckPoisonedAsync((T)@event);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <param name="result"><inheritdoc/></param>
        Task IEventRepository.WriteAuditAsync(object @event, Result result) => WriteAuditAsync((T)@event, result);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <param name="result"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        Task<PoisonMessageAction> IEventRepository.MarkAsPoisonedAsync(object @event, Result result) => MarkAsPoisonedAsync((T)@event, result);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        Task IEventRepository.RemovePoisonedAsync(object @event) => RemovePoisonedAsync((T)@event);
    }
}

#pragma warning restore CA1716 