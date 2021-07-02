// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Provides the Partition Key for an entity class to drive persistence partitioning where applicable.
    /// </summary>
    public interface IPartitionKey
    {
        /// <summary>
        /// Gets the partition key.
        /// </summary>
        string PartitionKey { get; }
    }
}