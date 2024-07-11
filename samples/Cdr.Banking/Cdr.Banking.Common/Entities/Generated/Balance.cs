/*
 * This file is automatically generated; any changes will be lost. 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoreEx.Entities;

namespace Cdr.Banking.Common.Entities
{
    /// <summary>
    /// Represents the Balance entity.
    /// </summary>
    public partial class Balance : IIdentifier<string>
    {
        /// <summary>
        /// Gets or sets the <c>Account</c> identifier.
        /// </summary>
        [JsonPropertyName("accountId")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the Current Balance.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Gets or sets the Available Balance.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// Gets or sets the Credit Limit.
        /// </summary>
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// Gets or sets the Amortised Limit.
        /// </summary>
        public decimal AmortisedLimit { get; set; }

        /// <summary>
        /// Gets or sets the Currency.
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the Purses.
        /// </summary>
        public BalancePurseCollection? Purses { get; set; }
    }
}