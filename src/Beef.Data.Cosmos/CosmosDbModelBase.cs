// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the base <b>CosmosDb/DocumentDb</b> <b>model</b> capabilities.
    /// </summary>
    public abstract class CosmosDbModelBase : IStringIdentifier, IETag
    {
        /// <summary>
        /// Gets or sets the <see cref="IStringIdentifier"/>.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IETag"/>.
        /// </summary>
        [JsonProperty("_etag")]
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live (https://docs.microsoft.com/en-us/azure/cosmos-db/time-to-live).
        /// </summary>
        [JsonProperty("ttl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? TimeToLive { get; set; }
    }
}