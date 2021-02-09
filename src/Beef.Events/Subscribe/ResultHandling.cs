// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the <see cref="Result"/> handling; i.e. how to action a given <see cref="SubscriberStatus"/>.
    /// </summary>
    public enum ResultHandling
    {
        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is unexpected and should throw an <see cref="EventSubscriberUnhandledException"/> allowing the hosting process
        /// to determine the appropriate action based on its default unhandled exception behaviour.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue silently.
        /// </summary>
        ContinueSilent,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue after writing a log message using the <see cref="EventSubscriberHostArgs.Logger"/>.
        /// </summary>
        ContinueWithLogging,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue after writing an audit message using the <see cref="EventSubscriberHostArgs.AuditWriter"/>.
        /// </summary>
        ContinueWithAudit
    }
}