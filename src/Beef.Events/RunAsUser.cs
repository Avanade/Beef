// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events
{
    /// <summary>
    /// Provides the run as user options as either <see cref="Originating"/> or <see cref="System"/>.
    /// </summary>
    public enum RunAsUser
    {
        /// <summary>
        /// Run as the originating user (see <see cref="EventData.Username"/>) for the message.
        /// </summary>
        Originating,

        /// <summary>
        /// Run as a standard system user (see <see cref="EventSubscriberHost.SystemUsername"/>).
        /// </summary>
        System
    }
}