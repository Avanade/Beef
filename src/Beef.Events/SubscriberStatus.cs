﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events
{
    /// <summary>
    /// Provides the status of an <see cref="EventSubscriber.ReceiveAsync(EventData)"/> and <see cref="EventSubscriber{T}.ReceiveAsync(EventData{T})"/>. Note that where a subscriber throws a 
    /// <see cref="Beef.ValidationException"/> or <see cref="Beef.BusinessException"/> this will automatically result in an <see cref="InvalidData"/>; likewise, a <see cref="Beef.NotFoundException"/>
    /// will automatically result in a <see cref="DataNotFound"/>.
    /// </summary>
    public enum SubscriberStatus
    {
        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates that the corresponding data was not found and therefore nothing could be actioned.
        /// </summary>
        DataNotFound,

        /// <summary>
        /// Indicates that the <see cref="EventData"/> was invalid and therefore nothing could be actioned.
        /// </summary>
        InvalidEventData,

        /// <summary>
        /// Indicates that the data was invalid and therefore nothing could be actioned.
        /// </summary>
        InvalidData,

        /// <summary>
        /// Indicates that a subscriber could not be found for the Subject/Action conbination and was skipped (this cannot be returned as it is used internally).
        /// </summary>
        NotSubscribed,

        /// <summary>
        /// Indicates that an unhandled <see cref="System.Exception"/> occured and the <see cref="IEventSubscriber">subscriber</see> <see cref="IEventSubscriber.UnhandledExceptionHandling"/> was set to <see cref="UnhandledExceptionHandling.ThrowException"/>.
        /// </summary>
        /// <remarks>This will always result in an audit write (see <see cref="IAuditWriter.WriteAuditAsync(object, Result)"/>).</remarks>
        UnhandledException,

        /// <summary>
        /// Indicates that an <see cref="System.Exception"/> occured and the <see cref="IEventSubscriber">subscriber</see> <see cref="IEventSubscriber.UnhandledExceptionHandling"/> was set to <see cref="UnhandledExceptionHandling.Continue"/> (swallow and carry on).
        /// </summary>
        /// <remarks>This will always result in an audit write (see <see cref="IAuditWriter.WriteAuditAsync(object, Result)"/>).</remarks>
        ExceptionContinue,

        /// <summary>
        /// Indicates that the event is considered poison (continues to result in an <see cref="UnhandledException"/>) and has been explicitly marked as <see cref="PoisonMessageAction.PoisonSkip"/> (swallow and carry on)..
        /// </summary>
        PoisonSkipped,

        /// <summary>
        /// Indicates that the event does not match the expected poison message and it is uncertain whether it has been successfully processed and is audited accordingly.
        /// </summary>
        PoisonMismatch
    }
}