/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options
#pragma warning disable CA2227, CA1819 // Collection/Array properties should be read only; ignored, as acceptable for a DTO.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Entities
{
    /// <summary>
    /// Represents the Credit Card Account entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CreditCardAccount : EntityBase, IEquatable<CreditCardAccount>
    {
        #region Privates

        private decimal _minPaymentAmount;
        private decimal _paymentDueAmount;
        private string? _paymentCurrency;
        private DateTime _paymentDueDate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Min Payment Amount.
        /// </summary>
        [JsonProperty("minPaymentAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Min Payment Amount")]
        public decimal MinPaymentAmount
        {
            get => _minPaymentAmount;
            set => SetValue(ref _minPaymentAmount, value, false, false, nameof(MinPaymentAmount));
        }

        /// <summary>
        /// Gets or sets the Payment Due Amount.
        /// </summary>
        [JsonProperty("paymentDueAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Payment Due Amount")]
        public decimal PaymentDueAmount
        {
            get => _paymentDueAmount;
            set => SetValue(ref _paymentDueAmount, value, false, false, nameof(PaymentDueAmount));
        }

        /// <summary>
        /// Gets or sets the Payment Currency.
        /// </summary>
        [JsonProperty("paymentCurrency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Payment Currency")]
        public string? PaymentCurrency
        {
            get => _paymentCurrency;
            set => SetValue(ref _paymentCurrency, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(PaymentCurrency));
        }

        /// <summary>
        /// Gets or sets the Payment Due Date.
        /// </summary>
        [JsonProperty("paymentDueDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Payment Due Date")]
        public DateTime PaymentDueDate
        {
            get => _paymentDueDate;
            set => SetValue(ref _paymentDueDate, value, false, DateTimeTransform.DateOnly, nameof(PaymentDueDate));
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is CreditCardAccount val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="CreditCardAccount"/> is equal to the current <see cref="CreditCardAccount"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="CreditCardAccount"/> to compare with the current <see cref="CreditCardAccount"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="CreditCardAccount"/> is equal to the current <see cref="CreditCardAccount"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(CreditCardAccount? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(MinPaymentAmount, value.MinPaymentAmount)
                && Equals(PaymentDueAmount, value.PaymentDueAmount)
                && Equals(PaymentCurrency, value.PaymentCurrency)
                && Equals(PaymentDueDate, value.PaymentDueDate);
        }

        /// <summary>
        /// Compares two <see cref="CreditCardAccount"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="CreditCardAccount"/> A.</param>
        /// <param name="b"><see cref="CreditCardAccount"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (CreditCardAccount? a, CreditCardAccount? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="CreditCardAccount"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="CreditCardAccount"/> A.</param>
        /// <param name="b"><see cref="CreditCardAccount"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (CreditCardAccount? a, CreditCardAccount? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="CreditCardAccount"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="CreditCardAccount"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(MinPaymentAmount);
            hash.Add(PaymentDueAmount);
            hash.Add(PaymentCurrency);
            hash.Add(PaymentDueDate);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="CreditCardAccount"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="CreditCardAccount"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<CreditCardAccount>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="CreditCardAccount"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="CreditCardAccount"/> to copy from.</param>
        public void CopyFrom(CreditCardAccount from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            MinPaymentAmount = from.MinPaymentAmount;
            PaymentDueAmount = from.PaymentDueAmount;
            PaymentCurrency = from.PaymentCurrency;
            PaymentDueDate = from.PaymentDueDate;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="CreditCardAccount"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="CreditCardAccount"/>.</returns>
        public override object Clone()
        {
            var clone = new CreditCardAccount();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="CreditCardAccount"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            MinPaymentAmount = Cleaner.Clean(MinPaymentAmount);
            PaymentDueAmount = Cleaner.Clean(PaymentDueAmount);
            PaymentCurrency = Cleaner.Clean(PaymentCurrency, StringTrim.UseDefault, StringTransform.UseDefault);
            PaymentDueDate = Cleaner.Clean(PaymentDueDate, DateTimeTransform.DateOnly);

            OnAfterCleanUp();
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial
        {
            get
            {
                return Cleaner.IsInitial(MinPaymentAmount)
                    && Cleaner.IsInitial(PaymentDueAmount)
                    && Cleaner.IsInitial(PaymentCurrency)
                    && Cleaner.IsInitial(PaymentDueDate);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(CreditCardAccount from);

        #endregion
    }
}

#pragma warning restore CA2227, CA1819
#pragma warning restore IDE0005
#nullable restore