/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Newtonsoft.Json;

namespace Cdr.Banking.Business.Data.Model
{
    /// <summary>
    /// Represents the Credit Card Account model.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CreditCardAccount
    {
        /// <summary>
        /// Gets or sets the Min Payment Amount.
        /// </summary>
        [JsonProperty("minPaymentAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal MinPaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the Payment Due Amount.
        /// </summary>
        [JsonProperty("paymentDueAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal PaymentDueAmount { get; set; }

        /// <summary>
        /// Gets or sets the Payment Currency.
        /// </summary>
        [JsonProperty("paymentCurrency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? PaymentCurrency { get; set; }

        /// <summary>
        /// Gets or sets the Payment Due Date.
        /// </summary>
        [JsonProperty("paymentDueDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime PaymentDueDate { get; set; }
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore