/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoreEx.Entities;

namespace Cdr.Banking.Business.Data.Model
{
    /// <summary>
    /// Represents the Account model for data persistence model.
    /// </summary>
    public partial class Account : IIdentifier<string>
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the Creation Date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the Display Name.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the Nickname.
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// Gets or sets the Open Status.
        /// </summary>
        public string? OpenStatus { get; set; }

        /// <summary>
        /// Indicates whether Is Owned.
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        /// Gets or sets the Masked Number.
        /// </summary>
        public string? MaskedNumber { get; set; }

        /// <summary>
        /// Gets or sets the Product Category.
        /// </summary>
        public string? ProductCategory { get; set; }

        /// <summary>
        /// Gets or sets the Product Name.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Gets or sets the Bsb.
        /// </summary>
        public string? Bsb { get; set; }

        /// <summary>
        /// Gets or sets the Account Number.
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the Bundle Name.
        /// </summary>
        public string? BundleName { get; set; }

        /// <summary>
        /// Gets or sets the Specific Account U Type.
        /// </summary>
        public string? SpecificAccountUType { get; set; }

        /// <summary>
        /// Gets or sets the Term Deposit.
        /// </summary>
        public TermDepositAccount? TermDeposit { get; set; }

        /// <summary>
        /// Gets or sets the Credit Card.
        /// </summary>
        public CreditCardAccount? CreditCard { get; set; }

        /// <summary>
        /// Gets or sets the Balance.
        /// </summary>
        public Balance? Balance { get; set; }
    }
}

#pragma warning restore
#nullable restore