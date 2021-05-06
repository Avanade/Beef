/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json;
using RefDataNamespace = My.Hr.Business.Entities;

namespace My.Hr.Business.Entities
{
    /// <summary>
    /// Represents the Termination Detail entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class TerminationDetail : EntityBase, IEquatable<TerminationDetail>
    {
        #region Privates

        private DateTime _date;
        private string? _reasonSid;
        private string? _reasonText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Date.
        /// </summary>
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Date")]
        public DateTime Date
        {
            get => _date;
            set => SetValue(ref _date, value, false, DateTimeTransform.DateOnly, nameof(Date));
        }

        /// <summary>
        /// Gets or sets the <see cref="Reason"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("reason", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Reason")]
        public string? ReasonSid
        {
            get => _reasonSid;
            set => SetValue(ref _reasonSid, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Reason));
        }

        /// <summary>
        /// Gets the corresponding {{Reason}} text (read-only where selected).
        /// </summary>
        [JsonProperty("reasonText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ReasonText { get => _reasonText ?? GetRefDataText(() => Reason); set => _reasonText = value; }

        /// <summary>
        /// Gets or sets the Reason (see <see cref="RefDataNamespace.TerminationReason"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Reason")]
        public RefDataNamespace.TerminationReason? Reason
        {
            get => _reasonSid;
            set => SetValue(ref _reasonSid, value, false, false, nameof(Reason)); 
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is TerminationDetail val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="TerminationDetail"/> is equal to the current <see cref="TerminationDetail"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/> to compare with the current <see cref="TerminationDetail"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="TerminationDetail"/> is equal to the current <see cref="TerminationDetail"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(TerminationDetail? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(Date, value.Date)
                && Equals(ReasonSid, value.ReasonSid);
        }

        /// <summary>
        /// Compares two <see cref="TerminationDetail"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="TerminationDetail"/> A.</param>
        /// <param name="b"><see cref="TerminationDetail"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (TerminationDetail? a, TerminationDetail? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="TerminationDetail"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="TerminationDetail"/> A.</param>
        /// <param name="b"><see cref="TerminationDetail"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (TerminationDetail? a, TerminationDetail? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="TerminationDetail"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="TerminationDetail"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Date);
            hash.Add(ReasonSid);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="TerminationDetail"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TerminationDetail"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<TerminationDetail>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="TerminationDetail"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="TerminationDetail"/> to copy from.</param>
        public void CopyFrom(TerminationDetail from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            Date = from.Date;
            ReasonSid = from.ReasonSid;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="TerminationDetail"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="TerminationDetail"/>.</returns>
        public override object Clone()
        {
            var clone = new TerminationDetail();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="TerminationDetail"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Date = Cleaner.Clean(Date, DateTimeTransform.DateOnly);
            ReasonSid = Cleaner.Clean(ReasonSid);

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
                return Cleaner.IsInitial(Date)
                    && Cleaner.IsInitial(ReasonSid);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(TerminationDetail from);

        #endregion
    }
}

#pragma warning restore
#nullable restore