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
    /// Represents the Balance entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Balance : EntityBase, IStringIdentifier, IEquatable<Balance>
    {
        #region Privates

        private string? _id;
        private decimal _currentBalance;
        private decimal _availableBalance;
        private decimal _creditLimit;
        private decimal _amortisedLimit;
        private string? _currency;
        private BalancePurseCollection? _purses;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Account"/> identifier.
        /// </summary>
        [JsonProperty("accountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Identifier")]
        public string? Id
        {
            get => _id;
            set => SetValue(ref _id, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Id));
        }

        /// <summary>
        /// Gets or sets the Current Balance.
        /// </summary>
        [JsonProperty("currentBalance", DefaultValueHandling = DefaultValueHandling.Include)]
        [Display(Name="Current Balance")]
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set => SetValue(ref _currentBalance, value, false, false, nameof(CurrentBalance));
        }

        /// <summary>
        /// Gets or sets the Available Balance.
        /// </summary>
        [JsonProperty("availableBalance", DefaultValueHandling = DefaultValueHandling.Include)]
        [Display(Name="Available Balance")]
        public decimal AvailableBalance
        {
            get => _availableBalance;
            set => SetValue(ref _availableBalance, value, false, false, nameof(AvailableBalance));
        }

        /// <summary>
        /// Gets or sets the Credit Limit.
        /// </summary>
        [JsonProperty("creditLimit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Credit Limit")]
        public decimal CreditLimit
        {
            get => _creditLimit;
            set => SetValue(ref _creditLimit, value, false, false, nameof(CreditLimit));
        }

        /// <summary>
        /// Gets or sets the Amortised Limit.
        /// </summary>
        [JsonProperty("amortisedLimit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Amortised Limit")]
        public decimal AmortisedLimit
        {
            get => _amortisedLimit;
            set => SetValue(ref _amortisedLimit, value, false, false, nameof(AmortisedLimit));
        }

        /// <summary>
        /// Gets or sets the Currency.
        /// </summary>
        [JsonProperty("currency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Currency")]
        public string? Currency
        {
            get => _currency;
            set => SetValue(ref _currency, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Currency));
        }

        /// <summary>
        /// Gets or sets the Purses.
        /// </summary>
        [JsonProperty("purses", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Purses")]
        public BalancePurseCollection? Purses
        {
            get => _purses;
            set => SetValue(ref _purses, value, false, true, nameof(Purses));
        }

        #endregion

        #region IChangeTracking

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes (resets <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        /// <remarks>Ends and commits the entity changes (see <see cref="EntityBase.EndEdit"/>).</remarks>
        public override void AcceptChanges()
        {
            Purses?.AcceptChanges();
            base.AcceptChanges();
        }

        /// <summary>
        /// Determines that until <see cref="AcceptChanges"/> is invoked property changes are to be logged (see <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        public override void TrackChanges()
        {
            Purses?.TrackChanges();
            base.TrackChanges();
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Balance val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="Balance"/> is equal to the current <see cref="Balance"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="Balance"/> to compare with the current <see cref="Balance"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Balance"/> is equal to the current <see cref="Balance"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Balance? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(Id, value.Id)
                && Equals(CurrentBalance, value.CurrentBalance)
                && Equals(AvailableBalance, value.AvailableBalance)
                && Equals(CreditLimit, value.CreditLimit)
                && Equals(AmortisedLimit, value.AmortisedLimit)
                && Equals(Currency, value.Currency)
                && Equals(Purses, value.Purses);
        }

        /// <summary>
        /// Compares two <see cref="Balance"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="Balance"/> A.</param>
        /// <param name="b"><see cref="Balance"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (Balance? a, Balance? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="Balance"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="Balance"/> A.</param>
        /// <param name="b"><see cref="Balance"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (Balance? a, Balance? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="Balance"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="Balance"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(CurrentBalance);
            hash.Add(AvailableBalance);
            hash.Add(CreditLimit);
            hash.Add(AmortisedLimit);
            hash.Add(Currency);
            hash.Add(Purses);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Balance"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Balance"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Balance>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Balance"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Balance"/> to copy from.</param>
        public void CopyFrom(Balance from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            Id = from.Id;
            CurrentBalance = from.CurrentBalance;
            AvailableBalance = from.AvailableBalance;
            CreditLimit = from.CreditLimit;
            AmortisedLimit = from.AmortisedLimit;
            Currency = from.Currency;
            Purses = CopyOrClone(from.Purses, Purses);

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Balance"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Balance"/>.</returns>
        public override object Clone()
        {
            var clone = new Balance();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Balance"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Id = Cleaner.Clean(Id, StringTrim.UseDefault, StringTransform.UseDefault);
            CurrentBalance = Cleaner.Clean(CurrentBalance);
            AvailableBalance = Cleaner.Clean(AvailableBalance);
            CreditLimit = Cleaner.Clean(CreditLimit);
            AmortisedLimit = Cleaner.Clean(AmortisedLimit);
            Currency = Cleaner.Clean(Currency, StringTrim.UseDefault, StringTransform.UseDefault);
            Purses = Cleaner.Clean(Purses);

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
                return Cleaner.IsInitial(Id)
                    && Cleaner.IsInitial(CurrentBalance)
                    && Cleaner.IsInitial(AvailableBalance)
                    && Cleaner.IsInitial(CreditLimit)
                    && Cleaner.IsInitial(AmortisedLimit)
                    && Cleaner.IsInitial(Currency)
                    && Cleaner.IsInitial(Purses);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Balance from);

        #endregion
    }
}

#pragma warning restore CA2227, CA1819
#pragma warning restore IDE0005
#nullable restore