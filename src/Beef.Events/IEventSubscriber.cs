// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Enables the <see cref="EventData"/> subscriber processor capabilities.
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets the <see cref="RunAsUser"/> option.
        /// </summary>
        RunAsUser RunAsUser { get; }

        /// <summary>
        /// Gets the <see cref="UnhandledExceptionHandling"/> option.
        /// </summary>
        UnhandledExceptionHandling UnhandledExceptionHandling { get; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidEventData"/> status (overrides <see cref="EventSubscriberHostArgs.InvalidEventDataHandling"/>).
        /// </summary>
        ResultHandling? InvalidEventDataHandling { get; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (overrides <see cref="EventSubscriberHostArgs.DataNotFoundHandling"/>).
        /// </summary>
        ResultHandling? DataNotFoundHandling { get; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidData"/> status (overrides <see cref="EventSubscriberHostArgs.InvalidDataHandling"/>).
        /// </summary>
        ResultHandling? InvalidDataHandling { get; }

        /// <summary>
        /// Gets the maximum number of attempts before the event is automatically skipped.
        /// </summary>
        /// <remarks>This functionality is dependent on the <see cref="EventSubscriberHost"/> providing the functionality to check and action.</remarks>
        int? MaxAttempts { get; }

        /// <summary>
        /// Gets the value <see cref="Type"/> if any.
        /// </summary>
        Type? ValueType { get; }

        /// <summary>
        /// Indicates whether a <c>null</c> value is considered invalid data and a corresponding <see cref="Result.InvalidData(BusinessException, ResultHandling?)"/> should automatically result.
        /// </summary>
        bool ConsiderNullValueAsInvalidData { get; }

        /// <summary>
        /// Gets or sets the originating <see cref="IEventSubscriberData"/> (intended for internal use only).
        /// </summary>
        IEventSubscriberData? OriginatingData { get; set; }

        /// <summary>
        /// Receive and process the subscribed <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        Task<Result> ReceiveAsync(EventData eventData);
    }
}