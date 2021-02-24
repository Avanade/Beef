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

namespace Cdr.Banking.Business.Data.Model
{
    /// <summary>
    /// Represents the Balance Purse model.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class BalancePurse
    {
        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        [JsonProperty("amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the Currency.
        /// </summary>
        [JsonProperty("currency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Currency { get; set; }
    }

    /// <summary>
    /// Represents the <see cref="BalancePurse"/> collection.
    /// </summary>
    public partial class BalancePurseCollection : List<BalancePurse> { }
}

#pragma warning restore
#nullable restore