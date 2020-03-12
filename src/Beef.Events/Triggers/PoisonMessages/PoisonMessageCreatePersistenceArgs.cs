// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Represents the <see cref="PoisonMessagePersistence"/> <see cref="PoisonMessagePersistence.Create"/> arguments.
    /// </summary>
    public class PoisonMessageCreatePersistenceArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="ResilientEventHubOptions"/>.
        /// </summary>
        public ResilientEventHubOptions? Options { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfiguration"/>.
        /// </summary>
        public IConfiguration? Config { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PartitionContext"/>.
        /// </summary>
        public PartitionContext? Context { get; set; }
    }
}