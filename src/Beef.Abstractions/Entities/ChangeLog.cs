// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a Change log class.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ChangeLog : EntityBase, IEquatable<ChangeLog>
    {
        private DateTime? _createdDate;
        private string? _createdBy;
        private DateTime? _updatedDate;
        private string? _updatedBy;

        /// <summary>
        /// Gets or sets the Created <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedDate
        {
            get => _createdDate; 
            set => SetValue(ref _createdDate, value, false, DateTimeTransform.UseDefault, nameof(CreatedDate)); 
        }

        /// <summary>
        /// Gets or sets the Created username.
        /// </summary>
        [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
        public string? CreatedBy
        {
            get => _createdBy;
            set => SetValue(ref _createdBy!, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(CreatedBy));
        }

        /// <summary>
        /// Gets or sets the Updated <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("updatedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UpdatedDate
        {
            get => _updatedDate;
            set => SetValue(ref _updatedDate, value, false, DateTimeTransform.UseDefault, nameof(UpdatedDate));
        }

        /// <summary>
        /// Gets or sets the Updated username.
        /// </summary>
        [JsonProperty("updatedBy", NullValueHandling = NullValueHandling.Ignore)]
        public string? UpdatedBy
        {
            get => _updatedBy;
            set => SetValue(ref _updatedBy!, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(UpdatedBy));
        }

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is ChangeLog val))
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ChangeLog"/> is equal to the current <see cref="ChangeLog"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(ChangeLog? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return base.Equals((object)value)
                && Equals(CreatedDate, value.CreatedDate)
                && Equals(CreatedBy, value.CreatedBy)
                && Equals(UpdatedDate, value.UpdatedDate)
                && Equals(UpdatedBy, value.UpdatedBy);
        }

        /// <summary>
        /// Compares two <see cref="ChangeLog"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="ChangeLog"/> A.</param>
        /// <param name="b"><see cref="ChangeLog"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator ==(ChangeLog? a, ChangeLog? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="ChangeLog"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="ChangeLog"/> A.</param>
        /// <param name="b"><see cref="ChangeLog"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator !=(ChangeLog? a, ChangeLog? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="ChangeLog"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="ChangeLog"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(CreatedDate);
            hash.Add(CreatedBy);
            hash.Add(UpdatedDate);
            hash.Add(UpdatedBy);
            return base.GetHashCode() ^ hash.ToHashCode();
        }

        #endregion

        #region ICopyFrom

        /// <summary>
        /// Performs a copy from another <see cref="ChangeLog"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ChangeLog"/> to copy from.</param>
        public void CopyFrom(ChangeLog from)
        {
            var fval = ValidateCopyFromType<ChangeLog>(from);

            base.CopyFrom(fval);
            CreatedDate = fval.CreatedDate;
            CreatedBy = fval.CreatedBy;
            UpdatedDate = fval.UpdatedDate;
            UpdatedBy = fval.UpdatedBy;
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a deep copy of the <see cref="ChangeLog"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="ChangeLog"/>.</returns>
        public override object Clone()
        {
            ChangeLog clone = new();
            clone.CopyFrom(this);
            return clone;
        }

        #endregion

        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="ChangeLog"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            CreatedDate = Cleaner.Clean(CreatedDate, DateTimeTransform.UseDefault);
            CreatedBy = Cleaner.Clean(CreatedBy, StringTrim.UseDefault, StringTransform.UseDefault);
            UpdatedDate = Cleaner.Clean(UpdatedDate, DateTimeTransform.UseDefault);
            UpdatedBy = Cleaner.Clean(UpdatedBy, StringTrim.UseDefault, StringTransform.UseDefault);
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial
        {
            get
            {
                return Cleaner.IsInitial(CreatedDate)
                    && Cleaner.IsInitial(CreatedBy)
                    && Cleaner.IsInitial(UpdatedDate)
                    && Cleaner.IsInitial(UpdatedBy);
            }
        }

        #endregion
    }
}