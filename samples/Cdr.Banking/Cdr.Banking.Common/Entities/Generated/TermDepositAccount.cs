/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

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
    /// Represents the Term Deposit Account entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class TermDepositAccount : EntityBase, IEquatable<TermDepositAccount>
    {
        #region Privates

        private DateTime _lodgementDate;
        private DateTime _maturityDate;
        private decimal _maturityAmount;
        private string? _maturityCurrency;
        private string? _maturityInstructionsSid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Lodgement Date.
        /// </summary>
        [JsonProperty("lodgementDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Lodgement Date")]
        [DisplayFormat(DataFormatString = Beef.Entities.StringFormat.DateOnlyFormat)]
        public DateTime LodgementDate
        {
            get => _lodgementDate;
            set => SetValue(ref _lodgementDate, value, false, DateTimeTransform.DateOnly, nameof(LodgementDate)); 
        }

        /// <summary>
        /// Gets or sets the Maturity Date.
        /// </summary>
        [JsonProperty("maturityDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Maturity Date")]
        [DisplayFormat(DataFormatString = Beef.Entities.StringFormat.DateOnlyFormat)]
        public DateTime MaturityDate
        {
            get => _maturityDate;
            set => SetValue(ref _maturityDate, value, false, DateTimeTransform.DateOnly, nameof(MaturityDate)); 
        }

        /// <summary>
        /// Gets or sets the Maturity Amount.
        /// </summary>
        [JsonProperty("maturityAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Maturity Amount")]
        public decimal MaturityAmount
        {
            get => _maturityAmount;
            set => SetValue(ref _maturityAmount, value, false, false, nameof(MaturityAmount)); 
        }

        /// <summary>
        /// Gets or sets the Maturity Currency.
        /// </summary>
        [JsonProperty("maturityCurrency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Maturity Currency")]
        public string? MaturityCurrency
        {
            get => _maturityCurrency;
            set => SetValue(ref _maturityCurrency, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(MaturityCurrency)); 
        }

        /// <summary>
        /// Gets or sets the <see cref="MaturityInstructions"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("maturityInstructions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Maturity Instructions")]
        public string? MaturityInstructionsSid
        {
            get => _maturityInstructionsSid;
            set => SetValue(ref _maturityInstructionsSid, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(MaturityInstructions));
        }

        /// <summary>
        /// Gets or sets the Maturity Instructions (see <see cref="RefDataNamespace.MaturityInstructions"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Maturity Instructions")]
        public RefDataNamespace.MaturityInstructions? MaturityInstructions
        {
            get => _maturityInstructionsSid;
            set => SetValue(ref _maturityInstructionsSid, value, false, false, nameof(MaturityInstructions)); 
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is TermDepositAccount val))
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TermDepositAccount"/> is equal to the current <see cref="TermDepositAccount"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(TermDepositAccount? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return base.Equals((object)value)
                && Equals(LodgementDate, value.LodgementDate)
                && Equals(MaturityDate, value.MaturityDate)
                && Equals(MaturityAmount, value.MaturityAmount)
                && Equals(MaturityCurrency, value.MaturityCurrency)
                && Equals(MaturityInstructionsSid, value.MaturityInstructionsSid);
        }

        /// <summary>
        /// Compares two <see cref="TermDepositAccount"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="TermDepositAccount"/> A.</param>
        /// <param name="b"><see cref="TermDepositAccount"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (TermDepositAccount? a, TermDepositAccount? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="TermDepositAccount"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="TermDepositAccount"/> A.</param>
        /// <param name="b"><see cref="TermDepositAccount"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (TermDepositAccount? a, TermDepositAccount? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="TermDepositAccount"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="TermDepositAccount"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(LodgementDate);
            hash.Add(MaturityDate);
            hash.Add(MaturityAmount);
            hash.Add(MaturityCurrency);
            hash.Add(MaturityInstructionsSid);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion
        
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="TermDepositAccount"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TermDepositAccount"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<TermDepositAccount>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="TermDepositAccount"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TermDepositAccount"/> to copy from.</param>
        public void CopyFrom(TermDepositAccount from)
        {
            CopyFrom((EntityBase)from);
            LodgementDate = from.LodgementDate;
            MaturityDate = from.MaturityDate;
            MaturityAmount = from.MaturityAmount;
            MaturityCurrency = from.MaturityCurrency;
            MaturityInstructionsSid = from.MaturityInstructionsSid;

            OnAfterCopyFrom(from);
        }
    
        #endregion
        
        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="TermDepositAccount"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="TermDepositAccount"/>.</returns>
        public override object Clone()
        {
            var clone = new TermDepositAccount();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="TermDepositAccount"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            LodgementDate = Cleaner.Clean(LodgementDate, DateTimeTransform.DateOnly);
            MaturityDate = Cleaner.Clean(MaturityDate, DateTimeTransform.DateOnly);
            MaturityAmount = Cleaner.Clean(MaturityAmount);
            MaturityCurrency = Cleaner.Clean(MaturityCurrency, StringTrim.End, StringTransform.EmptyToNull);
            MaturityInstructionsSid = Cleaner.Clean(MaturityInstructionsSid);

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
                return Cleaner.IsInitial(LodgementDate)
                    && Cleaner.IsInitial(MaturityDate)
                    && Cleaner.IsInitial(MaturityAmount)
                    && Cleaner.IsInitial(MaturityCurrency)
                    && Cleaner.IsInitial(MaturityInstructionsSid);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(TermDepositAccount from);

        #endregion
    } 
}

#nullable restore