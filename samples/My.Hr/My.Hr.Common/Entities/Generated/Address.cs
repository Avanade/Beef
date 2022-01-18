/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Newtonsoft.Json;

namespace My.Hr.Common.Entities
{
    /// <summary>
    /// Represents the Address entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Address
    {
        /// <summary>
        /// Gets or sets the Street1.
        /// </summary>
        [JsonProperty("street1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Street1 { get; set; }

        /// <summary>
        /// Gets or sets the Street2.
        /// </summary>
        [JsonProperty("street2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Street2 { get; set; }

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        [JsonProperty("city", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? City { get; set; }

        /// <summary>
        /// Gets the corresponding <see cref="State"/> text (read-only where selected).
        /// </summary>
        [JsonProperty("stateText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? StateText { get; set ; }

        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the Post Code.
        /// </summary>
        [JsonProperty("postCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? PostCode { get; set; }
    }
}

#pragma warning restore
#nullable restore