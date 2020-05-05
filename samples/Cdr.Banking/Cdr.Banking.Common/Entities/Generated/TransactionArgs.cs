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
    /// Represents the <see cref="Account"/> arguments entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class TransactionArgs : EntityBase, IEquatable<TransactionArgs>
    {
        #region Privates

        private DateTime? _fromDate;
        private DateTime? _toDate;
        private decimal? _minAmount;
        private decimal? _maxAmount;
        private string? _text;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the From (oldest time).
        /// </summary>
        [JsonProperty("oldest-time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Oldest time")]
        [DisplayFormat(DataFormatString = Beef.Entities.StringFormat.DateTimeFormat)]
        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetValue(ref _fromDate, value, false, DateTimeTransform.DateTimeLocal, nameof(FromDate)); 
        }

        /// <summary>
        /// Gets or sets the To (newest time).
        /// </summary>
        [JsonProperty("newest-time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Newest time")]
        [DisplayFormat(DataFormatString = Beef.Entities.StringFormat.DateTimeFormat)]
        public DateTime? ToDate
        {
            get => _toDate;
            set => SetValue(ref _toDate, value, false, DateTimeTransform.DateTimeLocal, nameof(ToDate)); 
        }

        /// <summary>
        /// Gets or sets the Min Amount.
        /// </summary>
        [JsonProperty("min-amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Min Amount")]
        public decimal? MinAmount
        {
            get => _minAmount;
            set => SetValue(ref _minAmount, value, false, false, nameof(MinAmount)); 
        }

        /// <summary>
        /// Gets or sets the Max Amount.
        /// </summary>
        [JsonProperty("max-amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Max Amount")]
        public decimal? MaxAmount
        {
            get => _maxAmount;
            set => SetValue(ref _maxAmount, value, false, false, nameof(MaxAmount)); 
        }

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Text")]
        public string? Text
        {
            get => _text;
            set => SetValue(ref _text, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(Text)); 
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
            if (obj == null || !(obj is TransactionArgs val))
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TransactionArgs"/> is equal to the current <see cref="TransactionArgs"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(TransactionArgs? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return base.Equals((object)value)
                && Equals(FromDate, value.FromDate)
                && Equals(ToDate, value.ToDate)
                && Equals(MinAmount, value.MinAmount)
                && Equals(MaxAmount, value.MaxAmount)
                && Equals(Text, value.Text);
        }

        /// <summary>
        /// Compares two <see cref="TransactionArgs"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="TransactionArgs"/> A.</param>
        /// <param name="b"><see cref="TransactionArgs"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (TransactionArgs? a, TransactionArgs? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="TransactionArgs"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="TransactionArgs"/> A.</param>
        /// <param name="b"><see cref="TransactionArgs"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (TransactionArgs? a, TransactionArgs? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="TransactionArgs"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="TransactionArgs"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(FromDate);
            hash.Add(ToDate);
            hash.Add(MinAmount);
            hash.Add(MaxAmount);
            hash.Add(Text);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion
        
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="TransactionArgs"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TransactionArgs"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<TransactionArgs>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="TransactionArgs"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TransactionArgs"/> to copy from.</param>
        public void CopyFrom(TransactionArgs from)
        {
            CopyFrom((EntityBase)from);
            FromDate = from.FromDate;
            ToDate = from.ToDate;
            MinAmount = from.MinAmount;
            MaxAmount = from.MaxAmount;
            Text = from.Text;

            OnAfterCopyFrom(from);
        }
    
        #endregion
        
        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="TransactionArgs"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="TransactionArgs"/>.</returns>
        public override object Clone()
        {
            var clone = new TransactionArgs();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="TransactionArgs"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            FromDate = Cleaner.Clean(FromDate, DateTimeTransform.DateTimeLocal);
            ToDate = Cleaner.Clean(ToDate, DateTimeTransform.DateTimeLocal);
            MinAmount = Cleaner.Clean(MinAmount);
            MaxAmount = Cleaner.Clean(MaxAmount);
            Text = Cleaner.Clean(Text, StringTrim.End, StringTransform.EmptyToNull);

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
                return Cleaner.IsInitial(FromDate)
                    && Cleaner.IsInitial(ToDate)
                    && Cleaner.IsInitial(MinAmount)
                    && Cleaner.IsInitial(MaxAmount)
                    && Cleaner.IsInitial(Text);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(TransactionArgs from);

        #endregion
    } 
}

#nullable restore