// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the <see cref="Result"/> handling; i.e. how to action a given <see cref="SubscriberStatus"/>.
    /// </summary>
    public enum ResultHandling
    {
        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is unexpected and should stop, allowing the hosting process to determine the appropriate action.
        /// </summary>
        Stop,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue silently.
        /// </summary>
        ContinueSilent,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue (write <see cref="Beef.Diagnostics.Logger">log message</see>).
        /// </summary>
        ContinueWithLogging,

        /// <summary>
        /// Indicates that the <see cref="IEventSubscriber">subscriber</see> <see cref="Result"/> is expected and to continue (an <see cref="EventData"/> audit is required).
        /// </summary>
        ContinueWithAudit
    }
}