// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Represents <b>Poison Message</b> processing action.
    /// </summary>
    public enum PoisonMessageAction
    {
        /// <summary>
        /// This is reserved for internal use; please do not use. 
        /// </summary>
        Undetermined,

        /// <summary>
        /// The message (event) is not poison and processing should occur as normal.
        /// </summary>
        NotPoison,

        /// <summary>
        /// The message (event) is considered poison and processing should continue to retry until successful or a <see cref="PoisonSkip"/> is initiated.
        /// </summary>
        PoisonRetry,

        /// <summary>
        /// The message (event) was determined as poison and should be skipped to allow further processing of messages (events).
        /// </summary>
        PoisonSkip
    }
}