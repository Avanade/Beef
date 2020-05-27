// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Subscribe
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
        /// Indicates that the data was invalid and therefore nothing could be actioned.
        /// </summary>
        InvalidData,

        /// <summary>
        /// Indicates that a subscriber could not be found for the Subject/Action conbination and was skipped (this cannot be returned as it is used internally).
        /// </summary>
        NotSubscribed,

        /// <summary>
        /// Indicates that an unhandled <see cref="System.Exception"/> occured and the <see cref="IEventSubscriber">subscriber</see> <see cref="IEventSubscriber.UnhandledExceptionHandling"/> was set to <see cref="UnhandledExceptionHandling.Stop"/>.
        /// </summary>
        UnhandledException,

        /// <summary>
        /// Indicates that an <see cref="System.Exception"/> occured and the <see cref="IEventSubscriber">subscriber</see> <see cref="IEventSubscriber.UnhandledExceptionHandling"/> was set to <see cref="UnhandledExceptionHandling.Continue"/>.
        /// </summary>
        ExceptionContinue
    }
}